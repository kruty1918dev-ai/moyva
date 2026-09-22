using System.Globalization;
using System.Text;

namespace Kruty1918.Telemetry.Serialization
{
    /// <summary>
    /// Minimal strict JSON writer for event/batch payloads (not the canonical fingerprint
    /// serializer — see Canonicalization). Writes valid UTF-8 JSON to a StringBuilder.
    /// Invariant culture. Non-finite floats are written as null by policy.
    /// </summary>
    public sealed class TelemetryJsonWriter
    {
        private readonly StringBuilder _sb;
        private bool _needsComma;

        public TelemetryJsonWriter(StringBuilder sb) { _sb = sb; }
        public TelemetryJsonWriter() : this(new StringBuilder(256)) { }

        public override string ToString() => _sb.ToString();
        public int Length => _sb.Length;

        public TelemetryJsonWriter BeginObject() { Comma(); _sb.Append('{'); _needsComma = false; return this; }
        public TelemetryJsonWriter EndObject() { _sb.Append('}'); _needsComma = true; return this; }
        public TelemetryJsonWriter BeginArray() { Comma(); _sb.Append('['); _needsComma = false; return this; }
        public TelemetryJsonWriter EndArray() { _sb.Append(']'); _needsComma = true; return this; }

        public TelemetryJsonWriter Key(string name) { Comma(); String(name); _sb.Append(':'); _needsComma = false; return this; }

        private void Comma() { if (_needsComma) _sb.Append(','); _needsComma = true; }

        public TelemetryJsonWriter Value(int v) { Comma(); _sb.Append(v.ToString(CultureInfo.InvariantCulture)); return this; }
        public TelemetryJsonWriter Value(long v) { Comma(); _sb.Append(v.ToString(CultureInfo.InvariantCulture)); return this; }
        public TelemetryJsonWriter Value(bool v) { Comma(); _sb.Append(v ? "true" : "false"); return this; }
        public TelemetryJsonWriter Value(float v) { Comma(); Number(v); return this; }
        public TelemetryJsonWriter Value(double v) { Comma(); Number(v); return this; }
        public TelemetryJsonWriter Null() { Comma(); _sb.Append("null"); return this; }

        public TelemetryJsonWriter Value(string v)
        {
            Comma();
            if (v == null) { _sb.Append("null"); return this; }
            String(v);
            return this;
        }

        public TelemetryJsonWriter Field(string name, int v) { Key(name); _needsComma = false; return Value(v); }
        public TelemetryJsonWriter Field(string name, long v) { Key(name); _needsComma = false; return Value(v); }
        public TelemetryJsonWriter Field(string name, bool v) { Key(name); _needsComma = false; return Value(v); }
        public TelemetryJsonWriter Field(string name, float v) { Key(name); _needsComma = false; return Value(v); }
        public TelemetryJsonWriter Field(string name, double v) { Key(name); _needsComma = false; return Value(v); }
        public TelemetryJsonWriter Field(string name, string v) { Key(name); _needsComma = false; return Value(v); }

        private void Number(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) { _sb.Append("null"); return; }
            _sb.Append(v.ToString("R", CultureInfo.InvariantCulture));
        }

        private void String(string s)
        {
            _sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': _sb.Append("\\\""); break;
                    case '\\': _sb.Append("\\\\"); break;
                    case '\n': _sb.Append("\\n"); break;
                    case '\r': _sb.Append("\\r"); break;
                    case '\t': _sb.Append("\\t"); break;
                    case '\b': _sb.Append("\\b"); break;
                    case '\f': _sb.Append("\\f"); break;
                    default:
                        if (c < ' ') _sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else _sb.Append(c);
                        break;
                }
            }
            _sb.Append('"');
        }
    }
}
