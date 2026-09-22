using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kruty1918.Telemetry.Serialization
{
    /// <summary>
    /// Minimal strict JSON reader producing a DOM of JsonValue. Used for ACK payloads,
    /// remote config and batch headers. Rejects trailing garbage and malformed input.
    /// </summary>
    public sealed class TelemetryJsonReader
    {
        private readonly string _s;
        private int _i;

        private TelemetryJsonReader(string s) { _s = s; }

        public static JsonValue Parse(string json)
        {
            var r = new TelemetryJsonReader(json);
            r.Ws();
            var v = r.ReadValue();
            r.Ws();
            if (r._i != r._s.Length) throw new FormatException("Trailing data at " + r._i);
            return v;
        }

        private void Ws() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }
        private char Peek() => _i < _s.Length ? _s[_i] : '\0';
        private char Take() => _i < _s.Length ? _s[_i++] : throw new FormatException("Unexpected end");

        private void Expect(char c)
        {
            if (Take() != c) throw new FormatException($"Expected '{c}' at {_i - 1}");
        }

        private JsonValue ReadValue()
        {
            Ws();
            char c = Peek();
            switch (c)
            {
                case '{': return ReadObject();
                case '[': return ReadArray();
                case '"': return new JsonValue(ReadString());
                case 't': Literal("true"); return new JsonValue(true);
                case 'f': Literal("false"); return new JsonValue(false);
                case 'n': Literal("null"); return new JsonValue();
                default:
                    if (c == '-' || char.IsDigit(c)) return ReadNumber();
                    throw new FormatException($"Unexpected '{c}' at {_i}");
            }
        }

        private void Literal(string lit)
        {
            for (int k = 0; k < lit.Length; k++)
                if (Take() != lit[k]) throw new FormatException($"Bad literal at {_i}");
        }

        private JsonValue ReadObject()
        {
            Expect('{');
            var obj = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
            Ws();
            if (Peek() == '}') { _i++; return new JsonValue(obj); }
            while (true)
            {
                Ws();
                string key = ReadString();
                Ws(); Expect(':');
                obj[key] = ReadValue();
                Ws();
                char c = Take();
                if (c == '}') break;
                if (c != ',') throw new FormatException($"Expected ',' at {_i - 1}");
            }
            return new JsonValue(obj);
        }

        private JsonValue ReadArray()
        {
            Expect('[');
            var arr = new List<JsonValue>();
            Ws();
            if (Peek() == ']') { _i++; return new JsonValue(arr); }
            while (true)
            {
                arr.Add(ReadValue());
                Ws();
                char c = Take();
                if (c == ']') break;
                if (c != ',') throw new FormatException($"Expected ',' at {_i - 1}");
            }
            return new JsonValue(arr);
        }

        private string ReadString()
        {
            Expect('"');
            var sb = new StringBuilder();
            while (true)
            {
                char c = Take();
                if (c == '"') return sb.ToString();
                if (c == '\\')
                {
                    char e = Take();
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw new FormatException("Bad \\u");
                            sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _i += 4;
                            break;
                        default: throw new FormatException($"Bad escape '\\{e}'");
                    }
                }
                else sb.Append(c);
            }
        }

        private JsonValue ReadNumber()
        {
            int start = _i;
            if (Peek() == '-') _i++;
            while (char.IsDigit(Peek())) _i++;
            bool isDouble = false;
            if (Peek() == '.') { isDouble = true; _i++; while (char.IsDigit(Peek())) _i++; }
            if (Peek() == 'e' || Peek() == 'E')
            {
                isDouble = true; _i++;
                if (Peek() == '+' || Peek() == '-') _i++;
                while (char.IsDigit(Peek())) _i++;
            }
            string num = _s.Substring(start, _i - start);
            if (!isDouble && long.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                return new JsonValue(l);
            if (double.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                return new JsonValue(d);
            throw new FormatException($"Bad number '{num}'");
        }
    }

    /// <summary>Discriminated JSON DOM value.</summary>
    public readonly struct JsonValue
    {
        public readonly object Raw;
        public JsonValue(object raw) { Raw = raw; }

        public bool IsNull => Raw == null;
        public bool IsObject => Raw is Dictionary<string, JsonValue>;
        public bool IsArray => Raw is List<JsonValue>;

        public Dictionary<string, JsonValue> Obj => (Dictionary<string, JsonValue>)Raw;
        public List<JsonValue> Arr => (List<JsonValue>)Raw;
        public string Str => Raw as string;
        public long Int => Raw is long l ? l : Raw is double d ? (long)d : throw new FormatException("not number");
        public double Num => Raw is long l ? l : Raw is double d ? d : throw new FormatException("not number");
        public bool Bool => Raw is bool b ? b : throw new FormatException("not bool");

        public bool TryGet(string key, out JsonValue v)
        {
            if (IsObject && Obj.TryGetValue(key, out v)) return true;
            v = default; return false;
        }

        public string GetString(string key)
            => TryGet(key, out var v) && v.Raw is string s ? s : null;
        public long GetInt(string key, long fallback = 0)
            => TryGet(key, out var v) && (v.Raw is long || v.Raw is double) ? v.Int : fallback;
        public bool GetBool(string key, bool fallback = false)
            => TryGet(key, out var v) && v.Raw is bool b ? b : fallback;
    }
}
