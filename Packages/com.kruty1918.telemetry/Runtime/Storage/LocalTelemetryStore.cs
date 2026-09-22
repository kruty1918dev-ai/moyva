using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Diagnostics;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Storage
{
    /// <summary>
    /// File-system spool:
    ///   {root}/pending/*.tmb   — sealed, awaiting upload
    ///   {root}/inflight/*.tmb  — claimed by uploader, not yet ACKed
    ///   {root}/quarantine/*.tmb — corrupt/permanent-failure evidence (retention applies)
    ///   {root}/attempts.json   — per-batch upload attempt counters (resumable)
    ///
    /// Commit model: write {id}.tmp, rename to {id}.tmb. On Recover(): tmp files are
    /// deleted (never committed), inflight returns to pending, checksum-invalid files
    /// move to quarantine. Concurrent access is guarded by a process-local gate;
    /// cross-process safety relies on unique batch ids + atomic renames.
    /// </summary>
    public sealed class LocalTelemetryStore : ILocalTelemetryStore
    {
        public const string Extension = ".tmb";

        public sealed class Policy
        {
            public long MaxTotalBytes = 32 * 1024 * 1024;
            public int MaxPendingBatches = 512;
            /// <summary>Quarantine retention; older files are deleted on Recover.</summary>
            public TimeSpan QuarantineRetention = TimeSpan.FromDays(7);
        }

        private readonly string _root;
        private readonly Policy _policy;
        private readonly TelemetryDiagnostics _diag;
        private readonly ITelemetryLogger _log;
        private readonly object _gate = new object();
        private readonly Dictionary<string, int> _attempts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly string _attemptsPath;

        public string RootPath => _root;

        public LocalTelemetryStore(string root, Policy policy = null,
            TelemetryDiagnostics diag = null, ITelemetryLogger log = null)
        {
            _root = root;
            _policy = policy ?? new Policy();
            _diag = diag ?? new TelemetryDiagnostics();
            _log = log ?? new NullTelemetryLogger();
            _attemptsPath = Path.Combine(_root, "attempts.json");
        }

        private string Dir(BatchLocation loc) => Path.Combine(_root,
            loc == BatchLocation.Pending ? "pending" : loc == BatchLocation.InFlight ? "inflight" : "quarantine");

        public int Recover()
        {
            lock (_gate)
            {
                Directory.CreateDirectory(Dir(BatchLocation.Pending));
                Directory.CreateDirectory(Dir(BatchLocation.InFlight));
                Directory.CreateDirectory(Dir(BatchLocation.Quarantine));
                LoadAttempts();
                int recovered = 0;
                // Crash mid-upload: inflight batches were never ACKed → back to pending.
                foreach (var f in Directory.GetFiles(Dir(BatchLocation.InFlight), "*" + Extension))
                {
                    string dest = Path.Combine(Dir(BatchLocation.Pending), Path.GetFileName(f));
                    try { if (File.Exists(dest)) File.Delete(dest); File.Move(f, dest); recovered++; }
                    catch (Exception e) { _log.Warn("recover inflight: " + e.Message); }
                }
                // Uncommitted tmp writes never became batches.
                foreach (var loc in new[] { BatchLocation.Pending, BatchLocation.InFlight, BatchLocation.Quarantine })
                    foreach (var f in Directory.GetFiles(Dir(loc), "*.tmp"))
                        try { File.Delete(f); } catch { }
                // Validate pending+inflight integrity; corrupt → quarantine.
                foreach (var loc in new[] { BatchLocation.Pending, BatchLocation.InFlight })
                    foreach (var f in Directory.GetFiles(Dir(loc), "*" + Extension))
                    {
                        try { BatchFileCodec.Decode(File.ReadAllBytes(f)); }
                        catch (Exception e)
                        {
                            _diag.RecordDrop(DropReason.Corruption);
                            _log.Warn("corrupt batch → quarantine: " + Path.GetFileName(f) + " " + e.Message);
                            MoveTo(f, BatchLocation.Quarantine);
                        }
                    }
                // Quarantine retention sweep.
                var cutoff = DateTime.UtcNow - _policy.QuarantineRetention;
                foreach (var f in Directory.GetFiles(Dir(BatchLocation.Quarantine), "*" + Extension))
                    if (File.GetLastWriteTimeUtc(f) < cutoff)
                        try { File.Delete(f); } catch { }
                return recovered;
            }
        }

        public void Save(TelemetryBatch batch)
        {
            if (batch == null || batch.Payload == null) return;
            lock (_gate)
            {
                var path = Path.Combine(Dir(BatchLocation.Pending), batch.BatchId + Extension);
                BatchFileCodec.WriteAtomic(path, BatchFileCodec.Encode(batch));
                _diag.BatchesStored++;
                _diag.BytesStored += batch.Payload.Length;
                EnforceLimitsLocked();
            }
        }

        public IReadOnlyList<StoredBatchInfo> List(BatchLocation location)
        {
            lock (_gate)
            {
                var list = new List<StoredBatchInfo>();
                if (!Directory.Exists(Dir(location))) return list;
                foreach (var f in Directory.GetFiles(Dir(location), "*" + Extension))
                {
                    var info = new StoredBatchInfo
                    {
                        BatchId = Path.GetFileNameWithoutExtension(f),
                        Path = f,
                        Location = location,
                        FileBytes = new FileInfo(f).Length,
                        CreatedUtcTicks = File.GetCreationTimeUtc(f).Ticks,
                    };
                    _attempts.TryGetValue(info.BatchId, out info.Attempts);
                    // Priority lives in the header; read lazily only when evicting.
                    info.Priority = -1;
                    list.Add(info);
                }
                // Oldest first within the location.
                list.Sort((a, b) => a.CreatedUtcTicks.CompareTo(b.CreatedUtcTicks));
                return list;
            }
        }

        public TelemetryBatch Load(StoredBatchInfo info)
        {
            lock (_gate)
                return BatchFileCodec.Decode(File.ReadAllBytes(info.Path));
        }

        public void MarkInFlight(string batchId)
        {
            lock (_gate)
            {
                var src = Path.Combine(Dir(BatchLocation.Pending), batchId + Extension);
                var dst = Path.Combine(Dir(BatchLocation.InFlight), batchId + Extension);
                if (!File.Exists(src)) return;
                if (File.Exists(dst)) File.Delete(dst);
                File.Move(src, dst);
                _attempts.TryGetValue(batchId, out int n);
                _attempts[batchId] = n + 1;
                SaveAttempts();
            }
        }

        public void Complete(string batchId)
        {
            lock (_gate)
            {
                DeleteIn(BatchLocation.InFlight, batchId);
                DeleteIn(BatchLocation.Pending, batchId); // paranoia: never leave a copy
                if (_attempts.Remove(batchId)) SaveAttempts();
            }
        }

        public void Fail(string batchId, bool retryable, string reason)
        {
            lock (_gate)
            {
                var inflight = Path.Combine(Dir(BatchLocation.InFlight), batchId + Extension);
                if (!File.Exists(inflight)) return;
                if (retryable)
                {
                    var dst = Path.Combine(Dir(BatchLocation.Pending), batchId + Extension);
                    if (File.Exists(dst)) File.Delete(dst);
                    File.Move(inflight, dst);
                }
                else
                {
                    _diag.BatchesQuarantined++;
                    _log.Warn("batch quarantined: " + batchId + " " + reason);
                    MoveTo(inflight, BatchLocation.Quarantine);
                }
            }
        }

        public long TotalBytes
        {
            get
            {
                lock (_gate)
                {
                    long total = 0;
                    foreach (BatchLocation loc in Enum.GetValues(typeof(BatchLocation)))
                        foreach (var f in Directory.GetFiles(Dir(loc), "*" + Extension))
                            total += new FileInfo(f).Length;
                    return total;
                }
            }
        }

        public int EnforceLimits()
        {
            lock (_gate) return EnforceLimitsLocked();
        }

        private int EnforceLimitsLocked()
        {
            int evicted = 0;
            while (true)
            {
                long total = TotalBytesUnlocked();
                var pending = ListUnlocked(BatchLocation.Pending);
                if (total <= _policy.MaxTotalBytes && pending.Count <= _policy.MaxPendingBatches)
                    break;
                // Choose eviction victim: lowest priority value (highest enum), then oldest.
                StoredBatchInfo victim = null;
                int victimPriority = -1;
                foreach (var p in pending)
                {
                    int pr = ReadPriority(p);
                    if (pr > victimPriority) { victimPriority = pr; victim = p; }
                }
                if (victim == null) break;
                if (victimPriority <= (int)TelemetryPriority.Critical)
                {
                    // Critical data is never silently dropped: stop and let the store fill.
                    _log.Warn("storage limit reached; only Critical batches remain — refusing to evict");
                    break;
                }
                _diag.RecordDrop(DropReason.StoragePressure);
                _diag.BatchesQuarantined++;
                MoveTo(victim.Path, BatchLocation.Quarantine);
                evicted++;
            }
            return evicted;
        }

        private int ReadPriority(StoredBatchInfo info)
        {
            if (info.Priority >= 0) return info.Priority;
            try
            {
                var bytes = File.ReadAllBytes(info.Path);
                // Read header only: magic + len + header.
                if (bytes.Length < 8) return (int)TelemetryPriority.Low;
                int headerLen = BitConverter.ToInt32(bytes, 4);
                var h = TelemetryJsonReader.Parse(System.Text.Encoding.UTF8.GetString(bytes, 8, Math.Min(headerLen, bytes.Length - 8)));
                info.Priority = (int)h.GetInt("priority", (int)TelemetryPriority.Normal);
            }
            catch { info.Priority = (int)TelemetryPriority.Low; }
            return info.Priority;
        }

        public void Purge()
        {
            lock (_gate)
            {
                foreach (BatchLocation loc in Enum.GetValues(typeof(BatchLocation)))
                    foreach (var f in Directory.GetFiles(Dir(loc), "*" + Extension))
                        try { File.Delete(f); } catch { }
                _attempts.Clear();
                SaveAttempts();
            }
        }

        public int AttemptCount(string batchId)
        {
            lock (_gate) return _attempts.TryGetValue(batchId, out var n) ? n : 0;
        }

        private long TotalBytesUnlocked()
        {
            long total = 0;
            foreach (BatchLocation loc in Enum.GetValues(typeof(BatchLocation)))
            {
                var d = Dir(loc);
                if (!Directory.Exists(d)) continue;
                foreach (var f in Directory.GetFiles(d, "*" + Extension))
                    total += new FileInfo(f).Length;
            }
            return total;
        }

        private List<StoredBatchInfo> ListUnlocked(BatchLocation location)
        {
            var list = new List<StoredBatchInfo>();
            var d = Dir(location);
            if (!Directory.Exists(d)) return list;
            foreach (var f in Directory.GetFiles(d, "*" + Extension))
                list.Add(new StoredBatchInfo
                {
                    BatchId = Path.GetFileNameWithoutExtension(f),
                    Path = f,
                    Location = location,
                    FileBytes = new FileInfo(f).Length,
                    CreatedUtcTicks = File.GetCreationTimeUtc(f).Ticks,
                    Priority = -1,
                });
            list.Sort((a, b) => a.CreatedUtcTicks.CompareTo(b.CreatedUtcTicks));
            return list;
        }

        private void DeleteIn(BatchLocation loc, string batchId)
        {
            var p = Path.Combine(Dir(loc), batchId + Extension);
            if (File.Exists(p)) File.Delete(p);
        }

        private void MoveTo(string path, BatchLocation dest)
        {
            var dst = Path.Combine(Dir(dest), Path.GetFileName(path));
            try { if (File.Exists(dst)) File.Delete(dst); File.Move(path, dst); }
            catch (Exception e) { _log.Warn("move " + path + ": " + e.Message); }
        }

        private void LoadAttempts()
        {
            _attempts.Clear();
            try
            {
                if (!File.Exists(_attemptsPath)) return;
                var json = TelemetryJsonReader.Parse(File.ReadAllText(_attemptsPath));
                if (json.IsObject)
                    foreach (var kv in json.Obj)
                        _attempts[kv.Key] = (int)kv.Value.Int;
            }
            catch { }
        }

        private void SaveAttempts()
        {
            try
            {
                var w = new TelemetryJsonWriter();
                w.BeginObject();
                foreach (var kv in _attempts) w.Field(kv.Key, kv.Value);
                w.EndObject();
                BatchFileCodec.WriteAtomic(_attemptsPath, System.Text.Encoding.UTF8.GetBytes(w.ToString()));
            }
            catch (Exception e) { _log.Warn("attempts save: " + e.Message); }
        }
    }
}
