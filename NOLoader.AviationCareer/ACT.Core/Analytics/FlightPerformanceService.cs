using System.Text;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public sealed class FlightPerformanceService : IACTSessionService
    {
        private readonly ACTSessionRuntime _session;
        public float PeakGLoad { get; private set; }
        public float BellySlideDistanceM { get; private set; }
        public float GlideDistanceM { get; private set; }
        public float StormFlightSec { get; private set; }
        public float FogFlightSec { get; private set; }
        private bool _bellySlideActive;
        private bool _glideActive;
        private float _lastSampleTime;
        private string _weatherId = "clear";

        public FlightPerformanceService(ACTSessionRuntime session) => _session = session;

        public void ResetSession()
        {
            PeakGLoad = 0;
            BellySlideDistanceM = 0;
            GlideDistanceM = 0;
            StormFlightSec = 0;
            FogFlightSec = 0;
            _bellySlideActive = false;
            _glideActive = false;
            _lastSampleTime = 0;
        }

        public void OnEvent(ACTEvent evt)
        {
            if (evt.Kind == ACTEventKind.SessionStarted)
                _weatherId = evt.Payload;
            else if (evt.Kind == ACTEventKind.GearChanged)
            {
                bool gearDown = evt.Payload.IndexOf("DOWN", System.StringComparison.OrdinalIgnoreCase) >= 0;
                _bellySlideActive = !gearDown && evt.Payload.IndexOf("belly", System.StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else if (evt.Kind == ACTEventKind.EntityTransformDelta)
                ApplyTransformPayload(evt);
        }

        private void ApplyTransformPayload(ACTEvent evt)
        {
            var parts = evt.Payload.Split('|');
            if (parts.Length < 5)
                return;
            if (!float.TryParse(parts[0], out var g)) return;
            if (!float.TryParse(parts[1], out var alt)) return;
            if (!float.TryParse(parts[2], out var spd)) return;
            if (!float.TryParse(parts[3], out var dt)) return;
            if (!float.TryParse(parts[4], out var dist)) return;

            if (g > PeakGLoad)
                PeakGLoad = g;

            if (_bellySlideActive)
                BellySlideDistanceM += dist;
            if (_glideActive || (spd > 100f && alt > 50f && parts.Length > 5 && parts[5] == "nofuel"))
                GlideDistanceM += dist;

            if (_weatherId.IndexOf("storm", System.StringComparison.OrdinalIgnoreCase) >= 0)
                StormFlightSec += dt;
            if (_weatherId.IndexOf("fog", System.StringComparison.OrdinalIgnoreCase) >= 0)
                FogFlightSec += dt;

            _lastSampleTime = evt.SessionTime;
        }

        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Peak G: {PeakGLoad:F1}");
            sb.AppendLine($"Belly slide: {BellySlideDistanceM:F0} m");
            sb.AppendLine($"Glide: {GlideDistanceM:F0} m");
            sb.AppendLine($"Storm time: {StormFlightSec:F0} s");
            sb.AppendLine($"Fog time: {FogFlightSec:F0} s");
            return sb.ToString();
        }
    }
}
