using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MenuPreviewIdUtility
    {
        public static string Normalize(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
        }

        public static string BaseId(string id)
        {
            string normalized = Normalize(id);
            int separator = normalized.IndexOf('-');
            return separator > 0 ? normalized.Substring(0, separator) : normalized;
        }

        public static bool TryResolve(
            Dictionary<string, MenuPreviewSpriteData> cache,
            string id,
            out MenuPreviewSpriteData data)
        {
            data = default;
            string normalized = Normalize(id);
            if (cache == null || cache.Count == 0 || string.IsNullOrEmpty(normalized))
                return false;
            if (cache.TryGetValue(normalized, out data))
                return true;

            string baseId = BaseId(normalized);
            if (!string.IsNullOrEmpty(baseId) && cache.TryGetValue(baseId, out data))
                return true;

            foreach (var pair in cache)
            {
                if (pair.Key.StartsWith(baseId + "-", StringComparison.OrdinalIgnoreCase))
                {
                    data = pair.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
