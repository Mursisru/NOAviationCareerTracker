using System.IO;
using System.Text;

namespace NOLoader.AviationCareer.ACT.Recording
{
    public static class LegacyLogImporter
    {
        public static bool TryImportToNorep(string logPath, ACTNorepWriter writer, string missionName)
        {
            if (!File.Exists(logPath) || writer == null)
                return false;

            writer.StartSession(missionName);
            foreach (var line in File.ReadAllLines(logPath))
            {
                if (!line.StartsWith("[HB:", System.StringComparison.Ordinal))
                    continue;
                uint tick = ParseTick(line);
                writer.EnqueueExternal(new ActFrame(tick, NorepFormat.FrameType.EntityTransformDelta, Encoding.UTF8.GetBytes("legacy")));
            }
            writer.StopAndFinalize();
            return true;
        }

        private static uint ParseTick(string line)
        {
            int tIdx = line.IndexOf("T=", System.StringComparison.Ordinal);
            if (tIdx < 0)
                return 0;
            float.TryParse(line.Substring(tIdx + 2).Split(' ')[0], out var sec);
            return (uint)(sec * 1000f);
        }
    }
}
