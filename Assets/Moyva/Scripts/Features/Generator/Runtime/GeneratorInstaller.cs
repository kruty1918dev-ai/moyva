using Kruty1918.Moyva.Tiles;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    public class GeneratorInstaller : MonoInstaller
    {
        [SerializeField] private TileView _tileViewPrefab;

        public override void InstallBindings()
        {
            TileGenerator tileGenerator = new TileGenerator(_tileViewPrefab, Container.Resolve<IGridService>());

            Container.Bind<TileGenerator>().FromInstance(tileGenerator).AsSingle();

            tileGenerator.GenerateTiles();
        }
    }
}
