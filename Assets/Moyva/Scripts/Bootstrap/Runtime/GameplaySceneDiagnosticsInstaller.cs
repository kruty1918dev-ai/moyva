using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.SaveSystem;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    public sealed class GameplaySceneDiagnosticsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<IWorldGenerationDiagnostics>())
                Container.Bind<IWorldGenerationDiagnostics>().To<SceneWorldGenerationDiagnostics>().AsSingle().NonLazy();
            Container.BindInterfacesTo<GameplaySceneDiagnosticsBootstrap>().AsSingle()
                .WithArguments(gameObject.scene.name)
                .NonLazy();
        }

        private sealed class GameplaySceneDiagnosticsBootstrap : IInitializable
        {
            private readonly IWorldGenerationDiagnostics _diagnostics;
            private readonly string _sceneName;

            public GameplaySceneDiagnosticsBootstrap(
                IWorldGenerationDiagnostics diagnostics,
                string sceneName)
            {
                _diagnostics = diagnostics;
                _sceneName = sceneName;
            }

            public void Initialize()
            {
                _diagnostics.ReplayProjectContextInstalledFromEnvironment();
                _diagnostics.SceneContextInstalled(
                    $"scene={_sceneName}, mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}");
            }
        }
    }
}
