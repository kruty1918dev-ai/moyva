using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Telemetry.Compression;
using Kruty1918.Telemetry.Serialization;
using Kruty1918.Telemetry.Upload;

namespace Kruty1918.Telemetry.Harness
{
    /// <summary>
    /// Reference implementation of the ingestion endpoint contract, in-process.
    /// Mirrors the Node/Worker backend semantics exactly so client tests validate
    /// the same protocol the real server implements:
    ///
    ///   envelope headers → verify checksum → resolve dataset group transactionally
    ///   → check BatchId idempotency → persist immutable raw object → register batch
    ///   → structured ACK.
    /// </summary>
    public sealed class MockIngestionServer
    {
        public sealed class StoredObject
        {
            public string Key;
            public byte[] Bytes;
        }

        public sealed class BatchRecord
        {
            public string BatchId;
            public string Checksum;
            public string Fingerprint;
            public string GroupId;
            public string ObjectRef;
            public int AcceptedEvents;
            public string Status;
            public string AckId;
        }

        public sealed class DatasetGroup
        {
            public string GroupId;
            public string ProjectId;
            public string Environment;
            public string Fingerprint;
            public int BatchCount;
            public long EventCount;
            public long FirstSeenUtcTicks;
        }

        private readonly object _gate = new object();
        private readonly Dictionary<string, BatchRecord> _batches = new Dictionary<string, BatchRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, DatasetGroup> _groups = new Dictionary<string, DatasetGroup>(StringComparer.Ordinal);
        private readonly Dictionary<string, StoredObject> _objects = new Dictionary<string, StoredObject>(StringComparer.Ordinal);
        private readonly string _rawDir;
        public int RequestCount;
        public int DecompressCalls;

        public const int MaxBodyBytes = 16 * 1024 * 1024;

        public MockIngestionServer(string rawDir = null) { _rawDir = rawDir; }

        public IReadOnlyDictionary<string, BatchRecord> Batches => _batches;
        public IReadOnlyDictionary<string, DatasetGroup> Groups => _groups;
        public IReadOnlyDictionary<string, StoredObject> Objects => _objects;

        /// <summary>Node-compatible handler used by LoopbackTransport.</summary>
        public (int status, string body) Ingest(Dictionary<string, string> headers, byte[] body)
        {
            lock (_gate)
            {
                RequestCount++;
                try { return IngestLocked(headers, body); }
                catch (FormatException fe) { return (400, Error("bad_request", fe.Message)); }
                catch (InvalidDataException de) { return (422, Error("integrity", de.Message)); }
            }
        }

        private (int, string) IngestLocked(Dictionary<string, string> h, byte[] body)
        {
            if (body == null || body.Length > MaxBodyBytes) return (413, Error("too_large", "body"));
            if (!h.TryGetValue("X-Telemetry-Protocol", out var proto) || proto != "1")
                return (400, Error("protocol", "unsupported"));
            string project = H(h, "X-Telemetry-Project") ?? "dev";
            string env = H(h, "X-Telemetry-Environment") ?? "dev";
            string batchId = Req(h, "X-Telemetry-Batch-Id");
            string fp = Req(h, "X-Telemetry-Fingerprint");
            string checksum = Req(h, "X-Telemetry-Payload-Sha256");
            int eventCount = int.Parse(Req(h, "X-Telemetry-Event-Count"));
            string compression = H(h, "X-Telemetry-Compression") ?? "none";
            string session = H(h, "X-Telemetry-App-Session-Id") ?? "unknown";

            // Verify payload: decompress with cap, checksum over uncompressed bytes.
            byte[] raw = body;
            if (compression == "gzip")
            {
                DecompressCalls++;
                raw = new GZipCompressionProvider().Decompress(body, MaxBodyBytes * 4);
            }
            else if (compression != "none") return (400, Error("compression", "unsupported"));
            if (BatchFileCodec.Sha256Hex(raw) != checksum)
                return (422, Error("checksum", "payload mismatch"));

            // Idempotency: same BatchId → same checksum must yield the original ACK;
            // different checksum → conflict, never silently accepted.
            if (_batches.TryGetValue(batchId, out var existing))
            {
                if (existing.Checksum != checksum)
                    return (409, Error("checksum_conflict", "batchId reused with different payload"));
                return (200, AckJson(existing, "duplicate"));
            }

            // Transactional group resolution: one group per (project, env, fingerprint).
            string groupKey = project + "/" + env + "/" + fp;
            if (!_groups.TryGetValue(groupKey, out var group))
            {
                group = new DatasetGroup
                {
                    GroupId = "grp-" + Guid.NewGuid().ToString("N"),
                    ProjectId = project, Environment = env, Fingerprint = fp,
                    FirstSeenUtcTicks = DateTime.UtcNow.Ticks,
                };
                _groups[groupKey] = group;
            }

            // Deterministic immutable object key.
            var day = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string key = $"raw/{project}/{env}/{fp}/date={day}/session={session}/batch={batchId}.jsonl"
                         + (compression == "gzip" ? ".gz" : "");
            if (_objects.ContainsKey(key))
                return (409, Error("object_conflict", "key exists"));
            _objects[key] = new StoredObject { Key = key, Bytes = body };
            if (_rawDir != null)
            {
                var path = Path.Combine(_rawDir, key.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, body);
            }

            var rec = new BatchRecord
            {
                BatchId = batchId, Checksum = checksum, Fingerprint = fp,
                GroupId = group.GroupId, ObjectRef = key, AcceptedEvents = eventCount,
                Status = "accepted", AckId = "ack-" + Guid.NewGuid().ToString("N"),
            };
            _batches[batchId] = rec;
            group.BatchCount++;
            group.EventCount += eventCount;
            return (200, AckJson(rec, "accepted"));
        }

        private static string AckJson(BatchRecord rec, string status)
        {
            var w = new TelemetryJsonWriter();
            w.BeginObject();
            w.Field("protocol", 1);
            w.Field("ackId", rec.AckId);
            w.Field("batchId", rec.BatchId);
            w.Field("checksum", rec.Checksum);
            w.Field("fingerprint", rec.Fingerprint);
            w.Field("acceptedEvents", rec.AcceptedEvents);
            w.Field("groupId", rec.GroupId);
            w.Field("objectRef", rec.ObjectRef);
            w.Field("status", status);
            w.Field("validatorVersion", "mock-1");
            w.EndObject();
            return w.ToString();
        }

        private static string Error(string code, string msg)
            => $"{{\"error\":\"{code}\",\"message\":\"{msg}\"}}";

        private static string H(Dictionary<string, string> h, string k)
            => h.TryGetValue(k, out var v) ? v : null;

        private static string Req(Dictionary<string, string> h, string k)
            => h.TryGetValue(k, out var v) && !string.IsNullOrEmpty(v) ? v
                : throw new FormatException("missing header " + k);
    }
}
