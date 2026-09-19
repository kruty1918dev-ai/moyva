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
                    _pendingUnits[unitId] = (pending.Position, clampedRange, pending.Shape, pending.Modifiers, pending.OwnerId);
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

            if (SourceContributesToLocalGrid(unitId))
            {
                var tiles = ComputeVisibleTiles(unitId, position, clampedRange);
                _unitVisibleTiles[unitId] = tiles;

                foreach (var tile in tiles)
                    AddVisibleTile(tile);
            }

            FlushVisual();
        }

        public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
            => RegisterVisionArea(areaId, position, visionRange, shape);

        /// <summary>
        /// Registers a vision source in the global catalog. The source only
        /// contributes tiles to the local grid when its owner is the local
        /// perspective owner (or unowned).
        /// </summary>
        private void RegisterVisionArea(
            string unitId,
            Vector2Int position,
            int visionRange,
            FogRevealShape? shape,
            FogVisionModifiers modifiers = default,
            string ownerId = null)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (!_initialized)
            {
                _pendingUnits[unitId] =
                    (position, visionRange, shape, modifiers, ownerId);
                return;
            }

            TrackSourceOwner(unitId, ownerId);
            RemoveVisibleTiles(unitId);

            visionRange = ClampVisionRange(visionRange);
            _unitVisionRange[unitId] = visionRange;
            _unitPositions[unitId] = position;
            _unitVisionModifiers[unitId] = modifiers;

            if (shape.HasValue)
                _fixedVisionShapes[unitId] = shape.Value;
            else
                _fixedVisionShapes.Remove(unitId);

            if (SourceContributesToLocalGrid(unitId))
            {
                IReadOnlyList<Vector2Int> tiles =
                    ComputeInitialVisibleTiles(
                        unitId,
                        position,
                        visionRange);
                _unitVisibleTiles[unitId] = tiles;

                foreach (Vector2Int tile in tiles)
                    AddVisibleTile(tile);
            }

            FlushVisual();
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
                string pendingOwner = _pendingUnits.TryGetValue(unitId, out var pending)
                    ? pending.OwnerId
                    : GetSourceOwner(unitId);
                _pendingUnits[unitId] = (newPosition, pendingRange, shape, ResolveUnitVisionModifiers(unitId), pendingOwner);
                return;
            }

            // Unknown global sources are never created from a position update:
            // a source enters the local grid only through registration.
            if (!_unitPositions.ContainsKey(unitId))
                return;

            if (_unitVisibleTiles.TryGetValue(unitId, out var oldTiles))
            {
                foreach (var t in oldTiles)
                    RemoveVisibleTile(t);
            }

            int range = _unitVisionRange.TryGetValue(unitId, out int r) ? r : _defaultVisionRange;
            _unitPositions[unitId] = newPosition;

            if (SourceContributesToLocalGrid(unitId))
            {
                var newTiles = ComputeVisibleTiles(unitId, newPosition, range);
                _unitVisibleTiles[unitId] = newTiles;

                foreach (var t in newTiles)
                    AddVisibleTile(t);
            }

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
                _sourceOwners.Remove(unitId);
                return;
            }

            bool hadLocalTiles = RemoveVisibleTiles(unitId);

            _unitVisionRange.Remove(unitId);
            _unitPositions.Remove(unitId);
            _fixedVisionShapes.Remove(unitId);
            _unitVisionModifiers.Remove(unitId);
            _sourceOwners.Remove(unitId);

            if (!hadLocalTiles)
                return;

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
