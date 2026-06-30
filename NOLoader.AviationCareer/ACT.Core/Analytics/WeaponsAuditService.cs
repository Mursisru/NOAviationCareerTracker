using System.Collections.Generic;
using System.Text;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public sealed class WeaponsAuditService : IACTSessionService
    {
        private readonly ACTSessionRuntime _session;
        private readonly Dictionary<string, int> _fired = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _intercepts = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        public int TotalInterceptXp { get; private set; }

        public WeaponsAuditService(ACTSessionRuntime session) => _session = session;

        public void ResetSession()
        {
            _fired.Clear();
            _intercepts.Clear();
            TotalInterceptXp = 0;
        }

        public void OnEvent(ACTEvent evt)
        {
            switch (evt.Kind)
            {
                case ACTEventKind.WeaponFired:
                    Increment(_fired, evt.Payload);
                    break;
                case ACTEventKind.MissileCleared:
                    if (evt.Payload.IndexOf("intercept", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Increment(_intercepts, "intercept");
                        int xp = Progression.WeaponsXpTable.TryGet(evt.Payload);
                        TotalInterceptXp += xp;
                        ACTBootstrap.XPEngine?.AwardWeaponXp(evt.Payload);
                    }
                    break;
            }
        }

        public string GetRecommendation()
        {
            string best = "—";
            int max = 0;
            foreach (var kv in _fired)
            {
                if (kv.Value > max)
                {
                    max = kv.Value;
                    best = kv.Key;
                }
            }
            return best;
        }

        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Intercept XP: {TotalInterceptXp}");
            foreach (var kv in _fired)
                sb.AppendLine($"Fired {kv.Key}: {kv.Value}");
            foreach (var kv in _intercepts)
                sb.AppendLine($"Intercept {kv.Key}: {kv.Value}");
            return sb.ToString();
        }

        private static void Increment(Dictionary<string, int> map, string key)
        {
            if (string.IsNullOrEmpty(key))
                key = "unknown";
            map.TryGetValue(key, out var c);
            map[key] = c + 1;
        }
    }
}
