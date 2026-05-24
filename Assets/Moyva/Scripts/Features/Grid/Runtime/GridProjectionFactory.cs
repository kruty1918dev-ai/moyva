using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public static class GridProjectionFactory
    {
        public static IGridProjection Create(MoyvaProjectSettingsSO settings)
        {
            settings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
            settings.Normalize();

            return settings.DefaultProjectionMode switch
            {
                GridProjectionMode.Orthographic3D => new Orthographic3DGridProjection(settings),
                GridProjectionMode.Isometric2D => new IsometricGridProjection(settings),
                GridProjectionMode.Isometric3DPreview => new IsometricGridProjection(settings),
                GridProjectionMode.HexPointy2D => new HexAxialGridProjection(settings),
                GridProjectionMode.HexFlat2D => new HexAxialGridProjection(settings),
                _ => new OrthogonalGridProjection(settings),
            };
        }
    }
}