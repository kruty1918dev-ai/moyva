using System.Collections.Generic;
using System.Diagnostics;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridChunkSurfaceService : IConstructionBuildGridChunkSurfaceService
    {
        private const int BuildGridRenderQueue = 3990;
        private const string PlaneName = "ConstructionBuildGridChunkSurface";
        private const string OverlaysRootName = "Overlays";

        private static readonly int LineColorPropertyId = Shader.PropertyToID("_LineColor");
        private static readonly int FillColorPropertyId = Shader.PropertyToID("_FillColor");
        private static readonly int ValidLineColorPropertyId = Shader.PropertyToID("_ValidLineColor");
        private static readonly int ValidFillColorPropertyId = Shader.PropertyToID("_ValidFillColor");
        private static readonly int InvalidLineColorPropertyId = Shader.PropertyToID("_InvalidLineColor");
        private static readonly int InvalidFillColorPropertyId = Shader.PropertyToID("_InvalidFillColor");
        private static readonly int LineWidthPropertyId = Shader.PropertyToID("_LineWidth");
        private static readonly int EdgeMaskPropertyId = Shader.PropertyToID("_EdgeMask");
        private static readonly int GridOriginXZPropertyId = Shader.PropertyToID("_GridOriginXZ");
        private static readonly int CellSizeXZPropertyId = Shader.PropertyToID("_CellSizeXZ");
        private static readonly int UseCellMaskPropertyId = Shader.PropertyToID("_UseCellMask");
        private static readonly int CellMaskTexturePropertyId = Shader.PropertyToID("_CellMaskTex");
        private static readonly int ChunkTileOriginPropertyId = Shader.PropertyToID("_ChunkTileOrigin");
        private static readonly int ChunkTileSizePropertyId = Shader.PropertyToID("_ChunkTileSize");
        private static readonly int SurfaceLiftPropertyId = Shader.PropertyToID("_SurfaceLift");
        private static readonly int MinUpNormalYPropertyId = Shader.PropertyToID("_MinUpNormalY");

        private readonly IMapChunkLayoutService _chunkLayout;
        private readonly IMapVisualChunkRootService _chunkRoots;
        private readonly IMapVisualChunkRegistry _chunkRegistry;
        private readonly IConstructionBuildGridChunkSurfaceBuilder _builder;
        private readonly IConstructionBuildGridTileFilter _tileFilter;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly IConstructionBuildGridDiagnostics _diagnostics;
        private readonly Dictionary<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> _handles = new();
        private readonly Queue<MapChunkCoord> _geometryQueue = new();
        private readonly Queue<MapChunkCoord> _maskQueue = new();
        private readonly HashSet<MapChunkCoord> _queuedGeometry = new();
        private readonly HashSet<MapChunkCoord> _queuedMasks = new();
        private readonly HashSet<MapChunkCoord> _fullMaskUpdates = new();
        private readonly Dictionary<MapChunkCoord, RectInt> _dirtyMaskRegions = new();

        private Material _material;
        private bool _visible;
        private int _maskRevision;

        [Inject]
        public ConstructionBuildGridChunkSurfaceService(
            IMapChunkLayoutService chunkLayout,
            IMapVisualChunkRootService chunkRoots,
            IConstructionBuildGridChunkSurfaceBuilder builder,
            IConstructionBuildGridTileFilter tileFilter,
            [InjectOptional] IMapVisualChunkRegistry chunkRegistry = null,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null,
            [InjectOptional] IConstructionBuildGridDiagnostics diagnostics = null)
        {
            _chunkLayout = chunkLayout;
            _chunkRoots = chunkRoots;
            _builder = builder;
            _tileFilter = tileFilter;
            _chunkRegistry = chunkRegistry;
            _gridGeometry = gridGeometry;
            _settingsProvider = settingsProvider;
            _diagnostics = diagnostics;
        }

        public bool MaterialReady => _material != null;
        public bool IsUpdating => _geometryQueue.Count > 0 || _maskQueue.Count > 0;

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
            _material.SetColor(ValidLineColorPropertyId, new Color(0.28f, 1f, 0.42f, lineColor.a));
            _material.SetColor(ValidFillColorPropertyId, new Color(0.20f, 0.82f, 0.32f, fillColor.a));
            _material.SetColor(InvalidLineColorPropertyId, new Color(1f, 0.26f, 0.22f, lineColor.a));
            _material.SetColor(InvalidFillColorPropertyId, new Color(0.92f, 0.12f, 0.10f, fillColor.a));
            _material.SetFloat(LineWidthPropertyId, lineWidth);
        }

        public void ResetWorld()
        {
            Clear();
            EnsureVisibleChunks(invalidateMasks: true);
        }

        public void EnsureVisibleChunks(bool invalidateMasks)
        {
            if (!CanBuild())
                return;

            IReadOnlyList<MapChunkDescriptor> chunks = _chunkLayout.Chunks;
            for (int i = 0; i < chunks.Count; i++)
            {
                MapChunkDescriptor descriptor = chunks[i];
                if (!IsCameraVisible(descriptor.Coord))
                    continue;

                if (!_handles.ContainsKey(descriptor.Coord))
                    EnqueueGeometry(descriptor.Coord);
                else if (invalidateMasks
                         || _handles[descriptor.Coord].MaskDirty
                         || _handles[descriptor.Coord].AppliedMaskRevision != _maskRevision)
                {
                    if (invalidateMasks || _handles[descriptor.Coord].AppliedMaskRevision != _maskRevision)
                        _fullMaskUpdates.Add(descriptor.Coord);
                    EnqueueMask(descriptor.Coord);
                }
            }

            ApplyChunkVisibility();
        }

        public void InvalidateAllMasks()
        {
            _maskRevision++;
            _dirtyMaskRegions.Clear();
            if (_handles.Count == 0)
            {
                EnsureVisibleChunks(invalidateMasks: true);
                return;
            }

            foreach (KeyValuePair<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> pair in _handles)
            {
                _fullMaskUpdates.Add(pair.Key);
                if (IsCameraVisible(pair.Key))
                    EnqueueMask(pair.Key);
                else
                    pair.Value.MaskDirty = true;
            }

            EnsureVisibleChunks(invalidateMasks: false);
        }

        public void InvalidateRegion(Vector2Int center, int radius)
        {
            if (_chunkLayout == null || !_chunkLayout.IsConfigured)
                return;

            int safeRadius = Mathf.Max(0, radius);
            Vector2Int minTile = new(
                Mathf.Clamp(center.x - safeRadius, 0, Mathf.Max(0, _chunkLayout.Width - 1)),
                Mathf.Clamp(center.y - safeRadius, 0, Mathf.Max(0, _chunkLayout.Height - 1)));
            Vector2Int maxTile = new(
                Mathf.Clamp(center.x + safeRadius, 0, Mathf.Max(0, _chunkLayout.Width - 1)),
                Mathf.Clamp(center.y + safeRadius, 0, Mathf.Max(0, _chunkLayout.Height - 1)));
            if (!_chunkLayout.TryGetChunkCoord(minTile, out MapChunkCoord minCoord)
                || !_chunkLayout.TryGetChunkCoord(maxTile, out MapChunkCoord maxCoord))
            {
                InvalidateAllMasks();
                return;
            }

            for (int chunkX = minCoord.X; chunkX <= maxCoord.X; chunkX++)
            {
                for (int chunkY = minCoord.Y; chunkY <= maxCoord.Y; chunkY++)
                {
                    var coord = new MapChunkCoord(chunkX, chunkY);
                    if (_handles.ContainsKey(coord))
                    {
                        ConstructionBuildGridChunkSurfaceHandle handle = _handles[coord];
                        handle.MaskDirty = true;
                        RectInt dirtyRegion = IntersectTileRects(
                            handle.TileRect,
                            CreateTileRect(minTile.x, minTile.y, maxTile.x + 1, maxTile.y + 1));
                        if (dirtyRegion.width > 0 && dirtyRegion.height > 0)
                            MergeDirtyMaskRegion(coord, dirtyRegion);
                        if (IsCameraVisible(coord))
                            EnqueueMask(coord);
                    }
                    else if (IsCameraVisible(coord))
                        EnqueueGeometry(coord);
                }
            }
        }

        public void ProcessUpdates(float budgetMilliseconds)
        {
            if (!IsUpdating || !CanBuild())
                return;

            long startedAt = Stopwatch.GetTimestamp();
            double budgetTicks = Mathf.Max(0.1f, budgetMilliseconds) * Stopwatch.Frequency / 1000.0;
            bool processedAny = false;

            while (IsUpdating && (!processedAny || Stopwatch.GetTimestamp() - startedAt < budgetTicks))
            {
                if (_maskQueue.Count > 0)
                    ProcessNextMask();
                else
                    ProcessNextGeometry();

                processedAny = true;
            }

            ApplyChunkVisibility();
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (visible)
                EnsureVisibleChunks(invalidateMasks: false);
            ApplyChunkVisibility();
        }

        public void Hide() => SetVisible(false);

        public void ApplyChunkVisibility()
        {
            foreach (KeyValuePair<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> pair in _handles)
            {
                GameObject gameObject = pair.Value.GameObject;
                if (gameObject != null)
                    gameObject.SetActive(_visible && IsCameraVisible(pair.Key));
            }
        }

        public void Clear()
        {
            _geometryQueue.Clear();
            _maskQueue.Clear();
            _queuedGeometry.Clear();
            _queuedMasks.Clear();
            _fullMaskUpdates.Clear();
            _dirtyMaskRegions.Clear();

            foreach (ConstructionBuildGridChunkSurfaceHandle handle in _handles.Values)
            {
                DestroyUnityObject(handle.CellMask);
                DestroyUnityObject(handle.Mesh);
                DestroyUnityObject(handle.GameObject);
            }

            _handles.Clear();
        }

        public void Dispose()
        {
            Clear();
            DestroyUnityObject(_material);
            _material = null;
        }

        private void ProcessNextGeometry()
        {
            MapChunkCoord coord = _geometryQueue.Dequeue();
            _queuedGeometry.Remove(coord);

            if (_handles.ContainsKey(coord)
                || !_chunkLayout.TryGetDescriptor(coord, out MapChunkDescriptor descriptor))
            {
                return;
            }

            BuildChunk(descriptor);
        }

        private void ProcessNextMask()
        {
            MapChunkCoord coord = _maskQueue.Dequeue();
            _queuedMasks.Remove(coord);
            if (!_handles.TryGetValue(coord, out ConstructionBuildGridChunkSurfaceHandle handle))
                return;

            bool requiresFullUpdate = _fullMaskUpdates.Remove(coord);
            RectInt updateRect = !requiresFullUpdate
                                 && _dirtyMaskRegions.TryGetValue(coord, out RectInt dirtyRegion)
                ? dirtyRegion
                : handle.TileRect;
            _dirtyMaskRegions.Remove(coord);
            UpdateMask(handle, updateRect);
        }

        private void UpdateMask(ConstructionBuildGridChunkSurfaceHandle handle, RectInt updateRect)
        {
            byte[] buffer = handle.CellMaskBuffer;
            RectInt rect = handle.TileRect;
            int general = 0;
            int valid = 0;
            int invalid = 0;
            int hidden = 0;
            for (int tileY = updateRect.yMin; tileY < updateRect.yMax; tileY++)
            {
                int rowStart = (tileY - rect.yMin) * rect.width;
                for (int tileX = updateRect.xMin; tileX < updateRect.xMax; tileX++)
                {
                    var tile = new Vector2Int(tileX, tileY);
                    ConstructionBuildGridTileVisualState visualState = _tileFilter.ResolveVisualState(tile);
                    CountVisualState(visualState, ref general, ref valid, ref invalid, ref hidden);
                    buffer[rowStart + tileX - rect.xMin] = EncodeVisualState(visualState);
                }
            }

            handle.CellMask.SetPixelData(buffer, 0);
            handle.CellMask.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            handle.AppliedMaskRevision = _maskRevision;
            handle.MaskDirty = false;
            _diagnostics?.LogChunkMaskUpdated(updateRect, general, valid, invalid, hidden);
        }

        private void BuildChunk(MapChunkDescriptor descriptor)
        {
            if (!_builder.TryBuild(descriptor, out Mesh mesh) || mesh == null)
                return;

            Transform chunkRoot = _chunkRoots.GetOrCreateRoot(descriptor.Coord);
            if (chunkRoot == null)
            {
                DestroyUnityObject(mesh);
                return;
            }

            Transform overlaysRoot = GetOrCreateOverlaysRoot(chunkRoot);
            DestroyExistingPlane(overlaysRoot);

            GameObject gameObject = new(PlaneName);
            gameObject.transform.SetParent(overlaysRoot, false);

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterials = BuildMaterialArray(mesh);

            RectInt rect = descriptor.TileRect;
            byte[] maskBuffer = new byte[Mathf.Max(1, rect.width * rect.height)];
            Texture2D cellMask = CreateCellMask(descriptor, maskBuffer);
            ApplyRendererProperties(meshRenderer, descriptor, cellMask);

            var handle = new ConstructionBuildGridChunkSurfaceHandle(
                descriptor.Coord,
                gameObject,
                mesh,
                meshRenderer,
                rect,
                cellMask,
                maskBuffer);
            _handles[descriptor.Coord] = handle;
            PopulateMask(handle);
        }

        private Texture2D CreateCellMask(MapChunkDescriptor descriptor, byte[] initialBuffer)
        {
            RectInt rect = descriptor.TileRect;
            var texture = new Texture2D(
                Mathf.Max(1, rect.width),
                Mathf.Max(1, rect.height),
                TextureFormat.R8,
                mipChain: false,
                linear: true)
            {
                name = $"ConstructionBuildGridMask_{descriptor.Coord.X}_{descriptor.Coord.Y}",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixelData(initialBuffer, 0);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return texture;
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
            MapChunkDescriptor descriptor,
            Texture2D cellMask)
        {
            var block = new MaterialPropertyBlock();
            block.SetVector(EdgeMaskPropertyId, Vector4.one);
            block.SetFloat(UseCellMaskPropertyId, 1f);
            block.SetTexture(CellMaskTexturePropertyId, cellMask);
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

        private void EnqueueGeometry(MapChunkCoord coord)
        {
            if (_queuedGeometry.Add(coord))
                _geometryQueue.Enqueue(coord);
        }

        private void EnqueueMask(MapChunkCoord coord)
        {
            if (_queuedMasks.Add(coord))
                _maskQueue.Enqueue(coord);
        }

        private bool CanBuild()
            => _material != null
                && _chunkLayout != null
                && _chunkLayout.IsConfigured
                && _chunkRoots != null
                && _builder != null
                && _tileFilter != null;

        private bool IsCameraVisible(MapChunkCoord coord)
            => _chunkRegistry == null || _chunkRegistry.IsCameraVisible(coord);

        private static byte EncodeVisualState(ConstructionBuildGridTileVisualState state)
        {
            return state switch
            {
                ConstructionBuildGridTileVisualState.General => 85,
                ConstructionBuildGridTileVisualState.Invalid => 170,
                ConstructionBuildGridTileVisualState.Valid => byte.MaxValue,
                _ => byte.MinValue,
            };
        }

        private void PopulateMask(ConstructionBuildGridChunkSurfaceHandle handle)
        {
            if (handle == null)
                return;
            UpdateMask(handle, handle.TileRect);
        }

        private void MergeDirtyMaskRegion(MapChunkCoord coord, RectInt region)
        {
            if (!_dirtyMaskRegions.TryGetValue(coord, out RectInt existing))
            {
                _dirtyMaskRegions[coord] = region;
                return;
            }

            _dirtyMaskRegions[coord] = CreateTileRect(
                Mathf.Min(existing.xMin, region.xMin),
                Mathf.Min(existing.yMin, region.yMin),
                Mathf.Max(existing.xMax, region.xMax),
                Mathf.Max(existing.yMax, region.yMax));
        }

        private static RectInt IntersectTileRects(RectInt first, RectInt second)
        {
            int xMin = Mathf.Max(first.xMin, second.xMin);
            int yMin = Mathf.Max(first.yMin, second.yMin);
            int xMax = Mathf.Min(first.xMax, second.xMax);
            int yMax = Mathf.Min(first.yMax, second.yMax);
            return xMax > xMin && yMax > yMin
                ? CreateTileRect(xMin, yMin, xMax, yMax)
                : new RectInt();
        }

        private static RectInt CreateTileRect(int xMin, int yMin, int xMax, int yMax)
            => new RectInt(xMin, yMin, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));

        private static void CountVisualState(
            ConstructionBuildGridTileVisualState state,
            ref int general,
            ref int valid,
            ref int invalid,
            ref int hidden)
        {
            switch (state)
            {
                case ConstructionBuildGridTileVisualState.General:
                    general++;
                    break;
                case ConstructionBuildGridTileVisualState.Valid:
                    valid++;
                    break;
                case ConstructionBuildGridTileVisualState.Invalid:
                    invalid++;
                    break;
                default:
                    hidden++;
                    break;
            }
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

            GameObject gameObject = new(OverlaysRootName);
            gameObject.transform.SetParent(chunkRoot, false);
            return gameObject.transform;
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
