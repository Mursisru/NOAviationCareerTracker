using System.Collections.Generic;
using UnityEngine;

namespace NOLoader.AviationCareer.ACT.UI
{
    public sealed class ACTUIRegistry
    {
        private readonly Dictionary<ACTGameContextKind, GameObject> _hosts = new Dictionary<ACTGameContextKind, GameObject>();

        public void RegisterHost(ACTGameContextKind kind, GameObject host)
        {
            if (host == null)
                return;
            _hosts[kind] = host;
        }

        public GameObject? GetHost(ACTGameContextKind kind)
        {
            _hosts.TryGetValue(kind, out var go);
            return go;
        }

        public void Clear()
        {
            foreach (var kv in _hosts)
            {
                if (kv.Value != null)
                    Object.Destroy(kv.Value);
            }
            _hosts.Clear();
        }
    }
}
