using System.Collections.Generic;
using System.Threading;

namespace Kruty1918.Telemetry.Diagnostics
{
    public enum DropReason
    {
        StoragePressure = 0,
        Sampling = 1,
        Consent = 2,
        Invalid = 3,
        Corruption = 4,
        Expired = 5,
        Shutdown = 6,
    }

    /// <summary>Minimal logger seam — NullLogger default; Unity adapter maps to Debug.unityLogger.</summary>
    public interface ITelemetryLogger
    {
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg);
    }

    public sealed class NullTelemetryLogger : ITelemetryLogger
    {
        public void Info(string msg) { }
        public void Warn(string msg) { }
        public void Error(string msg) { }
    }

    /// <summary>
    /// Thread-safe counters for observability into the pipeline itself.
    /// Exposed to the editor dashboard and optionally emitted as telemetry.
    /// </summary>
    public sealed class TelemetryDiagnostics
    {
        public long EventsTracked;
        public long EventsRejected;
        public long EventsDropped;
        public long BatchesSealed;
        public long BatchesStored;
        public long BatchesUploaded;
        public long BatchesQuarantined;
        public long UploadAttempts;
        public long UploadRetries;
        public long BytesStored;
        public long BytesUploaded;
        public string LastError;
        public long LastUploadUtcTicks;

        private readonly Dictionary<DropReason, long> _drops =
            new Dictionary<DropReason, long>();
        private readonly object _gate = new object();

        public void RecordDrop(DropReason reason, long count = 1)
        {
            lock (_gate)
            {
                _drops.TryGetValue(reason, out long cur);
                _drops[reason] = cur + count;
            }
            Interlocked.Add(ref EventsDropped, count);
        }

        public Dictionary<DropReason, long> DropCounts()
        {
            lock (_gate) return new Dictionary<DropReason, long>(_drops);
        }

        public void NoteError(string err) => LastError = err;
    }
}
