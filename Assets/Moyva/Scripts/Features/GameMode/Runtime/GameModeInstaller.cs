using Kruty1918.Moyva.GameMode.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    public sealed class GameModeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
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
                Debug.LogWarning("[GameModeInstaller] GameModeUIController не знайдено у сцені. Кнопки перемикання режимів не будуть ініціалізовані.");
            }

            var pauseMenuController = Object.FindFirstObjectByType<InGamePauseMenuController>(FindObjectsInactive.Include);
            if (pauseMenuController != null)
            {
                Container.QueueForInject(pauseMenuController);
                Container.BindInterfacesAndSelfTo<InGamePauseMenuController>()
                    .FromInstance(pauseMenuController)
                    .AsSingle()
                    .NonLazy();
            }
            else
            {
                Debug.LogWarning("[GameModeInstaller] InGamePauseMenuController не знайдено у сцені. Внутрішньоігрове меню паузи не буде ініціалізовано.");
            }

            // Явний порядок Initialize() — менше число = раніше.
            Container.Bind<IGameStateService>()
                .To<GameStateService>()
                .AsSingle();

            Container.BindExecutionOrder<GameModeChangeRequestRouter>(-10);
            Container.BindExecutionOrder<GameModePanelController>(-10);
            Container.BindExecutionOrder<GameModeUIController>(-5);
            Container.BindExecutionOrder<InGamePauseMenuController>(-5);
        }
    }
}
