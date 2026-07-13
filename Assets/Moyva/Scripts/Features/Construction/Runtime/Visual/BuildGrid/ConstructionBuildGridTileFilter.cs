using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Maps the explicit grid mode and the authoritative placement query to a visual
    /// state. It does not implement any gameplay placement rules itself.
    /// </summary>
    internal sealed class ConstructionBuildGridTileFilter : IConstructionBuildGridTileFilter
    {
        private readonly IGridService _gridService;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly BuildModeGridStateController _stateController;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;

        [Inject]
        public ConstructionBuildGridTileFilter(
            IGridService gridService,
            IConstructionPlacementQuery placementQuery,
            BuildModeGridStateController stateController,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _gridService = gridService;
            _placementQuery = placementQuery;
            _stateController = stateController;
            _settingsProvider = settingsProvider;
        }

        public bool ShouldRender(Vector2Int position)
            => ResolveVisualState(position) != ConstructionBuildGridTileVisualState.Missing;

        public ConstructionBuildGridTileVisualState ResolveVisualState(Vector2Int position)
        {
            if (_gridService == null || !_gridService.TryGetTileData(position, out _))
                return ConstructionBuildGridTileVisualState.Missing;

            // An unfiltered chunk surface is a neutral grid. Its mask is independent
            // from selection and placement state, so it can be prepared off-screen.
            if (UsesUnfilteredChunkSurface())
                return ConstructionBuildGridTileVisualState.General;

            switch (_stateController.State)
            {
                case BuildModeGridState.Hidden:
                    return ConstructionBuildGridTileVisualState.Missing;

                case BuildModeGridState.General:
                    return ConstructionBuildGridTileVisualState.General;

                case BuildModeGridState.BuildingSelected:
                    return ShouldRenderForPlacement(position, _stateController.SelectedBuildingId)
                        ? ConstructionBuildGridTileVisualState.Valid
                        : ConstructionBuildGridTileVisualState.Invalid;

                default:
                    return ConstructionBuildGridTileVisualState.Missing;
            }
        }

        public bool ShouldRenderForPlacement(
            Vector2Int position,
            string buildingId,
            Vector2Int? ignoredPendingPosition = null)
        {
            if (_gridService == null
                || !_gridService.TryGetTileData(position, out _)
                || _placementQuery == null
                || string.IsNullOrWhiteSpace(buildingId))
            {
                return false;
            }

            var request = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                includeResources: true);
            return _placementQuery.EvaluatePlacement(request).IsValid;
        }

        private bool UsesUnfilteredChunkSurface()
            => _settingsProvider?.BuildGridRenderMode == ConstructionBuildGridRenderMode.ChunkSurfacePlane
               && !_settingsProvider.BuildGridSurfacePlaneUseBuildableFilter;
    }
}
