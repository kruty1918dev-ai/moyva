using System.Collections.Generic;

namespace Kruty1918.Telemetry.Contracts
{
    public enum ContractFieldType
    {
        Int = 0, Long = 1, Float = 2, Double = 3, Bool = 4, String = 5,
        IntArray = 6, LongArray = 7, FloatArray = 8, StringArray = 9,
        Object = 10, ObjectArray = 11,
    }

    /// <summary>
    /// One field of an <see cref="EventContract"/>. Carries both structural rules
    /// (type, required, ranges, length caps) and semantic metadata (units, meaning)
    /// that participates in the dataset fingerprint when flagged.
    /// </summary>
    public sealed class ContractField
    {
        public string Name;
        public ContractFieldType Type;
        public bool Required;
        /// <summary>Physical/logical unit, e.g. "tiles", "seconds", "count". Part of fingerprint when <see cref="FingerprintInclude"/>.</summary>
        public string Unit;
        /// <summary>Human + machine readable meaning of the value. Changing this changes the fingerprint.</summary>
        public string Semantics;
        public double? Min;
        public double? Max;
        /// <summary>Max string length / array length. Default 256 for strings, 1024 for arrays.</summary>
        public int MaxLength;
        /// <summary>When true this field's metadata participates in the dataset fingerprint.</summary>
        public bool FingerprintInclude = true;
        /// <summary>Privacy classification applied to the VALUE (strings are sanitized).</summary>
        public PrivacyClass Privacy = PrivacyClass.Gameplay;

        public ContractField(string name, ContractFieldType type, bool required = true)
        {
            Name = name; Type = type; Required = required;
            MaxLength = type == ContractFieldType.String ? 256 : 1024;
        }

        public bool ValidateScalar(object value, out string error)
        {
            error = null;
            if (value == null)
            {
                if (Required) error = "required-null";
                return Required == false;
            }
            double d;
            switch (Type)
            {
                case ContractFieldType.Int:
                case ContractFieldType.Long:
                    if (!(value is long || value is int)) { error = "type"; return false; }
                    d = value is long l ? l : (int)value;
                    break;
                case ContractFieldType.Float:
                case ContractFieldType.Double:
                    if (value is float f) d = f;
                    else if (value is double dd) d = dd;
                    else if (value is int i) d = i;
                    else if (value is long ll) d = ll;
                    else { error = "type"; return false; }
                    if (double.IsNaN(d) || double.IsInfinity(d)) { error = "non-finite"; return false; }
                    break;
                case ContractFieldType.Bool:
                    if (!(value is bool)) { error = "type"; return false; }
                    return true;
                case ContractFieldType.String:
                    if (!(value is string s)) { error = "type"; return false; }
                    if (MaxLength > 0 && s.Length > MaxLength) { error = "maxlen"; return false; }
                    return true;
                default:
                    return true; // arrays/objects validated by their writers
            }
            if (Min.HasValue && d < Min.Value) { error = "min"; return false; }
            if (Max.HasValue && d > Max.Value) { error = "max"; return false; }
            return true;
        }
    }
}
