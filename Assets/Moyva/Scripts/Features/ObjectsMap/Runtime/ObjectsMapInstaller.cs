using Kruty1918.Moyva.MapChunks.Runtime;
using Kruty1918.Moyva.ObjectsMap.API;
using Zenject;

namespace Kruty1918.Moyva.ObjectsMap.Runtime
{
    public class ObjectsMapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            MapChunkFeatureBindings.Install(Container);

            Container.BindInterfacesAndSelfTo<ChunkedObjectsMapService>()
                .AsSingle()
                .NonLazy();

            // Канонічна object-map реалізація має ініціалізуватись першою,
            // щоб підписатись на сигнали до решти сервісів.
            Container.BindExecutionOrder<ChunkedObjectsMapService>(-10);
        }
    }
}
