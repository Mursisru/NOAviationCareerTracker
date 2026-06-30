using System;

namespace NOLoader.AviationCareer.ACT.Core.Progression
{
    public static class RankSystem
    {
        public const int MaxRank = 20;
        private const long BaseXpPerRank = 1000L;
        private const float RankGrowth = 1.25f;

        public static long XpRequiredForRank(int rank)
        {
            if (rank <= 1)
                return 0;
            rank = Math.Min(rank, MaxRank);
            long total = 0;
            for (int r = 2; r <= rank; r++)
                total += (long)(BaseXpPerRank * Math.Pow(RankGrowth, r - 2));
            return total;
        }

        public static int GetRankForXp(long totalXp)
        {
            for (int r = MaxRank; r >= 1; r--)
            {
                if (totalXp >= XpRequiredForRank(r))
                    return r;
            }
            return 1;
        }

        public static long XpToNextRank(long totalXp, int currentRank)
        {
            if (currentRank >= MaxRank)
                return 0;
            return XpRequiredForRank(currentRank + 1) - totalXp;
        }

        public static string GetRankTitle(int rank)
        {
            string[] titles =
            {
                "Cadet", "Ensign", "Lieutenant", "Captain", "Major",
                "Colonel", "Brigadier", "General", "Ace", "Double Ace",
                "Strategist", "Tactician", "Commander", "Marshal", "Supreme Ace",
                "Ghost Pilot", "Legend", "Myth", "Demigod", "Aviation Immortal"
            };
            int idx = Math.Max(0, Math.Min(rank - 1, titles.Length - 1));
            return titles[idx];
        }
    }
}
