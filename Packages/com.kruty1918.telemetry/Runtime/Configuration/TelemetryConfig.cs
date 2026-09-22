using System;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Configuration
{
    public enum TelemetryConsentLevel
    {
        /// <summary>Nothing is recorded. Sink returns Disabled.</summary>
        Disabled = 0,
        /// <summary>Gameplay/operational telemetry only (default privacy-safe set).</summary>
        Analytics = 1,
        /// <summary>Extended research/ML data (decision traces, richer state).</summary>
        Research = 2,
    }

    /// <summary>Local (baked) configuration — safe defaults work with zero setup.</summary>
    public sealed class TelemetryConfig
    {
        /// <summary>Root folder for the spool. Resolved by the host (persistentDataPath subdir).</summary>
        public string StorageRoot;
        public bool Enabled = true;
        public TelemetryConsentLevel Consent = TelemetryConsentLevel.Analytics;
        public int MaxEventsPerBatch = 512;
        public int MaxBatchBytes = 512 * 1024;
        public long MaxStorageBytes = 32 * 1024 * 1024;
        public int MaxPendingBatches = 512;
        /// <summary>Compression id: "none" | "gzip".</summary>
        public string Compression = "gzip";
        /// <summary>Producer identity recorded in batch lineage.</summary>
        public string ProducerId = "host-app";
        public string ProducerVersion = "0.0.0";
        /// <summary>Flush cadence hint for the host loop, seconds.</summary>
        public float FlushIntervalSeconds = 30f;
        /// <summary>Upload tick cadence hint, seconds.</summary>
        public float UploadIntervalSeconds = 60f;
        /// <summary>Product/project id reported to the backend.</summary>
        public string ProjectId = "dev";
        public string EnvironmentName = "dev";
    }

    /// <summary>
    /// Remote configuration envelope delivered by the backend / file override.
    /// Absent or expired config ⇒ local-only mode keeps spooling; when a config
    /// appears, pending batches become upload candidates automatically.
    /// </summary>
    public sealed class TelemetryRemoteConfig
    {
        public const int CurrentVersion = 1;

        public int ConfigVersion;
        public bool Enabled = true;
        public string Endpoint;
        public string ProjectId;
        public string EnvironmentName;
        public string ClientToken; // public, non-secret identifier
        public int[] SupportedProtocols = { TelemetryBatch.ProtocolVersion };
        public int MaxBatchBytes = 4 * 1024 * 1024;
        public int MaxEventsPerBatch = 4096;
        public string[] CompressionModes = { "gzip", "none" };
        /// <summary>UTC ISO-8601 expiry; after this the config is ignored.</summary>
        public string ExpiresUtc;
        /// <summary>Sampling overrides: contractId → rate 0..1.</summary>
        public readonly System.Collections.Generic.Dictionary<string, float> Sampling =
            new System.Collections.Generic.Dictionary<string, float>(StringComparer.Ordinal);

        public bool IsExpired(DateTime utcNow)
            => !string.IsNullOrEmpty(ExpiresUtc)
               && DateTime.TryParse(ExpiresUtc, null,
                   System.Globalization.DateTimeStyles.RoundtripKind, out var t)
               && utcNow > t;

        public bool SupportsProtocol(int v)
        {
            foreach (var p in SupportedProtocols) if (p == v) return true;
            return false;
        }

        public static TelemetryRemoteConfig Parse(string json)
        {
            var v = Serialization.TelemetryJsonReader.Parse(json);
            var c = new TelemetryRemoteConfig
            {
                ConfigVersion = (int)v.GetInt("configVersion"),
                Enabled = v.GetBool("enabled", true),
                Endpoint = v.GetString("endpoint"),
                ProjectId = v.GetString("projectId"),
                EnvironmentName = v.GetString("environment"),
                ClientToken = v.GetString("clientToken"),
                MaxBatchBytes = (int)v.GetInt("maxBatchBytes", 4 * 1024 * 1024),
                MaxEventsPerBatch = (int)v.GetInt("maxEventsPerBatch", 4096),
                ExpiresUtc = v.GetString("expiresUtc"),
            };
            if (v.TryGet("supportedProtocols", out var sp) && sp.IsArray)
            {
                var list = new System.Collections.Generic.List<int>();
                foreach (var p in sp.Arr) list.Add((int)p.Int);
                c.SupportedProtocols = list.ToArray();
            }
            if (v.TryGet("compressionModes", out var cm) && cm.IsArray)
            {
                var list = new System.Collections.Generic.List<string>();
                foreach (var m in cm.Arr) if (m.Raw is string s) list.Add(s);
                c.CompressionModes = list.ToArray();
            }
            if (v.TryGet("sampling", out var sm) && sm.IsObject)
                foreach (var kv in sm.Obj) c.Sampling[kv.Key] = (float)kv.Value.Num;
            return c;
        }
    }

    /// <summary>Config source seam — file, remote fetch, or baked.</summary>
    public interface ITelemetryConfigProvider
    {
        System.Threading.Tasks.Task<TelemetryRemoteConfig> GetAsync(System.Threading.CancellationToken ct);
    }

    /// <summary>Reads a JSON file; missing file = null (local-only).</summary>
    public sealed class FileConfigProvider : ITelemetryConfigProvider
    {
        private readonly string _path;
        public FileConfigProvider(string path) { _path = path; }

        public System.Threading.Tasks.Task<TelemetryRemoteConfig> GetAsync(System.Threading.CancellationToken ct)
        {
            try
            {
                if (!System.IO.File.Exists(_path)) return System.Threading.Tasks.Task.FromResult<TelemetryRemoteConfig>(null);
                var c = TelemetryRemoteConfig.Parse(System.IO.File.ReadAllText(_path));
                if (c.IsExpired(DateTime.UtcNow)) return System.Threading.Tasks.Task.FromResult<TelemetryRemoteConfig>(null);
                return System.Threading.Tasks.Task.FromResult(c);
            }
            catch { return System.Threading.Tasks.Task.FromResult<TelemetryRemoteConfig>(null); }
        }
    }
}
