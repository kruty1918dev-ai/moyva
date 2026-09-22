using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kruty1918.Telemetry.Canonicalization
{
    /// <summary>
    /// Normalized object-graph node for deterministic canonical serialization.
    /// Maps are serialized with ordinal-sorted keys; callers that need unordered
    /// collections must sort before constructing (or use <see cref="CanonicalValue.SortedSet"/>).
    /// </summary>
    public sealed class CanonicalValue
    {
        public enum Kind { Null, Bool, Int, Real, Text, List, Map }

        public Kind Type { get; }
        public object Raw { get; }

        private CanonicalValue(Kind t, object raw) { Type = t; Raw = raw; }

        public static readonly CanonicalValue Null = new CanonicalValue(Kind.Null, null);
        public static CanonicalValue Of(bool v) => new CanonicalValue(Kind.Bool, v);
        public static CanonicalValue Of(long v) => new CanonicalValue(Kind.Int, v);
        public static CanonicalValue Of(int v) => Of((long)v);
        public static CanonicalValue Of(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v))
                throw new ArgumentException("non-finite value cannot enter fingerprint input");
            return new CanonicalValue(Kind.Real, v);
        }
        public static CanonicalValue Of(string v) => v == null ? Null : new CanonicalValue(Kind.Text, v);
        public static CanonicalValue Of(IEnumerable<CanonicalValue> items)
            => new CanonicalValue(Kind.List, new List<CanonicalValue>(items));

        /// <summary>Build a map from possibly-unordered pairs; serialized with ordinal-sorted keys.</summary>
        public static CanonicalValue Of(IEnumerable<KeyValuePair<string, CanonicalValue>> fields)
        {
            var sorted = new SortedDictionary<string, CanonicalValue>(StringComparer.Ordinal);
            foreach (var f in fields)
            {
                if (sorted.ContainsKey(f.Key))
                    throw new ArgumentException("duplicate canonical key: " + f.Key);
                sorted[f.Key] = f.Value ?? Null;
            }
            return new CanonicalValue(Kind.Map, new List<KeyValuePair<string, CanonicalValue>>(sorted));
        }

        /// <summary>Build a map from possibly-unordered pairs; duplicate keys throw.</summary>
        public static CanonicalValue Map(params KeyValuePair<string, CanonicalValue>[] fields) => Of(fields);
        public static KeyValuePair<string, CanonicalValue> F(string name, CanonicalValue v)
            => new KeyValuePair<string, CanonicalValue>(name, v ?? Null);

        /// <summary>A semantically unordered collection: items are sorted by canonical form.</summary>
        public static CanonicalValue SortedSet(IEnumerable<CanonicalValue> items)
        {
            var encoded = new List<(string canon, CanonicalValue v)>();
            foreach (var it in items)
            {
                var c = CanonicalJson.Write(it);
                encoded.Add((c, it));
            }
            encoded.Sort((a, b) => string.CompareOrdinal(a.canon, b.canon));
            var list = new List<CanonicalValue>(encoded.Count);
            foreach (var e in encoded) list.Add(e.v);
            return new CanonicalValue(Kind.List, list);
        }

        public List<CanonicalValue> AsList => (List<CanonicalValue>)Raw;
        public List<KeyValuePair<string, CanonicalValue>> AsMap => (List<KeyValuePair<string, CanonicalValue>>)Raw;
    }

    /// <summary>
    /// Deterministic canonical JSON serializer used exclusively for fingerprints and
    /// contract identity. Guarantees: ordinal key order, invariant numbers
    /// (integers bare, reals as round-trip "R" normalized), \uXXXX for control chars,
    /// no whitespace, UTF-8 output without BOM, explicit null, no dictionary-order
    /// dependence (maps are stored pre-sorted), no timestamps injected.
    /// </summary>
    public static class CanonicalJson
    {
        public static string Write(CanonicalValue v)
        {
            var sb = new StringBuilder(256);
            Write(v, sb);
            return sb.ToString();
        }

        public static byte[] WriteUtf8(CanonicalValue v)
            => Encoding.UTF8.GetBytes(Write(v)); // UTF8Encoding emits no BOM via GetBytes

        private static void Write(CanonicalValue v, StringBuilder sb)
        {
            switch (v.Type)
            {
                case CanonicalValue.Kind.Null: sb.Append("null"); break;
                case CanonicalValue.Kind.Bool: sb.Append((bool)v.Raw ? "true" : "false"); break;
                case CanonicalValue.Kind.Int: sb.Append(((long)v.Raw).ToString(CultureInfo.InvariantCulture)); break;
                case CanonicalValue.Kind.Real:
                    // Normalize -0 and integral reals so 5 and 5.0 serialize identically.
                    double d = (double)v.Raw;
                    if (d == Math.Floor(d) && Math.Abs(d) < 9.0e15)
                        sb.Append(((long)d).ToString(CultureInfo.InvariantCulture));
                    else
                        sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
                    break;
                case CanonicalValue.Kind.Text: WriteString((string)v.Raw, sb); break;
                case CanonicalValue.Kind.List:
                    sb.Append('[');
                    var list = v.AsList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Write(list[i], sb);
                    }
                    sb.Append(']');
                    break;
                case CanonicalValue.Kind.Map:
                    sb.Append('{');
                    var map = v.AsMap;
                    for (int i = 0; i < map.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        WriteString(map[i].Key, sb);
                        sb.Append(':');
                        Write(map[i].Value, sb);
                    }
                    sb.Append('}');
                    break;
            }
        }

        private static void WriteString(string s, StringBuilder sb)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
