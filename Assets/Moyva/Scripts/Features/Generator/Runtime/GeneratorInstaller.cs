using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public class GeneratorInstaller : MonoInstaller
    {
        [SerializeField] private TileRegistrySO _registry;

        public override void InstallBindings()
        {
            IMapInstantiator tileGenerator = new MapVisualInstantiator(
                _registry, 
                Container.Resolve<IGridService>(), 
                Container.Resolve<IMapDataGenerator>(), 
                Container);

            Container.Bind<IMapInstantiator>().FromInstance(tileGenerator).AsSingle();

            _ = tileGenerator.BuildWorldAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                    Debug.LogError(t.Exception);
            });
        }
    }
}
