using System.Collections.Generic;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Storage
{
    public enum BatchLocation { Pending = 0, InFlight = 1, Quarantine = 2 }

    /// <summary>Metadata about a stored batch file without loading the payload.</summary>
    public sealed class StoredBatchInfo
    {
        public string BatchId;
        public string Path;
        public BatchLocation Location;
        public long FileBytes;
        public long CreatedUtcTicks;
        public int Priority;
        public int Attempts;
    }

    /// <summary>
    /// Crash-safe append-only local spool. Contract:
    ///  - Save() writes via tmp+rename (commit on rename).
    ///  - Recover() at open: inflight→pending, stale tmp deleted, corrupt→quarantine.
    ///  - MarkInFlight/Complete/Fail model resumable upload state on disk.
    ///  - Bounded size with deterministic priority-based eviction + drop accounting.
    /// </summary>
    public interface ILocalTelemetryStore
    {
        string RootPath { get; }
        /// <summary>Scan directories, recover from crash state, verify checksums.</summary>
        int Recover();
        void Save(TelemetryBatch batch);
        IReadOnlyList<StoredBatchInfo> List(BatchLocation location);
        TelemetryBatch Load(StoredBatchInfo info);
        void MarkInFlight(string batchId);
        /// <summary>ACK-verified: permanently remove the batch file.</summary>
        void Complete(string batchId);
        /// <summary>retryable → back to pending; permanent → quarantine.</summary>
        void Fail(string batchId, bool retryable, string reason);
        long TotalBytes { get; }
        /// <summary>Evict lowest-priority batches until under limit. Critical never evicted.</summary>
        int EnforceLimits();
        /// <summary>Consent purge: delete everything immediately.</summary>
        void Purge();
    }
}
