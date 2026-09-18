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
            RegisterVisionArea(signal.UnitId, signal.Position, ClampVisionRange(requestedRange), null, modifiers, signal.OwnerId);
            RegisterOwnerVisionArea(signal.OwnerId, signal.UnitId, signal.Position, ClampVisionRange(requestedRange), null, modifiers);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            UpdateUnitPosition(signal.UnitId, signal.NewPosition);
            UpdateUnitPosition(signal.SourceFactionId, signal.UnitId, signal.NewPosition);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            UnregisterUnit(signal.UnitId);
            UnregisterUnit(null, signal.UnitId);
        }

        private void OnUnitGarrisonStateChanged(
            UnitGarrisonStateChangedSignal signal)
        {
            if (signal.IsGarrisoned)
            {
                UnregisterUnit(signal.UnitId);
                // A garrisoned unit stops providing vision for its owner too.
                UnregisterUnit(signal.OwnerId, signal.UnitId);
                return;
            }

            int requestedRange = signal.VisionRange > 0
                ? signal.VisionRange
                : _defaultVisionRange;
            RegisterVisionArea(
                signal.UnitId,
                signal.UnitPosition,
                ClampVisionRange(requestedRange),
                null,
                default,
                signal.OwnerId);
            RegisterUnit(
                signal.OwnerId,
                signal.UnitId,
                signal.UnitPosition,
                ClampVisionRange(requestedRange));
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            string areaId = GetBuildingVisionAreaId(signal.Position);
            UnregisterUnit(areaId);
            UnregisterUnit(signal.OwnerId, areaId);
        }

        private void OnBuildingOwnershipTransferred(
            BuildingOwnershipTransferredSignal signal)
        {
            string areaId = GetBuildingVisionAreaId(signal.Position);
            TransferFixedVisionAreaOwner(
                areaId,
                signal.PreviousOwnerId,
                signal.NewOwnerId);

            // Keep the global catalog's owner in sync so the building's vision
            // joins/leaves the local perspective together with ownership.
            if (_unitPositions.ContainsKey(areaId))
            {
                TrackSourceOwner(areaId, signal.NewOwnerId);
                ReapplyLocalPerspectiveSource(areaId);
                FlushVisual();
            }
        }

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
