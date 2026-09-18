using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Local-perspective ownership for the shared fog grid.
    ///
    /// <see cref="FogOfWarService._stateGrid"/> is the LOCAL player's view.
    /// Every globally registered vision source records an owner in
    /// <see cref="_sourceOwners"/>; only sources owned by the local player (or
    /// unowned reveal/preview sources) contribute tiles to that grid. Per-owner
    /// grids in <see cref="_ownerStates"/> stay authoritative for owner-scoped
    /// queries (multiplayer filtering, bots, save snapshots).
    /// </summary>
    internal sealed partial class FogOfWarService : IFogOwnerVisibilityFeed
    {
        /// <summary>
        /// Owner of each globally registered vision source. Empty string means
        /// an unowned source that always contributes to the local perspective.
        /// </summary>
        private readonly Dictionary<string, string> _sourceOwners =
            new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Owner id the local perspective belongs to. Null while unresolved —
        /// in that state every source contributes (single-player/dev fallback).
        /// </summary>
        private string _localPerspectiveOwnerId;

        /// <summary>
        /// Cells that transitioned into Visible per owner since the last
        /// notification flush. Filled while owner grids mutate, dispatched once
        /// per public mutation.
        /// </summary>
        private readonly Dictionary<string, HashSet<Vector2Int>> _ownerVisibleGainedCells =
            new Dictionary<string, HashSet<Vector2Int>>(StringComparer.Ordinal);

        public event Action<string, IReadOnlyCollection<Vector2Int>> CellsBecameVisible;

        public string LocalPerspectiveOwnerId => _localPerspectiveOwnerId;

        /// <summary>
        /// Sets the owner the local fog perspective belongs to. Rebuilds the
        /// local grid when the value actually changes; explored memory is kept.
        /// </summary>
        public void SetLocalPerspectiveOwnerId(string ownerId)
        {
            string normalized = NormalizeFogOwnerId(ownerId);
            if (string.Equals(
                    _localPerspectiveOwnerId,
                    normalized,
                    StringComparison.Ordinal))
            {
                return;
            }

            _localPerspectiveOwnerId = normalized;
            if (_initialized)
                RecalculateAllVisibility();
        }

        /// <summary>
        /// True when a source owned by <paramref name="ownerId"/> contributes
        /// to the local grid. Null/empty owners always contribute; when the
        /// local owner is unresolved every owner contributes (dev/offline
        /// fallback before the bootstrap initializer runs).
        /// </summary>
        public bool IsLocalPerspectiveOwner(string ownerId)
        {
            ownerId = NormalizeFogOwnerId(ownerId);
            if (ownerId == null)
                return true;

            string local = NormalizeFogOwnerId(_localPerspectiveOwnerId);
            if (local == null)
                return true;

            return string.Equals(ownerId, local, StringComparison.Ordinal);
        }

        /// <summary>
        /// Records the owner of a globally registered vision source.
        /// </summary>
        private void TrackSourceOwner(string sourceId, string ownerId)
        {
            _sourceOwners[sourceId] = NormalizeFogOwnerId(ownerId) ?? string.Empty;
        }

        private string GetSourceOwner(string sourceId)
            => !string.IsNullOrWhiteSpace(sourceId)
               && _sourceOwners.TryGetValue(sourceId, out string owner)
                ? owner
                : string.Empty;

        /// <summary>
        /// True when the globally registered source currently contributes to
        /// the local perspective grid.
        /// </summary>
        private bool SourceContributesToLocalGrid(string sourceId)
            => IsLocalPerspectiveOwner(GetSourceOwner(sourceId));

        /// <summary>
        /// Removes a source's tiles from the local grid and re-applies them
        /// when the source still contributes. Used after the owner of a source
        /// changes (ownership transfer).
        /// </summary>
        private void ReapplyLocalPerspectiveSource(string sourceId)
        {
            RemoveVisibleTiles(sourceId);
            if (!SourceContributesToLocalGrid(sourceId)
                || !_unitPositions.TryGetValue(sourceId, out var position))
            {
                return;
            }

            int range = _unitVisionRange.TryGetValue(sourceId, out int stored)
                ? stored
                : _defaultVisionRange;
            var tiles = ComputeVisibleTiles(sourceId, position, range);
            _unitVisibleTiles[sourceId] = tiles;
            foreach (var tile in tiles)
                AddVisibleTile(tile);
        }

        private void TrackOwnerVisibleGained(
            string ownerId,
            FogStateGrid grid,
            Vector2Int tile)
        {
            if (grid.GetState(tile) == FogStateType.Visible)
                return;

            if (!_ownerVisibleGainedCells.TryGetValue(ownerId, out var cells))
            {
                cells = new HashSet<Vector2Int>();
                _ownerVisibleGainedCells[ownerId] = cells;
            }

            cells.Add(tile);
        }

        /// <summary>
        /// Dispatches CellsBecameVisible for every owner with pending cells.
        /// Called once per public owner-state mutation; also safe to call with
        /// an empty buffer.
        /// </summary>
        private void FlushOwnerVisibilityGained()
        {
            var handler = CellsBecameVisible;
            if (handler == null || _ownerVisibleGainedCells.Count == 0)
            {
                _ownerVisibleGainedCells.Clear();
                return;
            }

            foreach (var pair in _ownerVisibleGainedCells)
            {
                if (pair.Value.Count == 0)
                    continue;

                handler(pair.Key, pair.Value);
            }

            _ownerVisibleGainedCells.Clear();
        }
    }
}
