using System.Collections.Generic;
using NOLoader.AviationCareer.ACT.Core.Data;

namespace NOLoader.AviationCareer.ACT.Core.Progression
{
    public static class WeightedKDCalculator
    {
        public static float Compute(float destroyedSum, float lostSum)
        {
            if (lostSum <= 0f)
                return destroyedSum > 0f ? float.MaxValue : 0f;
            return destroyedSum / lostSum;
        }

        public static float UnitValue(string unitName)
        {
            var entry = UnitEconomyDatabase.TryGet(unitName);
            if (entry == null)
                return 0f;
            return entry.CostMillions + entry.Weight;
        }
    }

    public sealed class ACTXPEngine
    {
        private readonly ACTProfileStore _profile;
        private long _pendingOverlayXp;
        private readonly List<ComboFactor> _activeCombos = new List<ComboFactor>();

        public ACTXPEngine(ACTProfileStore profile)
        {
            _profile = profile;
        }

        public void AddCombo(ComboFactorKind kind, float patternBonus = 0f)
        {
            if (kind == ComboFactorKind.TacticalPattern)
                _activeCombos.Add(new ComboFactor(kind, patternBonus));
            else
                _activeCombos.Add(new ComboFactor(kind, ComboMultiplierEngine.GetDelta(kind)));
        }

        public void ClearCombos() => _activeCombos.Clear();

        public long AwardKillXp(string victimName, bool isEnemy)
        {
            if (!isEnemy)
                return 0;
            var entry = UnitEconomyDatabase.TryGet(victimName);
            if (entry == null)
                return 0;
            float mult = ComboMultiplierEngine.Compute(_activeCombos);
            long xp = (long)(entry.Xp * mult);
            ApplyXp(xp);
            return xp;
        }

        public long ApplyPenalty(KillContext ctx)
        {
            long penalty = PenaltyEngine.ComputePenalty(ctx);
            if (penalty <= 0)
                return 0;
            ApplyXp(-penalty);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.XpPenaltyApplied, 0f, payload: ctx.VictimName));
            return penalty;
        }

        public long AwardWeaponXp(string weaponName)
        {
            int xp = WeaponsXpTable.TryGet(weaponName);
            if (xp <= 0)
                return 0;
            ApplyXp(xp);
            return xp;
        }

        private void ApplyXp(long delta)
        {
            _profile.ApplySessionXp(delta);
            if (delta >= ACTConfigCache.Instance.XpOverlayThreshold)
            {
                ACTEventBus.Publish(new ACTEvent(ACTEventKind.XpAwarded, 0f, payload: delta.ToString()));
                _pendingOverlayXp = 0;
            }
            else if (delta > 0)
            {
                _pendingOverlayXp += delta;
                if (_pendingOverlayXp >= ACTConfigCache.Instance.XpOverlayThreshold)
                {
                    ACTEventBus.Publish(new ACTEvent(ACTEventKind.XpAwarded, 0f, payload: _pendingOverlayXp.ToString()));
                    _pendingOverlayXp = 0;
                }
            }
        }
    }

    public static class WeaponsXpTable
    {
        private static readonly Dictionary<string, int> InterceptXp = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["Piledriver TBM HE"] = 200,
            ["ALND-4"] = 250,
            ["Piledriver TBM 20kt"] = 350,
            ["GPO-N 1.5kt"] = 500,
            ["GPO-N 250kt"] = 750
        };

        public static int TryGet(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName))
                return 5;
            foreach (var kv in InterceptXp)
            {
                if (weaponName.IndexOf(kv.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return kv.Value;
            }
            return 5;
        }
    }
}
