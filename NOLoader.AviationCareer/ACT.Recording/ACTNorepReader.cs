using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NOLoader.AviationCareer.ACT.Recording
{
    public sealed class ACTNorepReader
    {
        public Guid SessionId { get; private set; }
        public string MissionName { get; private set; } = string.Empty;
        public string WeatherId { get; private set; } = string.Empty;
        public List<ActFrame> Frames { get; } = new List<ActFrame>();

        public bool Load(string path)
        {
            Frames.Clear();
            if (!File.Exists(path))
                return false;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var header = new byte[NorepFormat.HeaderSize];
                if (fs.Read(header, 0, header.Length) != header.Length)
                    return false;

                for (int i = 0; i < 6; i++)
                {
                    if (header[i] != NorepFormat.Magic[i])
                        return false;
                }

                var guidBytes = new byte[16];
                Buffer.BlockCopy(header, 8, guidBytes, 0, 16);
                SessionId = new Guid(guidBytes);
                ushort missionLen = BitConverter.ToUInt16(header, 44);
                ushort weatherLen = BitConverter.ToUInt16(header, 46);
                var missionBytes = new byte[missionLen];
                var weatherBytes = new byte[weatherLen];
                fs.Read(missionBytes, 0, missionLen);
                fs.Read(weatherBytes, 0, weatherLen);
                MissionName = Encoding.UTF8.GetString(missionBytes);
                WeatherId = Encoding.UTF8.GetString(weatherBytes);

                long footerPos = fs.Length - NorepFormat.FooterSize;
                while (fs.Position < footerPos - 9)
                {
                    var tickBytes = new byte[4];
                    if (fs.Read(tickBytes, 0, 4) != 4)
                        break;
                    uint tick = BitConverter.ToUInt32(tickBytes, 0);
                    int typeByte = fs.ReadByte();
                    if (typeByte < 0)
                        break;
                    var lenBytes = new byte[4];
                    if (fs.Read(lenBytes, 0, 4) != 4)
                        break;
                    int len = BitConverter.ToInt32(lenBytes, 0);
                    var payload = new byte[len];
                    if (len > 0 && fs.Read(payload, 0, len) != len)
                        break;
                    Frames.Add(new ActFrame(tick, (NorepFormat.FrameType)typeByte, payload));
                }
            }
            return true;
        }
    }
}
