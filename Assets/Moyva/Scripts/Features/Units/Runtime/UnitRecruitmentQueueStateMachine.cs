using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitRecruitmentQueueStateMachine
    {
        private readonly struct QueueKey : IEquatable<QueueKey>
        {
            public QueueKey(string ownerId, Vector2Int position)
            {
                OwnerId = ownerId;
                Position = position;
            }

            public string OwnerId { get; }
            public Vector2Int Position { get; }

            public bool Equals(QueueKey other)
                => string.Equals(OwnerId, other.OwnerId, StringComparison.Ordinal)
                   && Position == other.Position;

            public override bool Equals(object obj)
                => obj is QueueKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((OwnerId != null
                        ? StringComparer.Ordinal.GetHashCode(OwnerId)
                        : 0) * 397) ^ Position.GetHashCode();
                }
            }
        }

        private sealed class Entry
        {
            public long QueueId;
            public string OwnerId;
            public Vector2Int Position;
            public string UnitTypeId;
            public int CompletedTurns;
            public int TrainingTurns;
            public long EnqueuedGlobalTurn;
            public long LastProgressGlobalTurn;
        }

        private readonly Dictionary<QueueKey, List<Entry>> _queues = new();
        private long _nextQueueId = 1;

        public bool CanEnqueue(
            string ownerId,
            Vector2Int position,
            int capacity,
            out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                reason = "Recruitment owner is empty.";
                return false;
            }

            if (capacity < 1)
            {
                reason = "Recruitment queue capacity is invalid.";
                return false;
            }

            var key = new QueueKey(ownerId, position);
            if (_queues.TryGetValue(key, out List<Entry> queue)
                && queue.Count >= capacity)
            {
                reason = $"Recruitment queue is full ({queue.Count}/{capacity}).";
                return false;
            }

            return true;
        }

        public UnitRecruitmentQueueItemSnapshot EnqueueValidated(
            string ownerId,
            Vector2Int position,
            string unitTypeId,
            int trainingTurns,
            long globalTurn)
        {
            var key = new QueueKey(ownerId, position);
            if (!_queues.TryGetValue(key, out List<Entry> queue))
            {
                queue = new List<Entry>();
                _queues.Add(key, queue);
            }

            var entry = new Entry
            {
                QueueId = _nextQueueId++,
                OwnerId = ownerId,
                Position = position,
                UnitTypeId = unitTypeId,
                CompletedTurns = 0,
                TrainingTurns = Math.Max(1, trainingTurns),
                EnqueuedGlobalTurn = Math.Max(1L, globalTurn),
                LastProgressGlobalTurn = Math.Max(1L, globalTurn),
            };
            queue.Add(entry);
            return Snapshot(entry);
        }

        public bool AdvanceOwnerTurn(string ownerId, long globalTurn)
        {
            if (string.IsNullOrWhiteSpace(ownerId) || globalTurn < 1)
                return false;

            var keys = new List<QueueKey>();
            foreach (QueueKey key in _queues.Keys)
            {
                if (string.Equals(key.OwnerId, ownerId, StringComparison.Ordinal))
                    keys.Add(key);
            }

            keys.Sort(CompareKeys);
            bool changed = false;
            for (int index = 0; index < keys.Count; index++)
            {
                List<Entry> queue = _queues[keys[index]];
                if (queue.Count == 0)
                    continue;

                Entry head = queue[0];
                if (head.CompletedTurns >= head.TrainingTurns)
                    continue;
                if (head.EnqueuedGlobalTurn >= globalTurn
                    || head.LastProgressGlobalTurn >= globalTurn)
                {
                    continue;
                }

                head.CompletedTurns++;
                head.LastProgressGlobalTurn = globalTurn;
                changed = true;
            }

            return changed;
        }

        public bool RemoveBuildingQueues(Vector2Int position)
        {
            var keys = new List<QueueKey>();
            foreach (QueueKey key in _queues.Keys)
            {
                if (key.Position == position)
                    keys.Add(key);
            }

            bool changed = false;
            for (int index = 0; index < keys.Count; index++)
                changed |= _queues.Remove(keys[index]);
            return changed;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(
            string ownerId,
            Vector2Int position)
        {
            var key = new QueueKey(ownerId, position);
            if (!_queues.TryGetValue(key, out List<Entry> queue)
                || queue.Count == 0)
            {
                return Array.Empty<UnitRecruitmentQueueItemSnapshot>();
            }

            var result = new UnitRecruitmentQueueItemSnapshot[queue.Count];
            for (int index = 0; index < queue.Count; index++)
                result[index] = Snapshot(queue[index]);
            return result;
        }

        public bool TryPeekReady(
            string ownerId,
            Vector2Int position,
            out UnitRecruitmentQueueItemSnapshot item)
        {
            var key = new QueueKey(ownerId, position);
            if (_queues.TryGetValue(key, out List<Entry> queue)
                && queue.Count > 0
                && queue[0].CompletedTurns >= queue[0].TrainingTurns)
            {
                item = Snapshot(queue[0]);
                return true;
            }

            item = default;
            return false;
        }

        private static int CompareKeys(QueueKey left, QueueKey right)
        {
            int byX = left.Position.x.CompareTo(right.Position.x);
            if (byX != 0)
                return byX;
            int byY = left.Position.y.CompareTo(right.Position.y);
            if (byY != 0)
                return byY;
            return string.CompareOrdinal(left.OwnerId, right.OwnerId);
        }

        private static UnitRecruitmentQueueItemSnapshot Snapshot(Entry entry)
        {
            bool ready = entry.CompletedTurns >= entry.TrainingTurns;
            return new UnitRecruitmentQueueItemSnapshot(
                entry.QueueId,
                entry.OwnerId,
                entry.Position,
                entry.UnitTypeId,
                entry.CompletedTurns,
                entry.TrainingTurns,
                entry.EnqueuedGlobalTurn,
                ready
                    ? UnitRecruitmentQueueStatus.Ready
                    : UnitRecruitmentQueueStatus.Training);
        }
    }
}
