namespace NOLoader.AviationCareer.ACT.Recording
{
    public static class NorepFormat
    {
        public const int HeaderSize = 64;
        public const int FooterSize = 16;
        public const ushort Version = 0x0001;

        public static readonly byte[] Magic = { (byte)'N', (byte)'O', (byte)'R', (byte)'E', (byte)'P', 0 };
        public static readonly byte[] MagicEnd = { (byte)'P', (byte)'E', (byte)'R', (byte)'N' };

        public enum FrameType : byte
        {
            EntityTransformDelta = 0x01,
            InputEdge = 0x02,
            CombatEvent = 0x03,
            RadarStateChange = 0x04,
            EntitySpawn = 0x05,
            EntityDespawn = 0x06,
            SessionMarker = 0x07,
            BatchTransform = 0x08
        }

        public enum EntityType : byte
        {
            Aircraft = 0,
            Missile = 1,
            Ground = 2,
            Ship = 3,
            Building = 4,
            Static = 5
        }

        public enum CombatEventKind : byte
        {
            Fire = 0,
            Kill = 1,
            Damage = 2,
            MissileLaunch = 3,
            MissileIntercept = 4,
            Detonation = 5,
            Eject = 6,
            GearChange = 7
        }
    }
}
