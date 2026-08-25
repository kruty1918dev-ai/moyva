using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.UIActions.Runtime;
using Kruty1918.Moyva.Turns.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    public sealed class GameModeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            UiActionsInstaller.Install(Container);
            TurnBindings.Install(Container);

            Container.Bind<IGameModeService>()
                .To<GameModeService>()
                .AsSingle()
                .NonLazy(); // Ініціалізується одразу

            Container.BindInterfacesAndSelfTo<GameModePanelController>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GameModeChangeRequestRouter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GameModeUiActionHandler>()
                .AsSingle()
                .NonLazy();

            var gameModeUiController = Object.FindFirstObjectByType<GameModeUIController>(FindObjectsInactive.Include);
            if (gameModeUiController != null)
            {
                Container.QueueForInject(gameModeUiController);
                Container.BindInterfacesAndSelfTo<GameModeUIController>()
                    .FromInstance(gameModeUiController)
                    .AsSingle()
                    .NonLazy();
            }
            else
            {
            }

            // Явний порядок Initialize() — менше число = раніше.
            Container.Bind<IGameStateService>()
                .To<GameStateService>()
                .AsSingle();

            Container.Bind<IExitMatchSceneLoader>()
                .To<HomeMenuSceneLoader>()
                .AsSingle();

            Container.Bind<IExitMatchCoordinator>()
                .To<ExitMatchCoordinator>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameplayPauseInputController>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GameplayPauseMenuPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindExecutionOrder<GameModeChangeRequestRouter>(-10);
            Container.BindExecutionOrder<GameModeUiActionHandler>(-10);
            Container.BindExecutionOrder<GameModePanelController>(-10);
            Container.BindExecutionOrder<GameModeUIController>(-5);
            // Observe Esc before construction input. In Construction mode this
            // controller yields, then the construction layer consumes it.
            Container.BindExecutionOrder<GameplayPauseInputController>(-100);
        }
    }
}
