using System.Collections.Generic;
using System.Text;
using Kruty1918.Telemetry.Canonicalization;

namespace Kruty1918.Telemetry.Fingerprinting
{
    /// <summary>
    /// Explains semantic differences between two fingerprint inputs — lists concrete
    /// changes like "semantics.movementSpeed: 5 -> 6" instead of two opaque hashes.
    /// </summary>
    public static class FingerprintDiff
    {
        public sealed class Change
        {
            public string Path;
            public string Old;
            public string New;
            public override string ToString() => $"{Path}: {Old} -> {New}";
        }

        public static List<Change> Diff(FingerprintInput oldInput, FingerprintInput newInput)
        {
            var changes = new List<Change>();
            DiffNode("$", oldInput?.ToCanonical() ?? CanonicalValue.Null,
                newInput?.ToCanonical() ?? CanonicalValue.Null, changes);
            return changes;
        }

        public static string Format(List<Change> changes)
        {
            if (changes.Count == 0) return "no semantic differences";
            var sb = new StringBuilder();
            foreach (var c in changes) sb.AppendLine(c.ToString());
            return sb.ToString();
        }

        private static void DiffNode(string path, CanonicalValue a, CanonicalValue b, List<Change> changes)
        {
            if (a.Type != b.Type) { Add(path, a, b, changes); return; }
            switch (a.Type)
            {
                case CanonicalValue.Kind.Map:
                    var am = ToMap(a.AsMap);
                    var bm = ToMap(b.AsMap);
                    foreach (var key in UnionKeys(am, bm))
                    {
                        bool ha = am.TryGetValue(key, out var av);
                        bool hb = bm.TryGetValue(key, out var bv);
                        string p = path + "." + key;
                        if (!ha) changes.Add(new Change { Path = p, Old = "<absent>", New = Render(bv) });
                        else if (!hb) changes.Add(new Change { Path = p, Old = Render(av), New = "<absent>" });
                        else DiffNode(p, av, bv, changes);
                    }
                    break;
                case CanonicalValue.Kind.List:
                    // Lists in the fingerprint model are either semantic-order-preserving
                    // or pre-sorted sets; compare element-wise, then report count deltas.
                    var al = a.AsList; var bl = b.AsList;
                    int n = al.Count < bl.Count ? al.Count : bl.Count;
                    for (int i = 0; i < n; i++)
                        DiffNode(path + "[" + i + "]", al[i], bl[i], changes);
                    for (int i = n; i < al.Count; i++)
                        changes.Add(new Change { Path = path + "[" + i + "]", Old = Render(al[i]), New = "<absent>" });
                    for (int i = n; i < bl.Count; i++)
                        changes.Add(new Change { Path = path + "[" + i + "]", Old = "<absent>", New = Render(bl[i]) });
                    break;
                default:
                    if (CanonicalJson.Write(a) != CanonicalJson.Write(b))
                        Add(path, a, b, changes);
                    break;
            }
        }

        private static void Add(string path, CanonicalValue a, CanonicalValue b, List<Change> changes)
            => changes.Add(new Change { Path = path, Old = Render(a), New = Render(b) });

        private static string Render(CanonicalValue v) => v == null ? "<absent>" : CanonicalJson.Write(v);

        private static Dictionary<string, CanonicalValue> ToMap(List<KeyValuePair<string, CanonicalValue>> list)
        {
            var d = new Dictionary<string, CanonicalValue>(System.StringComparer.Ordinal);
            foreach (var kv in list) d[kv.Key] = kv.Value;
            return d;
        }

        private static List<string> UnionKeys(Dictionary<string, CanonicalValue> a, Dictionary<string, CanonicalValue> b)
        {
            var keys = new SortedSet<string>(System.StringComparer.Ordinal);
            foreach (var k in a.Keys) keys.Add(k);
            foreach (var k in b.Keys) keys.Add(k);
            return new List<string>(keys);
        }
    }
}
