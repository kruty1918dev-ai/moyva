using System.Collections.Generic;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Validation;

namespace Kruty1918.Telemetry.Serialization
{
    /// <summary>
    /// A sealed, immutable batch of telemetry events — the atomic unit of spool,
    /// upload, idempotency and lineage. One file/object per batch, never per event.
    /// </summary>
    public sealed class TelemetryBatch
    {
        public const int FormatVersion = 1;
        public const int ProtocolVersion = 1;
        public const string Magic = "TMB1";

        public string BatchId;
        public string InstallationId;
        public string PlayerId;
        public string ApplicationSessionId;
        public string GameplaySessionId;
        public string MatchId;
        public string DatasetFingerprint;
        public int EventCount;
        public long FirstSequence = -1;
        public long LastSequence = -1;
        /// <summary>UTC ticks of seal time (wall clock, for ordering only — not fingerprinted).</summary>
        public long CreatedUtcTicks;
        /// <summary>Milliseconds since application start (logical/monotonic-ish clock).</summary>
        public long AppUptimeMs;
        public string CompressionId = "none";
        public int UncompressedBytes;
        /// <summary>SHA-256 hex (lowercase) of the UNCOMPRESSED payload.</summary>
        public string PayloadSha256;
        public TelemetryPriority Priority = TelemetryPriority.Normal;
        public DataQualityReport Quality;
        /// <summary>Contract (id,version) pairs referenced by events in this batch.</summary>
        public readonly List<KeyValuePair<string, int>> Contracts = new List<KeyValuePair<string, int>>();
        /// <summary>Lineage: producer package/version, client runtime id.</summary>
        public readonly Dictionary<string, string> Lineage = new Dictionary<string, string>(System.StringComparer.Ordinal);

        /// <summary>Raw payload bytes (compressed iff CompressionId != "none").</summary>
        public byte[] Payload;

        /// <summary>Uncompressed JSONL payload bytes.</summary>
        public byte[] RawPayload(Compression.ICompressionProvider compression)
            => CompressionId == "none" ? Payload : compression.Decompress(Payload, 64 * 1024 * 1024);

        public string HeaderJson()
        {
            var w = new TelemetryJsonWriter();
            w.BeginObject();
            w.Field("fmt", Magic);
            w.Field("fmtVer", FormatVersion);
            w.Field("proto", ProtocolVersion);
            w.Field("batchId", BatchId);
            w.Field("installationId", InstallationId);
            if (PlayerId != null) w.Field("playerId", PlayerId);
            w.Field("appSessionId", ApplicationSessionId);
            if (GameplaySessionId != null) w.Field("gameplaySessionId", GameplaySessionId);
            if (MatchId != null) w.Field("matchId", MatchId);
            w.Field("fingerprint", DatasetFingerprint);
            w.Field("events", EventCount);
            w.Field("firstSeq", FirstSequence);
            w.Field("lastSeq", LastSequence);
            w.Field("createdUtcTicks", CreatedUtcTicks);
            w.Field("appUptimeMs", AppUptimeMs);
            w.Field("compression", CompressionId);
            w.Field("uncompressedBytes", UncompressedBytes);
            w.Field("payloadBytes", Payload?.Length ?? 0);
            w.Field("payloadSha256", PayloadSha256);
            w.Field("priority", (int)Priority);
            w.Key("contracts").BeginArray();
            foreach (var c in Contracts)
            {
                w.BeginObject().Field("id", c.Key).Field("ver", c.Value).EndObject();
            }
            w.EndArray();
            if (Quality != null)
            {
                w.Key("quality").BeginObject();
                w.Field("score", Quality.Score);
                w.Field("decision", Quality.Decision.ToString());
                w.Field("valid", Quality.ValidEvents);
                w.Field("invalid", Quality.InvalidEvents);
                w.Field("violations", Quality.Violations.Count);
                w.Field("validator", DataQualityReport.ValidatorVersion);
                w.EndObject();
            }
            w.Key("lineage").BeginObject();
            foreach (var kv in Lineage) w.Field(kv.Key, kv.Value);
            w.EndObject();
            w.EndObject();
            return w.ToString();
        }

        public static TelemetryBatch FromHeader(JsonValue h)
        {
            var b = new TelemetryBatch
            {
                BatchId = h.GetString("batchId"),
                InstallationId = h.GetString("installationId"),
                PlayerId = h.GetString("playerId"),
                ApplicationSessionId = h.GetString("appSessionId"),
                GameplaySessionId = h.GetString("gameplaySessionId"),
                MatchId = h.GetString("matchId"),
                DatasetFingerprint = h.GetString("fingerprint"),
                EventCount = (int)h.GetInt("events"),
                FirstSequence = h.GetInt("firstSeq", -1),
                LastSequence = h.GetInt("lastSeq", -1),
                CreatedUtcTicks = h.GetInt("createdUtcTicks"),
                AppUptimeMs = h.GetInt("appUptimeMs"),
                CompressionId = h.GetString("compression") ?? "none",
                UncompressedBytes = (int)h.GetInt("uncompressedBytes"),
                PayloadSha256 = h.GetString("payloadSha256"),
                Priority = (TelemetryPriority)h.GetInt("priority", (int)TelemetryPriority.Normal),
            };
            if (h.TryGet("contracts", out var arr) && arr.IsArray)
                foreach (var c in arr.Arr)
                    b.Contracts.Add(new KeyValuePair<string, int>(c.GetString("id"), (int)c.GetInt("ver")));
            if (h.TryGet("lineage", out var lin) && lin.IsObject)
                foreach (var kv in lin.Obj)
                    if (kv.Value.Raw is string s) b.Lineage[kv.Key] = s;
            return b;
        }
    }
}
