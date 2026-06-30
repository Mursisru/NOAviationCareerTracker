using NOLoader.ModConfig;

namespace NOLoader.AviationCareer.ACT.Core
{
    public sealed class ACTConfigCache
    {
        public bool Enabled { get; private set; } = true;
        public int XpOverlayThreshold { get; private set; } = 250;
        public bool AutoRecord { get; private set; } = true;
        public float RecordFlushIntervalSec { get; private set; } = 2f;
        public float TacViewDefaultSpeed { get; private set; } = 1f;

        public static ACTConfigCache Instance { get; private set; } = new ACTConfigCache();

        public static void Load(ModIniConfig cfg)
        {
            var c = new ACTConfigCache
            {
                Enabled = cfg.GetBool("AviationCareer", "Enabled", true),
                XpOverlayThreshold = cfg.GetInt("AviationCareer", "XpOverlayThreshold", 250),
                AutoRecord = cfg.GetBool("AviationCareer", "AutoRecord", true),
                RecordFlushIntervalSec = cfg.GetFloat("AviationCareer", "RecordFlushIntervalSec", 2f),
                TacViewDefaultSpeed = cfg.GetFloat("AviationCareer", "TacViewDefaultSpeed", 1f)
            };
            Instance = c;
        }
    }
}
