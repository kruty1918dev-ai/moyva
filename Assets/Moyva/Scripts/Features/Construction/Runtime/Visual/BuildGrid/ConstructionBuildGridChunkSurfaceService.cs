
using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridChunkSurfaceService : IConstructionBuildGridChunkSurfaceService
    {
        private const int BuildGridRenderQueue = 3990;
        private const string PlaneName = "ConstructionBuildGridChunkSurface";
        private const string OverlaysRootName = "Overlays";
        private static readonly int LineColorPropertyId = Shader.PropertyToID("_LineColor");
        private static readonly int FillColorPropertyId = Shader.PropertyToID("_FillColor");
        private static readonly int LineWidthPropertyId = Shader.PropertyToID("_LineWidth");
        private static readonly int EdgeMaskPropertyId = Shader.PropertyToID("_EdgeMask");

        private readonly IMapChunkLayoutService _chunkLayout;
        private readonly IMapVisualChunkRootService _chunkRoots;
        private readonly IMapVisualChunkRegistry _chunkRegistry;
        private readonly IConstructionBuildGridChunkSurfaceBuilder _builder;
        private readonly Dictionary<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> _handles = new();

        private Material _material;
        private bool _visible;

        [Inject]
        public ConstructionBuildGridChunkSurfaceService(
            IMapChunkLayoutService chunkLayout,
            IMapVisualChunkRootService chunkRoots,
            IConstructionBuildGridChunkSurfaceBuilder builder,
            [InjectOptional] IMapVisualChunkRegistry chunkRegistry = null)
        {
            _chunkLayout = chunkLayout;
            _chunkRoots = chunkRoots;
            _builder = builder;
            _chunkRegistry = chunkRegistry;
        }

        public bool MaterialReady => _material != null;

        public void Initialize(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[ConstructionBuildGridChunkSurface] Shader '{shaderName}' not found. Chunk surface grid is disabled.");
                return;
            }

            _material = new Material(shader)
            {
                name = "ConstructionBuildGridChunkSurface_Material",
                renderQueue = BuildGridRenderQueue
            };

            _material.SetVector(EdgeMaskPropertyId, Vector4.one);
        }

        public void ApplyStyle(Color lineColor, Color fillColor, float lineWidth)
        {
            if (_material == null)
                return;

            _material.SetColor(LineColorPropertyId, lineColor);
            _material.SetColor(FillColorPropertyId, fillColor);
            _material.SetFloat(LineWidthPropertyId, lineWidth);
            _material.SetVector(EdgeMaskPropertyId, Vector4.one);
        }

        public void Rebuild()
        {
            Clear();

            if (_material == null || _chunkLayout == null || !_chunkLayout.IsConfigured || _chunkRoots == null || _builder == null)
                return;

            IReadOnlyList<MapChunkDescriptor> chunks = _chunkLayout.Chunks;
            for (int i = 0; i < chunks.Count; i++)
            {
                MapChunkDescriptor descriptor = chunks[i];
                if (!_builder.TryBuild(descriptor, out Mesh mesh) || mesh == null)
                    continue;

                Transform chunkRoot = _chunkRoots.GetOrCreateRoot(descriptor.Coord);
                if (chunkRoot == null)
                {
                    DestroyUnityObject(mesh);
                    continue;
                }

                Transform overlaysRoot = GetOrCreateOverlaysRoot(chunkRoot);
                DestroyExistingPlane(overlaysRoot);

                GameObject go = new GameObject(PlaneName);
                go.transform.SetParent(overlaysRoot, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = mesh;
                meshRenderer.sharedMaterial = _material;

                var handle = new ConstructionBuildGridChunkSurfaceHandle(descriptor.Coord, go, mesh, meshRenderer);
                _handles[descriptor.Coord] = handle;
            }

            ApplyChunkVisibility();
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            ApplyChunkVisibility();
        }

        public void Hide() => SetVisible(false);

        public void ApplyChunkVisibility()
        {
            foreach (var pair in _handles)
            {
                ConstructionBuildGridChunkSurfaceHandle handle = pair.Value;
                if (handle.GameObject == null)
                    continue;

                bool cameraVisible = _chunkRegistry == null || _chunkRegistry.IsCameraVisible(pair.Key);
                handle.GameObject.SetActive(_visible && cameraVisible);
            }
        }

        public void Clear()
        {
            foreach (ConstructionBuildGridChunkSurfaceHandle handle in _handles.Values)
            {
                if (handle.Mesh != null)
                    DestroyUnityObject(handle.Mesh);

                if (handle.GameObject != null)
                    DestroyUnityObject(handle.GameObject);
            }

            _handles.Clear();
        }

        private static Transform GetOrCreateOverlaysRoot(Transform chunkRoot)
        {
            Transform existing = chunkRoot.Find(OverlaysRootName);
            if (existing != null)
                return existing;

            GameObject go = new GameObject(OverlaysRootName);
            go.transform.SetParent(chunkRoot, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        private static void DestroyExistingPlane(Transform overlaysRoot)
        {
            Transform existing = overlaysRoot != null ? overlaysRoot.Find(PlaneName) : null;
            if (existing != null)
                DestroyUnityObject(existing.gameObject);
        }

        private static void DestroyUnityObject(Object unityObject)
        {
            if (unityObject == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(unityObject);
            else
                Object.DestroyImmediate(unityObject);
        }
    }
}
