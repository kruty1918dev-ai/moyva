using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Telemetry.Diagnostics;
using Kruty1918.Telemetry.Storage;

namespace Kruty1918.Telemetry.Upload
{
    /// <summary>
    /// Offline-aware upload queue. Drains pending batches through the transport with:
    ///  - bounded concurrency (default 1 — batches are ordered evidence)
    ///  - exponential backoff + jitter via <see cref="RetryPolicy"/>
    ///  - ACK verification before local delete (Matches: id+checksum+fingerprint+count)
    ///  - idempotent resend safety: duplicate uploads are ACKed as "duplicate"
    ///  - permanent failure → quarantine; malformed ACK → treated as retryable
    /// Driven by <see cref="TickAsync"/> (cooperative) — callers own scheduling.
    /// </summary>
    public sealed class TelemetryUploadCoordinator
    {
        private readonly ILocalTelemetryStore _store;
        private readonly ITelemetryTransport _transport;
        private readonly TelemetryDiagnostics _diag;
        private readonly ITelemetryLogger _log;
        private readonly RetryPolicy _retry;
        private readonly INetworkStatus _network;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private DateTime _nextAttemptUtc = DateTime.MinValue;
        private int _consecutiveFailures;

        /// <summary>Raised after a batch is durably ACKed and deleted locally.</summary>
        public event Action<UploadAck> BatchAcknowledged;
        public event Action<string, string> BatchQuarantined; // batchId, reason

        public Func<string> Endpoint;
        public Func<string> ProjectId;
        public Func<string> EnvironmentName;
        public Func<string> AuthToken;
        /// <summary>Clock seam for tests; defaults to DateTime.UtcNow.</summary>
        public Func<DateTime> UtcNow = () => DateTime.UtcNow;

        public TelemetryUploadCoordinator(
            ILocalTelemetryStore store,
            ITelemetryTransport transport,
            TelemetryDiagnostics diag = null,
            ITelemetryLogger log = null,
            RetryPolicy retry = null,
            INetworkStatus network = null)
        {
            _store = store;
            _transport = transport;
            _diag = diag ?? new TelemetryDiagnostics();
            _log = log ?? new NullTelemetryLogger();
            _retry = retry ?? new RetryPolicy();
            _network = network ?? new AlwaysOnlineStatus();
        }

        /// <summary>Run one drain cycle. Returns batches ACKed this cycle.</summary>
        public async Task<int> TickAsync(CancellationToken ct = default)
        {
            if (!await _gate.WaitAsync(0, ct).ConfigureAwait(false)) return 0;
            try
            {
                var endpoint = Endpoint?.Invoke();
                if (string.IsNullOrEmpty(endpoint) || !_network.IsOnline)
                    return 0; // local-only mode is valid: batches stay pending
                if (UtcNow() < _nextAttemptUtc) return 0;

                int acked = 0;
                var pending = _store.List(BatchLocation.Pending);
                foreach (var info in pending)
                {
                    if (ct.IsCancellationRequested) break;
                    if (UtcNow() < _nextAttemptUtc) break;
                    var outcome = await UploadOne(info, endpoint, ct).ConfigureAwait(false);
                    if (outcome == UploadOutcome.Acknowledged) { acked++; _consecutiveFailures = 0; }
                    else if (outcome == UploadOutcome.Permanent) { _consecutiveFailures = 0; }
                    else
                    {
                        _consecutiveFailures++;
                        _nextAttemptUtc = UtcNow() + _retry.DelayFor(_consecutiveFailures);
                        _diag.UploadRetries++;
                        break; // back off the whole queue on transport-level failure
                    }
                }
                return acked;
            }
            finally { _gate.Release(); }
        }

        private async Task<UploadOutcome> UploadOne(StoredBatchInfo info, string endpoint, CancellationToken ct)
        {
            Serialization.TelemetryBatch batch;
            try { batch = _store.Load(info); }
            catch (Exception e)
            {
                _store.Fail(info.BatchId, retryable: false, "unreadable: " + e.Message);
                return UploadOutcome.Permanent;
            }

            _store.MarkInFlight(batch.BatchId);
            _diag.UploadAttempts++;

            UploadResult result;
            try
            {
                result = await _transport.UploadAsync(new TelemetryUploadRequest
                {
                    Endpoint = endpoint,
                    ProjectId = ProjectId?.Invoke(),
                    Environment = EnvironmentName?.Invoke(),
                    AuthToken = AuthToken?.Invoke(),
                    Batch = batch,
                }, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { _store.Fail(batch.BatchId, true, "cancelled"); throw; }
            catch (Exception e)
            {
                _store.Fail(batch.BatchId, true, "transport: " + e.Message);
                _diag.NoteError(e.Message);
                return UploadOutcome.Retryable;
            }

            switch (result.Outcome)
            {
                case UploadOutcome.Acknowledged:
                    var ack = result.Ack;
                    if (ack == null || !ack.IsValid || ack.BatchId != batch.BatchId)
                    {
                        // Malformed ACK — never delete; server may have persisted.
                        _diag.NoteError("ack.invalid");
                        _store.Fail(batch.BatchId, true, "ack invalid");
                        return UploadOutcome.InvalidResponse;
                    }
                    if (ack.IsQuarantine)
                    {
                        _store.Fail(batch.BatchId, false, "server quarantine");
                        BatchQuarantined?.Invoke(batch.BatchId, "server");
                        return UploadOutcome.Permanent;
                    }
                    if (!ack.Matches(batch))
                    {
                        _diag.NoteError("ack.mismatch");
                        _store.Fail(batch.BatchId, true, "ack mismatch");
                        return UploadOutcome.InvalidResponse;
                    }
                    // Valid ACK — now, and only now, delete local copy.
                    _store.Complete(batch.BatchId);
                    _diag.BatchesUploaded++;
                    _diag.BytesUploaded += batch.Payload.Length;
                    _diag.LastUploadUtcTicks = UtcNow().Ticks;
                    BatchAcknowledged?.Invoke(ack);
                    return UploadOutcome.Acknowledged;

                case UploadOutcome.Permanent:
                    if (_retry.Exhausted(_store is LocalTelemetryStore lts ? lts.AttemptCount(batch.BatchId) : 1) || result.HttpStatus != 0)
                    {
                        _store.Fail(batch.BatchId, false, result.Error ?? "permanent");
                        BatchQuarantined?.Invoke(batch.BatchId, result.Error ?? "permanent");
                        return UploadOutcome.Permanent;
                    }
                    _store.Fail(batch.BatchId, true, result.Error);
                    return UploadOutcome.Retryable;

                case UploadOutcome.Retryable:
                case UploadOutcome.InvalidResponse:
                case UploadOutcome.Offline:
                default:
                {
                    int attempts = _store is LocalTelemetryStore l ? l.AttemptCount(batch.BatchId) : 1;
                    if (_retry.Exhausted(attempts))
                    {
                        _store.Fail(batch.BatchId, false, "attempts exhausted: " + result.Error);
                        BatchQuarantined?.Invoke(batch.BatchId, "attempts");
                        return UploadOutcome.Permanent;
                    }
                    _store.Fail(batch.BatchId, true, result.Error);
                    if (result.RetryAfterSeconds > 0)
                        _nextAttemptUtc = UtcNow() + TimeSpan.FromSeconds(result.RetryAfterSeconds);
                    return UploadOutcome.Retryable;
                }
            }
        }
    }
}
