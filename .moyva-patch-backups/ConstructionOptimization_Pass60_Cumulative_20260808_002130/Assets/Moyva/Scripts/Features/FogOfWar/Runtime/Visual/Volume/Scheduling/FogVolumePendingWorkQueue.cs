using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Stores pending volume visual work requests without executing any render path.
    /// </summary>
    internal sealed class FogVolumePendingWorkQueue : IFogVolumePendingWorkState, IFogVolumePendingWorkRequests, IFogVolumePendingWorkMaintenance
    {
        private readonly HashSet<Vector2Int> _pendingDirtyTiles = new HashSet<Vector2Int>();
        private readonly List<FogCellVisualChange> _pendingCellChanges = new List<FogCellVisualChange>();

        private int _mapWidth = 1;
        private int _mapHeight = 1;

        public bool HasPendingWork { get; private set; }

        public bool FullRebuildRequested { get; private set; }

        public IFogOfWarService FogService { get; private set; }

        public IReadOnlyCollection<Vector2Int> PendingDirtyTiles => _pendingDirtyTiles;

        public IReadOnlyList<FogCellVisualChange> PendingCellChanges => _pendingCellChanges;

        public int DirtyTileCount => _pendingDirtyTiles.Count;

        public int CellChangeCount => _pendingCellChanges.Count;

        public FogVolumePendingWorkSnapshot Snapshot => new FogVolumePendingWorkSnapshot(
            HasPendingWork,
            FullRebuildRequested,
            FogService,
            _pendingDirtyTiles,
            _pendingCellChanges);

        public void SetMapSize(int width, int height)
        {
            _mapWidth = Mathf.Max(1, width);
            _mapHeight = Mathf.Max(1, height);
        }

        public void RequestFullRebuild()
        {
            FullRebuildRequested = true;
            HasPendingWork = true;
        }

        public void RequestFullRebuild(IFogOfWarService fogService)
        {
            FogService = fogService;
            RequestFullRebuild();
        }

        public void RequestFullRebuildWhenFogServiceAvailable()
        {
            FullRebuildRequested = true;
            HasPendingWork = HasPendingWork || FogService != null;
        }

        public int RequestDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles, out int requested)
        {
            FogService = fogService;
            requested = 0;
            if (dirtyTiles != null)
            {
                foreach (var tile in dirtyTiles)
                {
                    requested++;
                    if (IsInBounds(tile))
                        _pendingDirtyTiles.Add(tile);
                }
            }

            HasPendingWork = HasPendingWork || _pendingDirtyTiles.Count > 0;
            return _pendingDirtyTiles.Count;
        }

        public int RequestCellsUpdate(IFogOfWarService fogService, IReadOnlyList<FogCellVisualChange> changes)
        {
            FogService = fogService;
            int accepted = 0;
            if (changes != null)
            {
                for (int i = 0; i < changes.Count; i++)
                {
                    var change = changes[i];
                    if (!IsInBounds(change.Cell))
                        continue;

                    _pendingCellChanges.Add(change);
                    _pendingDirtyTiles.Add(change.Cell);
                    accepted++;
                }
            }

            HasPendingWork = HasPendingWork || accepted > 0;
            return accepted;
        }

        public void Complete()
        {
            FullRebuildRequested = false;
            _pendingDirtyTiles.Clear();
            _pendingCellChanges.Clear();
            HasPendingWork = false;
        }

        public void ClearCellChanges()
        {
            _pendingCellChanges.Clear();
        }

        private bool IsInBounds(Vector2Int tile)
            => tile.x >= 0 && tile.x < _mapWidth && tile.y >= 0 && tile.y < _mapHeight;
    }
}
