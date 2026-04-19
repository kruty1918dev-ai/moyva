using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapObjectVisualRegistryService : IMapObjectVisualRegistryService
    {
        private readonly Dictionary<Vector2Int, Entry> _entries = new Dictionary<Vector2Int, Entry>();

        public void Clear()
        {
            _entries.Clear();
        }

        public void Register(string objectId, Vector2Int position, GameObject visual)
        {
            if (string.IsNullOrWhiteSpace(objectId) || visual == null)
                return;

            _entries[position] = new Entry(objectId, visual);
        }

        public bool TryGetVisual(string objectId, Vector2Int position, out GameObject visual)
        {
            if (_entries.TryGetValue(position, out var entry)
                && entry.Visual != null
                && string.Equals(entry.ObjectId, objectId, System.StringComparison.Ordinal))
            {
                visual = entry.Visual;
                return true;
            }

            if (_entries.TryGetValue(position, out entry) && entry.Visual == null)
                _entries.Remove(position);

            visual = null;
            return false;
        }

        private readonly struct Entry
        {
            public Entry(string objectId, GameObject visual)
            {
                ObjectId = objectId;
                Visual = visual;
            }

            public string ObjectId { get; }
            public GameObject Visual { get; }
        }
    }
}
