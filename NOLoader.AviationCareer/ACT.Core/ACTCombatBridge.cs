using NOLoader.AviationCareer.ACT.Core.Data;
using NOLoader.AviationCareer.ACT.Core.Progression;

namespace NOLoader.AviationCareer.ACT.Core
{
    internal static class ACTCombatBridge
    {
        public static void Handle(ACTEvent evt)
        {
            if (ACTBootstrap.XPEngine == null || ACTBootstrap.Profile == null || ACTBootstrap.Session == null)
                return;

            switch (evt.Kind)
            {
                case ACTEventKind.UnitDestroyed:
                    ProcessKill(evt);
                    break;
                case ACTEventKind.DamageApplied:
                    break;
            }
        }

        private static void ProcessKill(ACTEvent evt)
        {
            string victim = evt.Payload;
            var entry = UnitEconomyDatabase.TryGet(victim);
            if (entry == null)
                return;

            bool friendly = evt.Payload.IndexOf("friendly", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (friendly)
            {
                var ctx = new KillContext(victim, "player", true, false, 0f, true);
                ACTBootstrap.XPEngine.ApplyPenalty(ctx);
                return;
            }

            ACTBootstrap.XPEngine.AwardKillXp(victim, true);
            ACTBootstrap.Session.AddDestroyedValue(WeightedKDCalculator.UnitValue(victim));
            ACTBootstrap.Session.RecomputeKd();
            ACTBootstrap.Profile.DestroyedValueSum += WeightedKDCalculator.UnitValue(victim);
        }
    }
}
