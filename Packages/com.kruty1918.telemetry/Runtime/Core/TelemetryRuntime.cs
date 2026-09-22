using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Diagnostics;
using Kruty1918.Telemetry.Fingerprinting;
using Kruty1918.Telemetry.Identity;
using Kruty1918.Telemetry.Storage;
using Kruty1918.Telemetry.Upload;
using Kruty1918.Telemetry.Validation;

namespace Kruty1918.Telemetry.Core
{
    /// <summary>
    /// Composition root for the whole pipeline. Hosts wire it once, then call
    /// <see cref="Tick"/> periodically (or rely on <see cref="Start"/> background loop).
    /// Dispose performs a final flush + store sync. Gameplay must work when telemetry
    /// is disabled or broken — this class never throws into callers for data issues.
    /// </summary>
    public sealed class TelemetryRuntime : IDisposable
    {
        public TelemetryConfig Config { get; }
        public ContractRegistry Contracts { get; }
        public TelemetrySessionContext Session { get; }
        public TelemetryDiagnostics Diagnostics { get; }
        public LocalTelemetryStore Store { get; }
        public TelemetrySink Sink { get; }
        public TelemetryUploadCoordinator Uploader { get; }
        public FingerprintInput FingerprintInput { get; }
        public DatasetFingerprint Fingerprint { get; private set; }

        private readonly List<IBatchRule> _rules = new List<IBatchRule>();
        private readonly ITelemetryConfigProvider _configProvider;
        private readonly IKeyValueStore _kv;
        private TelemetryRemoteConfig _remote;
        private CancellationTokenSource _loop;
        private bool _disposed;

        public TelemetryRemoteConfig RemoteConfig => _remote;

        public TelemetryRuntime(TelemetryConfig config, ContractRegistry contracts,
            FingerprintInput fingerprintInput, string storageRoot,
            ITelemetryTransport transport = null,
            INetworkStatus network = null,
            ITelemetryConfigProvider configProvider = null,
            ITelemetryLogger log = null)
        {
            Config = config ?? new TelemetryConfig();
            Contracts = contracts ?? new ContractRegistry();
            Contracts.Freeze();
            FingerprintInput = fingerprintInput ?? new FingerprintInput();
            Fingerprint = DatasetFingerprint.Compute(FingerprintInput);
            Diagnostics = new TelemetryDiagnostics();
            _kv = new FileKeyValueStore(System.IO.Path.Combine(storageRoot ?? "telemetry", "kv.json"));
            Session = new TelemetrySessionContext(InstallationIdentity.GetOrCreate(_kv));
            var policy = new LocalTelemetryStore.Policy
            {
                MaxTotalBytes = Config.MaxStorageBytes,
                MaxPendingBatches = Config.MaxPendingBatches,
            };
            Store = new LocalTelemetryStore(System.IO.Path.Combine(storageRoot ?? "telemetry", "spool"),
                policy, Diagnostics, log);
            _configProvider = configProvider;
            _rules.Add(new LifecycleSequenceRule());
            _rules.Add(new StatisticalBatchRule());
            var compression = Config.Compression == "gzip"
                ? (Compression.ICompressionProvider)new Compression.GZipCompressionProvider()
                : new Compression.NoCompressionProvider();
            Sink = new TelemetrySink(Session, Contracts, Store, Diagnostics, log,
                () => Config, () => Fingerprint.Value, () => _rules, compression);
            Uploader = new TelemetryUploadCoordinator(Store, transport ?? new NullTelemetryTransport(),
                Diagnostics, log, network: network)
            {
                Endpoint = () => _remote?.Endpoint,
                ProjectId = () => _remote?.ProjectId ?? Config.ProjectId,
                EnvironmentName = () => _remote?.EnvironmentName ?? Config.EnvironmentName,
                AuthToken = () => _remote?.ClientToken,
            };
            Store.Recover();
        }

        /// <summary>Refresh remote config once; safe to call on a slow cadence.</summary>
        public async Task RefreshConfigAsync(CancellationToken ct = default)
        {
            if (_configProvider == null) return;
            var c = await _configProvider.GetAsync(ct).ConfigureAwait(false);
            if (c != null && c.SupportsProtocol(Serialization.TelemetryBatch.ProtocolVersion))
            {
                _remote = c;
                ApplySamplingOverrides(c);
            }
        }

        private void ApplySamplingOverrides(TelemetryRemoteConfig c)
        {
            foreach (var contract in Contracts.All())
                if (c.Sampling.TryGetValue(contract.ContractId, out float rate))
                    contract.SamplingRate = Clamp01(rate);
        }

        private static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;

        /// <summary>One cooperative cycle: flush if due, then drain uploads once.</summary>
        public async Task Tick(CancellationToken ct = default)
        {
            if (_disposed) return;
            Sink.Flush();
            await Uploader.TickAsync(ct).ConfigureAwait(false);
        }

        /// <summary>Background loop for hosts that don't drive Tick (Unity host does drive).</summary>
        public void Start()
        {
            if (_loop != null) return;
            _loop = new CancellationTokenSource();
            var ct = _loop.Token;
            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try { await Tick(ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                    catch (Exception e) { Diagnostics.NoteError("tick:" + e.GetType().Name); }
                    try { await Task.Delay(TimeSpan.FromSeconds(Config.UploadIntervalSeconds), ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
            });
        }

        /// <summary>Consent-level purge: wipe local spool and rotate installation id.</summary>
        public void PurgeAndRotateIdentity()
        {
            Store.Purge();
            _kv.Delete(InstallationIdentity.KeyName);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _loop?.Cancel();
            try { Sink.Flush(); } catch { }
        }
    }
}
