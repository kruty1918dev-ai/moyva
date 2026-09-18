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
            public IReadOnlyDictionary<string, float> PaidCosts;
            public string FundingSettlementId;
            public long QueueId;
            public string OwnerId;
            public Vector2Int Position;
            public string RecruitingBuildingId;
            public string UnitTypeId;
            public float TrainingSeconds;
            public float CompletedSeconds;
            public int CompletedTurns;
            public int TrainingTurns;
            public long EnqueuedGlobalTurn;
            public long LastProgressGlobalTurn;
        }

        private readonly Dictionary<QueueKey, List<Entry>> _queues = new();
        private long _nextQueueId = 1;
        public long NextQueueId => _nextQueueId;

        public bool CanEnqueue(string ownerId, Vector2Int position, int capacity, out string reason)
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
            if (_queues.TryGetValue(key, out List<Entry> queue) && queue.Count >= capacity)
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
            => EnqueueValidated(ownerId, position, string.Empty, unitTypeId, trainingTurns, globalTurn);

        public UnitRecruitmentQueueItemSnapshot EnqueueValidated(
            string ownerId,
            Vector2Int position,
            string recruitingBuildingId,
            string unitTypeId,
            int trainingTurns,
            long globalTurn,
            IReadOnlyDictionary<string, float> paidCosts = null,
            string fundingSettlementId = null,
            float trainingSeconds = 0f)
        {
            var key = new QueueKey(ownerId, position);
            if (!_queues.TryGetValue(key, out List<Entry> queue))
            {
                queue = new List<Entry>();
                _queues.Add(key, queue);
            }

            var entry = new Entry
            {
                PaidCosts = paidCosts == null ? null : new Dictionary<string, float>(paidCosts),
                FundingSettlementId = fundingSettlementId,
                QueueId = _nextQueueId++,
                OwnerId = ownerId,
                Position = position,
                RecruitingBuildingId = recruitingBuildingId ?? string.Empty,
                UnitTypeId = unitTypeId,
                TrainingSeconds = Math.Max(0f, trainingSeconds),
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

            List<QueueKey> keys = GetSortedKeys(ownerId);
            bool changed = false;
            for (int index = 0; index < keys.Count; index++)
            {
                List<Entry> queue = _queues[keys[index]];
                if (queue.Count == 0)
                    continue;

                Entry head = queue.Find(entry => entry.CompletedTurns < entry.TrainingTurns);
                if (head == null)
                    continue;
                if (head.EnqueuedGlobalTurn >= globalTurn || head.LastProgressGlobalTurn >= globalTurn)
                    continue;

                head.CompletedTurns++;
                head.LastProgressGlobalTurn = globalTurn;
                changed = true;
            }
            return changed;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> AdvanceRealtime(float seconds, float roundSeconds)
        {
            var changed = new List<UnitRecruitmentQueueItemSnapshot>();
            if (seconds <= 0f)
                return changed;
            foreach (QueueKey key in GetSortedKeys(null))
            {
                float remaining = seconds;
                foreach (Entry entry in _queues[key])
                {
                    if (entry.CompletedTurns >= entry.TrainingTurns)
                        continue;
                    if (entry.TrainingSeconds <= 0f)
                    {
                        entry.TrainingSeconds = entry.TrainingTurns * Math.Max(0.1f, roundSeconds);
                        entry.CompletedSeconds = entry.CompletedTurns * Math.Max(0.1f, roundSeconds);
                    }
                    int before = entry.CompletedTurns;
                    float advance = Math.Min(remaining, Math.Max(0f, entry.TrainingSeconds - entry.CompletedSeconds));
                    entry.CompletedSeconds += advance;
                    remaining -= advance;
                    entry.CompletedTurns = entry.CompletedSeconds >= entry.TrainingSeconds
                        ? entry.TrainingTurns
                        : Math.Min(entry.TrainingTurns - 1, (int)(entry.CompletedSeconds / entry.TrainingSeconds * entry.TrainingTurns));
                    if (entry.CompletedTurns != before)
                        changed.Add(Snapshot(entry));
                    if (remaining <= 0f)
                        break;
                }
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

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(string ownerId, Vector2Int position)
        {
            var key = new QueueKey(ownerId, position);
            if (!_queues.TryGetValue(key, out List<Entry> queue) || queue.Count == 0)
                return Array.Empty<UnitRecruitmentQueueItemSnapshot>();

            var result = new UnitRecruitmentQueueItemSnapshot[queue.Count];
            bool trainingAssigned = false;
            for (int index = 0; index < queue.Count; index++)
            {
                Entry entry = queue[index];
                bool unfinished = entry.CompletedTurns < entry.TrainingTurns;
                result[index] = Snapshot(entry, unfinished && trainingAssigned);
                trainingAssigned |= unfinished;
            }
            return result;
        }

        public bool TryPeekReady(string ownerId, Vector2Int position, out UnitRecruitmentQueueItemSnapshot item)
        {
            return TryGetReady(ownerId, position, 0, out item);
        }

        public bool TryGetReady(string ownerId, Vector2Int position, long queueId,
            out UnitRecruitmentQueueItemSnapshot item)
        {
            if (_queues.TryGetValue(new QueueKey(ownerId, position), out List<Entry> queue))
            {
                Entry entry = queue.Find(candidate => candidate.CompletedTurns >= candidate.TrainingTurns
                    && (queueId == 0 || candidate.QueueId == queueId));
                if (entry != null)
                {
                    item = Snapshot(entry);
                    return true;
                }
            }
            item = default;
            return false;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyHeads(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return Array.Empty<UnitRecruitmentQueueItemSnapshot>();

            List<QueueKey> keys = GetSortedKeys(ownerId);
            var result = new List<UnitRecruitmentQueueItemSnapshot>();
            for (int index = 0; index < keys.Count; index++)
            {
                List<Entry> queue = _queues[keys[index]];
                foreach (Entry entry in queue)
                    if (entry.CompletedTurns >= entry.TrainingTurns)
                        result.Add(Snapshot(entry));
            }
            return result;
        }

        public bool TryTakeReady(
            string ownerId,
            Vector2Int position,
            long expectedQueueId,
            out UnitRecruitmentQueueItemSnapshot item)
        {
            var key = new QueueKey(ownerId, position);
            item = default;
            if (!_queues.TryGetValue(key, out List<Entry> queue))
                return false;
            int index = queue.FindIndex(entry => entry.QueueId == expectedQueueId
                && entry.CompletedTurns >= entry.TrainingTurns);
            if (index < 0)
                return false;
            item = Snapshot(queue[index]);
            queue.RemoveAt(index);
            if (queue.Count == 0)
                _queues.Remove(key);
            return true;
        }

        public bool TryCancel(string ownerId, Vector2Int position, long queueId,
            out UnitRecruitmentQueueItemSnapshot item)
        {
            item = default;
            var key = new QueueKey(ownerId, position);
            if (!_queues.TryGetValue(key, out List<Entry> queue))
                return false;
            int index = queue.FindIndex(entry => entry.QueueId == queueId
                && entry.CompletedTurns < entry.TrainingTurns);
            if (index < 0)
                return false;
            item = Snapshot(queue[index]);
            queue.RemoveAt(index);
            if (queue.Count == 0)
                _queues.Remove(key);
            return true;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> CaptureAll()
        {
            List<QueueKey> keys = GetSortedKeys(null);
            var result = new List<UnitRecruitmentQueueItemSnapshot>();
            for (int keyIndex = 0; keyIndex < keys.Count; keyIndex++)
            {
                List<Entry> queue = _queues[keys[keyIndex]];
                for (int index = 0; index < queue.Count; index++)
                    result.Add(Snapshot(queue[index]));
            }
            return result;
        }

        public void RestoreAll(IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items)
        {
            var replacement = new Dictionary<QueueKey, List<Entry>>();
            var seenIds = new HashSet<long>();
            long nextQueueId = 1;

            if (items != null)
            {
                for (int index = 0; index < items.Count; index++)
                {
                    Entry entry = EntryFromSnapshot(items[index]);
                    if (!seenIds.Add(entry.QueueId))
                        throw new InvalidOperationException($"Duplicate recruitment queue id {entry.QueueId}.");

                    var key = new QueueKey(entry.OwnerId, entry.Position);
                    if (!replacement.TryGetValue(key, out List<Entry> queue))
                    {
                        queue = new List<Entry>();
                        replacement.Add(key, queue);
                    }
                    queue.Add(entry);
                    nextQueueId = Math.Max(nextQueueId, entry.QueueId + 1);
                }
            }

            _queues.Clear();
            foreach (KeyValuePair<QueueKey, List<Entry>> pair in replacement)
                _queues.Add(pair.Key, pair.Value);
            _nextQueueId = nextQueueId;
        }

        private List<QueueKey> GetSortedKeys(string ownerId)
        {
            var keys = new List<QueueKey>();
            foreach (QueueKey key in _queues.Keys)
            {
                if (ownerId == null || string.Equals(key.OwnerId, ownerId, StringComparison.Ordinal))
                    keys.Add(key);
            }
            keys.Sort(CompareKeys);
            return keys;
        }

        private static int CompareKeys(QueueKey left, QueueKey right)
        {
            int byX = left.Position.x.CompareTo(right.Position.x);
            if (byX != 0) return byX;
            int byY = left.Position.y.CompareTo(right.Position.y);
            if (byY != 0) return byY;
            return string.CompareOrdinal(left.OwnerId, right.OwnerId);
        }

        private static Entry EntryFromSnapshot(UnitRecruitmentQueueItemSnapshot item)
        {
            if (item.QueueId < 1)
                throw new ArgumentOutOfRangeException(nameof(item), "Recruitment queue id must be positive.");
            if (string.IsNullOrWhiteSpace(item.OwnerId))
                throw new ArgumentException("Recruitment queue owner is empty.", nameof(item));
            if (string.IsNullOrWhiteSpace(item.UnitTypeId))
                throw new ArgumentException("Recruitment unit type is empty.", nameof(item));

            int trainingTurns = Math.Max(1, item.TrainingTurns);
            int completedTurns = Math.Max(0, Math.Min(item.CompletedTurns, trainingTurns));
            long enqueued = Math.Max(1L, item.EnqueuedGlobalTurn);
            long lastProgress = Math.Max(enqueued, item.LastProgressGlobalTurn);
            return new Entry
            {
                PaidCosts = item.PaidCosts,
                FundingSettlementId = item.FundingSettlementId,
                QueueId = item.QueueId,
                OwnerId = item.OwnerId.Trim(),
                Position = item.RecruitingBuildingPosition,
                RecruitingBuildingId = item.RecruitingBuildingId?.Trim() ?? string.Empty,
                UnitTypeId = item.UnitTypeId.Trim(),
                TrainingSeconds = item.TrainingSeconds,
                CompletedSeconds = item.CompletedSeconds,
                CompletedTurns = completedTurns,
                TrainingTurns = trainingTurns,
                EnqueuedGlobalTurn = enqueued,
                LastProgressGlobalTurn = lastProgress,
            };
        }

        private static UnitRecruitmentQueueItemSnapshot Snapshot(Entry entry, bool waiting = false)
        {
            bool ready = entry.CompletedTurns >= entry.TrainingTurns;
            return new UnitRecruitmentQueueItemSnapshot(
                entry.QueueId,
                entry.OwnerId,
                entry.Position,
                entry.RecruitingBuildingId,
                entry.UnitTypeId,
                entry.CompletedTurns,
                entry.TrainingTurns,
                entry.EnqueuedGlobalTurn,
                entry.LastProgressGlobalTurn,
                ready ? UnitRecruitmentQueueStatus.Ready
                    : waiting ? UnitRecruitmentQueueStatus.Waiting : UnitRecruitmentQueueStatus.Training,
                entry.PaidCosts,
                entry.FundingSettlementId, entry.TrainingSeconds, entry.CompletedSeconds);
        }
    }
}
