using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Ordered work queue for grounding analyses. Items are processed
    /// strictly in enqueue order and land in the shared cache — results are
    /// never delivered per-completion, so batching cannot reorder the
    /// output. An epoch counter lets callers drop results issued for a
    /// stale world build.
    /// </summary>
    public sealed class SpriteGroundingBatch
    {
        public readonly struct Item
        {
            public Item(EntityId sourceKey, IPixelSource source, SpriteGroundingProfile profile)
            {
                SourceKey = sourceKey;
                Source = source;
                Profile = profile;
            }

            public readonly EntityId SourceKey;
            public readonly IPixelSource Source;
            public readonly SpriteGroundingProfile Profile;
        }

        private readonly List<Item> _items = new List<Item>();
        private readonly HashSet<(EntityId, int)> _seen = new HashSet<(EntityId, int)>();
        private int _head;

        /// <summary>Incremented on <see cref="Reset"/>; compare to discard stale runs.</summary>
        public int Epoch { get; private set; }

        /// <summary>Queued items not yet processed.</summary>
        public int PendingCount => _items.Count - _head;

        /// <summary>Enqueues a (source, profile) analysis once; duplicates are skipped.</summary>
        public bool Enqueue(EntityId sourceKey, IPixelSource source, in SpriteGroundingProfile profile)
        {
            if (source == null)
                return false;
            if (!_seen.Add((sourceKey, profile.GetHashCode())))
                return false;
            _items.Add(new Item(sourceKey, source, profile));
            return true;
        }

        /// <summary>
        /// Processes pending items in enqueue order until the millisecond
        /// budget is exhausted. Returns how many items remain. Passing a
        /// non-positive budget processes nothing.
        /// </summary>
        public int ProcessBudgeted(SpriteGroundingCache cache, double millisecondsBudget)
        {
            if (cache == null || millisecondsBudget <= 0.0)
                return PendingCount;

            var watch = Stopwatch.StartNew();
            while (_head < _items.Count)
            {
                if (watch.Elapsed.TotalMilliseconds >= millisecondsBudget)
                    break;
                Item item = _items[_head++];
                cache.GetOrAdd(item.SourceKey, item.Source, item.Profile);
            }
            return PendingCount;
        }

        /// <summary>Processes all remaining items regardless of time.</summary>
        public void Flush(SpriteGroundingCache cache)
        {
            while (_head < _items.Count)
            {
                Item item = _items[_head++];
                cache?.GetOrAdd(item.SourceKey, item.Source, item.Profile);
            }
        }

        /// <summary>Drops every pending item and marks a new epoch.</summary>
        public void Reset()
        {
            _items.Clear();
            _seen.Clear();
            _head = 0;
            Epoch++;
        }
    }
}
