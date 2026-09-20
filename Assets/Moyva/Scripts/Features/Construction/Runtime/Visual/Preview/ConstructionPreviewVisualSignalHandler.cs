using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>Обробник сигналів для preview-візуалів будівництва: показ, рух, drag, hover сітки, скасування.</summary>
    internal sealed class ConstructionPreviewVisualSignalHandler {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly LazyInject<IConstructionSessionCommands> _constructionService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly ConstructionPreviewVisualService _previewVisuals;
        private readonly ConstructionPlacedVisualService _placedVisuals;
        private readonly ConstructionWallVisualRefreshService _wallVisuals;
        private readonly ConstructionInfluenceRadiusVisualService _radiusVisuals;
        private readonly ConstructionBlockedFlashService _blockedFlashService;
        private readonly int _townHallBuildRadius;

        /// <summary>Створює обробника із сервісом preview-візуалів.</summary>
        [Inject]
        public ConstructionPreviewVisualSignalHandler(
            IBuildingRegistry buildingRegistry,
            LazyInject<IConstructionSessionCommands> constructionService,
            IWallTopologyService wallTopologyService,
            ConstructionPreviewVisualService previewVisuals,
            ConstructionPlacedVisualService placedVisuals,
            ConstructionWallVisualRefreshService wallVisuals,
            ConstructionInfluenceRadiusVisualService radiusVisuals,
            ConstructionBlockedFlashService blockedFlashService,
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

        /// <summary>Обробляє сигнал зміни preview будівлі.</summary>
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
                bool existed = _previewVisuals.TryGet(signal.Position, out GameObject existing);
                Quaternion previous = existed && existing != null ? existing.transform.rotation : Quaternion.identity;
                GameObject preview = _previewVisuals.Show(
                    signal,
                    def,
                    _constructionService.Value.GetActiveOwner());
                ApplyRotation(
                    preview,
                    signal.RotationQuarterTurns,
                    def.Presentation);
                if (existed && preview != null)
                {
                    var rotation = preview.GetComponent<ConstructionPreviewRotation>()
                        ?? preview.AddComponent<ConstructionPreviewRotation>();
                    rotation.Apply(previous, preview.transform.rotation);
                }
                ShowOrHidePreviewRadius(def, signal.Position);
                RefreshWallPreviewIfNeeded(signal);
            }
        }

        /// <summary>Обробляє сигнал переміщення preview.</summary>
        public void Handle(BuildingPreviewMovedSignal signal)
        {
            if (TryGetDefinition(signal.BuildingId, out BuildingDefinition def))
            {
                Quaternion baseRotation = ResolveBaseRotation(
                    signal.RotationQuarterTurns);
                _previewVisuals.TryMove(
                    signal.FromPosition,
                    signal.ToPosition,
                    signal.BuildingId,
                    def.ResolveVisualYOffset(),
                    def.Presentation,
                    baseRotation);
                if (_previewVisuals.TryGet(
                        signal.ToPosition,
                        out GameObject preview))
                {
                    ApplyRotation(
                        preview,
                        signal.RotationQuarterTurns,
                        def.Presentation);
                }
            }
        }

        private static void ApplyRotation(
            GameObject visual,
            int rotationQuarterTurns,
            EntityPresentationConfig presentation)
        {
            if (visual == null)
                return;

            visual.transform.rotation =
                EntityPresentationApplier.ResolveRotation(
                    ResolveBaseRotation(rotationQuarterTurns),
                    presentation);
        }

        private static Quaternion ResolveBaseRotation(int rotationQuarterTurns)
            => ConstructionRotationUtility.ToWorldRotation(
                ConstructionRotationUtility.Normalize(
                    rotationQuarterTurns));

        /// <summary>Обробляє сигнал drag-візуала preview.</summary>
        public void Handle(BuildingPreviewDragVisualSignal signal)
        {
            if (TryGetDefinition(signal.BuildingId, out BuildingDefinition def))
                _previewVisuals.MoveDragVisual(
                    signal.Position,
                    signal.BuildingId,
                    signal.WorldPosition,
                    signal.SnapToGrid,
                    signal.HasSnapTarget,
                    signal.SnapTargetPosition,
                    signal.IsSnapTargetValid,
                    def.ResolveVisualYOffset(),
                    def.Presentation,
                    ResolveBaseRotation(signal.RotationQuarterTurns));
        }

        /// <summary>Обробляє зміну hover-клітинки сітки будівництва.</summary>
        public void Handle(BuildGridHoverChangedSignal signal)
        {
            if (signal.HasTile)
                _previewVisuals.ShowGridHover(signal);
            else
                _previewVisuals.ClearGridHover();
        }

        /// <summary>Обробляє скасування будівництва.</summary>
        public void Handle(BuildingCancelledSignal signal)
        {
            _previewVisuals.Clear();
            _previewVisuals.ClearGridHover();
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
            return def != null && def.ResolvePreviewPrefab() != null;
        }

        private bool HasInfluenceRadius(BuildingDefinition def)
            => BuildingDefinitionCapabilities.IsSettlementCenter(def);

        private int ResolveInfluenceRadius(BuildingDefinition def)
            => BuildingDefinitionCapabilities.GetInfluenceRadius(def, _townHallBuildRadius);
    }
}
