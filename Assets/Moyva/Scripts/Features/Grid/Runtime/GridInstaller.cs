using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public class GridInstaller : MonoInstaller
    {
        [SerializeField] private TileRegistrySO tileRegistry;
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float tileSize = 1f;

        public override void InstallBindings()
        {
            Container.BindInstance(tileRegistry).AsSingle();
            Container.Bind<IGridService>().To<GridService>().AsSingle()
                .WithArguments(gridWidth, gridHeight, tileSize);
            Container.Bind<ITileSettingsService>().To<TileSettingsService>().AsSingle();
        }
    }
}