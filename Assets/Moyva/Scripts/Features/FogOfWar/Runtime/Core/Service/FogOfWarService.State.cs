using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        public FogStateType GetFogState(Vector2Int position)
            => _initialized ? _stateGrid.GetState(position) : FogStateType.Unexplored;

        public bool IsVisible(Vector2Int position)
            => _initialized && _stateGrid.IsVisible(position);

        public bool IsExplored(Vector2Int position)
            => _initialized && _stateGrid.IsExplored(position);

        public bool[,] GetExploredSnapshot()
        {
            if (!_initialized)
                return _pendingExploredSnapshot != null
                    ? FogStateGrid.CloneSnapshot(_pendingExploredSnapshot)
                    : null;

            return _stateGrid.GetExploredSnapshot();
        }

        public void LoadFromSnapshot(bool[,] explored)
        {
            if (explored == null)
                return;

            if (!_initialized)
            {
                _pendingExploredSnapshot = FogStateGrid.CloneSnapshot(explored);
                return;
            }

            _stateGrid.LoadExploredSnapshot(explored);
            _visualDirtyBuffer.Clear();
            _visualUpdater?.RebuildFullVisual(this);
            BumpVersion();
        }

        private bool IsInBounds(Vector2Int pos)
            => _stateGrid.IsInBounds(pos);

        private void AddVisibleTile(Vector2Int tile)
        {
            if (!IsInBounds(tile))
                return;

            FogStateType oldState = GetFogState(tile);
            int oldHeightKey = ResolveVisualHeightKey(tile);
            _stateGrid.IncrementVisible(tile);
            TrackVisualChange(tile, oldState, oldHeightKey);
        }

        private void RemoveVisibleTile(Vector2Int tile)
        {
            if (!IsInBounds(tile))
                return;

            FogStateType oldState = GetFogState(tile);
            int oldHeightKey = ResolveVisualHeightKey(tile);
            _stateGrid.DecrementVisible(tile);
            TrackVisualChange(tile, oldState, oldHeightKey);
        }

        private bool RemoveVisibleTiles(string unitId)
        {
            if (!_unitVisibleTiles.TryGetValue(unitId, out var tiles))
                return false;

            foreach (var tile in tiles)
                RemoveVisibleTile(tile);

            _unitVisibleTiles.Remove(unitId);
            return true;
        }

        private void RecalculateAllVisibility()
        {
            if (!_initialized)
                return;

            int visibleBefore = CountVisibleTiles();
            int exploredBefore = CountExploredTiles();
            Debug.Log($"{StartupRevealDiagTag} RecalculateAllVisibilityBegin source=vision-recalculation, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, visibleBefore={visibleBefore}, exploredBefore={exploredBefore}, map={_width}x{_height}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}.");
            _stateGrid.ClearVisibility();
            _unitVisibleTiles.Clear();

            foreach (var unitEntry in _unitPositions)
            {
                if (!_unitVisionRange.TryGetValue(unitEntry.Key, out int range))
                    range = _defaultVisionRange;

                var visibleTiles = ComputeVisibleTiles(unitEntry.Key, unitEntry.Value, range);
                _unitVisibleTiles[unitEntry.Key] = visibleTiles;

                foreach (var tile in visibleTiles)
                    _stateGrid.IncrementVisible(tile);
            }

            _visualUpdater?.RebuildFullVisual(this);
            BumpVersion();
            _visualDirtyBuffer.Clear();
            Debug.Log($"{DebugTag} FogService.RecalculateAllVisibility map={_width}x{_height}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, version={Version}.");
            Debug.Log($"{StartupRevealDiagTag} RecalculateAllVisibilityResult source=vision-recalculation, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, visibleBefore={visibleBefore}, visibleAfter={CountVisibleTiles()}, exploredBefore={exploredBefore}, exploredAfter={CountExploredTiles()}, gameplayChanged={visibleBefore != CountVisibleTiles() || exploredBefore != CountExploredTiles()}, visualUpdateDispatched={_visualUpdater != null}, visualFogDispersed={_visualUpdater != null && CountVisibleTiles() > 0}, version={Version}.");
        }

        private int CountVisibleTiles()
            => _initialized ? _stateGrid.CountVisibleTiles() : 0;

        private int CountExploredTiles()
            => _initialized ? _stateGrid.CountExploredTiles() : 0;

        private void CountFogStateTiles(out int visible, out int explored, out int unexplored)
        {
            if (!_initialized)
            {
                visible = 0;
                explored = 0;
                unexplored = 0;
                return;
            }

            _stateGrid.CountStates(out visible, out explored, out unexplored);
        }
    }
}
