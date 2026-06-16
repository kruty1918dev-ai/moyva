using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [RequireComponent(typeof(MeshRenderer))]
    public class FogQuadController : MonoBehaviour
    {
        private const int FogOverlaySortingOrder = short.MaxValue;
        private const int FogOverlayRenderQueue = 4000; // Overlay queue
        private const string DebugTag = "[MoyvaFogTrace]";
        private static readonly int GlobalFogCullEnabledId = Shader.PropertyToID("_MoyvaFogCullEnabled");
        private static readonly int GlobalFogCullThresholdId = Shader.PropertyToID("_MoyvaFogCullThreshold");
        private static readonly int GlobalFogWorldPlaneId = Shader.PropertyToID("_MoyvaFogWorldPlane");

        [SerializeField] private FogOfWarSettings _settings;

        private IFogOfWarService _fogService;
        private IFogTextureUpdater _textureUpdater;
        private IGridService _gridService;
        private IGridProjection _gridProjection;
        private SignalBus _signalBus;

        private Material _mat;
        private int _mapWidth = 10;
        private int _mapHeight = 10;
        private int _projectionMode = int.MinValue;
        private float _maxTerrainWorldY = float.MinValue;
        private float _worldCellSize = 1f;
        private bool _subscribed;

        [Inject]
        private void ConstructOptionalDependencies(
            [InjectOptional] IFogOfWarService fogService,
            [InjectOptional] IFogTextureUpdater textureUpdater,
            [InjectOptional] IGridService gridService,
            [InjectOptional] IGridProjection gridProjection,
            [InjectOptional] SignalBus signalBus)
        {
            _fogService = fogService;
            _textureUpdater = textureUpdater;
            _gridService = gridService;
            _gridProjection = gridProjection;
            _signalBus = signalBus;
        }

        private void Start()
        {
            int w = _gridService != null ? _gridService.GridWidth  : 10;
            int h = _gridService != null ? _gridService.GridHeight : 10;
            Debug.Log($"{DebugTag} FogQuad.Start grid={w}x{h}, hasGrid={_gridService != null}, hasFog={_fogService != null}, hasTextureUpdater={_textureUpdater != null}, hasSignalBus={_signalBus != null}.");
            InitializeOverlay(w, h);
            SubscribeToWorldGeneratedSignal();
        }

        private void Awake()
        {
            // Страховка від коротких артефактів при старті/перезавантаженні сцени:
            // глобальний fog-culling вимикаємо до повної ініціалізації fog texture.
            Shader.SetGlobalFloat(GlobalFogCullEnabledId, 0f);
            Shader.SetGlobalFloat(GlobalFogWorldPlaneId, 0f);
        }

        private void OnDestroy()
        {
            if (_signalBus != null && _subscribed)
                _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);

            Shader.SetGlobalFloat(GlobalFogCullEnabledId, 0f);
            Shader.SetGlobalFloat(GlobalFogWorldPlaneId, 0f);
            ReleaseMaterialInstance();
        }

        private void InitializeOverlay(int width, int height)
        {
            // На час (ре)ініціалізації тимчасово вимикаємо culling, щоб шейдер не
            // семплив застарілі глобали минулої сцени/карти.
            Shader.SetGlobalFloat(GlobalFogCullEnabledId, 0f);

            int w = Mathf.Max(1, width);
            int h = Mathf.Max(1, height);
            _mapWidth = w;
            _mapHeight = h;
            Debug.Log($"{DebugTag} FogQuad.InitializeOverlay begin map={w}x{h}, hasFog={_fogService != null}, hasTextureUpdater={_textureUpdater != null}, hasSettings={_settings != null}.");

            _projectionMode = _gridProjection != null ? (int)_gridProjection.ProjectionMode : (int)GridProjectionMode.Orthographic3D;

            Bounds worldBounds = ResolveWorldBounds(w, h);
            Vector2 edgePadding = ResolveEdgePaddingInCells();
            ApplyOverlayTransform(worldBounds, edgePadding);

            var mr = GetComponent<MeshRenderer>();
            if (_mat == null)
                _mat = mr.material;
            ApplyOverlayRenderPriority(mr);

            if (_mat == null)
            {
                Debug.LogWarning("[FogOfWar] FogQuadController: material не знайдено. Fog overlay вимкнено.");
                enabled = false;
                return;
            }

            if (_textureUpdater == null || _fogService == null)
            {
                Debug.LogWarning("[FogOfWar] FogQuadController: залежності не проінжекчені. Fog overlay вимкнено.");
                enabled = false;
                return;
            }

            _textureUpdater.Initialize(w, h, _mat);
            _fogService.Initialize(w, h);
            ApplyShaderWorldPlane();
            ApplyShaderCullingSettings();

            if (_settings != null)
                ApplySettingsToMaterial();

            Debug.Log($"{DebugTag} FogQuad.InitializeOverlay end map={_mapWidth}x{_mapHeight}, projection={_projectionMode}, position={transform.position}, scale={transform.localScale}.");
        }

        private void SubscribeToWorldGeneratedSignal()
        {
            if (_signalBus == null || _subscribed)
                return;

            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _subscribed = true;
            Debug.Log($"{DebugTag} FogQuad.SubscribeToWorldGeneratedSignal subscribed=true.");
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            int width = Mathf.Max(1, signal.Width);
            int height = Mathf.Max(1, signal.Height);
            float cellSize = ResolveSignalCellSize(signal);
            float maxTerrainWorldY = ResolveMaxTerrainWorldY(signal);
            bool terrainHeightChanged = !Mathf.Approximately(maxTerrainWorldY, _maxTerrainWorldY);
            bool cellSizeChanged = !Mathf.Approximately(cellSize, _worldCellSize);
            _maxTerrainWorldY = maxTerrainWorldY;
            _worldCellSize = cellSize;

            if (width == _mapWidth && height == _mapHeight && signal.ProjectionMode == _projectionMode && !terrainHeightChanged && !cellSizeChanged)
            {
                Debug.Log($"{DebugTag} FogQuad.OnWorldGenerated no-reinit signal={width}x{height}, current={_mapWidth}x{_mapHeight}, projection={signal.ProjectionMode}, cellSize={cellSize}, terrainHeightChanged={terrainHeightChanged}.");
                return;
            }

            Debug.Log($"{DebugTag} FogQuad.OnWorldGenerated reinit signal={width}x{height}, previous={_mapWidth}x{_mapHeight}, projection={_projectionMode}->{signal.ProjectionMode}, cellSize={_worldCellSize}, terrainHeightChanged={terrainHeightChanged}, cellSizeChanged={cellSizeChanged}.");
            InitializeOverlay(width, height);
        }

        private Bounds ResolveWorldBounds(int width, int height)
        {
            if (_worldCellSize > 0.0001f && _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
            {
                float safeWidth = Mathf.Max(1, width) * _worldCellSize;
                float safeDepth = Mathf.Max(1, height) * _worldCellSize;
                return new Bounds(
                    new Vector3((safeWidth - _worldCellSize) * 0.5f, 0f, (safeDepth - _worldCellSize) * 0.5f),
                    new Vector3(safeWidth, 1f, safeDepth));
            }

            if (_gridProjection != null)
                return _gridProjection.GetWorldBounds(width, height);

            return new Bounds(
                new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, 0f),
                new Vector3(width, height, 1f));
        }

        private static float ResolveSignalCellSize(WorldGeneratedDataSignal signal)
            => signal.CellSize > 0.0001f ? signal.CellSize : 1f;

        private void ApplyOverlayTransform(Bounds worldBounds, Vector2 edgePadding)
        {
            Vector2 edgePaddingWorld = ResolveEdgePaddingInWorldUnits(worldBounds, edgePadding);
            if (_gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
            {
                float topY = Resolve3DFogTopY(worldBounds);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                transform.localScale = new Vector3(worldBounds.size.x + edgePaddingWorld.x * 2f, worldBounds.size.z + edgePaddingWorld.y * 2f, 1f);
                transform.position = new Vector3(worldBounds.center.x, topY, worldBounds.center.z);
                return;
            }

            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(worldBounds.size.x + edgePaddingWorld.x * 2f, worldBounds.size.y + edgePaddingWorld.y * 2f, 1f);
            transform.position = new Vector3(worldBounds.center.x, worldBounds.center.y, -0.5f);
        }

        private float Resolve3DFogTopY(Bounds worldBounds)
        {
            float terrainTop = _maxTerrainWorldY > float.MinValue * 0.5f
                ? Mathf.Max(worldBounds.max.y, _maxTerrainWorldY)
                : worldBounds.max.y;
            float clearance = _settings != null ? Mathf.Max(0f, _settings.Fog3DTopClearance) : 0.08f;
            return terrainTop + clearance;
        }

        private float ResolveMaxTerrainWorldY(WorldGeneratedDataSignal signal)
        {
            if (_gridProjection == null || _gridProjection.WorldPlane != GridWorldPlane.XZ)
                return float.MinValue;

            float maxElevation = float.MinValue;
            if (signal.TerrainLevelMap != null)
            {
                int width = Mathf.Min(Mathf.Max(0, signal.Width), signal.TerrainLevelMap.GetLength(0));
                int height = Mathf.Min(Mathf.Max(0, signal.Height), signal.TerrainLevelMap.GetLength(1));
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    maxElevation = Mathf.Max(maxElevation, signal.TerrainLevelMap[x, y]);
            }

            if (maxElevation == float.MinValue && signal.HeightMap != null)
            {
                int width = Mathf.Min(Mathf.Max(0, signal.Width), signal.HeightMap.GetLength(0));
                int height = Mathf.Min(Mathf.Max(0, signal.Height), signal.HeightMap.GetLength(1));
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    maxElevation = Mathf.Max(maxElevation, signal.HeightMap[x, y]);
            }

            if (maxElevation == float.MinValue)
                return float.MinValue;

            return _gridProjection.GridToWorld(Vector2Int.zero, maxElevation, 0f).y;
        }

        private void ApplySettingsToMaterial()
        {
            // Fog colors
            _mat.SetColor("_UnexploredColor", _settings.UnexploredColor);
            _mat.SetColor("_ExploredColor",   _settings.ExploredColor);

            _mat.SetVector("_FogTileUVRect", new Vector4(0f, 0f, 1f, 1f));

            // Fog tile texture (tiled across fog overlay)
            if (_settings.FogTileSprite != null)
            {
                Texture2D tileTexture = _settings.FogTileSprite.texture;
                if (tileTexture != null)
                {
                    _mat.SetTexture("_FogTileTex", tileTexture);
                    _mat.SetVector("_FogTileUVRect", BuildSpriteUvRect(
                        _settings.FogTileSprite,
                        tileTexture,
                        _settings.FogTileSpritePixelSize));
                }
            }

            _mat.SetVector("_FogIconUVRect", new Vector4(0f, 0f, 1f, 1f));
            _mat.SetFloat("_UseFogIcons", 0f);

            // Fog icon texture (sample exact sprite rect from atlas)
            if (_settings.FogIconSprites != null && _settings.FogIconSprites.Length > 0)
            {
                // Use first sprite as source icon and pass its UV rect explicitly.
                Sprite iconSprite = _settings.FogIconSprites[0];
                Texture2D iconTexture = iconSprite != null ? iconSprite.texture : null;
                if (iconTexture != null)
                {
                    _mat.SetTexture("_FogIconTex", iconTexture);
                    _mat.SetVector("_FogIconUVRect", BuildSpriteUvRect(
                        iconSprite,
                        iconTexture,
                        _settings.FogIconSpritePixelSize));
                    _mat.SetFloat("_UseFogIcons", 1f);
                }
                
                // Determine grid size from icon count
                // Assume square grid: iconCount = gridSize^2
                int iconCount = _settings.FogIconSprites.Length;
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(iconCount));
                _mat.SetFloat("_IconGridSize", gridSize);
            }
            else
            {
                _mat.SetFloat("_IconGridSize", 1f);
            }

            // Tiling and scaling parameters
            _mat.SetFloat("_FogTileTiling", _settings.FogTileTiling);
            _mat.SetVector("_FogTileSpritePixelSize", new Vector4(
                Mathf.Max(1, _settings.FogTileSpritePixelSize.x),
                Mathf.Max(1, _settings.FogTileSpritePixelSize.y),
                0f,
                0f));
            _mat.SetVector("_FogTileSizeInCells", new Vector4(
                Mathf.Max(0.001f, _settings.FogTileSizeInCells.x),
                Mathf.Max(0.001f, _settings.FogTileSizeInCells.y),
                0f,
                0f));
            _mat.SetFloat("_FogTileSeamOverlapPixels", Mathf.Max(0f, _settings.FogTileSeamOverlapPixels));
            ApplyOverlayUvRemap();
            _mat.SetFloat("_FogIconScale", _settings.FogIconScale);
            _mat.SetVector("_FogIconGridSize", new Vector4(
                Mathf.Max(1, _settings.FogIconGridSize.x),
                Mathf.Max(1, _settings.FogIconGridSize.y),
                0f,
                0f));

            // Transparency blending
            _mat.SetFloat("_UnexploredAlpha", _settings.UnexploredAlpha);
            _mat.SetFloat("_ExploredAlpha", _settings.ExploredAlpha);
            
            // Icon blend intensity (how much icon blends into fog)
            _mat.SetFloat("_FogIconIntensity", 0.6f);
            ApplyShaderCullingSettings();
        }

        private void ApplyShaderCullingSettings()
        {
            bool enabled = _settings == null || _settings.EnableShaderFogCulling;
            float threshold = _settings != null ? _settings.ShaderFogCullThreshold : 0.01f;
            Shader.SetGlobalFloat(GlobalFogCullEnabledId, enabled ? 1f : 0f);
            Shader.SetGlobalFloat(GlobalFogCullThresholdId, Mathf.Clamp(threshold, 0f, 0.25f));
        }

        private void ApplyShaderWorldPlane()
        {
            bool usesXzPlane = _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ;
            Shader.SetGlobalFloat(GlobalFogWorldPlaneId, usesXzPlane ? 1f : 0f);
        }

        private static Vector4 BuildSpriteUvRect(Sprite sprite, Texture2D texture, Vector2Int pixelSize)
        {
            if (sprite == null || texture == null)
                return new Vector4(0f, 0f, 1f, 1f);

            Rect textureRect = sprite.textureRect;
            float width = Mathf.Clamp(Mathf.Max(1, pixelSize.x), 1f, texture.width - textureRect.x);
            float height = Mathf.Clamp(Mathf.Max(1, pixelSize.y), 1f, texture.height - textureRect.y);
            float invW = 1f / texture.width;
            float invH = 1f / texture.height;

            return new Vector4(
                textureRect.x * invW,
                textureRect.y * invH,
                width * invW,
                height * invH);
        }

        private void ApplyOverlayUvRemap()
        {
            Vector2 edgePadding = ResolveEdgePaddingInCells();
            float width = Mathf.Max(1, _mapWidth);
            float height = Mathf.Max(1, _mapHeight);
            _mat.SetVector("_FogOverlayUvRemap", new Vector4(
                (width + edgePadding.x * 2f) / width,
                (height + edgePadding.y * 2f) / height,
                -edgePadding.x / width,
                -edgePadding.y / height));
        }

        private void ReleaseMaterialInstance()
        {
            if (_mat == null)
                return;

            Destroy(_mat);
            _mat = null;
        }

        private Vector2 ResolveEdgePaddingInCells()
        {
            if (_settings == null)
                return Vector2.zero;

            Vector2 spriteSize = new Vector2(
                Mathf.Max(1, _settings.FogTileSpritePixelSize.x),
                Mathf.Max(1, _settings.FogTileSpritePixelSize.y));
            Vector2 tileSize = new Vector2(
                Mathf.Max(0.001f, _settings.FogTileSizeInCells.x),
                Mathf.Max(0.001f, _settings.FogTileSizeInCells.y));
            float paddingPixels = Mathf.Max(0f, _settings.FogMapEdgePaddingPixels);
            float overhangCells = Mathf.Max(0f, _settings.FogMapEdgeOverhangCells);
            return new Vector2(
                paddingPixels / spriteSize.x * tileSize.x + overhangCells,
                paddingPixels / spriteSize.y * tileSize.y + overhangCells);
        }

        private Vector2 ResolveEdgePaddingInWorldUnits(Bounds worldBounds, Vector2 edgePaddingCells)
        {
            float cellsX = Mathf.Max(1, _mapWidth);
            float cellsY = Mathf.Max(1, _mapHeight);
            float worldUnitsPerCellX = Mathf.Max(0.0001f, worldBounds.size.x / cellsX);
            float worldUnitsPerCellY = _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? Mathf.Max(0.0001f, worldBounds.size.z / cellsY)
                : Mathf.Max(0.0001f, worldBounds.size.y / cellsY);

            return new Vector2(
                edgePaddingCells.x * worldUnitsPerCellX,
                edgePaddingCells.y * worldUnitsPerCellY);
        }

        private void ApplyOverlayRenderPriority(Renderer renderer)
        {
            renderer.sortingOrder = FogOverlaySortingOrder;

            if (_mat != null)
                _mat.renderQueue = FogOverlayRenderQueue;
        }
    }
}
