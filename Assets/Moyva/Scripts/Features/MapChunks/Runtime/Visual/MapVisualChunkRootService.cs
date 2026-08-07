using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    public sealed class MapVisualChunkRootService : IMapVisualChunkRootService, IMapVisualChunkRootPruner
    {
        private const string RootName = "MapVisualChunks";
        private readonly Dictionary<MapChunkCoord, Transform> _roots = new();
        private Transform _root;

        public Transform GetOrCreateRoot(MapChunkCoord coord)
        {
            if (_roots.TryGetValue(coord, out var existing) && existing != null)
                return existing;

            EnsureRoot();
            string chunkName = $"MapChunk_{coord.X}_{coord.Y}";
            var existingChild = _root.Find(chunkName);
            if (existingChild != null)
            {
                _roots[coord] = existingChild;
                return existingChild;
            }

            var chunkObject = new GameObject(chunkName);
            chunkObject.transform.SetParent(_root, false);
            var transform = chunkObject.transform;
            _roots[coord] = transform;
            return transform;
        }

        public int RemoveRootsOutside(IReadOnlyCollection<MapChunkCoord> validCoords)
        {
            EnsureRoot();
            var validNames = new HashSet<string>();
            if (validCoords != null)
            {
                foreach (MapChunkCoord coord in validCoords)
                    validNames.Add($"MapChunk_{coord.X}_{coord.Y}");
            }

            int removed = 0;
            for (int i = _root.childCount - 1; i >= 0; i--)
            {
                Transform child = _root.GetChild(i);
                if (child == null || !child.name.StartsWith("MapChunk_", System.StringComparison.Ordinal))
                    continue;
                if (validNames.Contains(child.name))
                    continue;

                if (Application.isPlaying)
                    Object.Destroy(child.gameObject);
                else
                    Object.DestroyImmediate(child.gameObject);
                removed++;
            }

            if (_roots.Count > 0)
            {
                var stale = new List<MapChunkCoord>();
                foreach (var pair in _roots)
                {
                    string name = $"MapChunk_{pair.Key.X}_{pair.Key.Y}";
                    if (pair.Value == null || !validNames.Contains(name))
                        stale.Add(pair.Key);
                }
                for (int i = 0; i < stale.Count; i++)
                    _roots.Remove(stale[i]);
            }

            return removed;
        }

        public bool IsChunkRoot(Transform transform)
        {
            if (transform == null)
                return false;

            if (transform == _root)
                return true;

            return transform.parent == _root;
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;

            var existing = GameObject.Find(RootName);
            var rootObject = existing != null ? existing : new GameObject(RootName);
            _root = rootObject.transform;
            _root.position = Vector3.zero;
            _root.rotation = Quaternion.identity;
            _root.localScale = Vector3.one;
        }
    }
}
