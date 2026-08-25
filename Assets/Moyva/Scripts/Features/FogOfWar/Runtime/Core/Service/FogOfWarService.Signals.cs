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

        private void OnUnitGarrisonStateChanged(
            UnitGarrisonStateChangedSignal signal)
        {
            if (signal.IsGarrisoned)
            {
                UnregisterUnit(signal.UnitId);
                return;
            }

            int requestedRange = signal.VisionRange > 0
                ? signal.VisionRange
                : _defaultVisionRange;
            RegisterVisionArea(
                signal.UnitId,
                signal.UnitPosition,
                ClampVisionRange(requestedRange),
                null);
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            if (signal.HasRelocationSource && signal.RelocationSourcePosition != signal.Position)
                UnregisterUnit(GetBuildingVisionAreaId(signal.RelocationSourcePosition));

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
            _resolver.SetHeightMap(FogWorldVisualContextFactory.BuildVisibilityHeightMap(signal.TerrainLevelMap, signal.HeightMap));

            Vector2Int baseMapSize = FogWorldSignalUtility.ResolveBaseMapSize(signal);
            int signalWidth = baseMapSize.x;
            int signalHeight = baseMapSize.y;
            _visualContext = FogWorldVisualContextFactory.CreateFromSignal(signal, signalWidth, signalHeight);
            ResetVisualHeightSampler();
            _visualUpdater?.SetWorldContext(_visualContext);

            if (!_initialized)
            {
                Initialize(signalWidth, signalHeight);
                return;
            }

            if (_width != signalWidth || _height != signalHeight)
                ResizeToWorldDimensions(signalWidth, signalHeight);

            ApplyPendingRevealAreas("WorldGeneratedData");
            RecalculateAllVisibility();
        }

        private void ReplayCachedWorldGeneratedSignalIfAvailable()
        {
            if (_worldGenerationSignalState == null)
                return;

            if (_worldGenerationSignalState.TryGetWorldGeneratedData(out var signal))
            {
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
            return true;
        }
    }
}
