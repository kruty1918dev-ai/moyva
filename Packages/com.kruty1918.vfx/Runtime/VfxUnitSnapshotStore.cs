using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Vfx
{
    /// <summary>Зберігає знімки стану юнітів для VFX: позиція, тип, власник; дедуплікація спавн-ефектів у часовому вікні.</summary>
    public sealed class VfxUnitSnapshotStore
    {
        /// <summary>Знімок стану одного юніта.</summary>
        private sealed class Snapshot
        {
            /// <summary>Позиція юніта на сітці.</summary>
            public Vector2Int Position;
            /// <summary>Ідентифікатор типу юніта.</summary>
            public string UnitTypeId;
            /// <summary>Ідентифікатор власника юніта.</summary>
            public string OwnerId;
            /// <summary>Час спавну юніта.</summary>
            public float SpawnedAt;
        }

        private readonly Dictionary<string, Snapshot> _units =
            new Dictionary<string, Snapshot>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, float> _recentSpawnEffects =
            new Dictionary<string, float>(System.StringComparer.Ordinal);

        /// <summary>Вікно дедуплікації спавн-ефекту в секундах.</summary>
        public float SpawnDedupeWindow = 2f;

        /// <summary>Кількість відстежуваних юнітів.</summary>
        public int TrackedCount => _units.Count;

        /// <summary>Реєструє новоствореного юніта у сховищі.</summary>
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

        /// <summary>Оновлює позицію та власника відстежуваного юніта.</summary>
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

        /// <summary>Намагається отримати знімок юніта.</summary>
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

        /// <summary>Видаляє юніта зі сховища.</summary>
        public void Remove(string unitId)
        {
            if (!string.IsNullOrWhiteSpace(unitId))
                _units.Remove(unitId);
        }

        /// <summary>Намагається спожити право на спавн-ефект у вікні дедуплікації.</summary>
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

        /// <summary>Очищає сховище.</summary>
        public void Clear()
        {
            _units.Clear();
            _recentSpawnEffects.Clear();
        }
    }
}
