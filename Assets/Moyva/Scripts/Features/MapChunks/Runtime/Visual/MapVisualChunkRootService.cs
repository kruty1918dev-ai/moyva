using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    public sealed class MapVisualChunkRootService : IMapVisualChunkRootService
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
