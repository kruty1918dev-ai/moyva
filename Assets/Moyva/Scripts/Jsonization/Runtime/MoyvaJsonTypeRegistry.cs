using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Safe allow-listed registry for polymorphic Moyva JSON objects.
    /// JSON never contains AssemblyQualifiedName and cannot instantiate arbitrary CLR types.
    /// </summary>
    public static class MoyvaJsonTypeRegistry
    {
        private static readonly object Sync = new();
        private static readonly Dictionary<Type, Dictionary<string, Type>> Cache = new();

        public static string StableId(Type type)
        {
            if (type == null)
                return string.Empty;

            string name = type.Name;

            string[] suffixes =
            {
                "BuildingModuleDefinition",
                "BuildingModule",
                "Definition",
                "Config",
                "Settings",
                "Profile",
                "Node",
                "SO",
                "Asset"
            };

            foreach (string suffix in suffixes)
            {
                if (name.EndsWith(suffix, StringComparison.Ordinal) &&
                    name.Length > suffix.Length)
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];

                if (char.IsUpper(c) && i > 0 &&
                    (char.IsLower(name[i - 1]) ||
                     (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                {
                    sb.Append('-');
                }

                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString().Replace('_', '-');
        }

        public static Type Resolve(Type expectedBaseType, string stableId)
        {
            if (expectedBaseType == null ||
                string.IsNullOrWhiteSpace(stableId))
                return null;

            lock (Sync)
            {
                if (!Cache.TryGetValue(expectedBaseType, out var map))
                {
                    map = BuildMap(expectedBaseType);
                    Cache[expectedBaseType] = map;
                }

                if (map.TryGetValue(stableId, out var type))
                    return type;

                // Migration alias: legacy concrete class names are accepted
                // only if they resolve to an allow-listed Moyva type assignable
                // to the expected base.
                foreach (var pair in map)
                {
                    if (string.Equals(
                        pair.Value.Name,
                        stableId,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Value;
                    }
                }

                return null;
            }
        }

        private static Dictionary<string, Type> BuildMap(Type expectedBaseType)
        {
            var result = new Dictionary<string, Type>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (type == null ||
                        type.IsAbstract ||
                        type.IsInterface ||
                        !expectedBaseType.IsAssignableFrom(type))
                        continue;

                    string ns = type.Namespace ?? string.Empty;
                    if (!ns.StartsWith(
                            "Kruty1918.Moyva",
                            StringComparison.Ordinal))
                        continue;

                    string id = StableId(type);
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!result.ContainsKey(id))
                        result.Add(id, type);

                    // Legacy alias for migration of existing JSON.
                    if (!result.ContainsKey(type.Name))
                        result.Add(type.Name, type);
                }
            }

            return result;
        }


        public static Type ResolveConfigModel(string model, string schema)
        {
            string wantedModel = model ?? string.Empty;
            string wantedSchema = schema ?? string.Empty;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract ||
                        !typeof(MoyvaJsonConfigObject).IsAssignableFrom(type))
                        continue;

                    string ns = type.Namespace ?? string.Empty;
                    if (!ns.StartsWith("Kruty1918.Moyva", StringComparison.Ordinal))
                        continue;

                    string stable = StableId(type);
                    string typeSchema = SchemaForConfigType(type);
                    if ((!string.IsNullOrWhiteSpace(wantedModel) &&
                         string.Equals(stable, wantedModel, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(wantedSchema) &&
                         string.Equals(typeSchema, wantedSchema, StringComparison.OrdinalIgnoreCase)))
                        return type;
                }
            }

            return null;
        }

        public static string SchemaForConfigType(Type type)
        {
            if (type == null) return string.Empty;
            if (type.Name == "BuildingDefinitionAsset") return "moyva.building";
            if (type.Name == "UnitClassConfig") return "moyva.unit";
            if (type.Name == "GraphAsset") return "moyva.generator-graph";
            if (type.Name == "WorldCreationDefaultsSO") return "moyva.world-creation";
            return "moyva." + StableId(type);
        }

        public static void ClearCache()
        {
            lock (Sync)
                Cache.Clear();
        }
    }
}
