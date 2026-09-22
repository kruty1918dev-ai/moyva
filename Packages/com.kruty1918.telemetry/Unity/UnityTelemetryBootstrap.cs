using System.IO;
using UnityEngine;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Fingerprinting;
using Kruty1918.Telemetry.Upload;

namespace Kruty1918.Telemetry.Unity
{
    /// <summary>
    /// One-call composition for Unity hosts:
    ///   var host = UnityTelemetryBootstrap.Create(config, contracts, fingerprint);
    /// Creates the runtime rooted at persistentDataPath/telemetry, a host behaviour
    /// in the scene, and returns both. Product layers own the contract registry and
    /// fingerprint input; nothing here is product-specific.
    /// </summary>
    public static class UnityTelemetryBootstrap
    {
        public sealed class Handle
        {
            public TelemetryRuntime Runtime;
            public TelemetryHostBehaviour Host;
            public ITelemetrySink Sink => Runtime.Sink;
        }

        public static Handle Create(TelemetryConfig config = null,
            ContractRegistry contracts = null,
            FingerprintInput fingerprint = null,
            string storageRoot = null,
            ITelemetryTransport transport = null,
            INetworkStatus network = null,
            ITelemetryConfigProvider configProvider = null)
        {
            config = config ?? new TelemetryConfig();
            contracts = contracts ?? new ContractRegistry();
            StandardContracts.RegisterAll(contracts);
            storageRoot = storageRoot ?? Path.Combine(Application.persistentDataPath, "telemetry");

            var runtime = new TelemetryRuntime(config, contracts, fingerprint, storageRoot,
                transport ?? new UnityWebRequestTransport(),
                network ?? new UnityNetworkStatus(),
                configProvider,
                new UnityTelemetryLogger());

            var go = new GameObject("TelemetryHost");
            var host = go.AddComponent<TelemetryHostBehaviour>();
            host.Initialize(runtime);
            return new Handle { Runtime = runtime, Host = host };
        }
    }
}
