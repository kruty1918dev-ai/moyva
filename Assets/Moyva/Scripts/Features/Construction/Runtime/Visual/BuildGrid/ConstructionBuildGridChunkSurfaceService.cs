
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
        private static readonly int GridOriginXZPropertyId = Shader.PropertyToID("_GridOriginXZ");
        private static readonly int CellSizeXZPropertyId = Shader.PropertyToID("_CellSizeXZ");
        private static readonly int UseCellMaskPropertyId = Shader.PropertyToID("_UseCellMask");
        private static readonly int ChunkTileOriginPropertyId = Shader.PropertyToID("_ChunkTileOrigin");
        private static readonly int ChunkTileSizePropertyId = Shader.PropertyToID("_ChunkTileSize");
        private static readonly int SurfaceLiftPropertyId = Shader.PropertyToID("_SurfaceLift");
        private static readonly int MinUpNormalYPropertyId = Shader.PropertyToID("_MinUpNormalY");

        private readonly IMapChunkLayoutService _chunkLayout;
        private readonly IMapVisualChunkRootService _chunkRoots;
        private readonly IMapVisualChunkRegistry _chunkRegistry;
        private readonly IConstructionBuildGridChunkSurfaceBuilder _builder;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly Dictionary<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> _handles = new();

        private Material _material;
        private bool _visible;

        [Inject]
        public ConstructionBuildGridChunkSurfaceService(
            IMapChunkLayoutService chunkLayout,
            IMapVisualChunkRootService chunkRoots,
            IConstructionBuildGridChunkSurfaceBuilder builder,
            [InjectOptional] IMapVisualChunkRegistry chunkRegistry = null,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null)
        {
            _chunkLayout = chunkLayout;
            _chunkRoots = chunkRoots;
            _builder = builder;
            _chunkRegistry = chunkRegistry;
            _gridGeometry = gridGeometry;
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

            ApplySharedMaterialProperties();
        }

        public void ApplyStyle(Color lineColor, Color fillColor, float lineWidth)
        {
            if (_material == null)
                return;

            ApplySharedMaterialProperties();
            _material.SetColor(LineColorPropertyId, lineColor);
            _material.SetColor(FillColorPropertyId, fillColor);
            _material.SetFloat(LineWidthPropertyId, lineWidth);
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
                meshRenderer.sharedMaterials = BuildMaterialArray(mesh);
                ApplyRendererProperties(meshRenderer, descriptor);

                var handle = new ConstructionBuildGridChunkSurfaceHandle(
                    descriptor.Coord,
                    go,
                    mesh,
                    meshRenderer);
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

        private void ApplySharedMaterialProperties()
        {
            if (_material == null)
                return;

            _material.SetVector(EdgeMaskPropertyId, Vector4.one);
            _material.SetFloat(UseCellMaskPropertyId, 0f);
            _material.SetFloat(SurfaceLiftPropertyId, 0f);
            _material.SetFloat(MinUpNormalYPropertyId, 0.2f);

            if (_gridGeometry != null
                && _gridGeometry.TryGetCellSize(out Vector2 cellSize)
                && _gridGeometry.TryGetCellCenter(Vector2Int.zero, out Vector3 center))
            {
                _material.SetVector(GridOriginXZPropertyId, new Vector4(
                    center.x - cellSize.x * 0.5f,
                    center.z - cellSize.y * 0.5f,
                    0f,
                    0f));
                _material.SetVector(CellSizeXZPropertyId, new Vector4(cellSize.x, cellSize.y, 0f, 0f));
            }
        }

        private void ApplyRendererProperties(
            MeshRenderer renderer,
            MapChunkDescriptor descriptor)
        {
            if (renderer == null)
                return;

            var block = new MaterialPropertyBlock();
            block.SetVector(EdgeMaskPropertyId, Vector4.one);
            block.SetFloat(UseCellMaskPropertyId, 0f);
            block.SetVector(ChunkTileOriginPropertyId, new Vector4(
                descriptor.TileRect.xMin,
                descriptor.TileRect.yMin,
                0f,
                0f));
            block.SetVector(ChunkTileSizePropertyId, new Vector4(
                descriptor.TileRect.width,
                descriptor.TileRect.height,
                0f,
                0f));
            block.SetFloat(SurfaceLiftPropertyId, 0f);
            block.SetFloat(MinUpNormalYPropertyId, 0.2f);

            if (_gridGeometry != null
                && _gridGeometry.TryGetCellSize(out Vector2 cellSize)
                && _gridGeometry.TryGetCellCenter(Vector2Int.zero, out Vector3 center))
            {
                block.SetVector(GridOriginXZPropertyId, new Vector4(
                    center.x - cellSize.x * 0.5f,
                    center.z - cellSize.y * 0.5f,
                    0f,
                    0f));
                block.SetVector(CellSizeXZPropertyId, new Vector4(cellSize.x, cellSize.y, 0f, 0f));
            }

            renderer.SetPropertyBlock(block);
        }

        private Material[] BuildMaterialArray(Mesh mesh)
        {
            int count = Mathf.Max(1, mesh != null ? mesh.subMeshCount : 1);
            var materials = new Material[count];
            for (int i = 0; i < count; i++)
                materials[i] = _material;
            return materials;
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
