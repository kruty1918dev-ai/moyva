using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.InputRouting.Runtime;
using Zenject;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class InteractionsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InputRoutingBindings.Install(Container);

            Container.BindInterfacesAndSelfTo<TileClickInputService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<WorldInfoSelectionCoordinator>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<TileInteractionService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MapObjectWorldInfoPresenter>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MapObjectSelectionHighlightService>()
                .AsSingle();
        }
    }
}
