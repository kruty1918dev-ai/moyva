using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Applies fog-of-war side effects produced by committed buildings.
    ///
    /// Placement visibility checks belong to ConstructionPlacementEnvironmentRules.
    /// This class only owns post-build reveal/vision effects.
    /// </summary>
    internal sealed class ConstructionBuildingFogEffects
    {
        private readonly IFogOfWarService _fogOfWarService;
        private readonly IBuildingRegistry _buildingRegistry;

        public ConstructionBuildingFogEffects(
            IFogOfWarService fogOfWarService,
            IBuildingRegistry buildingRegistry)
        {
            _fogOfWarService = fogOfWarService;
            _buildingRegistry = buildingRegistry;
        }

        public void Apply(
            string buildingId,
            Vector2Int position)
        {
            if (_fogOfWarService == null
                || _buildingRegistry == null)
            {
                return;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);

            bool hasFogModule =
                BuildingDefinitionCapabilities.TryGetFogReveal(
                    definition,
                    out FogRevealBuildingModule fogReveal);

            int defenseBonus =
                BuildingDefinitionCapabilities
                    .GetDefenseVisionRevealBonus(definition);

            if (!hasFogModule && defenseBonus <= 0)
                return;

            int baseRadius = hasFogModule
                ? Mathf.Max(0, fogReveal.RevealRadius)
                : 0;

            int radius =
                Mathf.Max(0, baseRadius + defenseBonus);

            string areaId =
                GetBuildingFogVisionAreaId(position);

            if (radius <= 0)
            {
                _fogOfWarService.UnregisterUnit(areaId);
                return;
            }

            if (!hasFogModule)
            {
                _fogOfWarService.RegisterFixedVisionArea(
                    areaId,
                    position,
                    radius,
                    FogRevealShape.PixelCircle);
                return;
            }

            if (fogReveal.RevealWhileActive)
            {
                _fogOfWarService.RegisterFixedVisionArea(
                    areaId,
                    position,
                    radius,
                    fogReveal.Shape);
                return;
            }

            _fogOfWarService.UnregisterUnit(areaId);

            if (fogReveal.RevealOnBuilt)
            {
                _fogOfWarService.RevealArea(
                    position,
                    radius,
                    fogReveal.Shape,
                    keepVisible: false,
                    areaId);
            }
        }

        private static string GetBuildingFogVisionAreaId(
            Vector2Int position)
        {
            return $"building:{position.x}:{position.y}";
        }
    }
}
