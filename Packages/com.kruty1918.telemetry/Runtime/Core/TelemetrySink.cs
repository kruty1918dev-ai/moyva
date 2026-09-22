using System;
using System.Collections.Generic;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Diagnostics;
using Kruty1918.Telemetry.Privacy;
using Kruty1918.Telemetry.Serialization;
using Kruty1918.Telemetry.Storage;
using Kruty1918.Telemetry.Validation;

namespace Kruty1918.Telemetry.Core
{
    /// <summary>
    /// Default pipeline: contract resolve → sampling → validating serialization →
    /// in-memory batch buffer. No disk/network I/O on the caller thread; sealed
    /// batches are handed to <see cref="ILocalTelemetryStore"/> by the runtime
    /// on Flush()/Dispose() or when the builder hits a threshold.
    /// </summary>
    public sealed class TelemetrySink : ITelemetrySink
    {
        private readonly ContractRegistry _contracts;
        private readonly BatchBuilder _builder;
        private readonly ILocalTelemetryStore _store;
        private readonly TelemetryDiagnostics _diag;
        private readonly ITelemetryLogger _log;
        private readonly Func<TelemetryConfig> _config;
        private readonly Func<string> _fingerprint;
        private readonly Func<IEnumerable<IBatchRule>> _rules;
        private readonly Compression.ICompressionProvider _compression;
        private readonly object _gate = new object();
        private readonly Random _samplingRandom = new Random();

        public event Action<TelemetryBatch> BatchSealed;

        public TelemetrySessionContext Session { get; }

        public TelemetrySink(
            TelemetrySessionContext session,
            ContractRegistry contracts,
            ILocalTelemetryStore store,
            TelemetryDiagnostics diag,
            ITelemetryLogger log,
            Func<TelemetryConfig> config,
            Func<string> fingerprint,
            Func<IEnumerable<IBatchRule>> rules,
            Compression.ICompressionProvider compression)
        {
            Session = session;
            _contracts = contracts;
            _store = store;
            _diag = diag ?? new TelemetryDiagnostics();
            _log = log ?? new NullTelemetryLogger();
            _config = config;
            _fingerprint = fingerprint;
            _rules = rules ?? (() => null);
            _compression = compression ?? new Compression.NoCompressionProvider();
            _builder = new BatchBuilder(retainParsedForRules: true);
        }

        public TelemetryResult Track<T>(in T telemetryEvent) where T : struct, ITelemetryEvent
        {
            var e = telemetryEvent; // copy: 'in' params cannot be captured
            return TrackCore(e.EventType, e.ContractId, e.ContractVersion, w => e.WriteTo(w));
        }

        public TelemetryResult TrackRaw(string eventType, string contractId, int contractVersion,
            Action<ITelemetryEventWriter> writePayload)
            => TrackCore(eventType, contractId, contractVersion, writePayload);

        private TelemetryResult TrackCore(string eventType, string contractId, int contractVersion,
            Action<ITelemetryEventWriter> writePayload)
        {
            var cfg = _config?.Invoke() ?? new TelemetryConfig();
            if (!cfg.Enabled || cfg.Consent == TelemetryConsentLevel.Disabled)
                return TelemetryResult.Disabled();

            EventContract contract;
            lock (_gate)
            {
                if (!_contracts.TryResolve(contractId, contractVersion, out contract))
                {
                    _diag.EventsRejected++;
                    return TelemetryResult.Rejected("contract.missing:" + contractId + "@" + contractVersion);
                }
            }
            if (!contract.MatchesEventType(eventType))
            {
                _diag.EventsRejected++;
                return TelemetryResult.Rejected("contract.eventType-mismatch");
            }
            // Consent gate: research-tagged contracts need research consent.
            if (contract.Privacy == PrivacyClass.Research && cfg.Consent < TelemetryConsentLevel.Research)
            {
                _diag.RecordDrop(DropReason.Consent);
                return TelemetryResult.Dropped("consent");
            }
            // Sampling.
            if (contract.SamplingRate < 1f && _samplingRandom.NextDouble() >= contract.SamplingRate)
            {
                _diag.RecordDrop(DropReason.Sampling);
                return TelemetryResult.Dropped("sampling");
            }

            var violations = new List<ValidationViolation>();
            var json = new TelemetryJsonWriter();
            long seq = Session.NextSequence();
            json.BeginObject();
            json.Field("t", eventType);
            json.Field("c", contractId);
            json.Field("v", contractVersion);
            json.Field("seq", seq);
            json.Field("ts", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            json.Key("d").BeginObject();
            var writer = new ContractValidatingWriter(json, contract, violations);
            try { writePayload?.Invoke(writer); }
            catch (Exception e)
            {
                _diag.EventsRejected++;
                _diag.NoteError("write:" + e.GetType().Name);
                return TelemetryResult.Faulted("write:" + e.GetType().Name);
            }
            json.EndObject();
            json.EndObject();

            if (writer.MissingRequired(out string missing))
            {
                _diag.EventsRejected++;
                lock (_gate) _builder.AddInvalid();
                return TelemetryResult.Rejected("required:" + missing);
            }
            if (violations.Count > 0)
            {
                // Structural violations don't necessarily kill the event — but they
                // degrade it. Policy: reject events with any structural violation.
                _diag.EventsRejected++;
                lock (_gate) _builder.AddInvalid();
                return TelemetryResult.Rejected("field:" + violations[0].Scope + ":" + violations[0].Code);
            }

            lock (_gate)
            {
                _builder.AddEvent(json.ToString(), seq, contract);
                _diag.EventsTracked++;
                if (_builder.ShouldSeal) SealLocked(cfg);
            }
            return TelemetryResult.Ok;
        }

        public void Flush()
        {
            var cfg = _config?.Invoke() ?? new TelemetryConfig();
            lock (_gate) SealLocked(cfg);
        }

        /// <summary>Seal + store the current builder. Called under gate or via Flush.</summary>
        private void SealLocked(TelemetryConfig cfg)
        {
            if (_builder.IsEmpty) return;
            var batch = _builder.Seal(Session, _fingerprint?.Invoke(),
                cfg.Compression == "gzip" ? _compression : new Compression.NoCompressionProvider(),
                _rules(), cfg.ProducerId, cfg.ProducerVersion);
            _builder.Reset();
            _diag.BatchesSealed++;
            _store.Save(batch);
            BatchSealed?.Invoke(batch);
        }
    }
}
