using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using Kruty1918.Moyva.Shared.Connectivity;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для мультиплеєрної підсистеми.
    /// Підключіть у сцені для реєстрації всіх мережевих сервісів.
    /// </summary>
    internal sealed class MultiplayerConstructionRuntimeAuthority :
        IConstructionRuntimeAuthorityQuery
    {
        private readonly ISessionManager _sessionManager;

        public MultiplayerConstructionRuntimeAuthority(
            ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public bool IsAuthoritativeRuntime
        {
            get
            {
                if (_sessionManager == null)
                    return true;

                IReadOnlyList<Participant> participants =
                    _sessionManager.Participants;
                return participants == null
                    || participants.Count <= 1
                    || _sessionManager.IsLocalPlayerHost;
            }
        }
    }

    public sealed class MultiplayerInstaller : MonoInstaller
    {
        private const string Prefix = "[MultiplayerInstaller]";

        // Bind minimal, switchable wrappers synchronously so ILobbyService and INetworkProvider
        // are always resolvable by other installers during scene startup.
        public override void InstallBindings()
        {

            // Logging and config store required by switchable wrappers
            Container.Bind<IMultiplayerLogger>()
                .To<UnityMultiplayerLogger>()
                .AsSingle();

            Container.Bind<IConfigStore>()
                .To<BinaryConfigStore>()
                .AsSingle();

            Container.Bind<MultiplayerConfig>()
                .FromMethod(ctx =>
                {
                    var store = ctx.Container.Resolve<IConfigStore>();
                    var logger = ctx.Container.Resolve<IMultiplayerLogger>();
                    return MultiplayerConfigLifecycle.LoadValidateFreeze(store, logger);
                })
                .AsSingle();

            // Switchable network provider (single DI entry point that can switch implementations at runtime)
            Container.Bind<SwitchableNetworkProvider>()
                .AsSingle()
                .NonLazy();

            // Bind INetworkProvider to the switchable wrapper so existing code remains unchanged
            Container.Bind<INetworkProvider>()
                .FromMethod(ctx => ctx.Container.Resolve<SwitchableNetworkProvider>())
                .AsSingle();

            // Switchable lobby service
            Container.Bind<SwitchableLobbyService>()
                .AsSingle()
                .NonLazy();

            // Bind ILobbyService to the switchable wrapper
            Container.Bind<ILobbyService>()
                .FromMethod(ctx => ctx.Container.Resolve<SwitchableLobbyService>())
                .AsSingle();

            Container.Bind<IMultiplayerModeSelector>()
                .To<MultiplayerModeSelector>()
                .AsSingle()
                .NonLazy();

            if (!Container.HasBinding(typeof(IGameCommandSyncService)))
            {
                Container.Bind<IGameCommandSyncService>()
                    .To<GameCommandSyncService>()
                    .AsSingle();
            }

            EnsureAuthorityCoreBindings(Container);

            if (!Container.HasBinding(
                    typeof(IConstructionRuntimeAuthorityQuery)))
            {
                Container.Bind<IConstructionRuntimeAuthorityQuery>()
                    .To<MultiplayerConstructionRuntimeAuthority>()
                    .AsSingle();
            }

            if (!Container.HasBinding(typeof(StartingPositionSyncService)))
            {
                Container.BindInterfacesTo<StartingPositionSyncService>()
                    .AsSingle()
                    .NonLazy();
            }

            // Start the async install process but do not await it here — it will complete and
            // bind network-dependent services when ready. We intentionally do not use
            // `async void` to avoid race conditions where other installers run before
            // the wrapper bindings exist.
            var _ = Install(Container);
        }

        public static async Task Install(DiContainer container)
        {
            // If this static Install() is invoked directly (ProjectServicesInstaller calls it),
            // ensure the minimal, switchable wrappers are bound synchronously so other
            // installers can resolve `ILobbyService` immediately.
            try
            {
                if (!container.HasBinding(typeof(ILobbyService)))
                {
                    // Logging and config store required by switchable wrappers
                    if (!container.HasBinding(typeof(IMultiplayerLogger)))
                        container.Bind<IMultiplayerLogger>().To<UnityMultiplayerLogger>().AsSingle();

                    if (!container.HasBinding(typeof(IConfigStore)))
                        container.Bind<IConfigStore>().To<BinaryConfigStore>().AsSingle();

                    if (!container.HasBinding(typeof(MultiplayerConfig)))
                        container.Bind<MultiplayerConfig>().FromMethod(ctx =>
                        {
                            var store = ctx.Container.Resolve<IConfigStore>();
                            var logger = ctx.Container.Resolve<IMultiplayerLogger>();
                            return MultiplayerConfigLifecycle.LoadValidateFreeze(store, logger);
                        }).AsSingle();

                    if (!container.HasBinding(typeof(SwitchableNetworkProvider)))
                        container.Bind<SwitchableNetworkProvider>().AsSingle().NonLazy();

                    if (!container.HasBinding(typeof(INetworkProvider)))
                        container.Bind<INetworkProvider>().FromMethod(ctx => ctx.Container.Resolve<SwitchableNetworkProvider>()).AsSingle();

                    if (!container.HasBinding(typeof(SwitchableLobbyService)))
                        container.Bind<SwitchableLobbyService>().AsSingle().NonLazy();

                    // Finally bind ILobbyService to the switchable wrapper
                    if (!container.HasBinding(typeof(ILobbyService)))
                        container.Bind<ILobbyService>().FromMethod(ctx => ctx.Container.Resolve<SwitchableLobbyService>()).AsSingle();
                }

                if (!container.HasBinding(typeof(IMultiplayerModeSelector)))
                    container.Bind<IMultiplayerModeSelector>().To<MultiplayerModeSelector>().AsSingle().NonLazy();

                if (!container.HasBinding(typeof(IGameCommandSyncService)))
                    container.Bind<IGameCommandSyncService>().To<GameCommandSyncService>().AsSingle();

                EnsureAuthorityCoreBindings(container);

                if (!container.HasBinding(
                        typeof(IConstructionRuntimeAuthorityQuery)))
                {
                    container.Bind<IConstructionRuntimeAuthorityQuery>()
                        .To<MultiplayerConstructionRuntimeAuthority>()
                        .AsSingle();
                }

                if (!container.HasBinding(typeof(StartingPositionSyncService)))
                    container.BindInterfacesTo<StartingPositionSyncService>().AsSingle().NonLazy();
            }
            catch (Exception)
            {
            }
            var canUseUgs = false;
            var hasInternet = false;
            try
            {
                var initTask = UnityServices.InitializeAsync();
                var initTimeout = Task.Delay(TimeSpan.FromSeconds(6));
                var initCompleted = await Task.WhenAny(initTask, initTimeout);

                if (initCompleted == initTask)
                {
                    try
                    {
                        // Try quick anonymous sign-in if not already signed in/authorized
                        if (!AuthenticationService.Instance.IsSignedIn || !AuthenticationService.Instance.IsAuthorized)
                        {
                            MultiplayerClientScope.ApplyAuthenticationProfileIfNeeded();
                            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
                            var signInCompleted = await Task.WhenAny(signInTask, Task.Delay(TimeSpan.FromSeconds(6)));
                            if (signInCompleted == signInTask && AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                            {
                                canUseUgs = true;
                                hasInternet = true;
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            canUseUgs = true;
                            hasInternet = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"{Prefix} Exception during sign-in check: {ex.Message}");
                    }
                }
                else
                {
                }

                if (!canUseUgs)
                {
                    try { hasInternet = await InternetChecker.HasInternetAsync(3, 3); } catch (Exception ex) { Debug.LogError($"{Prefix} HTTP probe failed: {ex.Message}"); hasInternet = false; }
                }
            }
            catch (Exception)
            {
                try { hasInternet = await InternetChecker.HasInternetAsync(3, 3); } catch (Exception innerEx) { Debug.LogError($"{Prefix} HTTP probe fallback failed: {innerEx.Message}"); hasInternet = false; }
            }

            // We bound a preliminary MultiplayerConfig and logger synchronously in InstallBindings
            // so the switchable wrappers are resolvable during startup. Now compute the final
            // config taking connectivity into account and update the container + switchable
            // lobby provider accordingly.
            var store = container.Resolve<IConfigStore>();
            IMultiplayerLogger logger = container.HasBinding(typeof(IMultiplayerLogger))
                ? container.Resolve<IMultiplayerLogger>()
                : null;
            var cfg = MultiplayerConfigLifecycle.LoadValidateFreeze(store, logger);
            cfg = ApplyRiskFeatureToggles(cfg);
            MultiplayerConfig finalCfg;
            if (!canUseUgs)
            {
                finalCfg = new MultiplayerConfig(
                    cfg.SchemaVersion,
                    cfg.FallbackProviderType,
                    cfg.DefaultSessionRules,
                    cfg.StrictParticipantLock,
                    cfg.EnforceConfigConsistency,
                    cfg.MatchmakingEnabled,
                    cfg.RelaySettings,
                    cfg.WebSocketSettings,
                        cfg.FallbackProviderType,
                        cfg.ReconnectLocalTimeToleranceSeconds,
                        cfg.GracefulReconnectWindowSeconds,
                        cfg.EnableRelayProvider,
                        cfg.EnableHostMigration);
            }
            else
            {
                // If internet and UGS auth are available, prefer configured provider (e.g., Relay),
                // but detect if UGS Lobby package is actually present. If UGS is not
                // available, fall back to LAN provider so ILobbyService remains functional
                // for local multiplayer instead of silently returning null on create.
                bool ugsPresent = false;
                try
                {
                    Type t = null;
                    t = Type.GetType("Unity.Services.Lobbies.LobbyService, Unity.Services.Lobbies");
                    if (t == null)
                        t = Type.GetType("Unity.Services.Lobbies.LobbyService, Unity.Services.Multiplayer");
                    ugsPresent = t != null;
                }
                catch { }

                if (!ugsPresent)
                {
                    finalCfg = new MultiplayerConfig(
                        cfg.SchemaVersion,
                        NetworkProviderType.Lan,
                        cfg.DefaultSessionRules,
                        cfg.StrictParticipantLock,
                        cfg.EnforceConfigConsistency,
                        cfg.MatchmakingEnabled,
                        cfg.RelaySettings,
                        cfg.WebSocketSettings,
                        cfg.FallbackProviderType,
                        cfg.ReconnectLocalTimeToleranceSeconds,
                        cfg.GracefulReconnectWindowSeconds,
                        cfg.EnableRelayProvider,
                        cfg.EnableHostMigration);
                }
                else
                {
                    var relayReflectionValid = RelayNetworkProvider.TryValidateReflectionBindings(out var reflectionError);
                    if (cfg.ProviderType == NetworkProviderType.Relay && !relayReflectionValid)
                    {
                        var fallbackType = cfg.FallbackProviderType == NetworkProviderType.Relay
                            ? NetworkProviderType.Offline
                            : cfg.FallbackProviderType;
                        finalCfg = new MultiplayerConfig(
                            cfg.SchemaVersion,
                            fallbackType,
                            cfg.DefaultSessionRules,
                            cfg.StrictParticipantLock,
                            cfg.EnforceConfigConsistency,
                            cfg.MatchmakingEnabled,
                            cfg.RelaySettings,
                            cfg.WebSocketSettings,
                            cfg.FallbackProviderType,
                                cfg.ReconnectLocalTimeToleranceSeconds,
                                cfg.GracefulReconnectWindowSeconds,
                                cfg.EnableRelayProvider,
                                cfg.EnableHostMigration);
                    }
                    else
                    {
                        finalCfg = cfg;
                    }
                }
            }
            try
            {
                var switchable = container.Resolve<SwitchableLobbyService>();
                await switchable.SwitchToAsync(finalCfg.ProviderType);
            }
            catch (Exception)
            {
            }

            try
            {
                if (container.HasBinding(typeof(SwitchableNetworkProvider)))
                {
                    var switchableNetwork = container.Resolve<SwitchableNetworkProvider>();
                    await switchableNetwork.SwitchToAsync(ResolveNetworkBootstrapProviderType(finalCfg));
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (container.HasBinding(typeof(IMultiplayerModeSelector)))
                {
                    var modeSelector = container.Resolve<IMultiplayerModeSelector>();
                    await modeSelector.SetModeAsync(finalCfg.ProviderType);
                }
            }
            catch (Exception)
            {
            }

            // Автоматичний leave сесії при виході з гри (Application.quitting / wantsToQuit).
            container.BindInterfacesAndSelfTo<MultiplayerExitDisconnect>()
                .AsSingle()
                .NonLazy();

            // Late-join catch-up: host надсилає snapshot будівель + economy кожному новому peer.
            container.BindInterfacesAndSelfTo<WorldStateReplicationService>()
                .AsSingle()
                .NonLazy();

            // Міграція хоста та клонування світу
            container.Bind<IWorldCloneService>()
                .To<WorldCloneService>()
                .AsSingle();

            // Учасники та конфігурація
            container.Bind<IRoomAccessPolicyService>()
                .To<RoomAccessPolicyService>()
                .AsSingle();

            container.Bind<IMultiplayerQosMonitorService>()
                .To<MultiplayerQosMonitorService>()
                .AsSingle();

            container.Bind<IConfigSyncService>()
                .To<ConfigSyncService>()
                .AsSingle();

            // Identity-сервіс (UGS Auth коли доступно, інакше device id).
            container.Bind<IMultiplayerIdentityService>()
                .To<MultiplayerIdentityService>()
                .AsSingle();

            if (hasInternet)
            {
                container.Bind<IMultiplayerState>()
                    .To<MultiplayerState>()
                    .AsSingle();
            }
            else
            {
                container.Bind<IMultiplayerState>()
                    .To<OfflineMultiplayerState>()
                    .AsSingle();
            }

        }

        /// <summary>
        /// Binds session and construction-authority services before any async
        /// UGS/network probing. Scene installers must be able to resolve these
        /// bindings deterministically during their own InstallBindings pass.
        /// </summary>
        internal static void EnsureAuthorityCoreBindings(
            DiContainer container)
        {
            if (!container.HasBinding(typeof(IWorldSnapshotStore)))
            {
                container.Bind<IWorldSnapshotStore>()
                    .To<InMemoryWorldSnapshotStore>()
                    .AsSingle();
            }

            if (!container.HasBinding(typeof(IFailureHandlingPolicy)))
            {
                container.Bind<IFailureHandlingPolicy>()
                    .To<SimpleFailureHandlingPolicy>()
                    .AsSingle();
            }

            if (!container.HasBinding(typeof(IParticipantPolicyService)))
            {
                container.Bind<IParticipantPolicyService>()
                    .To<ParticipantPolicyService>()
                    .AsSingle();
            }

            if (!container.HasBinding(typeof(IWorldConsistencyService)))
            {
                container.Bind<IWorldConsistencyService>()
                    .To<WorldConsistencyService>()
                    .AsSingle();
            }

            if (!container.HasBinding(typeof(IHostMigrationService)))
            {
                container.Bind<IHostMigrationService>()
                    .To<HostMigrationService>()
                    .AsSingle();
            }

            if (!container.HasBinding(
                    typeof(IHostMigrationCheckpointService)))
            {
                container.Bind<IHostMigrationCheckpointService>()
                    .To<HostMigrationCheckpointService>()
                    .AsSingle();
            }

            if (!container.HasBinding(typeof(ISessionManager)))
            {
                container.Bind<ISessionManager>()
                    .To<SessionManager>()
                    .AsSingle();
            }

            if (!container.HasBinding(
                    typeof(Kruty1918.Moyva.GameMode.API.IExitMatchDisconnectHandler)))
            {
                container.Bind<
                        Kruty1918.Moyva.GameMode.API.IExitMatchDisconnectHandler>()
                    .To<ExitMatchDisconnectHandler>()
                    .AsSingle();
            }

            if (!container.HasBinding(
                    typeof(Kruty1918.Moyva.GameMode.API.IGamePauseModePolicy)))
            {
                container.Bind<
                        Kruty1918.Moyva.GameMode.API.IGamePauseModePolicy>()
                    .To<MultiplayerGamePauseModePolicy>()
                    .AsSingle();
            }

            if (!container.HasBinding(
                    typeof(IConstructionPlacementAuthorityPolicy)))
            {
                container.Bind<IConstructionPlacementAuthorityPolicy>()
                    .To<MultiplayerConstructionPlacementAuthorityPolicy>()
                    .AsSingle();
            }

            if (!container.HasBinding(
                    typeof(MultiplayerAuthorityService)))
            {
                container
                    .BindInterfacesAndSelfTo<
                        MultiplayerAuthorityService>()
                    .AsSingle()
                    .NonLazy();
            }
        }

        private static MultiplayerConfig ApplyRiskFeatureToggles(MultiplayerConfig config)
        {
            if (config.EnableRelayProvider)
                return config;

            var fallbackType = config.FallbackProviderType == NetworkProviderType.Relay
                ? NetworkProviderType.Offline
                : config.FallbackProviderType;

            var providerType = config.ProviderType == NetworkProviderType.Relay
                ? fallbackType
                : config.ProviderType;

            if (providerType == config.ProviderType && fallbackType == config.FallbackProviderType)
                return config;

            return new MultiplayerConfig(
                config.SchemaVersion,
                providerType,
                config.DefaultSessionRules,
                config.StrictParticipantLock,
                config.EnforceConfigConsistency,
                config.MatchmakingEnabled,
                config.RelaySettings,
                config.WebSocketSettings,
                fallbackType,
                config.ReconnectLocalTimeToleranceSeconds,
                config.GracefulReconnectWindowSeconds,
                config.EnableRelayProvider,
                config.EnableHostMigration);
        }

        private static NetworkProviderType ResolveNetworkBootstrapProviderType(MultiplayerConfig config)
        {
            if (!config.EnableRelayProvider && config.ProviderType == NetworkProviderType.Relay)
                return config.FallbackProviderType == NetworkProviderType.Relay ? NetworkProviderType.Offline : config.FallbackProviderType;

            if (config.ProviderType != NetworkProviderType.Relay || RelayNetworkProvider.IsRuntimeAvailable)
                return config.ProviderType;

            var fallbackType = config.FallbackProviderType == NetworkProviderType.Relay
                ? NetworkProviderType.Offline
                : config.FallbackProviderType;
            return fallbackType;
        }
    }
}
