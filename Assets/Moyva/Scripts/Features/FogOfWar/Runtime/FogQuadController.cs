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

                _mat.SetVector("_FogTileUVRect", new Vector4(0f, 0f, 1f, 1f));

                // Fog tile texture (tiled across fog overlay)
                if (_settings.FogTileSprite != null)
                {
                    Texture2D tileTexture = _settings.FogTileSprite.texture;
                    if (tileTexture != null)
                    {
                        _mat.SetTexture("_FogTileTex", tileTexture);

                        Rect textureRect = _settings.FogTileSprite.textureRect;
                        float invW = 1f / tileTexture.width;
                        float invH = 1f / tileTexture.height;
                        Vector4 uvRect = new Vector4(
                            textureRect.x * invW,
                            textureRect.y * invH,
                            textureRect.width * invW,
                            textureRect.height * invH);
                        _mat.SetVector("_FogTileUVRect", uvRect);
                    }
                }

            _mat.SetVector("_FogIconUVRect", new Vector4(0f, 0f, 1f, 1f));
            _mat.SetFloat("_FogIconRectCount", 1f);
            _mat.SetVectorArray("_FogIconUVRects", new[] { new Vector4(0f, 0f, 1f, 1f) });

            // Fog icon texture (sample exact sprite rect from atlas)
            if (_settings.FogIconSprites != null && _settings.FogIconSprites.Length > 0)
            {
                Sprite firstSprite = _settings.FogIconSprites[0];
                Texture2D iconTexture = firstSprite != null ? firstSprite.texture : null;
                if (iconTexture != null)
                {
                    _mat.SetTexture("_FogIconTex", iconTexture);

                    var uvRects = new System.Collections.Generic.List<Vector4>(MaxIconRects);
                    float invW = 1f / iconTexture.width;
                    float invH = 1f / iconTexture.height;

                    for (int i = 0; i < _settings.FogIconSprites.Length && uvRects.Count < MaxIconRects; i++)
                    {
                        Sprite sprite = _settings.FogIconSprites[i];
                        if (sprite == null || sprite.texture != iconTexture)
                            continue;

                        Rect iconRect = sprite.textureRect;
                        uvRects.Add(new Vector4(
                            iconRect.x * invW,
                            iconRect.y * invH,
                            iconRect.width * invW,
                            iconRect.height * invH));
                    }

                    if (uvRects.Count == 0)
                        uvRects.Add(new Vector4(0f, 0f, 1f, 1f));

                    _mat.SetVector("_FogIconUVRect", uvRects[0]);
                    _mat.SetVectorArray("_FogIconUVRects", uvRects);
                    _mat.SetFloat("_FogIconRectCount", uvRects.Count);
                }
            }

            // Tiling and scaling parameters
            _mat.SetFloat("_FogTileTiling", _settings.FogTileTiling);
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

        private void ApplyOverlayRenderPriority(Renderer renderer)
        {
            renderer.sortingOrder = FogOverlaySortingOrder;

            if (_mat != null)
                _mat.renderQueue = FogOverlayRenderQueue;
        }
    }
}
