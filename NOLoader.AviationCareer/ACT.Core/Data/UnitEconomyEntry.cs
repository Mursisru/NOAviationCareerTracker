namespace NOLoader.AviationCareer.ACT.Core.Data
{
    public enum UnitCategory
    {
        Aircraft,
        Ground,
        Naval,
        Static
    }

    public sealed class UnitEconomyEntry
    {
        public string Name { get; set; } = string.Empty;
        public UnitCategory Category { get; set; }
        public float Weight { get; set; }
        public int Xp { get; set; }
        public float CostMillions { get; set; }
        public string[] Aliases { get; set; } = System.Array.Empty<string>();
    }
}
