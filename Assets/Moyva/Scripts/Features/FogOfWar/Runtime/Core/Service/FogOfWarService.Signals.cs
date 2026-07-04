using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            int requestedRange = signal.VisionRange > 0 ? signal.VisionRange : _defaultVisionRange;
            var modifiers = signal.HasCustomVisionModifiers
                ? new FogVisionModifiers(signal.CanSeeCrest, signal.CrestVisibilityFactor, signal.DownSlopeVisionBonus, signal.SilhouettePenalty)
                : default;
            RegisterVisionArea(signal.UnitId, signal.Position, ClampVisionRange(requestedRange), null, modifiers);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
            => UpdateUnitPosition(signal.UnitId, signal.NewPosition);

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
            => UnregisterUnit(signal.UnitId);

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            RegisterFixedVisionArea(
                GetBuildingVisionAreaId(signal.Position),
                signal.Position,
                _defaultVisionRange,
                FogRevealShape.PixelCircle);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
            => UnregisterUnit(GetBuildingVisionAreaId(signal.Position));

        private void OnWorldGeneratedData(WorldGeneratedDataSignal signal)
        {
            if (ShouldSkipWorldGeneratedSignal(signal))
                return;

            Debug.Log($"{WorldGenDiagTag} Receiver.Fog.WorldGenerated RECEIVED frame={Time.frameCount}, map={signal.Width}x{signal.Height}, initializedBefore={_initialized}");
            _startupRevealFlow = _startupDiagnostics?.StartFlow(
                "fog-startup",
                new DiagnosticContext().Add("map", $"{signal.Width}x{signal.Height}"));
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.WorldGenerated, $"map={signal.Width}x{signal.Height}");
            _resolver.SetHeightMap(FogWorldVisualContextFactory.BuildVisibilityHeightMap(signal.TerrainLevelMap, signal.HeightMap));

            Vector2Int baseMapSize = FogWorldSignalUtility.ResolveBaseMapSize(signal);
            int signalWidth = baseMapSize.x;
            int signalHeight = baseMapSize.y;
            _visualContext = FogWorldVisualContextFactory.CreateFromSignal(signal, signalWidth, signalHeight);
            ResetVisualHeightSampler();
            _visualUpdater?.SetWorldContext(_visualContext);
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.FogServiceInitializeMap, $"baseMap={signalWidth}x{signalHeight}");
            Debug.Log($"{DebugTag} FogService.OnWorldGeneratedData signal={signal.Width}x{signal.Height}, baseMap={signalWidth}x{signalHeight}, initialized={_initialized}, current={_width}x{_height}, pendingReveals={_pendingRevealAreas.Count}.");

            if (!_initialized)
            {
                Initialize(signalWidth, signalHeight);
                return;
            }

            if (_width != signalWidth || _height != signalHeight)
                ResizeToWorldDimensions(signalWidth, signalHeight);

            ApplyPendingRevealAreas("WorldGeneratedData");
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.SpawnResolved, $"pendingReveals={_pendingRevealAreas.Count}");
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.RevealArea, $"reason=WorldGeneratedData");
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.RegisterCoreVision, $"fixedAreas={_fixedVisionShapes.Count}");
            RecalculateAllVisibility();
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.FlushVisual, $"visible={CountVisibleTiles()}, explored={CountExploredTiles()}");
            _startupDiagnostics?.CompleteStep(_startupRevealFlow, FogStartupDiagnosticSteps.VolumeUpdaterRebuild, $"visualUpdater={_visualUpdater != null}");
            _startupDiagnostics?.Report(_startupRevealFlow);
            Debug.Log($"{WorldGenDiagTag} Receiver.Fog.WorldGenerated APPLIED map={_width}x{_height}, initializedAfter={_initialized}, pendingRevealCount={_pendingRevealAreas.Count}");
            Debug.Log($"{DebugTag} FogService.OnWorldGeneratedData end map={_width}x{_height}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, pendingReveals={_pendingRevealAreas.Count}.");
        }

        private void ReplayCachedWorldGeneratedSignalIfAvailable()
        {
            if (_worldGenerationSignalState == null)
                return;

            if (_worldGenerationSignalState.TryGetWorldGeneratedData(out var signal))
            {
                Debug.Log($"{WorldGenDiagTag} Receiver.Fog.WorldGenerated REPLAY frame={Time.frameCount}, map={signal.Width}x{signal.Height}, initializedBefore={_initialized}");
                OnWorldGeneratedData(signal);
            }
        }

        private bool ShouldSkipWorldGeneratedSignal(WorldGeneratedDataSignal signal)
        {
            if (signal.SnapshotRevision <= 0 || signal.SnapshotRevision != _lastHandledWorldRevision)
            {
                _lastHandledWorldRevision = signal.SnapshotRevision;
                return false;
            }

            Debug.Log($"{WorldGenDiagTag} Receiver.Fog.WorldGenerated SKIP duplicate revision={signal.SnapshotRevision}, sequence={signal.StartupSequence}, source={signal.Source}.");
            return true;
        }
    }
}
