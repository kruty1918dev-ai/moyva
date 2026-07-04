using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Collects fog visual dirtiness produced by gameplay state changes.
    /// Owns dirty tile and cell-change buffers, but does not call visual updaters.
    /// </summary>
    internal sealed class FogVisualDirtyBuffer
    {
        private readonly HashSet<Vector2Int> _dirtyTiles = new HashSet<Vector2Int>();
        private readonly List<FogCellVisualChange> _changes = new List<FogCellVisualChange>();

        public IReadOnlyCollection<Vector2Int> DirtyTiles => _dirtyTiles;

        public IReadOnlyList<FogCellVisualChange> Changes => _changes;

        public int DirtyCount => _dirtyTiles.Count;

        public int ChangeCount => _changes.Count;

        public void TrackChange(Vector2Int tile, FogStateType oldState, FogStateType newState, int oldHeightKey, int newHeightKey)
        {
            if (oldState == newState && oldHeightKey == newHeightKey)
                return;

            _dirtyTiles.Add(tile);
            _changes.Add(new FogCellVisualChange(tile, oldState, newState, oldHeightKey, newHeightKey));
        }

        public void Clear()
        {
            _dirtyTiles.Clear();
            _changes.Clear();
        }
    }
}
