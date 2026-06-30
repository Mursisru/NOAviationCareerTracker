using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public sealed class RecorderService : IACTSessionService
    {
        private readonly ACTSessionRuntime _session;
        private readonly ACT.Recording.ACTNorepWriter _writer;
        public bool IsRecording { get; private set; }
        public string? CurrentFile { get; private set; }
        private readonly List<string> _recordings = new List<string>();

        public RecorderService(ACTSessionRuntime session, ACT.Recording.ACTNorepWriter writer)
        {
            _session = session;
            _writer = writer;
            RefreshRecordingList();
        }

        public void ResetSession()
        {
            IsRecording = false;
        }

        public void OnEvent(ACTEvent evt)
        {
            if (evt.Kind == ACTEventKind.SessionStarted)
            {
                IsRecording = true;
                CurrentFile = _writer.CurrentPath;
            }
            else if (evt.Kind == ACTEventKind.SessionEnded)
            {
                IsRecording = false;
                RefreshRecordingList();
            }
        }

        public void RefreshRecordingList()
        {
            _recordings.Clear();
            var dir = Path.Combine(ACTBootstrap.ModRoot, "Data", "Recordings");
            if (!Directory.Exists(dir))
                return;
            foreach (var f in Directory.GetFiles(dir, "*.norep"))
                _recordings.Add(f);
            _recordings.Sort();
        }

        public IReadOnlyList<string> Recordings => _recordings;

        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(IsRecording ? "Recording: ACTIVE" : "Recording: idle");
            if (!string.IsNullOrEmpty(CurrentFile))
                sb.AppendLine($"Current: {Path.GetFileName(CurrentFile)}");
            sb.AppendLine($"Saved replays: {_recordings.Count}");
            return sb.ToString();
        }
    }
}
