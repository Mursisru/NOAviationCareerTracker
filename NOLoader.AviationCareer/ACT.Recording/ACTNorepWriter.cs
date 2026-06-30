using System;
using System.IO;
using System.Text;
using System.Threading;

namespace NOLoader.AviationCareer.ACT.Recording
{
    public sealed class ACTNorepWriter : IDisposable
    {
        private readonly ACTDeltaBuffer _buffer;
        private readonly string _recordingsDir;
        private Thread? _writerThread;
        private volatile bool _running;
        private FileStream? _stream;
        private long _frameStreamStart;
        private uint _frameCount;
        private float _flushTimer;
        private Guid _sessionId;
        private string _missionName = "Menu";
        private string _weatherId = "clear";

        public string? CurrentPath { get; private set; }

        public ACTNorepWriter(ACTDeltaBuffer buffer, string recordingsDir)
        {
            _buffer = buffer;
            _recordingsDir = recordingsDir;
            Directory.CreateDirectory(recordingsDir);
        }

        public void StartSession(string meta)
        {
            StopAndFinalize();
            _sessionId = Guid.NewGuid();
            _missionName = string.IsNullOrEmpty(meta) ? "Menu" : meta;
            _weatherId = "clear";
            _frameCount = 0;
            var fileName = $"recording_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}_{Sanitize(_missionName)}_{_sessionId:N}.norep";
            CurrentPath = Path.Combine(_recordingsDir, fileName);
            _stream = new FileStream(CurrentPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            WriteHeader();
            _frameStreamStart = _stream.Position;
            _running = true;
            _writerThread = new Thread(WriterLoop) { IsBackground = true, Name = "ACT.NorepWriter" };
            _writerThread.Start();
        }

        public void EnqueueExternal(ActFrame frame) => _buffer.Enqueue(frame);

        public void StopAndFinalize()
        {
            _running = false;
            _buffer.Signal.Set();
            if (_writerThread != null && _writerThread.IsAlive)
                _writerThread.Join(2000);
            _writerThread = null;
            if (_stream == null)
                return;
            FlushQueue();
            WriteFooter();
            _stream.Dispose();
            _stream = null;
        }

        public void TickFlush(float intervalSec)
        {
            _flushTimer += intervalSec;
            if (_flushTimer >= intervalSec)
            {
                _flushTimer = 0;
                _buffer.Signal.Set();
            }
        }

        private void WriterLoop()
        {
            while (_running)
            {
                _buffer.Signal.Wait(500);
                _buffer.Signal.Reset();
                FlushQueue();
            }
        }

        private void FlushQueue()
        {
            if (_stream == null)
                return;
            int batch = 0;
            while (_buffer.TryDequeue(out var frame) && batch < 256)
            {
                WriteFrame(frame);
                batch++;
            }
            _stream.Flush();
        }

        private void WriteHeader()
        {
            if (_stream == null)
                return;
            var missionBytes = Encoding.UTF8.GetBytes(_missionName);
            var weatherBytes = Encoding.UTF8.GetBytes(_weatherId);
            var header = new byte[NorepFormat.HeaderSize];
            Buffer.BlockCopy(NorepFormat.Magic, 0, header, 0, 6);
            BitConverter.GetBytes(NorepFormat.Version).CopyTo(header, 6);
            _sessionId.ToByteArray().CopyTo(header, 8);
            BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).CopyTo(header, 24);
            BitConverter.GetBytes((long)0).CopyTo(header, 32);
            BitConverter.GetBytes(Crc32(_missionName)).CopyTo(header, 40);
            BitConverter.GetBytes((ushort)missionBytes.Length).CopyTo(header, 44);
            BitConverter.GetBytes((ushort)weatherBytes.Length).CopyTo(header, 46);
            BitConverter.GetBytes(0f).CopyTo(header, 48);
            BitConverter.GetBytes((uint)_buffer.Registry.Entities.Count).CopyTo(header, 52);
            BitConverter.GetBytes(_frameCount).CopyTo(header, 56);
            BitConverter.GetBytes((uint)0).CopyTo(header, 60);
            _stream.Write(header, 0, header.Length);
            _stream.Write(missionBytes, 0, missionBytes.Length);
            _stream.Write(weatherBytes, 0, weatherBytes.Length);
        }

        private void WriteFrame(ActFrame frame)
        {
            if (_stream == null)
                return;
            _stream.WriteByte((byte)(frame.TickMs & 0xFF));
            _stream.WriteByte((byte)((frame.TickMs >> 8) & 0xFF));
            _stream.WriteByte((byte)((frame.TickMs >> 16) & 0xFF));
            _stream.WriteByte((byte)((frame.TickMs >> 24) & 0xFF));
            _stream.WriteByte((byte)frame.Type);
            var len = frame.Payload?.Length ?? 0;
            _stream.WriteByte((byte)(len & 0xFF));
            _stream.WriteByte((byte)((len >> 8) & 0xFF));
            _stream.WriteByte((byte)((len >> 16) & 0xFF));
            _stream.WriteByte((byte)((len >> 24) & 0xFF));
            if (len > 0)
                _stream.Write(frame.Payload, 0, len);
            _frameCount++;
        }

        private void WriteFooter()
        {
            if (_stream == null)
                return;
            var frameLen = _stream.Position - _frameStreamStart;
            var crc = Crc32Bytes(_stream, _frameStreamStart, frameLen);
            var footer = new byte[NorepFormat.FooterSize];
            BitConverter.GetBytes((ulong)_stream.Position).CopyTo(footer, 0);
            BitConverter.GetBytes(crc).CopyTo(footer, 8);
            Buffer.BlockCopy(NorepFormat.MagicEnd, 0, footer, 12, 4);
            _stream.Write(footer, 0, footer.Length);
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "Menu";
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s.Length > 32 ? s.Substring(0, 32) : s;
        }

        private static uint Crc32(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            return Crc32Bytes(bytes, 0, bytes.Length);
        }

        private static uint Crc32Bytes(byte[] data, int offset, long length)
        {
            uint crc = 0xFFFFFFFF;
            int end = offset + (int)length;
            for (int i = offset; i < end && i < data.Length; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                    crc = (crc >> 1) ^ (0xEDB88320u & ~((crc & 1) - 1));
            }
            return ~crc;
        }

        private static uint Crc32Bytes(Stream stream, long offset, long length)
        {
            var buf = new byte[Math.Min(length, 8192)];
            stream.Seek(offset, SeekOrigin.Begin);
            long remaining = length;
            uint crc = 0xFFFFFFFF;
            while (remaining > 0)
            {
                int read = stream.Read(buf, 0, (int)Math.Min(remaining, buf.Length));
                if (read <= 0)
                    break;
                for (int i = 0; i < read; i++)
                {
                    crc ^= buf[i];
                    for (int b = 0; b < 8; b++)
                        crc = (crc >> 1) ^ (0xEDB88320u & ~((crc & 1) - 1));
                }
                remaining -= read;
            }
            return ~crc;
        }

        public void Dispose() => StopAndFinalize();
    }
}
