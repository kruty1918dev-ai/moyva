using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class ShoreMaskLayerSource
    {
        public Texture2D Texture;
        public Transform LayerTransform;
        public Vector3 BasePosition;
        public Vector3 BaseScale;
        public bool IsWater;
    }

    /// <summary>
    /// Рендерить RenderTexture-маску суші через GPU:
    /// кожен non-water шар блітується у RT через Blit,
    /// після чого RT передається у водний матеріал як _LandMaskTex.
    ///
    /// Підхід: Graphics.Blit з альфа-шейдером ("уся альфа → червоний канал") —
    /// не потребує окремої Camera, GameObject Layer або Renderer Feature.
    /// RT оновлюється після BuildWorld і може бути оновлена у будь-який момент
    /// через RebuildMask().
    /// </summary>
    internal sealed class ShoreMaskPrepass : MonoBehaviour
    {
        private static readonly int LandMaskTexId = Shader.PropertyToID("_LandMaskTex");
        private static readonly int UvOffsetId = Shader.PropertyToID("_UvOffset");
        private static readonly int UvScaleId = Shader.PropertyToID("_UvScale");

        private const bool DefaultAutoRebuildInRuntime = true;
        private const float DefaultRebuildInterval = 0.15f;
        private const int DefaultMaskResolutionScale = 4;

        private bool _autoRebuildInRuntime = DefaultAutoRebuildInRuntime;
        private float _rebuildInterval = DefaultRebuildInterval;
        private int _maskResolutionScale = DefaultMaskResolutionScale;

        private RenderTexture _rt;
        private Material _blitMat;
        private List<Material> _waterMaterials;
        private List<ShoreMaskLayerSource> _cachedLayerSources;
        private string[,] _cachedBiomeMap;
        private int _cachedMaskWidth;
        private int _cachedMaskHeight;
        private float _nextRebuildTime;
        private bool _isDirty = true;
        private Vector3[] _lastPositions;
        private Vector3[] _lastScales;
        private readonly Dictionary<Texture2D, bool> _opaqueCache = new();

        public void Configure(
            bool? autoRebuildInRuntime = null,
            float? rebuildInterval = null,
            int? maskResolutionScale = null)
        {
            if (autoRebuildInRuntime.HasValue)
                _autoRebuildInRuntime = autoRebuildInRuntime.Value;

            if (rebuildInterval.HasValue)
                _rebuildInterval = Mathf.Max(0f, rebuildInterval.Value);

            if (maskResolutionScale.HasValue)
                _maskResolutionScale = Mathf.Clamp(maskResolutionScale.Value, 1, 32);
        }

        // Шейдер для blit: малює alpha джерела в R-канал RT (адитивно)
        private static Shader BlitAlphaShader =>
            Shader.Find("Moyva/2D/Internal/AlphaToRed");

        private void OnDestroy()
        {
            ReleaseRT();
            if (_blitMat != null)
                Destroy(_blitMat);
        }

        private void LateUpdate()
        {
            if (!_autoRebuildInRuntime || !Application.isPlaying)
                return;

            if (_cachedLayerSources == null || _cachedMaskWidth <= 0 || _cachedMaskHeight <= 0)
                return;

            if (!CheckDirty())
                return;

            RebuildMaskInternal(_cachedLayerSources, _waterMaterials, _cachedBiomeMap, _cachedMaskWidth, _cachedMaskHeight, false);
        }

        private bool CheckDirty()
        {
            if (_isDirty)
                return true;

            var sources = _cachedLayerSources;
            if (sources == null)
                return false;

            if (_lastPositions == null || _lastPositions.Length != sources.Count)
                return true;

            for (int i = 0; i < sources.Count; i++)
            {
                var src = sources[i];
                if (src?.LayerTransform == null)
                    continue;

                if (src.LayerTransform.position != _lastPositions[i] ||
                    src.LayerTransform.localScale != _lastScales[i])
                    return true;
            }

            return false;
        }

        private void SnapshotTransforms()
        {
            var sources = _cachedLayerSources;
            if (sources == null)
                return;

            int count = sources.Count;
            if (_lastPositions == null || _lastPositions.Length != count)
            {
                _lastPositions = new Vector3[count];
                _lastScales = new Vector3[count];
            }

            for (int i = 0; i < count; i++)
            {
                var src = sources[i];
                if (src?.LayerTransform != null)
                {
                    _lastPositions[i] = src.LayerTransform.position;
                    _lastScales[i] = src.LayerTransform.localScale;
                }
            }

            _isDirty = false;
        }

        /// <summary>
        /// Будує (або перебудовує) RT з переданих даних і призначає RT у всі водні матеріали.
        /// </summary>
        /// <param name="layers">Усі шари WorldLayerData (і суша, і вода).</param>
        /// <param name="waterMaterials">
        /// Матеріали водних шарів що мають _LandMaskTex.
        /// Їхні mainTexture використовуються для визначення водних шарів при blit.
        /// </param>
        /// <param name="biomeMap">Fallback biome map (використовується якщо textures порожні).</param>
        /// <param name="maskWidth">Ширина RT у пікселях.</param>
        /// <param name="maskHeight">Висота RT у пікселях.</param>
        public void RebuildMask(
            List<ShoreMaskLayerSource> layerSources,
            List<Material> waterMaterials,
            string[,] biomeMap,
            int maskWidth,
            int maskHeight)
        {
            RebuildMaskInternal(layerSources, waterMaterials, biomeMap, maskWidth, maskHeight, true);
        }

        private void RebuildMaskInternal(
            List<ShoreMaskLayerSource> layerSources,
            List<Material> waterMaterials,
            string[,] biomeMap,
            int maskWidth,
            int maskHeight,
            bool cacheInputs)
        {
            if (cacheInputs)
            {
                _cachedLayerSources = layerSources;
                _cachedBiomeMap = biomeMap;
                _cachedMaskWidth = maskWidth;
                _cachedMaskHeight = maskHeight;
                _opaqueCache.Clear();
                _isDirty = true;
            }

            _waterMaterials = waterMaterials;

            int rtWidth = Mathf.Max(1, maskWidth * Mathf.Max(1, _maskResolutionScale));
            int rtHeight = Mathf.Max(1, maskHeight * Mathf.Max(1, _maskResolutionScale));
            EnsureRT(rtWidth, rtHeight);

            // Очищуємо в чорний (0 = вода)
            var prevRT = RenderTexture.active;
            RenderTexture.active = _rt;
            GL.Clear(false, true, Color.black);
            RenderTexture.active = prevRT;

            EnsureBlitMat();

            bool anyLayerBlit = false;

            if (layerSources != null)
            {
                for (int li = 0; li < layerSources.Count; li++)
                {
                    var source = layerSources[li];

                    if (source == null || source.Texture == null)
                        continue;

                    // Пропускаємо водні шари.
                    if (source.IsWater)
                        continue;

                    // Повністю суцільні шари ігноруємо — вони перетворять всю маску в суцільну сушу.
                    if (IsTextureMostlyOpaqueCached(source.Texture))
                        continue;

                    var uvOffset = Vector2.zero;
                    var uvScale = Vector2.one;

                    if (source.LayerTransform != null)
                    {
                        var currentPos = source.LayerTransform.position;
                        var currentScale = source.LayerTransform.localScale;

                        float worldWidth = Mathf.Max(maskWidth, 1);
                        float worldHeight = Mathf.Max(maskHeight, 1);

                        uvOffset = new Vector2(
                            (currentPos.x - source.BasePosition.x) / worldWidth,
                            (currentPos.y - source.BasePosition.y) / worldHeight);

                        uvScale = new Vector2(
                            source.BaseScale.x != 0f ? currentScale.x / source.BaseScale.x : 1f,
                            source.BaseScale.y != 0f ? currentScale.y / source.BaseScale.y : 1f);
                    }

                    _blitMat.SetVector(UvOffsetId, uvOffset);
                    _blitMat.SetVector(UvScaleId, uvScale);

                    // Blit: кидаємо альфу шару у R-канал RT адитивно (OneMinusSrcAlpha на dst)
                    _blitMat.SetTexture("_MainTex", source.Texture);
                    Graphics.Blit(source.Texture, _rt, _blitMat);
                    anyLayerBlit = true;
                }
            }

            // Fallback: якщо нема жодного шару — будуємо маску по biome grid (CPU → Texture2D → Blit).
            // Biome fallback не знає waterTileId, але може бути корисним при спрощеній генерації.
            if (!anyLayerBlit && biomeMap != null)
            {
                var fallbackTex = BuildBiomeFallback(biomeMap, maskWidth, maskHeight);
                Graphics.Blit(fallbackTex, _rt);
                Destroy(fallbackTex);
            }

            // Передаємо RT у всі водні матеріали
            AssignToMaterials();
            SnapshotTransforms();
        }

        private void AssignToMaterials()
        {
            if (_waterMaterials == null || _rt == null)
                return;

            foreach (var mat in _waterMaterials)
            {
                if (mat != null && mat.HasProperty(LandMaskTexId))
                    mat.SetTexture(LandMaskTexId, _rt);
            }
        }

        private void EnsureBlitMat()
        {
            if (_blitMat != null)
                return;

            var shader = BlitAlphaShader;
            if (shader == null)
            {
                // Якщо spеціальний шейдер ще не є у проєкті — тимчасово використовуємо стандартний blit.
                // У цьому режимі RGB-канали теж запишуться, але R-канал буде містити потрібне значення.
                Debug.LogWarning("[ShoreMaskPrepass] Shader 'Moyva/2D/Internal/AlphaToRed' not found. " +
                    "Using fallback Sprites/Default — _LandMaskTex will sample R channel (may have artifacts).");
                shader = Shader.Find("Sprites/Default");
            }

            _blitMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        private void EnsureRT(int maskWidth, int maskHeight)
        {
            if (_rt != null && _rt.width == maskWidth && _rt.height == maskHeight)
                return;

            ReleaseRT();

            var desc = new RenderTextureDescriptor(maskWidth, maskHeight)
            {
                depthBufferBits = 0,
                msaaSamples = 1,
                useMipMap = false,
                autoGenerateMips = false,
                sRGB = false,
                graphicsFormat = SystemInfo.IsFormatSupported(GraphicsFormat.R8_UNorm, GraphicsFormatUsage.Render)
                    ? GraphicsFormat.R8_UNorm
                    : GraphicsFormat.R8G8B8A8_UNorm,
            };

            _rt = new RenderTexture(desc)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "ShoreMaskRT",
            };
            _rt.Create();
        }

        private void ReleaseRT()
        {
            if (_rt == null)
                return;
            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }

        private bool IsTextureMostlyOpaqueCached(Texture2D tex)
        {
            if (_opaqueCache.TryGetValue(tex, out var cached))
                return cached;

            var colors = tex.GetPixels32();
            if (colors == null || colors.Length == 0)
            {
                _opaqueCache[tex] = false;
                return false;
            }

            int opaque = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].a > 8)
                    opaque++;
            }

            bool result = (opaque / (float)colors.Length) >= 0.98f;
            _opaqueCache[tex] = result;
            return result;
        }

        private static Texture2D BuildBiomeFallback(string[,] biomeMap, int w, int h)
        {
            // Biome fallback без waterTileId: просто білий = будь-який непорожній біом = суша.
            var tex = new Texture2D(w, h, TextureFormat.R8, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];

            int biomeW = biomeMap.GetLength(0);
            int biomeH = biomeMap.GetLength(1);

            for (int y = 0; y < h; y++)
            {
                int by = Mathf.Clamp((y * biomeH) / h, 0, biomeH - 1);
                for (int x = 0; x < w; x++)
                {
                    int bx = Mathf.Clamp((x * biomeW) / w, 0, biomeW - 1);
                    pixels[y * w + x] = string.IsNullOrEmpty(biomeMap[bx, by]) ? Color.black : Color.white;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
