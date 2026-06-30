using System.Collections.Generic;
using System.Text;
using NOLoader.AviationCareer.ACT.Core.Progression;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public sealed class TacticalPatternService : IACTSessionService
    {
        private readonly ACTSessionRuntime _session;
        private readonly RingBuffer45s _buffer = new RingBuffer45s();
        private readonly List<string> _detected = new List<string>();
        private float _seadStartTime;
        private bool _seadActive;

        public IReadOnlyList<string> DetectedPatterns => _detected;

        public TacticalPatternService(ACTSessionRuntime session) => _session = session;

        public void ResetSession()
        {
            _detected.Clear();
            _seadActive = false;
        }

        public void OnEvent(ACTEvent evt)
        {
            if (evt.Kind == ACTEventKind.EntityTransformDelta)
            {
                ParseAndPush(evt);
                TryMatchPatterns(evt.SessionTime);
            }
            else if (evt.Kind == ACTEventKind.SamHardLock)
            {
                _seadActive = true;
                _seadStartTime = evt.SessionTime;
            }
            else if (evt.Kind == ACTEventKind.WeaponFired && _seadActive)
            {
                if (evt.Payload.IndexOf("ARAD-116", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    TryAddPattern("SEAD Hard-Lock Duel", 0.7f, evt.SessionTime);
                _seadActive = false;
            }
        }

        private void ParseAndPush(ACTEvent evt)
        {
            var parts = evt.Payload.Split('|');
            if (parts.Length < 4)
                return;
            float.TryParse(parts[0], out var g);
            float.TryParse(parts[1], out var alt);
            float.TryParse(parts[2], out var spdKmh);
            float.TryParse(parts[3], out var mach);
            bool radarOff = parts.Length > 5 && parts[5] == "radarOff";
            _buffer.Push(new ACTTelemetrySample(evt.SessionTime, alt, spdKmh, mach, radarOff, g, false, 0f));
        }

        private void TryMatchPatterns(float time)
        {
            if (_buffer.Count < 2)
                return;

            var latest = _buffer.GetFromOldest(_buffer.Count - 1);
            if (latest.AltitudeM > 10000f && latest.SpeedMach >= 0.9f && latest.RadarOff)
                TryAddPattern("Stratosphere Ghost", 0.8f, time);

            if (latest.AltitudeM < 25f && latest.SpeedKmh > 400f && _buffer.Duration() >= 45f)
                TryAddPattern("NOE Terrain Masking", 0.6f, time);

            if (latest.BvrRangeKm > 35f && latest.SpeedMach > 1.4f)
                TryAddPattern("BVR Intercept", 0.5f, time);
        }

        private void TryAddPattern(string name, float bonus, float time)
        {
            if (_detected.Contains(name))
                return;
            _detected.Add(name);
            ACTBootstrap.XPEngine?.AddCombo(ComboFactorKind.TacticalPattern, bonus);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.PatternDetected, time, payload: name));
        }

        public string BuildReport()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _detected.Count; i++)
                sb.AppendLine($"✓ {_detected[i]}");
            if (_detected.Count == 0)
                sb.AppendLine("No patterns detected.");
            return sb.ToString();
        }
    }
}
