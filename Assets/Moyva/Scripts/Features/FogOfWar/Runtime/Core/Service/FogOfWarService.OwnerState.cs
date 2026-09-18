using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        private sealed class OwnerFogState
        {
            public readonly FogStateGrid Grid = new FogStateGrid();
            public readonly Dictionary<string, IReadOnlyList<Vector2Int>> VisibleTiles =
                new Dictionary<string, IReadOnlyList<Vector2Int>>(StringComparer.Ordinal);
            public readonly Dictionary<string, int> VisionRange =
                new Dictionary<string, int>(StringComparer.Ordinal);
            public readonly Dictionary<string, Vector2Int> Positions =
                new Dictionary<string, Vector2Int>(StringComparer.Ordinal);
            public readonly Dictionary<string, FogVisionModifiers> Modifiers =
                new Dictionary<string, FogVisionModifiers>(StringComparer.Ordinal);
            public readonly Dictionary<string, FogRevealShape> FixedShapes =
                new Dictionary<string, FogRevealShape>(StringComparer.Ordinal);
            public readonly List<FogPendingRevealArea> PendingReveals =
                new List<FogPendingRevealArea>();
            public bool[,] PendingExploredSnapshot;
        }

        public FogStateType GetFogState(string ownerId, Vector2Int position)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null || !_ownerStates.TryGetValue(ownerId, out var state))
                return FogStateType.Unexplored;
            return _initialized && state.Grid.IsReady
                ? state.Grid.GetState(position)
                : FogStateType.Unexplored;
        }

        public bool IsVisible(string ownerId, Vector2Int position)
            => GetFogState(ownerId, position) == FogStateType.Visible;

        public bool IsExplored(string ownerId, Vector2Int position)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            return ownerId != null
                   && _ownerStates.TryGetValue(ownerId, out var state)
                   && _initialized
                   && state.Grid.IsExplored(position);
        }

        public IReadOnlyCollection<string> GetKnownFogOwnerIds()
            => _ownerStates.Keys;

        public bool[,] GetExploredSnapshot(string ownerId)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null || !_ownerStates.TryGetValue(ownerId, out var state))
                return null;
            if (!_initialized)
                return state.PendingExploredSnapshot != null
                    ? FogStateGrid.CloneSnapshot(state.PendingExploredSnapshot)
                    : null;
            return state.Grid.GetExploredSnapshot();
        }

        public void LoadFromSnapshot(string ownerId, bool[,] explored)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null || explored == null)
                return;

            OwnerFogState state = GetOrCreateOwnerState(ownerId);
            if (!_initialized)
            {
                state.PendingExploredSnapshot = FogStateGrid.CloneSnapshot(explored);
                return;
            }

            EnsureOwnerGridReady(state);
            state.Grid.LoadExploredSnapshot(explored);
            RecalculateOwnerVisibility(ownerId, state);
            BumpVersion();
        }

        public void RegisterUnit(string ownerId, string unitId, Vector2Int position, int visionRange)
            => RegisterOwnerVisionArea(ownerId, unitId, position, visionRange, null, ResolveUnitVisionModifiers(unitId));

        public void RegisterFixedVisionArea(
            string ownerId,
            string areaId,
            Vector2Int position,
            int visionRange,
            FogRevealShape shape)
            => RegisterOwnerVisionArea(ownerId, areaId, position, visionRange, shape, default);

        public void RevealArea(
            string ownerId,
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible,
            string visibleAreaId = null)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null)
                return;

            radius = Mathf.Max(0, radius);
            OwnerFogState state = GetOrCreateOwnerState(ownerId);
            if (!_initialized)
            {
                state.PendingReveals.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                return;
            }

            if (!RevealTouchesCurrentMap(center, radius))
            {
                state.PendingReveals.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                return;
            }

            ApplyOwnerRevealArea(ownerId, state, center, radius, shape, keepVisible, visibleAreaId);
            BumpVersion();
        }

        public void UpdateUnitPosition(string ownerId, string unitId, Vector2Int newPosition)
        {
            ownerId = ResolveOwnerForVisionSource(ownerId, unitId);
            if (ownerId == null || string.IsNullOrWhiteSpace(unitId))
                return;

            OwnerFogState state = GetOrCreateOwnerState(ownerId);
            if (!state.Positions.ContainsKey(unitId))
            {
                int range = state.VisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;
                RegisterOwnerVisionArea(ownerId, unitId, newPosition, range, null, ResolveUnitVisionModifiers(unitId));
                return;
            }

            if (_initialized)
                RemoveOwnerVisibleTiles(state, unitId);
            state.Positions[unitId] = newPosition;
            if (_initialized)
                AddOwnerVisibleTiles(ownerId, state, unitId, newPosition);
            BumpVersion();
        }

        public void UpdateUnitVisionRange(string ownerId, string unitId, int visionRange)
        {
            ownerId = ResolveOwnerForVisionSource(ownerId, unitId);
            if (ownerId == null || string.IsNullOrWhiteSpace(unitId))
                return;

            OwnerFogState state = GetOrCreateOwnerState(ownerId);
            int clamped = ClampVisionRange(visionRange);
            if (state.VisionRange.TryGetValue(unitId, out int current) && current == clamped)
                return;
            state.VisionRange[unitId] = clamped;
            if (_initialized && state.Positions.TryGetValue(unitId, out var position))
            {
                RemoveOwnerVisibleTiles(state, unitId);
                AddOwnerVisibleTiles(ownerId, state, unitId, position);
            }
            BumpVersion();
        }

        public void UnregisterUnit(string ownerId, string unitId)
        {
            ownerId = ResolveOwnerForVisionSource(ownerId, unitId);
            if (ownerId == null || string.IsNullOrWhiteSpace(unitId))
                return;

            if (_ownerStates.TryGetValue(ownerId, out var state))
            {
                RemoveOwnerVisibleTiles(state, unitId);
                state.VisionRange.Remove(unitId);
                state.Positions.Remove(unitId);
                state.Modifiers.Remove(unitId);
                state.FixedShapes.Remove(unitId);
            }
            _ownerByVisionSourceId.Remove(unitId);
            BumpVersion();
        }

        public void TransferFixedVisionAreaOwner(string areaId, string previousOwnerId, string newOwnerId)
        {
            previousOwnerId = ResolveOwnerForVisionSource(previousOwnerId, areaId);
            newOwnerId = NormalizeFogOwnerId(newOwnerId);
            if (previousOwnerId == null || newOwnerId == null || string.IsNullOrWhiteSpace(areaId))
                return;
            if (string.Equals(previousOwnerId, newOwnerId, StringComparison.Ordinal))
                return;
            if (!_ownerStates.TryGetValue(previousOwnerId, out var previous))
                return;
            if (!previous.Positions.TryGetValue(areaId, out var position))
                return;

            int range = previous.VisionRange.TryGetValue(areaId, out int storedRange)
                ? storedRange
                : _defaultVisionRange;
            FogRevealShape? shape = previous.FixedShapes.TryGetValue(areaId, out var storedShape)
                ? storedShape
                : null;
            FogVisionModifiers modifiers = previous.Modifiers.TryGetValue(areaId, out var storedModifiers)
                ? storedModifiers
                : default;

            UnregisterUnit(previousOwnerId, areaId);
            RegisterOwnerVisionArea(newOwnerId, areaId, position, range, shape, modifiers);
        }

        private OwnerFogState GetOrCreateOwnerState(string ownerId)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null)
                return null;
            if (_ownerStates.TryGetValue(ownerId, out var state))
                return state;

            state = new OwnerFogState();
            _ownerStates.Add(ownerId, state);
            if (_initialized)
                EnsureOwnerGridReady(state);
            return state;
        }

        private void InitializeOwnerStates()
        {
            foreach (var pair in _ownerStates)
            {
                EnsureOwnerGridReady(pair.Value);
                ApplyPendingOwnerRevealAreas(pair.Key, pair.Value);
                RecalculateOwnerVisibility(pair.Key, pair.Value);
            }
        }

        private void EnsureOwnerGridReady(OwnerFogState state)
        {
            if (state == null || state.Grid.IsReady)
                return;

            state.Grid.Initialize(_width, _height);
            if (state.PendingExploredSnapshot != null)
            {
                state.Grid.LoadExploredSnapshot(state.PendingExploredSnapshot);
                state.PendingExploredSnapshot = null;
            }
        }

        private void RegisterOwnerVisionArea(
            string ownerId,
            string sourceId,
            Vector2Int position,
            int visionRange,
            FogRevealShape? shape,
            FogVisionModifiers modifiers)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null || string.IsNullOrWhiteSpace(sourceId))
                return;

            sourceId = sourceId.Trim();
            if (_ownerByVisionSourceId.TryGetValue(sourceId, out string previousOwner)
                && !string.Equals(previousOwner, ownerId, StringComparison.Ordinal))
            {
                UnregisterUnit(previousOwner, sourceId);
            }

            OwnerFogState state = GetOrCreateOwnerState(ownerId);
            if (_initialized)
            {
                EnsureOwnerGridReady(state);
                RemoveOwnerVisibleTiles(state, sourceId);
            }

            int clamped = ClampVisionRange(visionRange);
            state.VisionRange[sourceId] = clamped;
            state.Positions[sourceId] = position;
            state.Modifiers[sourceId] = modifiers;
            if (shape.HasValue)
                state.FixedShapes[sourceId] = shape.Value;
            else
                state.FixedShapes.Remove(sourceId);
            _ownerByVisionSourceId[sourceId] = ownerId;

            if (_initialized)
                AddOwnerVisibleTiles(ownerId, state, sourceId, position);
            BumpVersion();
        }

        private void AddOwnerVisibleTiles(
            string ownerId,
            OwnerFogState state,
            string sourceId,
            Vector2Int position)
        {
            int range = state.VisionRange.TryGetValue(sourceId, out int storedRange)
                ? storedRange
                : _defaultVisionRange;
            IReadOnlyList<Vector2Int> tiles = ComputeOwnerVisibleTiles(ownerId, state, sourceId, position, range);
            state.VisibleTiles[sourceId] = tiles;
            foreach (var tile in tiles)
                state.Grid.IncrementVisible(tile);
        }

        private bool RemoveOwnerVisibleTiles(OwnerFogState state, string sourceId)
        {
            if (state == null || !state.VisibleTiles.TryGetValue(sourceId, out var tiles))
                return false;

            foreach (var tile in tiles)
                state.Grid.DecrementVisible(tile);
            state.VisibleTiles.Remove(sourceId);
            return true;
        }

        private IReadOnlyList<Vector2Int> ComputeOwnerVisibleTiles(
            string ownerId,
            OwnerFogState state,
            string sourceId,
            Vector2Int position,
            int range)
        {
            if (state.FixedShapes.TryGetValue(sourceId, out var shape))
                return FogRevealShapeTileCalculator.ComputeShapeTiles(position, range, shape, _width, _height);

            var modifiers = state.Modifiers.TryGetValue(sourceId, out var storedModifiers)
                ? storedModifiers
                : default;
            var tiles = _resolver.ComputeVisibleTiles(position, range, _width, _height, modifiers);
            return AddOwnerSilhouetteTargetTiles(ownerId, sourceId, position, range, modifiers, tiles);
        }

        private IReadOnlyList<Vector2Int> AddOwnerSilhouetteTargetTiles(
            string ownerId,
            string observerSourceId,
            Vector2Int observerPosition,
            int range,
            FogVisionModifiers observerModifiers,
            IReadOnlyList<Vector2Int> sourceTiles)
        {
            if (_heightVisionService == null || _unitPositions.Count <= 1)
                return sourceTiles;

            int maxRange = _settings != null ? _settings.MaxVisionRange : 12;
            int searchRadius = _heightVisionService.GetSearchRadius(observerPosition, range, maxRange, observerModifiers);
            float threshold = _settings != null ? Mathf.Clamp(_settings.TerrainVisibilityThreshold, 0.01f, 1f) : 0.5f;
            HashSet<Vector2Int> visible = null;

            foreach (var targetEntry in _unitPositions)
            {
                if (string.Equals(targetEntry.Key, observerSourceId, StringComparison.Ordinal))
                    continue;
                if (_ownerByVisionSourceId.TryGetValue(targetEntry.Key, out string targetOwner)
                    && string.Equals(targetOwner, ownerId, StringComparison.Ordinal))
                {
                    continue;
                }

                var targetModifiers = ResolveUnitVisionModifiers(targetEntry.Key);
                if (targetModifiers.EffectiveSilhouettePenalty <= 0f)
                    continue;

                Vector2Int targetPosition = targetEntry.Value;
                if (!IsInBounds(targetPosition))
                    continue;

                int distance = Mathf.Max(Mathf.Abs(targetPosition.x - observerPosition.x), Mathf.Abs(targetPosition.y - observerPosition.y));
                if (distance > searchRadius)
                    continue;

                visible ??= new HashSet<Vector2Int>(sourceTiles);
                if (visible.Contains(targetPosition))
                    continue;

                float visibility = _heightVisionService.GetVisibilityFactor(observerPosition, targetPosition, range, maxRange, observerModifiers, targetModifiers);
                if (visibility >= threshold)
                    visible.Add(targetPosition);
            }

            return visible == null || visible.Count == sourceTiles.Count
                ? sourceTiles
                : new List<Vector2Int>(visible);
        }

        private void ApplyOwnerRevealArea(
            string ownerId,
            OwnerFogState state,
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible,
            string visibleAreaId)
        {
            EnsureOwnerGridReady(state);
            string areaId = keepVisible
                ? ResolveRevealVisibilityAreaId(center, radius, shape, visibleAreaId)
                : visibleAreaId;
            if (keepVisible)
            {
                RegisterOwnerVisionArea(ownerId, areaId, center, radius, shape, default);
                return;
            }

            var tiles = FogRevealShapeTileCalculator.ComputeShapeTiles(center, radius, shape, _width, _height);
            foreach (var tile in tiles)
                state.Grid.MarkExplored(tile);
        }

        private void ApplyPendingOwnerRevealAreas(string ownerId, OwnerFogState state)
        {
            if (state.PendingReveals.Count == 0)
                return;

            var reveals = state.PendingReveals.ToArray();
            state.PendingReveals.Clear();
            for (int index = 0; index < reveals.Length; index++)
            {
                var reveal = reveals[index];
                ApplyOwnerRevealArea(ownerId, state, reveal.Center, reveal.Radius, reveal.Shape, reveal.KeepVisible, reveal.VisibleAreaId);
            }
        }

        private void RecalculateOwnerVisibility(string ownerId, OwnerFogState state)
        {
            if (!_initialized || state == null)
                return;

            EnsureOwnerGridReady(state);
            state.Grid.ClearVisibility();
            state.VisibleTiles.Clear();
            foreach (var source in state.Positions)
                AddOwnerVisibleTiles(ownerId, state, source.Key, source.Value);
        }

        private Dictionary<string, bool[,]> CaptureOwnerExploredSnapshots()
        {
            var snapshots = new Dictionary<string, bool[,]>(StringComparer.Ordinal);
            foreach (var pair in _ownerStates)
            {
                bool[,] snapshot = _initialized && pair.Value.Grid.IsReady
                    ? pair.Value.Grid.GetExploredSnapshot()
                    : pair.Value.PendingExploredSnapshot;
                if (snapshot != null)
                    snapshots[pair.Key] = FogStateGrid.CloneSnapshot(snapshot);
            }
            return snapshots;
        }

        private void RestoreOwnerExploredSnapshots(
            IReadOnlyDictionary<string, bool[,]> snapshots)
        {
            if (snapshots == null || snapshots.Count == 0)
                return;

            foreach (var pair in snapshots)
            {
                if (!_ownerStates.TryGetValue(pair.Key, out var state)
                    || pair.Value == null)
                {
                    continue;
                }

                if (state.Grid.IsReady)
                    state.Grid.LoadExploredSnapshot(pair.Value);
                else
                    state.PendingExploredSnapshot = FogStateGrid.CloneSnapshot(pair.Value);
            }
        }

        private string ResolveOwnerForVisionSource(string ownerId, string sourceId)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId != null)
                return ownerId;
            return !string.IsNullOrWhiteSpace(sourceId)
                   && _ownerByVisionSourceId.TryGetValue(sourceId.Trim(), out string storedOwner)
                ? storedOwner
                : null;
        }

        private static string NormalizeFogOwnerId(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return null;
            return ownerId.Trim();
        }
    }
}
