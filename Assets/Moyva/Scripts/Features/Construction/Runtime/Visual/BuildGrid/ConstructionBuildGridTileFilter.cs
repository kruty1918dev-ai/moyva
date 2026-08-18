using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
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
        private readonly IFogStateReader _fogStateReader;

        [Inject]
        public ConstructionBuildGridTileFilter(
            IGridService gridService,
            IConstructionPlacementQuery placementQuery,
            BuildModeGridStateController stateController,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null,
            [InjectOptional] IFogStateReader fogStateReader = null)
        {
            _gridService = gridService;
            _placementQuery = placementQuery;
            _stateController = stateController;
            _settingsProvider = settingsProvider;
            _fogStateReader = fogStateReader;
        }

        public bool ShouldRender(Vector2Int position)
            => ResolveVisualState(position) != ConstructionBuildGridTileVisualState.Missing;

        public ConstructionBuildGridTileVisualState ResolveVisualState(Vector2Int position)
        {
            if (_gridService == null || !_gridService.TryGetTileData(position, out _))
                return ConstructionBuildGridTileVisualState.Missing;

            /*
             * Construction requires the tile to be currently Visible.
             * Reject hidden cells before mode dispatch and before the
             * expensive placement query.
             */
            if (_fogStateReader != null
                && !_fogStateReader.IsVisible(position))
            {
                return ConstructionBuildGridTileVisualState.Missing;
            }

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
                    return ResolvePlacementVisualState(
                        position,
                        _stateController.SelectedBuildingId);

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
                || (_fogStateReader != null
                    && !_fogStateReader.IsVisible(position))
                || _placementQuery == null
                || string.IsNullOrWhiteSpace(buildingId))
            {
                return false;
            }

            // The selected grid is the placement contract shown to the player.
            // It must include pending previews exactly like pointer-click validation,
            // otherwise a green cell can still reject the click.
            var request = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                includeResources: false,
                includePendingPlacements: true,
                attemptSource:
                    ConstructionPlacementAttemptSource.GridTileFilter,
                allowUniquePreviewRelocation: true);

            /*
             * CanPreview ignores resource affordability by contract.
             * Avoid the economy query on this boolean render path.
             */
            return _placementQuery
                .EvaluatePlacement(request)
                .CanPreview;
        }

        private ConstructionBuildGridTileVisualState ResolvePlacementVisualState(
            Vector2Int position,
            string buildingId)
        {
            if (_placementQuery == null || string.IsNullOrWhiteSpace(buildingId))
                return ConstructionBuildGridTileVisualState.General;

            var request = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                includeResources: true,
                includePendingPlacements: true,
                attemptSource:
                    ConstructionPlacementAttemptSource.GridTileFilter,
                allowUniquePreviewRelocation: true);
            ConstructionPlacementQueryResult result =
                _placementQuery.EvaluatePlacement(request);

            if (!result.CanSelect)
                return ConstructionBuildGridTileVisualState.General;
            if (!result.SpatialValid)
                return ConstructionBuildGridTileVisualState.Invalid;
            return result.ResourcesValid
                ? ConstructionBuildGridTileVisualState.Valid
                : ConstructionBuildGridTileVisualState.Unaffordable;
        }

        private bool UsesUnfilteredChunkSurface()
            => _settingsProvider?.BuildGridRenderMode == ConstructionBuildGridRenderMode.ChunkSurfacePlane
               && !_settingsProvider.BuildGridSurfacePlaneUseBuildableFilter;
    }
}
