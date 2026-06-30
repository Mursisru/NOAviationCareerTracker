using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NOLoader.AviationCareer.ACT.Core.Data
{
    public static class UnitEconomyDatabase
    {
        private static readonly Dictionary<string, UnitEconomyEntry> ByKey =
            new Dictionary<string, UnitEconomyEntry>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            ByKey.Clear();
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream("NOLoader.AviationCareer.ACT.Core.Data.UnitEconomyCatalog.json"))
            {
                if (stream == null)
                    return;
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    ParseJson(json);
                }
            }
        }

        public static UnitEconomyEntry? TryGet(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (ByKey.TryGetValue(name, out var direct))
                return direct;
            foreach (var kv in ByKey)
            {
                if (name.IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kv.Value;
            }
            return null;
        }

        public static IReadOnlyDictionary<string, UnitEconomyEntry> All => ByKey;

        private static void ParseJson(string json)
        {
            int i = 0;
            while ((i = json.IndexOf("\"name\"", i, StringComparison.Ordinal)) >= 0)
            {
                var entry = new UnitEconomyEntry();
                entry.Name = ReadString(json, ref i, "name");
                entry.Category = ParseCategory(ReadString(json, ref i, "category"));
                entry.Weight = ReadFloat(json, ref i, "weight");
                entry.Xp = (int)ReadFloat(json, ref i, "xp");
                entry.CostMillions = ReadFloat(json, ref i, "costMillions");
                Register(entry);
            }
        }

        private static void Register(UnitEconomyEntry entry)
        {
            ByKey[entry.Name] = entry;
            if (entry.Aliases != null)
            {
                for (int a = 0; a < entry.Aliases.Length; a++)
                {
                    if (!string.IsNullOrEmpty(entry.Aliases[a]))
                        ByKey[entry.Aliases[a]] = entry;
                }
            }
        }

        private static UnitCategory ParseCategory(string cat)
        {
            if (cat == "Ground") return UnitCategory.Ground;
            if (cat == "Naval") return UnitCategory.Naval;
            if (cat == "Static") return UnitCategory.Static;
            return UnitCategory.Aircraft;
        }

        private static string ReadString(string json, ref int i, string key)
        {
            var marker = $"\"{key}\"";
            i = json.IndexOf(marker, i, StringComparison.Ordinal);
            if (i < 0) return string.Empty;
            i = json.IndexOf(':', i) + 1;
            i = json.IndexOf('"', i) + 1;
            int end = json.IndexOf('"', i);
            var val = json.Substring(i, end - i);
            i = end + 1;
            return val;
        }

        private static float ReadFloat(string json, ref int i, string key)
        {
            var marker = $"\"{key}\"";
            i = json.IndexOf(marker, i, StringComparison.Ordinal);
            if (i < 0) return 0f;
            i = json.IndexOf(':', i) + 1;
            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
            int end = i;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '.' || json[end] == '-')) end++;
            float.TryParse(json.Substring(i, end - i), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var val);
            i = end;
            return val;
        }
    }
}
