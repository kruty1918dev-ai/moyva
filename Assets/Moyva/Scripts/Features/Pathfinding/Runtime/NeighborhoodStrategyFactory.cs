using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Pathfinding.API;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public static class NeighborhoodStrategyFactory
    {
        public static INeighborhoodStrategy Create(MoyvaProjectSettingsSO settings)
        {
            settings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
            settings.Normalize();

            return settings.ResolveNeighborhoodMode() switch
            {
                GridNeighborhoodMode.HexAxial6 => new HexAxialNeighborhoodStrategy(),
                GridNeighborhoodMode.VonNeumann4 => new VonNeumannNeighborhoodStrategy(),
                _ => new MooreNeighborhoodStrategy(),
            };
        }
    }
}