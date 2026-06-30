using System.Text;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public sealed class BlackBoxService : IACTSessionService
    {
        private const float CloseCallMaxDeltaSec = 2.0f;
        private readonly ACTSessionRuntime _session;
        public int CloseCallCount { get; private set; }
        private float _ejectTime = -1f;
        private float _crashTime = -1f;
        private bool _pilotSurvived;

        public BlackBoxService(ACTSessionRuntime session) => _session = session;

        public void ResetSession()
        {
            CloseCallCount = 0;
            _ejectTime = -1f;
            _crashTime = -1f;
            _pilotSurvived = false;
        }

        public void OnEvent(ACTEvent evt)
        {
            if (evt.Kind == ACTEventKind.PilotEjected)
            {
                _ejectTime = evt.SessionTime;
                _pilotSurvived = evt.Payload.IndexOf("survived", System.StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else if (evt.Kind == ACTEventKind.UnitDestroyed && evt.Payload.IndexOf("aircraft", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _crashTime = evt.SessionTime;
                EvaluateCloseCall();
            }
        }

        private void EvaluateCloseCall()
        {
            if (_ejectTime < 0 || _crashTime < 0)
                return;
            float delta = _crashTime - _ejectTime;
            if (delta >= 0 && delta <= CloseCallMaxDeltaSec && _pilotSurvived)
            {
                CloseCallCount++;
                ACTEventBus.Publish(new ACTEvent(ACTEventKind.PatternDetected, _crashTime, payload: "Close-Call Ejection"));
            }
        }

        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Close-Call Ejections: {CloseCallCount}");
            if (_ejectTime >= 0)
                sb.AppendLine($"Last eject T={_ejectTime:F2}s");
            return sb.ToString();
        }
    }
}
