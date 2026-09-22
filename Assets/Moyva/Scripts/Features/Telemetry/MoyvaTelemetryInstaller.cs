using System;
using System.IO;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Telemetry.Configuration;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Unity;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>
    /// Wires the telemetry pipeline into the gameplay container.
    /// Install from the scene/bootstrap installer: MoyvaTelemetryInstaller.Install(Container).
    /// Telemetry is strictly observational: every adapter only reads signals/state.
    ///
    /// Configuration precedence (no secrets in code/scenes):
    ///   defaults → env vars (MOYVA_TELEMETRY_*) → telemetry.remote.json
    ///   in persistentDataPath (endpoint, token, sampling — deliverable by backend).
    /// </summary>
    public sealed class MoyvaTelemetryInstaller : Installer<MoyvaTelemetryInstaller>
    {
        /// <summary>Runtime handle — resolved lazily on first use.</summary>
        public sealed class Context
        {
            public UnityTelemetryBootstrap.Handle Handle;
            public TelemetryRuntime Runtime => Handle?.Runtime;
            public ITelemetrySink Sink => Runtime?.Sink;
        }

        public override void InstallBindings()
        {
            Container.Bind<Context>().FromMethod(CreateContext).AsSingle();
            Container.BindInterfacesAndSelfTo<TelemetryRuntime>()
                .FromMethod(c => c.Container.Resolve<Context>().Runtime).AsSingle();
            Container.Bind<ITelemetrySink>().FromMethod(c => c.Container.Resolve<Context>().Sink).AsSingle();

            Container.BindInterfacesAndSelfTo<MoyvaSignalTelemetryAdapter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MoyvaBotTelemetryAdapter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MoyvaNetworkTelemetryAdapter>().AsSingle().NonLazy();
            Container.Bind<ITurnParticipant>().To<TelemetryTurnParticipant>().AsSingle();
        }

        private static Context CreateContext(InjectContext _)
        {
            var registry = MoyvaTelemetryContracts.CreateRegistry();
            var config = LoadConfig();
            var remoteConfigPath = Environment.GetEnvironmentVariable("MOYVA_TELEMETRY_REMOTE_CONFIG")
                ?? Path.Combine(Application.persistentDataPath, "telemetry.remote.json");

            var handle = UnityTelemetryBootstrap.Create(
                config: config,
                contracts: registry,
                fingerprint: MoyvaTelemetryContracts.CreateFingerprintInput(registry),
                configProvider: new FileConfigProvider(remoteConfigPath));
            return new Context { Handle = handle };
        }

        private static TelemetryConfig LoadConfig()
        {
            var c = new TelemetryConfig
            {
                ProducerId = MoyvaTelemetryContracts.ProducerId,
                ProducerVersion = Application.version,
                ProjectId = "moyva",
            };
            if (Env("ENABLED") is string e) c.Enabled = e == "1" || e.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (Env("CONSENT") is string consent
                && Enum.TryParse(consent, true, out TelemetryConsentLevel level))
                c.Consent = level;
            return c;

            string Env(string key) => Environment.GetEnvironmentVariable("MOYVA_TELEMETRY_" + key);
        }
    }
}
