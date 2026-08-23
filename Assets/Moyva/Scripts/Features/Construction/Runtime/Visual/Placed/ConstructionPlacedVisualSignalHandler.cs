using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPlacedVisualSignalHandler {
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const double SignalHandlerLogThresholdMs = 0.5d;
        private const double SignalHandlerWarnThresholdMs = 4d;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly ConstructionPreviewVisualService _previewVisuals;
        private readonly ConstructionPlacedVisualService _placedVisuals;
        private readonly ConstructionWallVisualRefreshService _wallVisuals;
        private readonly ConstructionInfluenceRadiusVisualService _radiusVisuals;
        private readonly ConstructionBuildGridOverlayService _buildGridOverlay;
        private readonly IConstructionLifecycle _constructionLifecycle;
        private readonly int _townHallBuildRadius;

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
            [InjectOptional] IConstructionLifecycle constructionLifecycle = null)
        {
            _buildingRegistry = buildingRegistry;
            _wallTopologyService = wallTopologyService;
            _previewVisuals = previewVisuals;
            _placedVisuals = placedVisuals;
            _wallVisuals = wallVisuals;
            _radiusVisuals = radiusVisuals;
            _buildGridOverlay = buildGridOverlay;
            _constructionLifecycle = constructionLifecycle;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
        }

        public void Handle(BuildingPlacedSignal signal)
        {
            double totalStartedAt =
                Time.realtimeSinceStartupAsDouble;
            double relocationMs = 0d;
            double previewReleaseMs = 0d;
            double placedVisualMs = 0d;
            double wallRefreshMs = 0d;

            if (signal.HasRelocationSource
                && signal.RelocationSourcePosition != signal.Position)
            {
                double startedAt =
                    Time.realtimeSinceStartupAsDouble;
                _placedVisuals.Remove(
                    signal.RelocationSourcePosition);
                relocationMs +=
                    (Time.realtimeSinceStartupAsDouble - startedAt)
                    * 1000d;
            }

            double previewStartedAt =
                Time.realtimeSinceStartupAsDouble;
            _previewVisuals.TryRelease(
                signal.Position,
                out GameObject previewVisual);
            _radiusVisuals.HidePreview();
            previewReleaseMs +=
                (Time.realtimeSinceStartupAsDouble - previewStartedAt)
                * 1000d;

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
            {
                if (previewVisual != null)
                    Object.Destroy(previewVisual);

                double wallStartedAt =
                    Time.realtimeSinceStartupAsDouble;
                _wallVisuals.RefreshPlacedNeighborhood(
                    signal.Position);
                wallRefreshMs +=
                    (Time.realtimeSinceStartupAsDouble - wallStartedAt)
                    * 1000d;

                LogPlacedSignalPerf(
                    signal,
                    totalStartedAt,
                    relocationMs,
                    previewReleaseMs,
                    placedVisualMs,
                    wallRefreshMs);
                return;
            }

            double placedStartedAt =
                Time.realtimeSinceStartupAsDouble;

            BuildingDefinition def =
                _buildingRegistry.GetById(signal.BuildingId);
            if (def?.Prefab != null)
            {
                bool showConstructionVisual =
                    ShouldUseConstructionVisual(
                        def,
                        _constructionLifecycle,
                        signal.Position);
                GameObject placedPrefab = showConstructionVisual
                    ? def.ResolveConstructionPrefab()
                    : def.Prefab;
                GameObject sourceVisual =
                    ResolveReusablePlacedSource(
                        previewVisual,
                        def,
                        placedPrefab);
                _placedVisuals.Replace(
                    signal.Position,
                    signal.BuildingId,
                    placedPrefab,
                    ConstructionRotationUtility.ToWorldRotation(
                        ConstructionRotationUtility.Normalize(
                            signal.RotationQuarterTurns)),
                    def.ResolveVisualYOffset(),
                    sourceVisual,
                    def.Presentation);

                if (showConstructionVisual)
                    _placedVisuals.MarkUnderConstruction(signal.Position);
            }
            else if (previewVisual != null)
            {
                Object.Destroy(previewVisual);
            }

            placedVisualMs +=
                (Time.realtimeSinceStartupAsDouble - placedStartedAt)
                * 1000d;

            LogPlacedSignalPerf(
                signal,
                totalStartedAt,
                relocationMs,
                previewReleaseMs,
                placedVisualMs,
                wallRefreshMs);
        }

        private static GameObject ResolveReusablePlacedSource(
            GameObject previewVisual,
            BuildingDefinition def,
            GameObject placedPrefab)
        {
            if (previewVisual == null || def == null)
                return null;

            GameObject previewPrefab =
                def.ResolvePreviewPrefab();
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

        private static void LogPlacedSignalPerf(
            BuildingPlacedSignal signal,
            double totalStartedAt,
            double relocationMs,
            double previewReleaseMs,
            double placedVisualMs,
            double wallRefreshMs)
        {
            if (!Debug.isDebugBuild)
                return;

            double totalMs =
                (Time.realtimeSinceStartupAsDouble - totalStartedAt)
                * 1000d;

            if (totalMs < SignalHandlerLogThresholdMs)
                return;

            string message =
                $"{PerfLogTag} placed-signal-visual " +
                $"building={signal.BuildingId} pos={signal.Position} " +
                $"totalMs={totalMs:F3} " +
                $"relocation={relocationMs:F3} " +
                $"previewRelease={previewReleaseMs:F3} " +
                $"placedVisual={placedVisualMs:F3} " +
                $"wallRefresh={wallRefreshMs:F3}";

            if (totalMs >= SignalHandlerWarnThresholdMs)
                Debug.LogWarning(message);
            else
                Debug.Log(message);
        }

        public void Handle(BuildingDemolishedSignal signal)
        {
            _placedVisuals.Remove(signal.Position);

            if (_placedVisuals.ClearSelectionIfMatches(signal.Position))
                _radiusVisuals.HideInspection();

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
                _wallVisuals.RefreshPlacedNeighborhood(signal.Position);
        }

        public void Handle(BuildingOperationalSignal signal)
        {
            BuildingDefinition def =
                _buildingRegistry.GetById(signal.BuildingId);
            if (def?.Prefab != null)
            {
                if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
                    _wallVisuals.RefreshPlacedNeighborhood(signal.Position);
                else
                    _placedVisuals.ReplaceWithStoredPose(
                        signal.Position,
                        def.Prefab,
                        def.ResolveVisualYOffset(),
                        def.Presentation);
            }

            _placedVisuals.MarkOperational(signal.Position);
        }

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
