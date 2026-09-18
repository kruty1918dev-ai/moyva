using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public static class ConstructionPreviewBindings
    {
        // Isolated, unpriced demonstration world. Placement/footprints/ownership
        // still pass through the same construction authority as gameplay.
        public static void Install(DiContainer container, IBuildingRegistry buildings)
        {
            container.Bind<IBuildingRegistry>().FromInstance(buildings);
            container.BindInterfacesAndSelfTo<ConstructionService>().FromMethod(context =>
                new ConstructionService(context.Container.Resolve<IObjectsMapService>(), buildings,
                    context.Container.Resolve<SignalBus>(), 0, 0, null, null, null, null,
                    context.Container.Resolve<IGridService>(), null,
                    context.Container.Resolve<ITileSettingsService>())).AsSingle();
        }
    }
}
