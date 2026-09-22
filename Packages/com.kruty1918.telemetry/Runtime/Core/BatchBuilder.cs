using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Serialization;
using Kruty1918.Telemetry.Validation;

namespace Kruty1918.Telemetry.Core
{
    /// <summary>
    /// Accumulates serialized event lines in memory until a seal threshold
    /// (event count, byte size, explicit Flush). No disk I/O happens here —
    /// sealing produces an immutable <see cref="TelemetryBatch"/> handed to the store.
    /// </summary>
    public sealed class BatchBuilder
    {
        private readonly StringBuilder _payload = new StringBuilder(64 * 1024);
        private readonly List<ContractField> _contractFieldScratch = new List<ContractField>();
        private readonly Dictionary<string, int> _contracts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<JsonValue> _parsedEvents; // retained only when quality rules are enabled
        private readonly int _maxEvents;
        private readonly int _maxBytes;
        private readonly bool _retainParsed;
        private int _invalid;
        private long _firstSeq = -1, _lastSeq = -1;
        private TelemetryPriority _maxPriority = TelemetryPriority.Low;

        public int EventCount { get; private set; }
        public int InvalidCount => _invalid;
        public int ApproxBytes => _payload.Length * 2;
        public bool IsEmpty => EventCount == 0 && _invalid == 0;

        public BatchBuilder(int maxEvents = 512, int maxBytes = 512 * 1024, bool retainParsedForRules = true)
        {
            _maxEvents = maxEvents;
            _maxBytes = maxBytes;
            _retainParsed = retainParsedForRules;
            _parsedEvents = retainParsedForRules ? new List<JsonValue>(64) : null;
        }

        public bool ShouldSeal => EventCount >= _maxEvents || ApproxBytes >= _maxBytes;

        /// <summary>Append one serialized event line (JSON object, no newline).</summary>
        public void AddEvent(string line, long sequence, EventContract contract)
        {
            _payload.Append(line).Append('\n');
            EventCount++;
            if (_firstSeq < 0) _firstSeq = sequence;
            _lastSeq = sequence;
            if (contract != null)
            {
                _contracts[contract.ContractId] = contract.Version;
                if (contract.Priority < _maxPriority) _maxPriority = contract.Priority;
            }
            if (_retainParsed)
            {
                try { _parsedEvents.Add(TelemetryJsonReader.Parse(line)); }
                catch { /* serialized by us; ignore */ }
            }
        }

        public void AddInvalid() => _invalid++;

        /// <summary>Seal into an immutable batch. Quality rules run over retained parsed events.</summary>
        public TelemetryBatch Seal(TelemetrySessionContext session, string fingerprint,
            Compression.ICompressionProvider compression, IEnumerable<IBatchRule> rules,
            string producerId, string producerVersion)
        {
            var report = new DataQualityReport
            {
                TotalEvents = EventCount + _invalid,
                ValidEvents = EventCount,
                InvalidEvents = _invalid,
            };
            if (rules != null && _parsedEvents != null)
                foreach (var r in rules)
                    r.Evaluate(_parsedEvents, report);

            byte[] raw = Encoding.UTF8.GetBytes(_payload.ToString());
            var batch = new TelemetryBatch
            {
                BatchId = TelemetrySessionContext.NewId(),
                InstallationId = session.InstallationId,
                PlayerId = session.PlayerId,
                ApplicationSessionId = session.ApplicationSessionId,
                GameplaySessionId = session.GameplaySessionId,
                MatchId = session.MatchId,
                DatasetFingerprint = fingerprint,
                EventCount = EventCount,
                FirstSequence = _firstSeq,
                LastSequence = _lastSeq,
                CreatedUtcTicks = DateTime.UtcNow.Ticks,
                Quality = report,
                UncompressedBytes = raw.Length,
                PayloadSha256 = BatchFileCodec.Sha256Hex(raw),
            };
            foreach (var kv in _contracts) batch.Contracts.Add(kv);
            batch.Lineage["producer"] = producerId ?? "unknown";
            batch.Lineage["producerVersion"] = producerVersion ?? "0";
            batch.Lineage["format"] = TelemetryBatch.Magic + "/" + TelemetryBatch.FormatVersion;
            batch.Priority = _maxPriority;
            if (compression != null && compression.Id != "none")
            {
                batch.Payload = compression.Compress(raw);
                batch.CompressionId = compression.Id;
            }
            else
            {
                batch.Payload = raw;
            }
            return batch;
        }

        public void Reset()
        {
            _payload.Length = 0;
            _contracts.Clear();
            _parsedEvents?.Clear();
            EventCount = 0;
            _invalid = 0;
            _firstSeq = _lastSeq = -1;
            _maxPriority = TelemetryPriority.Low;
        }
    }
}
