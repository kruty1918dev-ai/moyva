using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    public class GeneratorInstaller : MonoInstaller
    {
        [SerializeField] private TileRegistrySO _registry;

        public override void InstallBindings()
        {
            TileGenerator tileGenerator = new TileGenerator(_registry, Container.Resolve<IGridService>(), Container);

            Container.Bind<TileGenerator>().FromInstance(tileGenerator).AsSingle();

            tileGenerator.GenerateTiles();
        }
    }
}
