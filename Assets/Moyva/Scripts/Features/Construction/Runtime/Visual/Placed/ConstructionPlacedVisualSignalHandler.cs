using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>Обробник сигналів для візуалів розміщених будівель: ставлення, знесення, робочий стан, власник, виділення, режим гри.</summary>
    internal sealed class ConstructionPlacedVisualSignalHandler {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly ConstructionPreviewVisualService _previewVisuals;
        private readonly ConstructionPlacedVisualService _placedVisuals;
        private readonly ConstructionWallVisualRefreshService _wallVisuals;
        private readonly ConstructionInfluenceRadiusVisualService _radiusVisuals;
        private readonly ConstructionBuildGridOverlayService _buildGridOverlay;
        private readonly IConstructionLifecycle _constructionLifecycle;
        private readonly ConstructionOwnerPaletteResolver _paletteResolver;
        private readonly int _townHallBuildRadius;

        /// <summary>Створює обробника із сервісами візуалів будівель.</summary>
        [Inject]
        public ConstructionPlacedVisualSignalHandler(
            IBuildingRegistry buildingRegistry,
            IWallTopologyService wallTopologyService,
            ConstructionPreviewVisualService previewVisuals,
            ConstructionPlacedVisualService placedVisuals,
            ConstructionWallVisualRefreshService wallVisuals,
            ConstructionInfluenceRadiusVisualService radiusVisuals,
            ConstructionBuildGridOverlayService buildGridOverlay,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius,
            [InjectOptional] IConstructionLifecycle constructionLifecycle = null,
            [InjectOptional] ConstructionOwnerPaletteResolver paletteResolver = null)
        {
            _buildingRegistry = buildingRegistry;
            _wallTopologyService = wallTopologyService;
            _previewVisuals = previewVisuals;
            _placedVisuals = placedVisuals;
            _wallVisuals = wallVisuals;
            _radiusVisuals = radiusVisuals;
            _buildGridOverlay = buildGridOverlay;
            _constructionLifecycle = constructionLifecycle;
            _paletteResolver = paletteResolver;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
        }

        /// <summary>Обробляє сигнал розміщення будівлі.</summary>
        public void Handle(BuildingPlacedSignal signal)
        {
            if (signal.HasRelocationSource
                && signal.RelocationSourcePosition != signal.Position)
            {
                _placedVisuals.Remove(
                    signal.RelocationSourcePosition);
            }

            _previewVisuals.TryRelease(
                signal.Position,
                out GameObject previewVisual);
            _radiusVisuals.HidePreview();

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
            {
                if (previewVisual != null)
                    Object.Destroy(previewVisual);

                _wallVisuals.RefreshPlacedNeighborhood(
                    signal.Position);
                return;
            }

            BuildingDefinition def =
                _buildingRegistry.GetById(signal.BuildingId);
            if (def?.Prefab != null)
            {
                string ownerId = signal.OwnerId;
                bool showConstructionVisual =
                    ShouldUseConstructionVisual(
                        def,
                        _constructionLifecycle,
                        signal.Position);
                GameObject placedPrefab = showConstructionVisual
                    ? def.ResolveConstructionPrefab()
                    : ResolvePlacedPrefab(def, ownerId);
                GameObject sourceVisual =
                    ResolveReusablePlacedSource(
                        previewVisual,
                        def,
                        placedPrefab,
                        ownerId);
                _placedVisuals.Replace(
                    signal.Position,
                    signal.BuildingId,
                    placedPrefab,
                    ConstructionRotationUtility.ToWorldRotation(
                        ConstructionRotationUtility.Normalize(
                            signal.RotationQuarterTurns)),
                    def.ResolveVisualYOffset(),
                    sourceVisual,
                    def.Presentation,
                    ownerId);

                if (showConstructionVisual)
                    _placedVisuals.MarkUnderConstruction(signal.Position);
            }
            else if (previewVisual != null)
            {
                Object.Destroy(previewVisual);
            }

        }

        private GameObject ResolvePlacedPrefab(
            BuildingDefinition def,
            string ownerId)
            => _paletteResolver != null
                ? _paletteResolver.ResolvePlacedPrefab(def, ownerId)
                : def?.Prefab;

        private GameObject ResolveReusablePlacedSource(
            GameObject previewVisual,
            BuildingDefinition def,
            GameObject placedPrefab,
            string ownerId)
        {
            if (previewVisual == null || def == null)
                return null;

            GameObject previewPrefab = _paletteResolver != null
                ? _paletteResolver.ResolvePreviewPrefab(def, ownerId)
                : def.ResolvePreviewPrefab();
            if (previewPrefab != null
                && previewPrefab != placedPrefab)
            {
                Object.Destroy(previewVisual);
                return null;
            }

            return previewVisual;
        }

        internal static bool ShouldUseConstructionVisual(
            BuildingDefinition definition,
            IConstructionLifecycle lifecycle,
            Vector2Int position)
        {
            if (definition == null || definition.BuildTurns <= 0)
                return false;

            return lifecycle == null
                || !lifecycle.IsOperational(position);
        }

        /// <summary>Обробляє сигнал знесення будівлі.</summary>
        public void Handle(BuildingDemolishedSignal signal)
        {
            _placedVisuals.Remove(signal.Position);

            if (_placedVisuals.ClearSelectionIfMatches(signal.Position))
                _radiusVisuals.HideInspection();

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
                _wallVisuals.RefreshPlacedNeighborhood(signal.Position);
        }

        /// <summary>Обробляє сигнал переходу будівлі в робочий стан.</summary>
        public void Handle(BuildingOperationalSignal signal)
        {
            BuildingDefinition def =
                _buildingRegistry.GetById(signal.BuildingId);
            if (def?.Prefab != null)
            {
                if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
                {
                    _wallVisuals.RefreshPlacedNeighborhood(signal.Position);
                }
                else
                {
                    string ownerId = ResolveStoredOrSignalOwner(
                        signal.Position,
                        signal.OwnerId);
                    _placedVisuals.ReplaceWithStoredPose(
                        signal.Position,
                        ResolvePlacedPrefab(def, ownerId),
                        def.ResolveVisualYOffset(),
                        def.Presentation,
                        ownerId);
                }
            }

            _placedVisuals.MarkOperational(signal.Position);
        }

        /// <summary>Обробляє сигнал передачі власника будівлі.</summary>
        public void Handle(BuildingOwnershipTransferredSignal signal)
        {
            _placedVisuals.SetOwner(signal.Position, signal.NewOwnerId);

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
                return;

            BuildingDefinition def =
                _buildingRegistry.GetById(signal.BuildingId);
            if (!HasOwnerVariants(def)
                || ShouldUseConstructionVisual(
                    def,
                    _constructionLifecycle,
                    signal.Position))
            {
                // Neutral walls and scaffolding stay as-is; the stored owner
                // still drives the variant swap when the building completes.
                return;
            }

            _placedVisuals.ReplaceWithStoredPose(
                signal.Position,
                ResolvePlacedPrefab(def, signal.NewOwnerId),
                def.ResolveVisualYOffset(),
                def.Presentation,
                signal.NewOwnerId);
        }

        private string ResolveStoredOrSignalOwner(
            Vector2Int position,
            string signalOwnerId)
        {
            if (!string.IsNullOrWhiteSpace(signalOwnerId))
                return signalOwnerId;

            return _placedVisuals.TryGetOwner(position, out string stored)
                ? stored
                : null;
        }

        private static bool HasOwnerVariants(BuildingDefinition def)
            => def?.Presentation?.Variants?.PrefabVariants?.Count > 0;

        /// <summary>Обробляє зміну виділення у світі.</summary>
        public void Handle(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind != WorldInfoSelectionKind.Building || string.IsNullOrWhiteSpace(signal.ObjectId))
            {
                _placedVisuals.ClearSelection();
                _radiusVisuals.HideInspection();
                return;
            }

            _placedVisuals.Select(signal.Position);
            BuildingDefinition def = _buildingRegistry.GetById(signal.ObjectId);
            if (HasInfluenceRadius(def))
                _radiusVisuals.ShowInspection(signal.Position, ResolveInfluenceRadius(def));
            else
                _radiusVisuals.HideInspection();
        }

        /// <summary>Обробляє зміну режиму гри.</summary>
        public void Handle(GameModeChangedSignal signal)
        {
            bool active = signal.NewMode == GameModeType.Construction;
            _buildGridOverlay.SetConstructionModeActive(active);
            if (!active)
                _previewVisuals.Clear();
        }

        private bool HasInfluenceRadius(BuildingDefinition def)
            => BuildingDefinitionCapabilities.IsSettlementCenter(def);

        private int ResolveInfluenceRadius(BuildingDefinition def)
            => BuildingDefinitionCapabilities.GetInfluenceRadius(def, _townHallBuildRadius);
    }
}
