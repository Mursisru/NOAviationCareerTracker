using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NOLoader.AviationCareer.ACT.Recording
{
    public readonly struct ActFrame
    {
        public uint TickMs { get; }
        public NorepFormat.FrameType Type { get; }
        public byte[] Payload { get; }

        public ActFrame(uint tickMs, NorepFormat.FrameType type, byte[] payload)
        {
            TickMs = tickMs;
            Type = type;
            Payload = payload ?? Array.Empty<byte>();
        }
    }

    public sealed class ACTDeltaBuffer
    {
        private readonly ConcurrentQueue<ActFrame> _queue = new ConcurrentQueue<ActFrame>();
        private readonly ManualResetEventSlim _signal = new ManualResetEventSlim(false);
        public ACTEntityRegistry Registry { get; } = new ACTEntityRegistry();

        public void Enqueue(ActFrame frame)
        {
            _queue.Enqueue(frame);
            _signal.Set();
        }

        public bool TryDequeue(out ActFrame frame) => _queue.TryDequeue(out frame);

        public int Count => _queue.Count;

        public ManualResetEventSlim Signal => _signal;

        public void Clear()
        {
            while (_queue.TryDequeue(out _)) { }
            Registry.Clear();
        }
    }
}
