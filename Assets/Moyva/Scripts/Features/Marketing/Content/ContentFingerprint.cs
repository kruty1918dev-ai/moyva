using System;
using System.Collections.Generic;
using System.Text;

namespace Kruty1918.Moyva.Marketing.Content
{
    /// <summary>
    /// Stable fingerprint over the scanned content set. Any added/removed/
    /// re-GUIDed asset changes the fingerprint → the index auto-rebuilds.
    /// FNV-1a over sorted strings — deterministic across machines.
    /// </summary>
    public static class ContentFingerprint
    {
        public static string Compute(IEnumerable<string> parts)
        {
            var sorted = new List<string>(parts ?? Array.Empty<string>());
            sorted.Sort(StringComparer.Ordinal);

            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            foreach (var part in sorted)
            {
                foreach (byte b in Encoding.UTF8.GetBytes(part ?? string.Empty))
                {
                    hash ^= b;
                    hash *= prime;
                }
                hash ^= 0xFF;
                hash *= prime;
            }
            return hash.ToString("x16");
        }
    }
}
