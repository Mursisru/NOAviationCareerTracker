namespace NOLoader.AviationCareer.ACT.Core.Progression
{
    public enum ComboFactorKind
    {
        MultiKill,
        NuclearTactical,
        StrategicGift,
        TacticalPattern
    }

    public readonly struct ComboFactor
    {
        public ComboFactorKind Kind { get; }
        public float MultiplierDelta { get; }

        public ComboFactor(ComboFactorKind kind, float multiplierDelta)
        {
            Kind = kind;
            MultiplierDelta = multiplierDelta;
        }
    }

    public static class ComboMultiplierEngine
    {
        public const float Cap = 3.5f;

        public static float Compute(System.Collections.Generic.IReadOnlyList<ComboFactor> factors)
        {
            float sum = 1f;
            for (int i = 0; i < factors.Count; i++)
                sum += factors[i].MultiplierDelta;
            return System.Math.Min(Cap, sum);
        }

        public static float GetDelta(ComboFactorKind kind)
        {
            switch (kind)
            {
                case ComboFactorKind.MultiKill: return 0.5f;
                case ComboFactorKind.NuclearTactical: return 1.0f;
                case ComboFactorKind.StrategicGift: return 1.5f;
                default: return 0f;
            }
        }
    }
}
