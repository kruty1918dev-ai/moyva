using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class MenuWorldPreviewKingdomPlacementFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container.HasBinding<IMenuWorldPreviewKingdomPlacementService>())
                return;

            container.Bind<IMenuPreviewKingdomPlacementGeometry>().To<MenuPreviewKingdomPlacementGeometry>().AsSingle();
            container.Bind<IMenuPreviewKingdomPlacementValidator>().To<MenuPreviewKingdomPlacementValidator>().AsSingle();
            container.Bind<IMenuPreviewKingdomCandidatePicker>().To<MenuPreviewKingdomCandidatePicker>().AsSingle();
            container.Bind<IMenuPreviewKingdomPlacementWriter>().To<MenuPreviewKingdomPlacementWriter>().AsSingle();
            container.Bind<IMenuPreviewKingdomPlacementOrchestrator>().To<MenuPreviewKingdomPlacementOrchestrator>().AsSingle();
            container.Bind<IMenuWorldPreviewKingdomPlacementService>().To<MenuWorldPreviewKingdomPlacementService>().AsSingle();
        }
    }
}
