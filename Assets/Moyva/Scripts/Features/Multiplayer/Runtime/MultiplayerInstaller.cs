using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для мультиплеєрної підсистеми.
    /// Підключіть у сцені для реєстрації всіх мережевих сервісів.
    /// </summary>
    public sealed class MultiplayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Install(Container);
        }

        public static void Install(DiContainer container)
        {
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
                    return store.Exists() ? store.Load() : MultiplayerConfig.Default();
                })
                .AsSingle();

            // Мережевий провайдер
            container.Bind<INetworkProvider>()
                .FromMethod(ctx =>
                {
                    var config = ctx.Container.Resolve<MultiplayerConfig>();
                    var logger = ctx.Container.Resolve<IMultiplayerLogger>();
                    return NetworkProviderFactory.Create(config, logger);
                })
                .AsSingle();

            // Lobby-сервіс (UGS Lobby, якщо пакет встановлений; інакше офлайн-заглушка).
            container.Bind<ILobbyService>()
                .FromMethod(ctx =>
                {
                    var config = ctx.Container.Resolve<MultiplayerConfig>();
                    var logger = ctx.Container.Resolve<IMultiplayerLogger>();
                    if (config.ProviderType == NetworkProviderType.Offline)
                        return new OfflineLobbyService(logger);
                    return new UgsLobbyService(logger);
                })
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

            container.Bind<IMultiplayerState>()
                .To<MultiplayerState>()
                .AsSingle();
        }
    }
}
