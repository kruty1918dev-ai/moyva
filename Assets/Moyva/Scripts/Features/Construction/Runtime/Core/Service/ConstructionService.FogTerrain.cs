using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool IsBlockedByFog(Vector2Int position)
        {
            try
            {
                if (_placementRulesProvider != null
                    && (!_placementRulesProvider.EnableFogRules || !_placementRulesProvider.RequireVisibleFogTile))
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedByFog({position}): profile disabled fog rule");
                    return false;
                }

                if (_fogOfWarService == null)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedByFog({position}): _fogOfWarService == null, fog-перевірка відключена");
                    return false;
                }

                var fogState = _fogOfWarService.GetFogState(position);
                bool isBlocked = fogState != FogStateType.Visible;

                if (VerboseLogs && isBlocked)
                    Debug.Log($"[Construction] IsBlockedByFog({position}): BLOCKED (fogState={fogState})");

                return isBlocked;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в IsBlockedByFog({position}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private void ApplyBuildingFogReveal(string buildingId, Vector2Int position)
        {
            if (_fogOfWarService == null || _buildingRegistry == null)
                return;

            var definition = _buildingRegistry.GetById(buildingId);
            if (!BuildingDefinitionCapabilities.TryGetFogReveal(definition, out var fogReveal))
                return;

            int radius = Mathf.Max(0, fogReveal.RevealRadius);
            if (radius <= 0)
            {
                _fogOfWarService.UnregisterUnit(GetBuildingFogVisionAreaId(position));
                return;
            }

            string areaId = GetBuildingFogVisionAreaId(position);
            if (fogReveal.RevealWhileActive)
            {
                _fogOfWarService.RegisterFixedVisionArea(areaId, position, radius, fogReveal.Shape);
                return;
            }

            _fogOfWarService.UnregisterUnit(areaId);

            if (fogReveal.RevealOnBuilt)
                _fogOfWarService.RevealArea(position, radius, fogReveal.Shape, keepVisible: false, areaId);
        }

        private static string GetBuildingFogVisionAreaId(Vector2Int position)
            => $"building:{position.x}:{position.y}";

        private bool IsBlockedByTerrain(Vector2Int position, out string reason)
        {
            return ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                position,
                _gridService,
                _generatedTerrainLevelQuery,
                _tileSettings,
                _placementRulesProvider,
                _worldDefaults,
                out reason);
        }

        private string GetTileId(Vector2Int position)
        {
            return _gridService != null && _gridService.TryGetTileData(position, out string tileId)
                ? tileId
                : null;
        }
    }
}
