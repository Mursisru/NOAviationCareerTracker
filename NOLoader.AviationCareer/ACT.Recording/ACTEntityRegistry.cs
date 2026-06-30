using System.Collections.Generic;

namespace NOLoader.AviationCareer.ACT.Recording
{
    public sealed class ACTEntityRegistry
    {
        private readonly Dictionary<int, uint> _instanceToStable = new Dictionary<int, uint>();
        private readonly Dictionary<uint, EntityRecord> _entities = new Dictionary<uint, EntityRecord>();
        private uint _nextId = 1;

        public uint ResolveStableId(int instanceId)
        {
            if (_instanceToStable.TryGetValue(instanceId, out var id))
                return id;
            id = _nextId++;
            _instanceToStable[instanceId] = id;
            return id;
        }

        public void Register(uint stableId, NorepFormat.EntityType type, byte factionId, string name, uint prefabHash, bool playerControlled)
        {
            _entities[stableId] = new EntityRecord(stableId, type, factionId, name, prefabHash, playerControlled);
        }

        public IReadOnlyDictionary<uint, EntityRecord> Entities => _entities;

        public void Clear()
        {
            _instanceToStable.Clear();
            _entities.Clear();
            _nextId = 1;
        }
    }

    public readonly struct EntityRecord
    {
        public uint StableId { get; }
        public NorepFormat.EntityType Type { get; }
        public byte FactionId { get; }
        public string Name { get; }
        public uint PrefabHash { get; }
        public bool PlayerControlled { get; }

        public EntityRecord(uint stableId, NorepFormat.EntityType type, byte factionId, string name, uint prefabHash, bool playerControlled)
        {
            StableId = stableId;
            Type = type;
            FactionId = factionId;
            Name = name ?? string.Empty;
            PrefabHash = prefabHash;
            PlayerControlled = playerControlled;
        }
    }
}
