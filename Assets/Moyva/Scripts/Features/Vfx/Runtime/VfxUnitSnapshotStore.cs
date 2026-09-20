using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>
    /// Presentation-only mirror of unit identity/position. Signals deliver
    /// unit state changes; the store keeps the last known grid position so
    /// effects that need a location at authoritative-removal time
    /// (UnitDestroyedSignal carries only UnitId) stay reliable regardless of
    /// subscriber order. Also tracks recent spawns so the deployment effect
    /// is not duplicated by unit-created.
    /// </summary>
    public sealed class VfxUnitSnapshotStore
    {
        private sealed class Snapshot
        {
            public Vector2Int Position;
            public string UnitTypeId;
            public string OwnerId;
            public float SpawnedAt;
        }

        private readonly Dictionary<string, Snapshot> _units =
            new Dictionary<string, Snapshot>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, float> _recentSpawnEffects =
            new Dictionary<string, float>(System.StringComparer.Ordinal);

        /// <summary>Seconds during which a follow-up spawn/deploy effect for the same unit is suppressed.</summary>
        public float SpawnDedupeWindow = 2f;

        public int TrackedCount => _units.Count;

        public void TrackCreated(
            string unitId,
            Vector2Int position,
            string unitTypeId,
            string ownerId,
            float now)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            _units[unitId] = new Snapshot
            {
                Position = position,
                UnitTypeId = unitTypeId,
                OwnerId = ownerId,
                SpawnedAt = now,
            };
        }

        public void TrackMoved(string unitId, Vector2Int position, string ownerId)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (_units.TryGetValue(unitId, out Snapshot snapshot))
            {
                snapshot.Position = position;
                if (!string.IsNullOrWhiteSpace(ownerId))
                    snapshot.OwnerId = ownerId;
                return;
            }

            _units[unitId] = new Snapshot
            {
                Position = position,
                OwnerId = ownerId,
            };
        }

        public bool TryGet(
            string unitId,
            out Vector2Int position,
            out string unitTypeId,
            out string ownerId)
        {
            if (!string.IsNullOrWhiteSpace(unitId)
                && _units.TryGetValue(unitId, out Snapshot snapshot))
            {
                position = snapshot.Position;
                unitTypeId = snapshot.UnitTypeId;
                ownerId = snapshot.OwnerId;
                return true;
            }

            position = default;
            unitTypeId = null;
            ownerId = null;
            return false;
        }

        public void Remove(string unitId)
        {
            if (!string.IsNullOrWhiteSpace(unitId))
                _units.Remove(unitId);
        }

        /// <summary>
        /// Returns true when a spawn effect was already emitted for the unit
        /// within the dedupe window; otherwise records it now.
        /// </summary>
        public bool TryConsumeSpawnEffect(string unitId, float now)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return false;

            if (_recentSpawnEffects.TryGetValue(unitId, out float at)
                && now - at < SpawnDedupeWindow)
                return false;

            _recentSpawnEffects[unitId] = now;
            PruneSpawnEffects(now);
            return true;
        }

        private void PruneSpawnEffects(float now)
        {
            if (_recentSpawnEffects.Count < 32)
                return;

            _pruneBuffer ??= new List<string>(32);
            _pruneBuffer.Clear();
            foreach (var pair in _recentSpawnEffects)
            {
                if (now - pair.Value >= SpawnDedupeWindow)
                    _pruneBuffer.Add(pair.Key);
            }
            foreach (string key in _pruneBuffer)
                _recentSpawnEffects.Remove(key);
        }

        private List<string> _pruneBuffer;

        public void Clear()
        {
            _units.Clear();
            _recentSpawnEffects.Clear();
        }
    }
}
