using System;
using System.Collections.Generic;

namespace NOLoader.AviationCareer.ACT.Core
{
    public interface IACTSessionService
    {
        void OnEvent(ACTEvent evt);
        void ResetSession();
    }

    public sealed class ACTSessionRuntime
    {
        public float SessionStartTime { get; private set; }
        public bool IsActive { get; private set; }
        public long SessionXp { get; private set; }
        public long SessionPenalties { get; private set; }
        public float SessionWeightedKd { get; private set; }

        private readonly List<IACTSessionService> _services = new List<IACTSessionService>();
        private float _destroyedValue;
        private float _lostValue;

        public void RegisterService(IACTSessionService service)
        {
            if (service != null)
                _services.Add(service);
        }

        public void Dispatch(ACTEvent evt)
        {
            for (int i = 0; i < _services.Count; i++)
                _services[i].OnEvent(evt);

            if (evt.Kind == ACTEventKind.SessionStarted)
                BeginSession(evt.SessionTime);
            else if (evt.Kind == ACTEventKind.SessionEnded)
                EndSession();
            else if (evt.Kind == ACTEventKind.XpAwarded && long.TryParse(evt.Payload, out var xp))
                SessionXp += xp;
            else if (evt.Kind == ACTEventKind.XpPenaltyApplied)
                SessionPenalties += 1;
        }

        public void AddDestroyedValue(float v) => _destroyedValue += v;
        public void AddLostValue(float v) => _lostValue += v;

        public void RecomputeKd()
        {
            SessionWeightedKd = Progression.WeightedKDCalculator.Compute(_destroyedValue, _lostValue);
        }

        private void BeginSession(float t)
        {
            SessionStartTime = t;
            IsActive = true;
            SessionXp = 0;
            SessionPenalties = 0;
            _destroyedValue = 0;
            _lostValue = 0;
            for (int i = 0; i < _services.Count; i++)
                _services[i].ResetSession();
        }

        private void EndSession()
        {
            IsActive = false;
            RecomputeKd();
        }

        public float Elapsed(float now) => IsActive ? now - SessionStartTime : 0f;
    }
}
