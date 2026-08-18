using System.Collections.Generic;
using System.Diagnostics;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
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
        private static readonly int UnaffordableLineColorPropertyId = Shader.PropertyToID("_UnaffordableLineColor");
        private static readonly int UnaffordableFillColorPropertyId = Shader.PropertyToID("_UnaffordableFillColor");
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
        private readonly IFogStateReader _fogStateReader;
        private readonly Dictionary<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> _handles = new();
        private readonly Dictionary<MapChunkCoord, bool> _chunkFogVisibilityCache = new();
        private readonly Queue<MapChunkCoord> _geometryQueue = new();
        private readonly Queue<MapChunkCoord> _maskQueue = new();
        private readonly HashSet<MapChunkCoord> _queuedGeometry = new();
        private readonly HashSet<MapChunkCoord> _queuedMasks = new();
        private readonly HashSet<MapChunkCoord> _fullMaskUpdates = new();
        private readonly Dictionary<MapChunkCoord, RectInt> _dirtyMaskRegions = new();

        /*
         * A mask is evaluated incrementally, one tile row at a time.
         * The Texture2D is updated only when the complete rectangle is ready.
         */
        private MapChunkCoord? _activeMaskCoord;
        private RectInt _activeMaskRect;
        private int _activeMaskNextY;
        private int _activeMaskRevision;
        private int _activeMaskGeneral;
        private int _activeMaskValid;
        private int _activeMaskInvalid;
        private int _activeMaskHidden;

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
            [InjectOptional] IConstructionBuildGridDiagnostics diagnostics = null,
            [InjectOptional] IFogStateReader fogStateReader = null)
        {
            _chunkLayout = chunkLayout;
            _chunkRoots = chunkRoots;
            _builder = builder;
            _tileFilter = tileFilter;
            _chunkRegistry = chunkRegistry;
            _gridGeometry = gridGeometry;
            _settingsProvider = settingsProvider;
            _diagnostics = diagnostics;
            _fogStateReader = fogStateReader;
        }

        public bool MaterialReady => _material != null;
        public bool IsUpdating =>
            _activeMaskCoord.HasValue
            || _geometryQueue.Count > 0
            || _maskQueue.Count > 0;

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
            _material.SetColor(
                ValidLineColorPropertyId,
                ResolveConfiguredColor(
                    _settingsProvider?.BuildGridValidLineColor
                        ?? new Color(0.28f, 1f, 0.42f, 1f),
                    lineColor.a));
            _material.SetColor(
                ValidFillColorPropertyId,
                ResolveConfiguredColor(
                    _settingsProvider?.BuildGridValidFillColor
                        ?? new Color(0.20f, 0.82f, 0.32f, 1f),
                    fillColor.a));
            _material.SetColor(
                UnaffordableLineColorPropertyId,
                ResolveConfiguredColor(
                    _settingsProvider?.BuildGridUnaffordableLineColor
                        ?? new Color(1f, 0.68f, 0.12f, 1f),
                    lineColor.a));
            _material.SetColor(
                UnaffordableFillColorPropertyId,
                ResolveConfiguredColor(
                    _settingsProvider?.BuildGridUnaffordableFillColor
                        ?? new Color(0.95f, 0.48f, 0.08f, 1f),
                    fillColor.a));
            _material.SetColor(
                InvalidLineColorPropertyId,
                ResolveConfiguredColor(
                    _settingsProvider?.BuildGridInvalidLineColor
                        ?? new Color(1f, 0.26f, 0.22f, 1f),
                    lineColor.a));
            _material.SetColor(
                InvalidFillColorPropertyId,
                ResolveConfiguredColor(
                    _settingsProvider?.BuildGridInvalidFillColor
                        ?? new Color(0.92f, 0.12f, 0.10f, 1f),
                    fillColor.a));
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

                bool hasVisibleFogCells =
                    HasVisibleFogCells(
                        descriptor.Coord,
                        descriptor.TileRect);

                if (!hasVisibleFogCells)
                {
                    if (_handles.TryGetValue(
                            descriptor.Coord,
                            out ConstructionBuildGridChunkSurfaceHandle hiddenHandle))
                    {
                        hiddenHandle.MaskDirty = true;

                        if (hiddenHandle.GameObject != null
                            && hiddenHandle.GameObject.activeSelf)
                        {
                            hiddenHandle.GameObject.SetActive(false);
                        }
                    }

                    continue;
                }

                if (!_handles.TryGetValue(
                        descriptor.Coord,
                        out ConstructionBuildGridChunkSurfaceHandle handle))
                {
                    EnqueueGeometry(descriptor.Coord);
                }
                else if (invalidateMasks
                         || handle.MaskDirty
                         || handle.AppliedMaskRevision != _maskRevision)
                {
                    if (invalidateMasks
                        || handle.AppliedMaskRevision != _maskRevision)
                    {
                        _fullMaskUpdates.Add(descriptor.Coord);
                    }

                    EnqueueMask(descriptor.Coord);
                }
            }

            ApplyChunkVisibility();
        }

        public void InvalidateAllMasks()
        {
            _maskRevision++;
            _dirtyMaskRegions.Clear();
            _chunkFogVisibilityCache.Clear();
            if (_handles.Count == 0)
            {
                EnsureVisibleChunks(invalidateMasks: true);
                return;
            }

            foreach (KeyValuePair<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> pair in _handles)
            {
                _fullMaskUpdates.Add(pair.Key);

                bool shouldProcess =
                    IsCameraVisible(pair.Key)
                    && HasVisibleFogCells(
                        pair.Key,
                        pair.Value.TileRect);

                if (shouldProcess)
                {
                    EnqueueMask(pair.Key);
                }
                else
                {
                    pair.Value.MaskDirty = true;

                    if (pair.Value.GameObject != null
                        && pair.Value.GameObject.activeSelf)
                    {
                        pair.Value.GameObject.SetActive(false);
                    }
                }
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
                    _chunkFogVisibilityCache.Remove(coord);

                    if (_handles.TryGetValue(
                            coord,
                            out ConstructionBuildGridChunkSurfaceHandle handle))
                    {
                        handle.MaskDirty = true;
                        RectInt dirtyRegion = IntersectTileRects(
                            handle.TileRect,
                            CreateTileRect(minTile.x, minTile.y, maxTile.x + 1, maxTile.y + 1));
                        if (dirtyRegion.width > 0 && dirtyRegion.height > 0)
                            MergeDirtyMaskRegion(coord, dirtyRegion);

                        bool shouldProcess =
                            IsCameraVisible(coord)
                            && HasVisibleFogCells(
                                coord,
                                handle.TileRect);

                        if (shouldProcess)
                        {
                            EnqueueMask(coord);
                        }
                        else if (handle.GameObject != null
                                 && handle.GameObject.activeSelf)
                        {
                            handle.GameObject.SetActive(false);
                        }
                    }
                    else if (IsCameraVisible(coord)
                             && _chunkLayout.TryGetDescriptor(
                                 coord,
                                 out MapChunkDescriptor descriptor)
                             && HasVisibleFogCells(
                                 coord,
                                 descriptor.TileRect))
                    {
                        EnqueueGeometry(coord);
                    }
                }
            }
        }

        public void ProcessUpdates(float budgetMilliseconds)
        {
            if (!IsUpdating || !CanBuild())
                return;

            long startedAt =
                Stopwatch.GetTimestamp();

            double budgetTicks =
                Mathf.Max(
                    0.1f,
                    budgetMilliseconds)
                * Stopwatch.Frequency
                / 1000.0;

            bool processedAny = false;

            while (IsUpdating
                   && (
                       !processedAny
                       || Stopwatch.GetTimestamp() - startedAt
                          < budgetTicks
                   ))
            {
                bool didWork;

                if (_activeMaskCoord.HasValue
                    || _maskQueue.Count > 0)
                {
                    /*
                     * One iteration evaluates at most one tile row.
                     * This makes the configured budget meaningful.
                     */
                    didWork =
                        ProcessNextMaskRow();
                }
                else if (_geometryQueue.Count > 0)
                {
                    /*
                     * Geometry remains one chunk per task. Pass 1D already
                     * prevents geometry for fully fogged chunks.
                     */
                    ProcessNextGeometry();
                    didWork = true;
                }
                else
                {
                    break;
                }

                processedAny |= didWork;
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
            foreach (KeyValuePair<MapChunkCoord, ConstructionBuildGridChunkSurfaceHandle> pair in _handles)
            {
                GameObject gameObject = pair.Value.GameObject;
                if (gameObject == null)
                    continue;

                bool shouldBeVisible =
                    _visible
                    && pair.Value.MaskReady
                    && IsCameraVisible(pair.Key)
                    && HasVisibleFogCells(
                        pair.Key,
                        pair.Value.TileRect);

                if (gameObject.activeSelf != shouldBeVisible)
                    gameObject.SetActive(shouldBeVisible);
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
            _chunkFogVisibilityCache.Clear();
            CancelActiveMaskUpdate();

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

            if (!IsCameraVisible(coord)
                || !HasVisibleFogCells(
                    coord,
                    descriptor.TileRect))
            {
                return;
            }

            BuildChunk(descriptor);
        }

        private bool ProcessNextMaskRow()
        {
            if (!_activeMaskCoord.HasValue
                && !TryBeginNextMaskUpdate())
            {
                return false;
            }

            MapChunkCoord coord =
                _activeMaskCoord.Value;

            if (!_handles.TryGetValue(
                    coord,
                    out ConstructionBuildGridChunkSurfaceHandle handle))
            {
                CancelActiveMaskUpdate();
                return false;
            }

            if (!IsCameraVisible(coord)
                || !HasVisibleFogCells(
                    coord,
                    handle.TileRect))
            {
                handle.MaskDirty = true;

                if (handle.GameObject != null
                    && handle.GameObject.activeSelf)
                {
                    handle.GameObject.SetActive(false);
                }

                CancelActiveMaskUpdate();
                return false;
            }

            if (_activeMaskNextY
                >= _activeMaskRect.yMax)
            {
                CompleteActiveMaskUpdate(handle);
                return true;
            }

            UpdateMaskRow(
                handle,
                _activeMaskRect,
                _activeMaskNextY);

            _activeMaskNextY++;

            if (_activeMaskNextY
                >= _activeMaskRect.yMax)
            {
                CompleteActiveMaskUpdate(handle);
            }

            return true;
        }

        private bool TryBeginNextMaskUpdate()
        {
            while (_maskQueue.Count > 0)
            {
                MapChunkCoord coord =
                    _maskQueue.Dequeue();

                _queuedMasks.Remove(coord);

                if (!_handles.TryGetValue(
                        coord,
                        out ConstructionBuildGridChunkSurfaceHandle handle))
                {
                    continue;
                }

                if (!IsCameraVisible(coord)
                    || !HasVisibleFogCells(
                        coord,
                        handle.TileRect))
                {
                    handle.MaskDirty = true;

                    if (handle.GameObject != null
                        && handle.GameObject.activeSelf)
                    {
                        handle.GameObject.SetActive(false);
                    }

                    continue;
                }

                bool requiresFullUpdate =
                    _fullMaskUpdates.Remove(coord);

                RectInt updateRect =
                    !requiresFullUpdate
                    && _dirtyMaskRegions.TryGetValue(
                        coord,
                        out RectInt dirtyRegion)
                        ? dirtyRegion
                        : handle.TileRect;

                _dirtyMaskRegions.Remove(coord);

                if (updateRect.width <= 0
                    || updateRect.height <= 0)
                {
                    continue;
                }

                _activeMaskCoord =
                    coord;

                _activeMaskRect =
                    updateRect;

                _activeMaskNextY =
                    updateRect.yMin;

                _activeMaskRevision =
                    _maskRevision;

                _activeMaskGeneral = 0;
                _activeMaskValid = 0;
                _activeMaskInvalid = 0;
                _activeMaskHidden = 0;

                return true;
            }

            return false;
        }

        private void UpdateMaskRow(
            ConstructionBuildGridChunkSurfaceHandle handle,
            RectInt updateRect,
            int tileY)
        {
            byte[] buffer =
                handle.CellMaskBuffer;

            RectInt rect =
                handle.TileRect;

            int rowStart =
                (tileY - rect.yMin)
                * rect.width;

            for (int tileX = updateRect.xMin;
                 tileX < updateRect.xMax;
                 tileX++)
            {
                var tile =
                    new Vector2Int(
                        tileX,
                        tileY);

                ConstructionBuildGridTileVisualState visualState =
                    _tileFilter.ResolveVisualState(tile);

                CountVisualState(
                    visualState,
                    ref _activeMaskGeneral,
                    ref _activeMaskValid,
                    ref _activeMaskInvalid,
                    ref _activeMaskHidden);

                buffer[
                    rowStart
                    + tileX
                    - rect.xMin
                ] =
                    EncodeVisualState(
                        visualState);
            }
        }

        private void CompleteActiveMaskUpdate(
            ConstructionBuildGridChunkSurfaceHandle handle)
        {
            MapChunkCoord coord =
                _activeMaskCoord.Value;

            RectInt completedRect =
                _activeMaskRect;

            int completedRevision =
                _activeMaskRevision;

            int general =
                _activeMaskGeneral;

            int valid =
                _activeMaskValid;

            int invalid =
                _activeMaskInvalid;

            int hidden =
                _activeMaskHidden;

            /*
             * Upload once, after every row has been evaluated.
             * The previously complete GPU texture remains visible while
             * the new CPU buffer is being prepared.
             */
            handle.CellMask.SetPixelData(
                handle.CellMaskBuffer,
                0);

            handle.CellMask.Apply(
                updateMipmaps: false,
                makeNoLongerReadable: false);

            handle.AppliedMaskRevision =
                completedRevision;

            handle.MaskReady =
                true;

            CancelActiveMaskUpdate();

            bool hasNewerPendingWork =
                _maskRevision != completedRevision
                || _queuedMasks.Contains(coord)
                || _fullMaskUpdates.Contains(coord)
                || _dirtyMaskRegions.ContainsKey(coord);

            handle.MaskDirty =
                hasNewerPendingWork;

            _diagnostics?.LogChunkMaskUpdated(
                completedRect,
                general,
                valid,
                invalid,
                hidden);

            if (hasNewerPendingWork
                && !_queuedMasks.Contains(coord))
            {
                EnqueueMask(coord);
            }
        }

        private void CancelActiveMaskUpdate()
        {
            _activeMaskCoord = null;
            _activeMaskRect = default;
            _activeMaskNextY = 0;
            _activeMaskRevision = -1;
            _activeMaskGeneral = 0;
            _activeMaskValid = 0;
            _activeMaskInvalid = 0;
            _activeMaskHidden = 0;
        }

        private void BuildChunk(MapChunkDescriptor descriptor)
        {
            if (!HasVisibleFogCells(
                    descriptor.Coord,
                    descriptor.TileRect))
            {
                return;
            }

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

        private bool HasVisibleFogCells(
            MapChunkCoord coord,
            RectInt tileRect)
        {
            if (_fogStateReader == null)
                return true;

            if (_chunkFogVisibilityCache.TryGetValue(
                    coord,
                    out bool cached))
            {
                return cached;
            }

            for (int tileY = tileRect.yMin;
                 tileY < tileRect.yMax;
                 tileY++)
            {
                for (int tileX = tileRect.xMin;
                     tileX < tileRect.xMax;
                     tileX++)
                {
                    if (!_fogStateReader.IsVisible(
                            new Vector2Int(
                                tileX,
                                tileY)))
                    {
                        continue;
                    }

                    _chunkFogVisibilityCache[coord] =
                        true;

                    return true;
                }
            }

            _chunkFogVisibilityCache[coord] =
                false;

            return false;
        }

        private static byte EncodeVisualState(ConstructionBuildGridTileVisualState state)
        {
            return state switch
            {
                ConstructionBuildGridTileVisualState.General => 85,
                ConstructionBuildGridTileVisualState.Invalid => 128,
                ConstructionBuildGridTileVisualState.Unaffordable => 192,
                ConstructionBuildGridTileVisualState.Valid => byte.MaxValue,
                _ => byte.MinValue,
            };
        }

        private void PopulateMask(ConstructionBuildGridChunkSurfaceHandle handle)
        {
            if (handle == null)
                return;

            /*
             * Initial texture contains only zero/Missing values.
             * Keep the renderer hidden until the complete first mask has
             * been evaluated under the regular frame budget.
             */
            handle.MaskDirty = true;
            handle.MaskReady = false;
            _fullMaskUpdates.Add(handle.Coord);
            EnqueueMask(handle.Coord);
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
                case ConstructionBuildGridTileVisualState.Unaffordable:
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

        private static Color ResolveConfiguredColor(Color configured, float fallbackAlpha)
        {
            if (configured.a <= 0f)
                configured.a = fallbackAlpha;
            return configured;
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
