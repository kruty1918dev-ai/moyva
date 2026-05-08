using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [RequireComponent(typeof(MeshRenderer))]
    public class FogQuadController : MonoBehaviour
    {
        private const int MaxIconRects = 64;
        private const int MaxBitmaskRects = 16;
        private const int FogOverlaySortingOrder = short.MaxValue;
        private const int FogOverlayRenderQueue = 4000; // Overlay queue

        [SerializeField] private FogOfWarSettings _settings;

        [InjectOptional] private IFogOfWarService   _fogService;
        [InjectOptional] private IFogTextureUpdater _textureUpdater;
        [InjectOptional] private IGridService       _gridService;

        private Material _mat;

        private void Start()
        {
            int w = _gridService != null ? _gridService.GridWidth  : 10;
            int h = _gridService != null ? _gridService.GridHeight : 10;

            transform.localScale = new Vector3(w, h, 1f);
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

        private void ApplySettingsToMaterial()
        {
            // Fog colors
            _mat.SetColor("_UnexploredColor", _settings.UnexploredColor);
            _mat.SetColor("_ExploredColor",   _settings.ExploredColor);

            _mat.SetVector("_FogTileUVRect", Vector4.zero);
            _mat.SetVector("_FogMaskUVRect", Vector4.zero);
            _mat.SetFloat("_UseFogBitmask", 0f);
            _mat.SetVectorArray("_FogMaskUVRects", BuildDefaultRectArray(MaxBitmaskRects, Vector4.zero));

            // Fog tile texture (tiled across fog overlay)
            if (_settings.FogTileSprite != null)
            {
                Texture2D tileTexture = _settings.FogTileSprite.texture;
                if (tileTexture != null)
                {
                    _mat.SetTexture("_FogTileTex", tileTexture);
                    _mat.SetVector("_FogTileUVRect", BuildSpriteUvRect(_settings.FogTileSprite, tileTexture));
                }
            }

            ApplyBitmaskSettings();

            _mat.SetVector("_FogIconUVRect", Vector4.zero);
            _mat.SetFloat("_FogIconRectCount", 0f);
            _mat.SetVectorArray("_FogIconUVRects", BuildDefaultRectArray(MaxIconRects, Vector4.zero));

            // Fog icon texture (sample exact sprite rect from atlas)
            if (_settings.FogIconSprites != null && _settings.FogIconSprites.Length > 0)
            {
                Texture2D iconTexture = FindFirstSpriteTexture(_settings.FogIconSprites, MaxIconRects);
                if (iconTexture != null)
                {
                    _mat.SetTexture("_FogIconTex", iconTexture);

                    var uvRects = new System.Collections.Generic.List<Vector4>(MaxIconRects);

                    for (int i = 0; i < _settings.FogIconSprites.Length && uvRects.Count < MaxIconRects; i++)
                    {
                        Sprite sprite = _settings.FogIconSprites[i];
                        if (sprite == null || sprite.texture != iconTexture)
                            continue;

                        uvRects.Add(BuildSpriteUvRect(sprite, iconTexture));
                    }

                    if (uvRects.Count > 0)
                    {
                        _mat.SetVector("_FogIconUVRect", uvRects[0]);
                        _mat.SetVectorArray("_FogIconUVRects", uvRects);
                        _mat.SetFloat("_FogIconRectCount", uvRects.Count);
                    }
                }
            }

            // Tiling and scaling parameters
            _mat.SetFloat("_FogTileTiling", _settings.FogTileTiling);
            _mat.SetFloat("_FogIconScale", _settings.FogIconScale);
            _mat.SetFloat("_FogIconSeed", _settings.FogIconSeed);
            _mat.SetFloat("_FogIconDensity", Mathf.Clamp01(_settings.FogIconDensity));
            _mat.SetFloat("_FogIconJitter", Mathf.Clamp(_settings.FogIconJitter, 0f, 0.45f));
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

        private void ApplyBitmaskSettings()
        {
            if (_settings == null || !_settings.UseBitmaskAutotiling || _settings.FogBitmaskSprites == null)
                return;

            Sprite fallbackSprite = _settings.FogTileSprite;
            Texture2D atlasTexture = FindBitmaskAtlasTexture(fallbackSprite);

            if (atlasTexture == null)
                return;

            _mat.SetTexture("_FogMaskTex", atlasTexture);

            Vector4 fallbackRect = BuildBitmaskFallbackRect(fallbackSprite, atlasTexture);
            var rects = BuildDefaultRectArray(MaxBitmaskRects, fallbackRect);
            int assignedCount = Mathf.Min(_settings.FogBitmaskSprites.Length, MaxBitmaskRects);

            for (int i = 0; i < assignedCount; i++)
            {
                Sprite sprite = _settings.FogBitmaskSprites[i];
                if (sprite == null || sprite.texture != atlasTexture)
                    continue;

                rects[i] = BuildSpriteUvRect(sprite, atlasTexture);
            }

            _mat.SetVector("_FogMaskUVRect", fallbackRect);
            _mat.SetVectorArray("_FogMaskUVRects", rects);
            _mat.SetFloat("_UseFogBitmask", 1f);
        }

        private Texture2D FindBitmaskAtlasTexture(Sprite fallbackSprite)
        {
            if (fallbackSprite != null && fallbackSprite.texture != null)
                return fallbackSprite.texture;

            return FindFirstSpriteTexture(_settings.FogBitmaskSprites, MaxBitmaskRects);
        }

        private Vector4 BuildBitmaskFallbackRect(Sprite fallbackSprite, Texture2D atlasTexture)
        {
            if (fallbackSprite != null && fallbackSprite.texture == atlasTexture)
                return BuildSpriteUvRect(fallbackSprite, atlasTexture);

            if (_settings.FogBitmaskSprites != null)
            {
                for (int i = 0; i < _settings.FogBitmaskSprites.Length && i < MaxBitmaskRects; i++)
                {
                    Sprite sprite = _settings.FogBitmaskSprites[i];
                    if (sprite != null && sprite.texture == atlasTexture)
                        return BuildSpriteUvRect(sprite, atlasTexture);
                }
            }

            return Vector4.zero;
        }

        private static Texture2D FindFirstSpriteTexture(Sprite[] sprites, int maxCount)
        {
            if (sprites == null)
                return null;

            for (int i = 0; i < sprites.Length && i < maxCount; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite != null && sprite.texture != null)
                    return sprite.texture;
            }

            return null;
        }

        private static Vector4 BuildSpriteUvRect(Sprite sprite, Texture2D texture)
        {
            if (sprite == null || texture == null)
                return Vector4.zero;

            Rect textureRect = sprite.textureRect;
            float invW = 1f / texture.width;
            float invH = 1f / texture.height;

            return new Vector4(
                textureRect.x * invW,
                textureRect.y * invH,
                textureRect.width * invW,
                textureRect.height * invH);
        }

        private static Vector4[] BuildDefaultRectArray(int count, Vector4 value)
        {
            var result = new Vector4[count];
            for (int i = 0; i < count; i++)
                result[i] = value;
            return result;
        }

        private void ApplyOverlayRenderPriority(Renderer renderer)
        {
            renderer.sortingOrder = FogOverlaySortingOrder;

            if (_mat != null)
                _mat.renderQueue = FogOverlayRenderQueue;
        }
    }
}
