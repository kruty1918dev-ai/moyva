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
        private const int FallbackBuildingVisionRange = 3;

        private readonly IFogOfWarService _fogOfWarService;
        private readonly IFogOwnerVisionSourceRegistry _ownerVision;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly FogOfWarSettings _fogSettings;

        public ConstructionBuildingFogEffects(
            IFogOfWarService fogOfWarService,
            IBuildingRegistry buildingRegistry,
            FogOfWarSettings fogSettings = null)
        {
            _fogOfWarService = fogOfWarService;
            _ownerVision = fogOfWarService as IFogOwnerVisionSourceRegistry;
            _buildingRegistry = buildingRegistry;
            _fogSettings = fogSettings;
        }

        public void ApplyOnPlaced(
            string buildingId,
            Vector2Int position,
            string ownerId = null)
        {
            if (_fogOfWarService == null
                || _buildingRegistry == null)
            {
                return;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);

            if (ShouldDeferUntilOperational(definition))
                return;

            ApplyVision(definition, position, ownerId);
        }

        public void ApplyOnOperational(
            string buildingId,
            Vector2Int position,
            string ownerId = null)
        {
            if (_fogOfWarService == null
                || _buildingRegistry == null)
            {
                return;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);

            ApplyVision(definition, position, ownerId);
        }

        private void ApplyVision(
            BuildingDefinition definition,
            Vector2Int position,
            string ownerId)
        {
            bool hasFogModule =
                BuildingDefinitionCapabilities.TryGetFogReveal(
                    definition,
                    out FogRevealBuildingModule fogReveal);

            int defenseBonus =
                BuildingDefinitionCapabilities
                    .GetDefenseVisionRevealBonus(definition);

            int baseRadius = hasFogModule
                ? Mathf.Max(0, fogReveal.RevealRadius)
                : ResolveDefaultBuildingVisionRange();

            int radius =
                Mathf.Max(0, baseRadius + defenseBonus);

            string areaId =
                GetBuildingFogVisionAreaId(position);

            if (radius <= 0)
            {
                _fogOfWarService.UnregisterUnit(areaId);
                _ownerVision?.UnregisterUnit(ownerId, areaId);
                return;
            }

            if (!hasFogModule)
            {
                _fogOfWarService.RegisterFixedVisionArea(
                    areaId,
                    position,
                    radius,
                    FogRevealShape.PixelCircle);
                _ownerVision?.RegisterFixedVisionArea(
                    ownerId,
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
                _ownerVision?.RegisterFixedVisionArea(
                    ownerId,
                    areaId,
                    position,
                    radius,
                    fogReveal.Shape);
                return;
            }

            _fogOfWarService.UnregisterUnit(areaId);
            _ownerVision?.UnregisterUnit(ownerId, areaId);

            if (fogReveal.RevealOnBuilt)
            {
                _fogOfWarService.RevealArea(
                    position,
                    radius,
                    fogReveal.Shape,
                    keepVisible: false,
                    areaId);
                _ownerVision?.RevealArea(
                    ownerId,
                    position,
                    radius,
                    fogReveal.Shape,
                    keepVisible: false,
                    areaId);
            }
        }

        public void Remove(Vector2Int position, string ownerId = null)
        {
            string areaId = GetBuildingFogVisionAreaId(position);
            _fogOfWarService?.UnregisterUnit(areaId);
            _ownerVision?.UnregisterUnit(ownerId, areaId);
        }

        private bool ShouldDeferUntilOperational(
            BuildingDefinition definition)
        {
            if (definition == null || definition.BuildTurns <= 0)
                return false;

            return BuildingDefinitionCapabilities.TryGetFogReveal(
                       definition,
                       out FogRevealBuildingModule fogReveal)
                   && fogReveal.OnlyAfterConstructionComplete;
        }

        private int ResolveDefaultBuildingVisionRange()
            => Mathf.Max(
                1,
                _fogSettings != null
                    ? _fogSettings.DefaultVisionRange
                    : FallbackBuildingVisionRange);

        private static string GetBuildingFogVisionAreaId(
            Vector2Int position)
        {
            return $"building:{position.x}:{position.y}";
        }
    }
}
