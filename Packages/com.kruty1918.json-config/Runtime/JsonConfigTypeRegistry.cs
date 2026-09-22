using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kruty1918.JsonConfig
{
    /// <summary>
    /// Safe allow-listed registry for polymorphic host JSON objects.
    /// JSON never contains AssemblyQualifiedName and cannot instantiate arbitrary CLR types.
    /// </summary>
    public static class JsonConfigTypeRegistry
    {
        private static readonly object Sync = new();
        private static readonly Dictionary<Type, Dictionary<string, Type>> Cache = new();

        public static string StableId(Type type)
        {
            if (type == null)
                return string.Empty;

            string name = type.Name;

            List<string> suffixes = JsonConfigRuntime.Settings.StableIdSuffixes;
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
                // only if they resolve to an allow-listed host type assignable
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

            foreach (var assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
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

                    if (!NamespaceAllowed(type,
                            JsonConfigRuntime.Settings.RegistryNamespacePrefixes))
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

            foreach (var assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract ||
                        !typeof(JsonConfigObject).IsAssignableFrom(type))
                        continue;

                    if (!NamespaceAllowed(type,
                            JsonConfigRuntime.Settings.ModelNamespacePrefixes))
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

            var settings = JsonConfigRuntime.Settings;
            if (settings.SchemaNameResolver != null)
            {
                string resolved = settings.SchemaNameResolver(type);
                if (!string.IsNullOrWhiteSpace(resolved))
                    return resolved;
            }

            return settings.SchemaPrefix + "." + StableId(type);
        }

        /// <summary>
        /// Checks a type's namespace against a host allow-list of prefixes.
        /// An empty prefix list allows every namespace.
        /// </summary>
        internal static bool NamespaceAllowed(Type type, List<string> prefixes)
        {
            if (prefixes == null || prefixes.Count == 0)
                return true;

            string ns = type?.Namespace ?? string.Empty;
            for (int i = 0; i < prefixes.Count; i++)
            {
                string prefix = prefixes[i];
                if (!string.IsNullOrWhiteSpace(prefix) &&
                    ns.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        public static void ClearCache()
        {
            lock (Sync)
                Cache.Clear();
        }
    }
}
