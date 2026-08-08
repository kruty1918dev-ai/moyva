using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        public void RegisterUnit(string unitId, Vector2Int position, int visionRange)
            => RegisterVisionArea(unitId, position, visionRange, null);

        public void UpdateUnitVisionRange(string unitId, int visionRange)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            int clampedRange = ClampVisionRange(visionRange);

            if (!_initialized)
            {
                if (_pendingUnits.TryGetValue(unitId, out var pending))
                    _pendingUnits[unitId] = (pending.Position, clampedRange, pending.Shape, pending.Modifiers);
                else
                    _unitVisionRange[unitId] = clampedRange;
                return;
            }

            if (!_unitPositions.TryGetValue(unitId, out var position))
            {
                _unitVisionRange[unitId] = clampedRange;
                return;
            }

            if (_unitVisionRange.TryGetValue(unitId, out int current) && current == clampedRange)
                return;

            _unitVisionRange[unitId] = clampedRange;

            RemoveVisibleTiles(unitId);
            var tiles = ComputeVisibleTiles(unitId, position, clampedRange);
            _unitVisibleTiles[unitId] = tiles;

            foreach (var tile in tiles)
                AddVisibleTile(tile);

            FlushVisual();
        }

        public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
            => RegisterVisionArea(areaId, position, visionRange, shape);

        private void RegisterVisionArea(string unitId, Vector2Int position, int visionRange, FogRevealShape? shape, FogVisionModifiers modifiers = default)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            Debug.Log($"{StartupRevealDiagTag} RegisterVisionAreaDecision id={unitId}, source={(shape.HasValue ? "fixed-vision-area" : "unit-vision")}, startPoint={position}, range={visionRange}, shape={(shape.HasValue ? shape.Value.ToString() : "visibility-resolver")}, initialized={_initialized}, map={_width}x{_height}, case={(_initialized ? "apply-now" : "queued-not-initialized")}, visibleBefore={CountVisibleTiles()}, exploredBefore={CountExploredTiles()}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}.");
            if (!_initialized)
            {
                _pendingUnits[unitId] = (position, visionRange, shape, modifiers);
                Debug.Log($"{StartupRevealDiagTag} RegisterVisionAreaQueued id={unitId}, source={(shape.HasValue ? "fixed-vision-area" : "unit-vision")}, startPoint={position}, range={visionRange}, pendingUnits={_pendingUnits.Count}.");
                return;
            }

            int visibleBefore = CountVisibleTiles();
            int exploredBefore = CountExploredTiles();
            RemoveVisibleTiles(unitId);

            visionRange = ClampVisionRange(visionRange);
            _unitVisionRange[unitId] = visionRange;
            _unitPositions[unitId] = position;
            _unitVisionModifiers[unitId] = modifiers;

            if (shape.HasValue)
                _fixedVisionShapes[unitId] = shape.Value;
            else
                _fixedVisionShapes.Remove(unitId);

            var tiles = ComputeInitialVisibleTiles(unitId, position, visionRange);
            _unitVisibleTiles[unitId] = tiles;

            foreach (var t in tiles)
                AddVisibleTile(t);

            int dirtyBeforeFlush = _visualDirtyBuffer.DirtyCount;
            int changesBeforeFlush = _visualDirtyBuffer.ChangeCount;
            var flushResult = FlushVisual();
            Debug.Log($"{StartupRevealDiagTag} RegisterVisionAreaResult id={unitId}, source={(shape.HasValue ? "fixed-vision-area" : "unit-vision")}, startPoint={position}, range={visionRange}, shape={(shape.HasValue ? shape.Value.ToString() : "visibility-resolver")}, tiles={tiles.Count}, visibleBefore={visibleBefore}, visibleAfter={CountVisibleTiles()}, exploredBefore={exploredBefore}, exploredAfter={CountExploredTiles()}, gameplayChanged={dirtyBeforeFlush > 0 || changesBeforeFlush > 0 || visibleBefore != CountVisibleTiles() || exploredBefore != CountExploredTiles()}, dirtySent={flushResult.DirtyCount}, changesSent={flushResult.ChangeCount}, visualUpdateDispatched={flushResult.UpdaterCalled}, visualFogDispersed={flushResult.VisualFogDispersalRequested}, visualUpdater={flushResult.UpdaterType ?? "null"}.");
        }

        public void UpdateUnitPosition(string unitId, Vector2Int newPosition)
        {
            if (!_initialized)
            {
                int pendingRange = _unitVisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;

                FogRevealShape? shape = _fixedVisionShapes.TryGetValue(unitId, out var storedShape)
                    ? storedShape
                    : null;
                _pendingUnits[unitId] = (newPosition, pendingRange, shape, ResolveUnitVisionModifiers(unitId));
                return;
            }

            if (!_unitVisibleTiles.TryGetValue(unitId, out var oldTiles))
            {
                int fallbackRange = _unitVisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;

                RegisterVisionArea(unitId, newPosition, fallbackRange, null, ResolveUnitVisionModifiers(unitId));
                return;
            }

            foreach (var t in oldTiles)
                RemoveVisibleTile(t);

            int range = _unitVisionRange.TryGetValue(unitId, out int r) ? r : _defaultVisionRange;
            _unitPositions[unitId] = newPosition;
            var newTiles = ComputeVisibleTiles(unitId, newPosition, range);
            _unitVisibleTiles[unitId] = newTiles;

            foreach (var t in newTiles)
                AddVisibleTile(t);

            FlushVisual();
        }

        public void UnregisterUnit(string unitId)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (!_initialized)
            {
                _pendingUnits.Remove(unitId);
                _unitVisionRange.Remove(unitId);
                _unitPositions.Remove(unitId);
                _fixedVisionShapes.Remove(unitId);
                _unitVisionModifiers.Remove(unitId);
                return;
            }

            if (!RemoveVisibleTiles(unitId))
                return;

            _unitVisionRange.Remove(unitId);
            _unitPositions.Remove(unitId);
            _fixedVisionShapes.Remove(unitId);
            _unitVisionModifiers.Remove(unitId);

            FlushVisual();
        }

        internal IReadOnlyList<FogFixedVisionAreaSnapshot> GetFixedVisionAreasSnapshot()
        {
            var snapshot = new List<FogFixedVisionAreaSnapshot>(_fixedVisionShapes.Count);
            foreach (var shapePair in _fixedVisionShapes)
            {
                string areaId = shapePair.Key;
                if (string.IsNullOrWhiteSpace(areaId))
                    continue;

                if (!_unitPositions.TryGetValue(areaId, out Vector2Int position))
                    continue;

                if (!_unitVisionRange.TryGetValue(areaId, out int visionRange))
                    continue;

                snapshot.Add(new FogFixedVisionAreaSnapshot(areaId, position, visionRange, shapePair.Value));
            }

            return snapshot;
        }

        internal void LoadFixedVisionAreasSnapshot(IReadOnlyList<FogFixedVisionAreaSnapshot> areas)
        {
            if (areas == null || areas.Count == 0)
                return;

            for (int index = 0; index < areas.Count; index++)
            {
                var area = areas[index];
                if (string.IsNullOrWhiteSpace(area.AreaId) || area.VisionRange <= 0)
                    continue;

                RegisterFixedVisionArea(area.AreaId, area.Position, area.VisionRange, area.Shape);
            }
        }

        private static string GetBuildingVisionAreaId(Vector2Int position)
            => $"{BuildingVisionAreaPrefix}{position.x}:{position.y}";
    }
}
