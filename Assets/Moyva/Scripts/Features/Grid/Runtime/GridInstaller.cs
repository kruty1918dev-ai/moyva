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

        public override void InstallBindings()
        {
            int resolvedWidth = gridWidth;
            int resolvedHeight = gridHeight;
            if (TryGetLaunchWorldDimensions(out int launchWidth, out int launchHeight))
            {
                resolvedWidth = launchWidth;
                resolvedHeight = launchHeight;
            }

            Container.BindInstance(tileRegistry).AsSingle();
            Container.Bind<IGridService>().To<GridService>().AsSingle()
                .WithArguments(resolvedWidth, resolvedHeight);
            Container.Bind<ITileSettingsService>().To<TileSettingsService>().AsSingle();
        }

        private static bool TryGetLaunchWorldDimensions(out int width, out int height)
        {
            width = 0;
            height = 0;

            var contextType = System.Type.GetType("Kruty1918.Moyva.SaveSystem.GameLaunchContext, Kruty1918.Moyva.SaveSystem");
            var method = contextType?.GetMethod("TryGetWorldDimensions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
                return false;

            object[] args = { width, height };
            if (!(method.Invoke(null, args) is bool result) || !result)
                return false;

            width = (int)args[0];
            height = (int)args[1];
            return width > 0 && height > 0;
        }
    }
}