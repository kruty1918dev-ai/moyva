using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPreviewVisualSignalHandler : IConstructionPreviewVisualSignalHandler
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IConstructionPreviewVisualService _previewVisuals;
        private readonly IConstructionPlacedVisualService _placedVisuals;
        private readonly IConstructionWallVisualRefreshService _wallVisuals;
        private readonly IConstructionInfluenceRadiusVisualService _radiusVisuals;
        private readonly IConstructionBlockedFlashService _blockedFlashService;
        private readonly int _townHallBuildRadius;

        [Inject]
        public ConstructionPreviewVisualSignalHandler(
            IBuildingRegistry buildingRegistry,
            LazyInject<IConstructionService> constructionService,
            IWallTopologyService wallTopologyService,
            IConstructionPreviewVisualService previewVisuals,
            IConstructionPlacedVisualService placedVisuals,
            IConstructionWallVisualRefreshService wallVisuals,
            IConstructionInfluenceRadiusVisualService radiusVisuals,
            IConstructionBlockedFlashService blockedFlashService,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius)
        {
            _buildingRegistry = buildingRegistry;
            _constructionService = constructionService;
            _wallTopologyService = wallTopologyService;
            _previewVisuals = previewVisuals;
            _placedVisuals = placedVisuals;
            _wallVisuals = wallVisuals;
            _radiusVisuals = radiusVisuals;
            _blockedFlashService = blockedFlashService;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
        }

        public void Handle(BuildingPreviewChangedSignal signal)
        {
            if (_constructionService.Value.IsDemolishMode)
            {
                HandleDemolitionPreview(signal);
                return;
            }

            if (signal.PreviewState == BuildingPreviewState.None)
            {
                _previewVisuals.Remove(signal.Position);
                _radiusVisuals.HidePreview();
                RefreshWallPreviewIfNeeded(signal);
                return;
            }

            if (signal.PreviewState == BuildingPreviewState.Blocked)
            {
                FlashBlocked(signal.Position);
                return;
            }

            if (TryGetDefinition(signal.BuildingId, out BuildingDefinition def))
            {
                _previewVisuals.Show(signal, def);
                ShowOrHidePreviewRadius(def, signal.Position);
                RefreshWallPreviewIfNeeded(signal);
            }
        }

        public void Handle(BuildingCancelledSignal signal)
        {
            _previewVisuals.Clear();
            _placedVisuals.ClearDemolitionPreviewStyles();
            _radiusVisuals.HidePreview();
        }

        private void HandleDemolitionPreview(BuildingPreviewChangedSignal signal)
        {
            if (signal.PreviewState == BuildingPreviewState.None)
                _placedVisuals.RestoreDemolitionPreview(signal.Position);
            else if (signal.PreviewState == BuildingPreviewState.Blocked)
                FlashBlocked(signal.Position);
            else
                _placedVisuals.MarkDemolitionPreview(signal.Position);
        }

        private void FlashBlocked(Vector2Int position)
        {
            if (_previewVisuals.TryGet(position, out GameObject preview))
                _blockedFlashService.Flash(preview, isGhostPreview: true);
            else if (_placedVisuals.TryGetPlacedVisual(position, out GameObject placed))
                _blockedFlashService.Flash(placed, isGhostPreview: false);
        }

        private void RefreshWallPreviewIfNeeded(BuildingPreviewChangedSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(signal.BuildingId) && _wallTopologyService.IsWallOrGate(signal.BuildingId))
                _wallVisuals.RefreshPreviewNeighborhood(signal.Position, signal.BuildingId);
        }

        private void ShowPreviewRadius(string buildingId, Vector2Int position)
        {
            if (TryGetDefinition(buildingId, out BuildingDefinition def))
                ShowOrHidePreviewRadius(def, position);
        }

        private void ShowOrHidePreviewRadius(BuildingDefinition def, Vector2Int position)
        {
            if (HasInfluenceRadius(def))
                _radiusVisuals.ShowPreview(position, ResolveInfluenceRadius(def));
            else
                _radiusVisuals.HidePreview();
        }

        private bool TryGetDefinition(string buildingId, out BuildingDefinition def)
        {
            def = string.IsNullOrWhiteSpace(buildingId) ? null : _buildingRegistry.GetById(buildingId);
            return def != null && def.Prefab != null;
        }

        private bool HasInfluenceRadius(BuildingDefinition def)
            => def != null && (BuildingDefinitionCapabilities.IsTownHall(def) || BuildingDefinitionCapabilities.IsCastle(def));

        private int ResolveInfluenceRadius(BuildingDefinition def)
            => BuildingDefinitionCapabilities.GetInfluenceRadius(def, _townHallBuildRadius);
    }
}
