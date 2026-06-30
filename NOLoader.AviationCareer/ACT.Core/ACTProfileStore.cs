using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NOLoader.AviationCareer.ACT.Core.Progression;

namespace NOLoader.AviationCareer.ACT.Core
{
    public sealed class ACTProfileStore
    {
        private readonly string _path;

        public long TotalXp { get; set; }
        public int CurrentRank { get; set; } = 1;
        public int XpOverlayThreshold { get; set; } = 250;
        public float LifetimeWeightedKd { get; set; }
        public string FavoriteAircraft { get; set; } = "—";
        public string FavoriteTactic { get; set; } = "—";
        public string FavoriteWeapon { get; set; } = "—";
        public float DestroyedValueSum { get; set; }
        public float LostValueSum { get; set; }

        public ACTProfileStore(string path)
        {
            _path = path;
        }

        public void Load()
        {
            if (!File.Exists(_path))
                return;

            foreach (var line in File.ReadAllLines(_path))
            {
                var idx = line.IndexOf('=');
                if (idx <= 0)
                    continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();
                switch (key)
                {
                    case "TotalXp": long.TryParse(val, out var xp); TotalXp = xp; break;
                    case "CurrentRank": int.TryParse(val, out var rank); CurrentRank = rank; break;
                    case "XpOverlayThreshold": int.TryParse(val, out var th); XpOverlayThreshold = th; break;
                    case "LifetimeWeightedKd": float.TryParse(val, out var kd); LifetimeWeightedKd = kd; break;
                    case "FavoriteAircraft": FavoriteAircraft = val; break;
                    case "FavoriteTactic": FavoriteTactic = val; break;
                    case "FavoriteWeapon": FavoriteWeapon = val; break;
                    case "DestroyedValueSum": float.TryParse(val, out var d); DestroyedValueSum = d; break;
                    case "LostValueSum": float.TryParse(val, out var l); LostValueSum = l; break;
                }
            }
        }

        public void Save()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TotalXp={TotalXp}");
            sb.AppendLine($"CurrentRank={CurrentRank}");
            sb.AppendLine($"XpOverlayThreshold={XpOverlayThreshold}");
            sb.AppendLine($"LifetimeWeightedKd={LifetimeWeightedKd:F4}");
            sb.AppendLine($"FavoriteAircraft={FavoriteAircraft}");
            sb.AppendLine($"FavoriteTactic={FavoriteTactic}");
            sb.AppendLine($"FavoriteWeapon={FavoriteWeapon}");
            sb.AppendLine($"DestroyedValueSum={DestroyedValueSum:F2}");
            sb.AppendLine($"LostValueSum={LostValueSum:F2}");
            File.WriteAllText(_path, sb.ToString());
        }

        public void ApplySessionXp(long delta)
        {
            TotalXp = Math.Max(0, TotalXp + delta);
            CurrentRank = RankSystem.GetRankForXp(TotalXp);
        }
    }
}
