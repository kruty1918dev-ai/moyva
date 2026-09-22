using System;
using System.Security.Cryptography;
using System.Text;
using Kruty1918.Telemetry.Canonicalization;

namespace Kruty1918.Telemetry.Fingerprinting
{
    /// <summary>
    /// Deterministic dataset fingerprint: SHA-256 over the canonical serialization of
    /// a <see cref="FingerprintInput"/>. Fixed format: "tfp1." + 64 lowercase hex chars.
    /// Two datasets share a fingerprint iff their semantic inputs are identical —
    /// a fingerprint defines a dataset GROUP on the backend.
    /// </summary>
    public readonly struct DatasetFingerprint : IEquatable<DatasetFingerprint>
    {
        public const string Prefix = "tfp1.";
        public const string Algorithm = "SHA-256";
        public const string Encoding = "canonical-json/utf8-nobom";

        public readonly string Value;

        private DatasetFingerprint(string value) { Value = value; }

        public static DatasetFingerprint Compute(FingerprintInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = CanonicalJson.WriteUtf8(input.ToCanonical());
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(Prefix.Length + 64);
                sb.Append(Prefix);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return new DatasetFingerprint(sb.ToString());
            }
        }

        /// <summary>Parse a formatted fingerprint; throws on malformed input.</summary>
        public static DatasetFingerprint Parse(string s)
        {
            if (!IsValid(s)) throw new FormatException("malformed fingerprint: " + s);
            return new DatasetFingerprint(s);
        }

        public static bool IsValid(string s)
        {
            if (s == null || !s.StartsWith(Prefix, StringComparison.Ordinal) || s.Length != Prefix.Length + 64)
                return false;
            for (int i = Prefix.Length; i < s.Length; i++)
            {
                char c = s[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
            }
            return true;
        }

        public bool Equals(DatasetFingerprint other) => Value == other.Value;
        public override bool Equals(object obj) => obj is DatasetFingerprint o && Equals(o);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => Value;
        public static bool operator ==(DatasetFingerprint a, DatasetFingerprint b) => a.Equals(b);
        public static bool operator !=(DatasetFingerprint a, DatasetFingerprint b) => !a.Equals(b);

        public static readonly DatasetFingerprint Empty = new DatasetFingerprint(Prefix + new string('0', 64));
    }
}
