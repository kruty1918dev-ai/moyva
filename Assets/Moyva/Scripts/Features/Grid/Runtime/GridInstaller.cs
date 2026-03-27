using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Grid
{
    public class GridInstaller : MonoInstaller
    {
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float tileSize = 1f;

        public override void InstallBindings()
        {
            Container.Bind<IGridService>().To<GridService>().AsSingle()
                .WithArguments(gridWidth, gridHeight, tileSize);
        }
    }
}
