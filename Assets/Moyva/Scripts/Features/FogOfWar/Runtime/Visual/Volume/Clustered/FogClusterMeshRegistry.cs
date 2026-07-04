using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterMeshRegistry : IFogClusterMeshRegistry
    {
        private readonly Dictionary<FogClusterKey, FogClusterMeshHandle> _handles = new Dictionary<FogClusterKey, FogClusterMeshHandle>();
        private GameObject _root;
        private Transform _rootParent;

        public FogClusterMeshHandle GetOrCreate(FogClusterKey key)
        {
            if (_handles.TryGetValue(key, out var existing) && existing?.GameObject != null)
                return existing;

            EnsureRoot();
            var clusterObject = new GameObject($"FogCluster_{key.ClusterX}_{key.ClusterY}");
            clusterObject.transform.SetParent(_root.transform, false);

            var meshFilter = clusterObject.AddComponent<MeshFilter>();
            var meshRenderer = clusterObject.AddComponent<MeshRenderer>();
            var mesh = new Mesh
            {
                name = $"FogClusterMesh_{key.ClusterX}_{key.ClusterY}"
            };
            meshFilter.sharedMesh = mesh;

            var handle = new FogClusterMeshHandle
            {
                Key = key,
                GameObject = clusterObject,
                MeshFilter = meshFilter,
                MeshRenderer = meshRenderer,
                Mesh = mesh
            };
            _handles[key] = handle;
            return handle;
        }

        public bool TryGet(FogClusterKey key, out FogClusterMeshHandle handle)
            => _handles.TryGetValue(key, out handle) && handle?.GameObject != null;

        public void Remove(FogClusterKey key)
        {
            if (!_handles.TryGetValue(key, out var handle))
                return;

            DestroyHandle(handle);
            _handles.Remove(key);
        }

        public void ClearAll()
        {
            foreach (var handle in _handles.Values)
                DestroyHandle(handle);

            _handles.Clear();
            DestroyObject(_root);
            _root = null;
        }

        public IEnumerable<FogClusterMeshHandle> GetAll()
            => _handles.Values;

        public void SetRootParent(Transform parent)
        {
            _rootParent = parent;
            if (_root != null && _rootParent != null && _root.transform.parent != _rootParent)
                _root.transform.SetParent(_rootParent, true);
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;

            _root = new GameObject("Fog Clustered Volume");
            _root.transform.position = Vector3.zero;
            _root.transform.rotation = Quaternion.identity;
            _root.transform.localScale = Vector3.one;
            if (_rootParent != null)
                _root.transform.SetParent(_rootParent, true);
        }

        private static void DestroyHandle(FogClusterMeshHandle handle)
        {
            if (handle == null)
                return;

            DestroyObject(handle.Mesh);
            DestroyObject(handle.GameObject);
        }

        private static void DestroyObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
