using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using System.Threading.Tasks;
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
    public sealed class MultiplayerInstaller : MonoInstaller
    {
        private const string Prefix = "[MultiplayerInstaller]";
        public override async void InstallBindings()
        {
            Debug.Log($"{Prefix} InstallBindings start.");
            await Install(Container);
            Debug.Log($"{Prefix} InstallBindings end.");
        }

        public static async Task Install(DiContainer container)
        {
            Debug.Log("[MultiplayerInstaller] Install start.");
            var hasInternet = false;
            try
            {
                Debug.Log($"{Prefix} Probing Unity Services initialization (timeout 6s)");
                var initTask = UnityServices.InitializeAsync();
                var initTimeout = Task.Delay(TimeSpan.FromSeconds(6));
                var initCompleted = await Task.WhenAny(initTask, initTimeout);

                if (initCompleted == initTask)
                {
                    Debug.Log($"{Prefix} UnityServices.InitializeAsync completed.");
                    try
                    {
                        // Try quick anonymous sign-in if not already signed in
                        if (!AuthenticationService.Instance.IsSignedIn)
                        {
                            Debug.Log($"{Prefix} Attempting anonymous sign-in (timeout 6s)");
                            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
                            var signInCompleted = await Task.WhenAny(signInTask, Task.Delay(TimeSpan.FromSeconds(6)));
                            if (signInCompleted == signInTask && AuthenticationService.Instance.IsSignedIn)
                            {
                                Debug.Log($"{Prefix} Anonymous sign-in succeeded.");
                                hasInternet = true;
                            }
                            else
                            {
                                Debug.Log($"{Prefix} Anonymous sign-in timed out or failed.");
                            }
                        }
                        else
                        {
                            Debug.Log($"{Prefix} Already signed in.");
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
                    Debug.Log($"{Prefix} UnityServices.InitializeAsync timed out.");
                }

                if (!hasInternet)
                {
                    Debug.Log($"{Prefix} Falling back to HTTP-based InternetChecker probe.");
                    try { hasInternet = await InternetChecker.HasInternetAsync(3, 3); } catch (Exception ex) { Debug.LogError($"{Prefix} HTTP probe failed: {ex.Message}"); hasInternet = false; }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{Prefix} Connectivity probe failed: {ex.Message}");
                try { hasInternet = await InternetChecker.HasInternetAsync(3, 3); } catch (Exception innerEx) { Debug.LogError($"{Prefix} HTTP probe fallback failed: {innerEx.Message}"); hasInternet = false; }
            }

            Debug.Log($"{Prefix} Connectivity probe result: hasInternet={hasInternet}");

            // Логування
            container.Bind<IMultiplayerLogger>()
                .To<UnityMultiplayerLogger>()
                .AsSingle();

            // Конфігурація
            container.Bind<IConfigStore>()
                .To<BinaryConfigStore>()
                .AsSingle();

            container.Bind<MultiplayerConfig>()
                .FromMethod(ctx =>
                {
                    var store = ctx.Container.Resolve<IConfigStore>();
                    var cfg = store.Exists() ? store.Load() : MultiplayerConfig.Default();
                    if (!hasInternet)
                    {
                        // Force offline provider when network is unavailable to avoid creating relay/ws providers
                        return new MultiplayerConfig(
                            cfg.SchemaVersion,
                            NetworkProviderType.Offline,
                            cfg.DefaultSessionRules,
                            cfg.StrictParticipantLock,
                            cfg.EnforceConfigConsistency,
                            cfg.MatchmakingEnabled,
                            cfg.RelaySettings,
                            cfg.WebSocketSettings,
                            cfg.FallbackProviderType);
                    }
                    return cfg;
                })
                .AsSingle();

            // Switchable network provider (single DI entry point that can switch implementations at runtime)
            container.Bind<SwitchableNetworkProvider>()
                .AsSingle()
                .NonLazy();

            // Bind INetworkProvider to the switchable wrapper so existing code remains unchanged
            container.Bind<INetworkProvider>()
                .FromMethod(ctx => ctx.Container.Resolve<SwitchableNetworkProvider>())
                .AsSingle();

            // Switchable lobby service
            container.Bind<SwitchableLobbyService>()
                .AsSingle()
                .NonLazy();

            // Bind ILobbyService to the switchable wrapper
            container.Bind<ILobbyService>()
                .FromMethod(ctx => ctx.Container.Resolve<SwitchableLobbyService>())
                .AsSingle();

            // Сховище знімків світу
            container.Bind<IWorldSnapshotStore>()
                .To<InMemoryWorldSnapshotStore>()
                .AsSingle();

            // Обробка відмов
            container.Bind<IFailureHandlingPolicy>()
                .To<SimpleFailureHandlingPolicy>()
                .AsSingle();

            // Основні сервіси
            container.Bind<ISessionManager>()
                .To<SessionManager>()
                .AsSingle();

            container.Bind<IParticipantPolicyService>()
                .To<ParticipantPolicyService>()
                .AsSingle();

            container.Bind<IWorldConsistencyService>()
                .To<WorldConsistencyService>()
                .AsSingle();

            // Міграція хоста та клонування світу
            container.Bind<IHostMigrationService>()
                .To<HostMigrationService>()
                .AsSingle();

            container.Bind<IWorldCloneService>()
                .To<WorldCloneService>()
                .AsSingle();

            // Учасники та конфігурація
            container.Bind<IParticipantFallbackService>()
                .To<ParticipantFallbackService>()
                .AsSingle();

            container.Bind<IConfigSyncService>()
                .To<ConfigSyncService>()
                .AsSingle();

            // Синхронізація ігрових команд
            container.Bind<IGameCommandSyncService>()
                .To<GameCommandSyncService>()
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

            // NetworkModeController: orchestrates runtime switching and auto-probing
            container.Bind<NetworkModeController>()
                .AsSingle();
        }
    }
}
