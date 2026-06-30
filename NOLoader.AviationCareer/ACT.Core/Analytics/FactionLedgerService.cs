using System.Collections.Generic;
using System.Text;

namespace NOLoader.AviationCareer.ACT.Core.Analytics
{
    public sealed class FactionLedgerService : IACTSessionService
    {
        private readonly ACTSessionRuntime _session;
        public int FriendlyFireEvents { get; private set; }
        public int CollateralEvents { get; private set; }
        public int GuardianAngelEvents { get; private set; }
        public int KillsByTeam { get; private set; }
        private readonly List<string> _log = new List<string>();

        public FactionLedgerService(ACTSessionRuntime session) => _session = session;

        public void ResetSession()
        {
            FriendlyFireEvents = 0;
            CollateralEvents = 0;
            GuardianAngelEvents = 0;
            KillsByTeam = 0;
            _log.Clear();
        }

        public void OnEvent(ACTEvent evt)
        {
            switch (evt.Kind)
            {
                case ACTEventKind.DamageApplied:
                    if (evt.Payload.IndexOf("friendly", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        FriendlyFireEvents++;
                    break;
                case ACTEventKind.UnitDestroyed:
                    KillsByTeam++;
                    _log.Add($"[{evt.SessionTime:F1}] Kill entity={evt.SourceEntityId}");
                    break;
                case ACTEventKind.MissileTargetAssigned:
                    break;
            }
        }

        public void RecordGuardianAngel(float time, uint interceptorId, uint missileId)
        {
            GuardianAngelEvents++;
            _log.Add($"[{time:F1}] GuardianAngel interceptor={interceptorId} missile={missileId}");
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.PatternDetected, time, interceptorId, missileId, "GuardianAngel"));
        }

        public void RecordCollateral(float time) => CollateralEvents++;

        public string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Kills: {KillsByTeam}");
            sb.AppendLine($"Friendly Fire: {FriendlyFireEvents}");
            sb.AppendLine($"Collateral: {CollateralEvents}");
            sb.AppendLine($"Guardian Angel: {GuardianAngelEvents}");
            for (int i = 0; i < _log.Count && i < 20; i++)
                sb.AppendLine(_log[i]);
            return sb.ToString();
        }
    }
}
