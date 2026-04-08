using Kruty1918.Moyva.GameMode.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    internal sealed class GameModeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameModeService>()
                .To<GameModeService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameModePanelController>()
                .AsSingle();
        }
    }
}
