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

        [SerializeField] private FogOfWarSettings _settings;

        [InjectOptional] private IFogOfWarService   _fogService;
        [InjectOptional] private IFogTextureUpdater _textureUpdater;
        [InjectOptional] private IGridService       _gridService;
        [InjectOptional] private SignalBus          _signalBus;

        private Material _mat;
        private int _mapWidth = 10;
        private int _mapHeight = 10;
        private bool _subscribed;

        private void Start()
        {
            int w = _gridService != null ? _gridService.GridWidth  : 10;
            int h = _gridService != null ? _gridService.GridHeight : 10;
            InitializeOverlay(w, h);
            SubscribeToWorldGeneratedSignal();
        }

        private void OnDestroy()
        {
            if (_signalBus != null && _subscribed)
                _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        private void InitializeOverlay(int width, int height)
        {
            int w = Mathf.Max(1, width);
            int h = Mathf.Max(1, height);
            _mapWidth = w;
            _mapHeight = h;

            Vector2 edgePadding = ResolveEdgePaddingInCells();
            transform.localScale = new Vector3(w + edgePadding.x * 2f, h + edgePadding.y * 2f, 1f);
            transform.position   = new Vector3((w - 1) * 0.5f, (h - 1) * 0.5f, -0.5f);

            var mr = GetComponent<MeshRenderer>();
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

            if (_settings != null)
                ApplySettingsToMaterial();
        }

        private void SubscribeToWorldGeneratedSignal()
        {
            if (_signalBus == null || _subscribed)
                return;

            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _subscribed = true;
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            int width = Mathf.Max(1, signal.Width);
            int height = Mathf.Max(1, signal.Height);
            if (width == _mapWidth && height == _mapHeight)
                return;

            InitializeOverlay(width, height);
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
            return new Vector2(
                paddingPixels / spriteSize.x * tileSize.x,
                paddingPixels / spriteSize.y * tileSize.y);
        }

        private void ApplyOverlayRenderPriority(Renderer renderer)
        {
            renderer.sortingOrder = FogOverlaySortingOrder;

            if (_mat != null)
                _mat.renderQueue = FogOverlayRenderQueue;
        }
    }
}
