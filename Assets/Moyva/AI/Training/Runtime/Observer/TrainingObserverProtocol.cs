using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingObserverEnvelope
    {
        public int protocolVersion;
        public string messageType;
        public string sessionId;
        public int arenaId = -1;
        public long episodeId;
        public long sequence;
        public object payload;
    }

    public sealed class ObserverProtocolStatus
    {
        public string reason;
    }

    public static class TrainingObserverProtocol
    {
        public const int CurrentVersion = 1;
        public const string Command = "Command";
        public const string AgentDecision = "AgentDecisionEvent";
        public const string Snapshot = "ArenaSnapshot";
        public const string ResyncRequired = "ResyncRequired";
        public const string Rejected = "Rejected";
        public const string Subscribed = "Subscribed";
        public const string Unsubscribed = "Unsubscribed";
        public const string AgentSelected = "AgentSelected";
        public const int MinimumMessageBytes = 1024;

        public static TrainingObserverEnvelope Create(string messageType, string sessionId, int arenaId,
            long episodeId, long sequence, object payload)
        {
            return new TrainingObserverEnvelope
            {
                protocolVersion = CurrentVersion,
                messageType = messageType,
                sessionId = sessionId ?? string.Empty,
                arenaId = arenaId,
                episodeId = episodeId,
                sequence = sequence,
                payload = payload
            };
        }

        public static TrainingObserverEnvelope CreateCommand(ObserverCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return Create(Command, command.sessionId, command.arenaId, 0, command.lastSequence, command);
        }

        public static byte[] EncodeMessage(TrainingObserverEnvelope envelope)
            => Encoding.UTF8.GetBytes(SerializeEnvelope(envelope));

        public static byte[] EncodeFrame(TrainingObserverEnvelope envelope, int maxMessageBytes)
        {
            byte[] body = EncodeMessage(envelope);
            if (body.Length > maxMessageBytes) throw new InvalidOperationException("Observer message exceeds configured maximum.");
            var frame = new byte[body.Length + 4];
            frame[0] = (byte)(body.Length & 0xff);
            frame[1] = (byte)((body.Length >> 8) & 0xff);
            frame[2] = (byte)((body.Length >> 16) & 0xff);
            frame[3] = (byte)((body.Length >> 24) & 0xff);
            Buffer.BlockCopy(body, 0, frame, 4, body.Length);
            return frame;
        }

        public static bool TryDecodeFrame(byte[] frame, int maxMessageBytes, out TrainingObserverEnvelope envelope, out string reason)
        {
            envelope = null;
            reason = null;
            if (frame == null || frame.Length < 4) { reason = "Malformed frame header."; return false; }
            int length = frame[0] | (frame[1] << 8) | (frame[2] << 16) | (frame[3] << 24);
            if (length < 1) { reason = "Empty observer message."; return false; }
            if (length > maxMessageBytes) { reason = "Observer message exceeds configured maximum."; return false; }
            if (frame.Length != length + 4) { reason = "Observer frame length mismatch."; return false; }
            var body = new byte[length];
            Buffer.BlockCopy(frame, 4, body, 0, length);
            return TryDecodeMessage(body, maxMessageBytes, out envelope, out reason);
        }

        public static bool TryDecodeMessage(byte[] body, int maxMessageBytes, out TrainingObserverEnvelope envelope, out string reason)
        {
            envelope = null;
            reason = null;
            if (body == null || body.Length == 0) { reason = "Empty observer message."; return false; }
            if (body.Length > maxMessageBytes) { reason = "Observer message exceeds configured maximum."; return false; }
            try
            {
                object parsed = new JsonParser(Encoding.UTF8.GetString(body)).ParseValue();
                var root = parsed as Dictionary<string, object>;
                if (root == null) { reason = "Observer envelope must be a JSON object."; return false; }
                int version = Int(root, "protocolVersion", -1);
                if (version != CurrentVersion)
                {
                    reason = "Unsupported observer protocol version " + version + "; expected " + CurrentVersion + ".";
                    return false;
                }
                string type = String(root, "messageType");
                if (string.IsNullOrWhiteSpace(type)) { reason = "Observer messageType is required."; return false; }
                envelope = new TrainingObserverEnvelope
                {
                    protocolVersion = version,
                    messageType = type,
                    sessionId = String(root, "sessionId") ?? string.Empty,
                    arenaId = Int(root, "arenaId", -1),
                    episodeId = Long(root, "episodeId", 0),
                    sequence = Long(root, "sequence", 0),
                    payload = root.TryGetValue("payload", out object payload) ? payload : null
                };
                return true;
            }
            catch (Exception e)
            {
                reason = "Malformed observer JSON: " + e.Message;
                return false;
            }
        }

        public static bool TryParseCommand(TrainingObserverEnvelope envelope, out ObserverCommand command, out string reason)
        {
            command = null;
            reason = null;
            if (envelope == null || !string.Equals(envelope.messageType, Command, StringComparison.Ordinal))
            { reason = "Expected Command message."; return false; }
            if (envelope.payload is ObserverCommand direct)
            {
                command = direct;
            }
            else
            {
                var payload = envelope.payload as Dictionary<string, object>;
                if (payload == null) { reason = "Command payload must be a JSON object."; return false; }
                int version = Int(payload, "version", ObserverCommand.CurrentVersion);
                if (version != ObserverCommand.CurrentVersion)
                { reason = "Unsupported ObserverCommand version " + version + "."; return false; }
                int kindValue = Int(payload, "kind", -1);
                if (!Enum.IsDefined(typeof(ObserverCommandKind), kindValue))
                { reason = "Unknown observer command kind."; return false; }
                command = new ObserverCommand
                {
                    version = version,
                    kind = (ObserverCommandKind)kindValue,
                    sessionId = String(payload, "sessionId") ?? envelope.sessionId,
                    arenaId = Int(payload, "arenaId", envelope.arenaId),
                    agentId = String(payload, "agentId"),
                    lastSequence = Long(payload, "lastSequence", envelope.sequence),
                    visuals = Bool(payload, "visuals", false)
                };
            }
            if (command.version != ObserverCommand.CurrentVersion)
            { reason = "Unsupported ObserverCommand version " + command.version + "."; command = null; return false; }
            return true;
        }

        public static string SerializeEnvelope(TrainingObserverEnvelope e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            var b = new StringBuilder(512);
            b.Append('{');
            Field(b, "protocolVersion", e.protocolVersion); b.Append(',');
            Field(b, "messageType", e.messageType); b.Append(',');
            Field(b, "sessionId", e.sessionId ?? string.Empty); b.Append(',');
            Field(b, "arenaId", e.arenaId); b.Append(',');
            Field(b, "episodeId", e.episodeId); b.Append(',');
            Field(b, "sequence", e.sequence); b.Append(',');
            b.Append("\"payload\":").Append(SerializePayload(e.payload));
            return b.Append('}').ToString();
        }

        private static string SerializePayload(object payload)
        {
            if (payload == null) return "null";
            if (payload is ObserverCommand c) return SerializeCommand(c);
            if (payload is AgentDecisionEvent d) return SerializeDecision(d);
            if (payload is ArenaSnapshot s) return SerializeSnapshot(s);
            if (payload is ObserverProtocolStatus status) return "{\"reason\":" + Quote(status.reason) + "}";
            if (payload is Dictionary<string, object> dictionary) return SerializeDictionary(dictionary);
            return Quote(payload.ToString());
        }

        private static string SerializeCommand(ObserverCommand c)
        {
            var b = new StringBuilder(192).Append('{');
            Field(b, "version", c.version); b.Append(','); Field(b, "kind", (int)c.kind); b.Append(',');
            Field(b, "sessionId", c.sessionId ?? string.Empty); b.Append(','); Field(b, "arenaId", c.arenaId); b.Append(',');
            Field(b, "agentId", c.agentId); b.Append(','); Field(b, "lastSequence", c.lastSequence); b.Append(',');
            Field(b, "visuals", c.visuals); return b.Append('}').ToString();
        }

        private static string SerializeDecision(AgentDecisionEvent d)
        {
            var b = new StringBuilder(512).Append('{');
            Field(b, "version", d.version); b.Append(','); Field(b, "sessionId", d.sessionId); b.Append(',');
            Field(b, "arenaId", d.arenaId); b.Append(','); Field(b, "episodeId", d.episodeId); b.Append(',');
            Field(b, "sequence", d.sequence); b.Append(','); Field(b, "agentId", d.agentId); b.Append(',');
            Field(b, "scenarioId", d.scenarioId); b.Append(','); Field(b, "scenarioStep", d.scenarioStep); b.Append(',');
            b.Append("\"availableActions\":").Append(StringArray(d.availableActions)).Append(',');
            Field(b, "actionId", d.actionId); b.Append(','); Field(b, "targetId", d.targetId); b.Append(',');
            Field(b, "result", d.result); b.Append(','); Field(b, "rejectionReason", d.rejectionReason); b.Append(',');
            Field(b, "rewardDelta", d.rewardDelta); return b.Append('}').ToString();
        }

        private static string SerializeSnapshot(ArenaSnapshot s)
        {
            var b = new StringBuilder(1024).Append('{');
            Field(b, "version", s.version); b.Append(','); Field(b, "sessionId", s.sessionId); b.Append(',');
            Field(b, "arenaId", s.arenaId); b.Append(','); Field(b, "episodeId", s.episodeId); b.Append(',');
            Field(b, "sequence", s.sequence); b.Append(','); Field(b, "width", s.width); b.Append(','); Field(b, "height", s.height); b.Append(',');
            Field(b, "scenarioId", s.scenarioId); b.Append(','); Field(b, "scenarioStep", s.scenarioStep); b.Append(',');
            Field(b, "isComplete", s.isComplete); b.Append(','); Field(b, "status", s.status); b.Append(',');
            b.Append("\"cellLayers\":").Append(StringArray(s.cellLayers)).Append(',');
            b.Append("\"heights\":").Append(FloatArray(s.heights)).Append(',');
            b.Append("\"surfaces\":").Append(StringArray(s.surfaces)).Append(',');
            b.Append("\"staticObjects\":").Append(StringArray(s.staticObjects)).Append(',');
            b.Append("\"projection\":").Append(FloatArray(s.projection));
            return b.Append('}').ToString();
        }

        private static string SerializeDictionary(Dictionary<string, object> value)
        {
            var b = new StringBuilder().Append('{'); bool first = true;
            foreach (var pair in value)
            {
                if (!first) b.Append(','); first = false;
                b.Append(Quote(pair.Key)).Append(':').Append(SerializePrimitive(pair.Value));
            }
            return b.Append('}').ToString();
        }

        private static string SerializePrimitive(object value)
        {
            if (value == null) return "null";
            if (value is string s) return Quote(s);
            if (value is bool flag) return flag ? "true" : "false";
            if (value is Dictionary<string, object> d) return SerializeDictionary(d);
            if (value is List<object> list)
            {
                var b = new StringBuilder().Append('[');
                for (int i = 0; i < list.Count; i++) { if (i > 0) b.Append(','); b.Append(SerializePrimitive(list[i])); }
                return b.Append(']').ToString();
            }
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static void Field(StringBuilder b, string name, string value) => b.Append(Quote(name)).Append(':').Append(Quote(value));
        private static void Field(StringBuilder b, string name, bool value) => b.Append(Quote(name)).Append(':').Append(value ? "true" : "false");
        private static void Field(StringBuilder b, string name, int value) => b.Append(Quote(name)).Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
        private static void Field(StringBuilder b, string name, long value) => b.Append(Quote(name)).Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
        private static void Field(StringBuilder b, string name, float value) => b.Append(Quote(name)).Append(':').Append(value.ToString("R", CultureInfo.InvariantCulture));
        private static string StringArray(string[] values)
        {
            if (values == null) values = Array.Empty<string>(); var b = new StringBuilder().Append('[');
            for (int i = 0; i < values.Length; i++) { if (i > 0) b.Append(','); b.Append(Quote(values[i])); }
            return b.Append(']').ToString();
        }
        private static string FloatArray(float[] values)
        {
            if (values == null) values = Array.Empty<float>(); var b = new StringBuilder().Append('[');
            for (int i = 0; i < values.Length; i++) { if (i > 0) b.Append(','); b.Append(values[i].ToString("R", CultureInfo.InvariantCulture)); }
            return b.Append(']').ToString();
        }
        private static string Quote(string value)
        {
            if (value == null) return "null";
            var b = new StringBuilder(value.Length + 8).Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': b.Append("\\\""); break; case '\\': b.Append("\\\\"); break;
                    case '\b': b.Append("\\b"); break; case '\f': b.Append("\\f"); break;
                    case '\n': b.Append("\\n"); break; case '\r': b.Append("\\r"); break; case '\t': b.Append("\\t"); break;
                    default: if (c < 32) b.Append("\\u").Append(((int)c).ToString("x4")); else b.Append(c); break;
                }
            }
            return b.Append('"').ToString();
        }

        private static string String(Dictionary<string, object> d, string k) => d.TryGetValue(k, out object v) ? v as string : null;
        private static int Int(Dictionary<string, object> d, string k, int fallback)
            => d.TryGetValue(k, out object v) && TryLong(v, out long n) && n >= int.MinValue && n <= int.MaxValue ? (int)n : fallback;
        private static long Long(Dictionary<string, object> d, string k, long fallback)
            => d.TryGetValue(k, out object v) && TryLong(v, out long n) ? n : fallback;
        private static bool Bool(Dictionary<string, object> d, string k, bool fallback)
            => d.TryGetValue(k, out object v) && v is bool b ? b : fallback;
        private static bool TryLong(object value, out long result)
        {
            if (value is long l) { result = l; return true; }
            if (value is double f && f >= long.MinValue && f <= long.MaxValue) { result = (long)f; return true; }
            return long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private sealed class JsonParser
        {
            private readonly string _s; private int _i;
            public JsonParser(string s) { _s = s ?? string.Empty; }
            public object ParseValue()
            {
                Space(); if (_i >= _s.Length) throw new FormatException("Unexpected end of JSON.");
                char c = _s[_i];
                if (c == '{') return Object(); if (c == '[') return Array(); if (c == '"') return Text();
                if (c == 't') { Token("true"); return true; } if (c == 'f') { Token("false"); return false; }
                if (c == 'n') { Token("null"); return null; } return Number();
            }
            private Dictionary<string, object> Object()
            {
                var d = new Dictionary<string, object>(StringComparer.Ordinal); _i++; Space();
                if (Take('}')) return d;
                while (true)
                {
                    Space(); if (_i >= _s.Length || _s[_i] != '"') throw new FormatException("Expected object key.");
                    string key = Text(); Space(); Need(':'); d[key] = ParseValue(); Space();
                    if (Take('}')) return d; Need(',');
                }
            }
            private List<object> Array()
            {
                var a = new List<object>(); _i++; Space(); if (Take(']')) return a;
                while (true) { a.Add(ParseValue()); Space(); if (Take(']')) return a; Need(','); }
            }
            private string Text()
            {
                Need('"'); var b = new StringBuilder();
                while (_i < _s.Length)
                {
                    char c = _s[_i++]; if (c == '"') return b.ToString();
                    if (c != '\\') { b.Append(c); continue; }
                    if (_i >= _s.Length) throw new FormatException("Invalid JSON escape."); char e = _s[_i++];
                    switch (e)
                    {
                        case '"': b.Append('"'); break; case '\\': b.Append('\\'); break; case '/': b.Append('/'); break;
                        case 'b': b.Append('\b'); break; case 'f': b.Append('\f'); break; case 'n': b.Append('\n'); break;
                        case 'r': b.Append('\r'); break; case 't': b.Append('\t'); break;
                        case 'u': if (_i + 4 > _s.Length) throw new FormatException("Invalid unicode escape.");
                            b.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture)); _i += 4; break;
                        default: throw new FormatException("Invalid JSON escape.");
                    }
                }
                throw new FormatException("Unterminated JSON string.");
            }
            private object Number()
            {
                int start = _i; if (_i < _s.Length && _s[_i] == '-') _i++;
                while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                bool floating = false;
                if (_i < _s.Length && _s[_i] == '.') { floating = true; _i++; while (_i < _s.Length && char.IsDigit(_s[_i])) _i++; }
                if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E')) { floating = true; _i++; if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++; while (_i < _s.Length && char.IsDigit(_s[_i])) _i++; }
                string token = _s.Substring(start, _i - start); if (!floating && long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l)) return l;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)) return d;
                throw new FormatException("Invalid JSON number.");
            }
            private void Token(string token) { if (_i + token.Length > _s.Length || string.CompareOrdinal(_s, _i, token, 0, token.Length) != 0) throw new FormatException("Invalid JSON token."); _i += token.Length; }
            private void Space() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }
            private bool Take(char c) { Space(); if (_i < _s.Length && _s[_i] == c) { _i++; return true; } return false; }
            private void Need(char c) { if (!Take(c)) throw new FormatException("Expected '" + c + "'."); }
        }
    }
}
