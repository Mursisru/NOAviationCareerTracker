using NOLoader.AviationCareer.ACT.Core.Data;

namespace NOLoader.AviationCareer.ACT.Core.Progression
{
    public enum KillIntent
    {
        EnemyKill,
        IntentionalTeamkill,
        NuclearCollateral
    }

    public readonly struct KillContext
    {
        public string VictimName { get; }
        public string KillerName { get; }
        public bool IsDirectHit { get; }
        public bool IsNuclearBlast { get; }
        public float BlastYieldKt { get; }
        public bool IsFriendly { get; }

        public KillContext(string victimName, string killerName, bool isDirectHit, bool isNuclearBlast, float blastYieldKt, bool isFriendly)
        {
            VictimName = victimName ?? string.Empty;
            KillerName = killerName ?? string.Empty;
            IsDirectHit = isDirectHit;
            IsNuclearBlast = isNuclearBlast;
            BlastYieldKt = blastYieldKt;
            IsFriendly = isFriendly;
        }
    }

    public static class PenaltyEngine
    {
        public const int TeamkillMultiplier = 4;

        public static KillIntent Classify(KillContext ctx)
        {
            if (!ctx.IsFriendly)
                return KillIntent.EnemyKill;
            if (ctx.IsNuclearBlast && !ctx.IsDirectHit)
                return KillIntent.NuclearCollateral;
            if (ctx.IsDirectHit)
                return KillIntent.IntentionalTeamkill;
            return KillIntent.NuclearCollateral;
        }

        public static long ComputePenalty(KillContext ctx)
        {
            if (Classify(ctx) != KillIntent.IntentionalTeamkill)
                return 0;
            var entry = UnitEconomyDatabase.TryGet(ctx.VictimName);
            if (entry == null)
                return 0;
            return entry.Xp * TeamkillMultiplier;
        }
    }
}
