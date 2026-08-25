using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Evaluates placement constraints that depend only on world environment:
    /// fog visibility, terrain buildability and tile identity.
    ///
    /// ConstructionService owns placement orchestration; this plain C#
    /// component owns environment-specific rules.
    /// </summary>
    internal sealed class ConstructionPlacementEnvironmentRules
    {
        private readonly IFogOfWarService _fogOfWarService;
        private readonly IGridService _gridService;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly ITileSettingsService _tileSettings;
        private readonly IConstructionPlacementRulesProvider _placementRulesProvider;

        public ConstructionPlacementEnvironmentRules(
            IFogOfWarService fogOfWarService,
            IGridService gridService,
            IGeneratedTerrainLevelQuery generatedTerrainLevelQuery,
            ITileSettingsService tileSettings,
            IConstructionPlacementRulesProvider placementRulesProvider)
        {
            _fogOfWarService = fogOfWarService;
            _gridService = gridService;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _tileSettings = tileSettings;
            _placementRulesProvider = placementRulesProvider;
        }

        public bool IsBlockedByFog(Vector2Int position)
        {
            try
            {
                if (_placementRulesProvider != null
                    && (!_placementRulesProvider.EnableFogRules
                        || !_placementRulesProvider.RequireVisibleFogTile))
                    return false;

                if (_fogOfWarService == null)
                    return false;

                FogStateType fogState =
                    _fogOfWarService.GetFogState(position);
                return fogState != FogStateType.Visible;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[Construction] ПОМИЛКА в IsBlockedByFog({position}): " +
                    $"{ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public bool IsBlockedByTerrain(
            Vector2Int position,
            out string reason)
        {
            return ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                position,
                _gridService,
                _generatedTerrainLevelQuery,
                _tileSettings,
                _placementRulesProvider,
                out reason);
        }

        public string GetTileId(Vector2Int position)
        {
            return _gridService != null
                   && _gridService.TryGetTileData(
                       position,
                       out string tileId)
                ? tileId
                : null;
        }
    }
}
