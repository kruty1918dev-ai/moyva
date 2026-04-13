using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using Kruty1918.Moyva.Multiplayer.Runtime;
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
            // Логування
            Container.Bind<IMultiplayerLogger>()
                .To<UnityMultiplayerLogger>()
                .AsSingle();

            // Конфігурація
            Container.Bind<IConfigStore>()
                .To<BinaryConfigStore>()
                .AsSingle();

            Container.Bind<MultiplayerConfig>()
                .FromMethod(ctx =>
                {
                    var store = ctx.Container.Resolve<IConfigStore>();
                    return store.Exists() ? store.Load() : MultiplayerConfig.Default();
                })
                .AsSingle();

            // Мережевий провайдер
            Container.Bind<INetworkProvider>()
                .FromMethod(ctx =>
                {
                    var config = ctx.Container.Resolve<MultiplayerConfig>();
                    var logger = ctx.Container.Resolve<IMultiplayerLogger>();
                    return NetworkProviderFactory.Create(config, logger);
                })
                .AsSingle();

            // Сховище знімків світу
            Container.Bind<IWorldSnapshotStore>()
                .To<InMemoryWorldSnapshotStore>()
                .AsSingle();

            // Обробка відмов
            Container.Bind<IFailureHandlingPolicy>()
                .To<SimpleFailureHandlingPolicy>()
                .AsSingle();

            // Основні сервіси
            Container.Bind<ISessionManager>()
                .To<SessionManager>()
                .AsSingle();

            Container.Bind<IParticipantPolicyService>()
                .To<ParticipantPolicyService>()
                .AsSingle();

            Container.Bind<IWorldConsistencyService>()
                .To<WorldConsistencyService>()
                .AsSingle();

            // Міграція хоста та клонування світу
            Container.Bind<IHostMigrationService>()
                .To<HostMigrationService>()
                .AsSingle();

            Container.Bind<IWorldCloneService>()
                .To<WorldCloneService>()
                .AsSingle();

            // Учасники та конфігурація
            Container.Bind<IParticipantFallbackService>()
                .To<ParticipantFallbackService>()
                .AsSingle();

            Container.Bind<IConfigSyncService>()
                .To<ConfigSyncService>()
                .AsSingle();

            // Синхронізація ігрових команд
            Container.Bind<IGameCommandSyncService>()
                .To<GameCommandSyncService>()
                .AsSingle();
        }
    }
}
