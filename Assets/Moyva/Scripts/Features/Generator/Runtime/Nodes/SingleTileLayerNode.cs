using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    /// <summary>
    /// Генерує один шар (WorldLayerData) із єдиним типом тайлу.
    /// Знаходить спрайт тайлу за його ID через TileRegistrySO,
    /// будує Texture2D розміром W×H тайлів (1 піксель = 1 тайл на карті,
    /// але текстура будується з пікселів спрайту, розтягненого на всю карту).
    /// Без вихідного порту — результат передається через NodeContext
    /// (сторонній ефект) у List&lt;WorldLayerData&gt;, яку реєструє GraphBasedMapDataGenerator.
    /// </summary>
    [NodeInfo("Single Tile Layer", "Layers",
        "Генерує текстурний шар із одного тайлу: бере спрайт тайлу по ID, " +
        "будує Texture2D розміром у всю карту і повертає WorldLayerData для рендеру шару.")]
    public sealed class SingleTileLayerNode : NodeBase
    {
        private const string DefaultLodLayerShaderPath = "Moyva/2D/LayerMipLod";
        private const string DefaultWaterLayerShaderPath = "Moyva/2D/WaterLayerMipLodContour";

        [Header("Tile")]
        [Tooltip("ID тайлу, спрайт якого буде використано для побудови шару.")]
        [TileId]
        [SerializeField] private string _tileId = "";

        [Header("Registry")]
        [Tooltip("TileRegistrySO — реєстр тайлів проекту. Звідси беремо VisualPrefab → SpriteRenderer.sprite.")]
        [SerializeField] private TileRegistrySO _tileRegistry;

        [Header("Texture Quality")]
        [Tooltip("Масштаб роздільності шару відносно розміру мапи. 1 = повна, 0.5 = вдвічі менше по кожній осі.")]
        [Range(0.05f, 1f)]
        [SerializeField] private float _resolutionScale = 1f;

        [Tooltip("Фільтрація текстури шару.")]
        [SerializeField] private FilterMode _filterMode = FilterMode.Point;

        [Tooltip("Увімкнути mip maps (корисно при zoom out, але додає споживання пам'яті).")]
        [SerializeField] private bool _useMipMaps;

        [Tooltip("Формат текстури шару. Менш точні формати можуть зменшити споживання пам'яті.")]
        [SerializeField] private TextureFormat _textureFormat = TextureFormat.RGBA32;

        [Header("Layer Shader")]
        [Tooltip("Пряме посилання на Shader/Shader Graph для цього шару. Якщо задано — має пріоритет над шляхом нижче.")]
        [SerializeField] private Shader _layerShader;

        [Tooltip("Шлях шейдера який буде застосований до шару (наприклад \"Sprites/Default\" або \"Universal Render Pipeline/2D/Sprite-Lit-Default\"). " +
            "Якщо не задано та увімкнено mip maps, автоматично використовується \"Moyva/2D/LayerMipLod\".")]
        [SerializeField] private string _layerShaderPath = "";

        [Header("Sorting")]
        [Tooltip("Ім'я Sorting Layer для шару. Порожнє = \"Default\".")]
        [SerializeField] private string _sortingLayerName = "Default";

        [Tooltip("Sorting Order шару у вказаному Sorting Layer.")]
        [SerializeField] private int _sortingOrder;

        [Header("Object Holes")]
        [Tooltip("ID об'єктів або їхні префікси з ObjectMap, де потрібно пробити дірку в цьому шарі. " +
            "Наприклад: \"river\" зробить дірки там де є будь-яке river/*-об'єкти.")]
        [SerializeField] private string[] _holeObjectPrefixes = Array.Empty<string>();

        public override string Title => "Single Tile Layer";
        public override string Category => "Layers";

        public override PortDefinition[] Inputs => new[]
        {
            // Необов'язковий: якщо не підключено, розмір мапи береться з context.MapSize (GraphSharedSettings).
            PortDefinition.Input<string[,]>("BiomeMap (optional)"),
            // Необов'язковий: bool[,] маска дірок — де true роблимо alpha=0 (дірка в шарі).
            PortDefinition.Input<bool[,]>("HoleMask (optional)"),
            // Необов'язковий: ObjectMap — разом з _holeObjectPrefixes пробиває дірки де є вказані об'єкти.
            PortDefinition.Input<string[,]>("ObjectMap (optional)")
        };

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            // BiomeMap необов'язковий — якщо не підключено, беремо розмір із context.MapSize.
            var biomeMap = inputs.Length > 0 ? inputs[0] as string[,] : null;
            // HoleMask необов'язковий — де true робимо дірки (alpha=0).
            var holeMask = inputs.Length > 1 ? inputs[1] as bool[,] : null;
            // ObjectMap необов'язковий — разом з _holeObjectPrefixes пробиває дірки під об'єктами.
            var objectMap = inputs.Length > 2 ? inputs[2] as string[,] : null;

            int mapW, mapH;
            if (biomeMap != null)
            {
                mapW = biomeMap.GetLength(0);
                mapH = biomeMap.GetLength(1);
            }
            else
            {
                mapW = context.MapSize.x;
                mapH = context.MapSize.y;
                if (mapW <= 0 || mapH <= 0)
                    return NodeOutput.Error(
                        "[SingleTileLayerNode] Розмір мапи недоступний. " +
                        "Підключіть BiomeMap або задайте розмір у GraphSharedSettings.");
            }

            if (string.IsNullOrEmpty(_tileId))
                return NodeOutput.Error("TileId is not set.");

            var tileRegistry = _tileRegistry;
            if (tileRegistry == null)
                context.TryGetService(out tileRegistry);

            if (tileRegistry == null)
                return NodeOutput.Error("TileRegistrySO is not assigned.");

            var sprite = FindSpriteForTile(_tileId, tileRegistry);
            if (sprite == null && _tileRegistry != null && context.TryGetService<TileRegistrySO>(out var contextTileRegistry) && contextTileRegistry != _tileRegistry)
                sprite = FindSpriteForTile(_tileId, contextTileRegistry);

            WorldLayerData layerData;
            if (sprite == null)
            {
                Debug.LogWarning($"[SingleTileLayerNode] Sprite not found for tile '{_tileId}'. Using fallback white texture.");
                layerData = BuildFallbackLayerData(mapW, mapH);
            }
            else
            {
                var resolvedShader = ResolveLayerShader();
                layerData = new WorldLayerData
                {
                    LayerTileID = _tileId,
                    TileTexture = BuildTextureForSprite(sprite, mapW, mapH, _resolutionScale, _textureFormat, _useMipMaps, _filterMode, holeMask, objectMap, _holeObjectPrefixes),
                    LayerShader = resolvedShader,
                    LayerShaderName = resolvedShader != null ? resolvedShader.name : null,
                    SortingLayerName = string.IsNullOrEmpty(_sortingLayerName) ? "Default" : _sortingLayerName,
                    SortingOrder = _sortingOrder,
                };
            }

            if (context.TryGetService<List<WorldLayerData>>(out var collection))
                collection.Add(layerData);
            else
                Debug.LogWarning("[SingleTileLayerNode] List<WorldLayerData> not found in NodeContext. " +
                    "Шар не буде рендеритись. Переконайтесь що граф запускається через GraphBasedMapDataGenerator.");

            // Повертаємо текстуру шару як вихід для превью (node має 0 output-портів,
            // тому downstream ноди не будуть зачеплені, але GraphRunner збереже значення для превью).
            return NodeOutput.Success(layerData.TileTexture);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static Sprite FindSpriteForTile(string tileId, TileRegistrySO tileRegistry)
        {
            foreach (var def in tileRegistry.Definitions)
            {
                if (!string.Equals(def.Id, tileId, StringComparison.Ordinal))
                    continue;

                if (def.VisualPrefab == null)
                    return null;

                var sr = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                return sr != null ? sr.sprite : null;
            }
            return null;
        }

        private Shader ResolveLayerShader()
        {
            if (_layerShader != null)
                return _layerShader;

            string explicitPath = _layerShaderPath?.Trim();
            if (!string.IsNullOrEmpty(explicitPath))
                return Shader.Find(explicitPath);

            // Якщо явно не задано шейдер і це водний шар — використовуємо спеціалізований water-шейдер.
            if (string.Equals(_tileId, "water", StringComparison.OrdinalIgnoreCase))
                return Shader.Find(DefaultWaterLayerShaderPath);

            // Якщо mip maps увімкнені, дефолтно підключаємо LOD-шейдер для дешевшого zoom-out рендеру.
            return _useMipMaps ? Shader.Find(DefaultLodLayerShaderPath) : null;
        }

        /// <summary>
        /// Якщо holeMask задано: де holeMask[x,y]==true робимо alpha=0 (дірка).
        /// </summary>
        private static Texture2D BuildTextureForSprite(
            Sprite sprite,
            int mapW,
            int mapH,
            float resolutionScale,
            TextureFormat textureFormat,
            bool useMipMaps,
            FilterMode filterMode,
            bool[,] holeMask = null,
            string[,] objectMap = null,
            string[] holeObjectPrefixes = null)
        {
            // Зчитуємо пікселі вихідного спрайту
            var src = sprite.texture;
            var srcRect = sprite.textureRect;
            int srcX = (int)srcRect.x;
            int srcY = (int)srcRect.y;
            int srcW = (int)srcRect.width;
            int srcH = (int)srcRect.height;

            Color[] srcPixels = src.GetPixels(srcX, srcY, srcW, srcH);

            // Базова роздільність шару: печемо реальний sprite у кожну tile-комірку,
            // а resolutionScale керує кількістю texel'ів на один tile.
            float scale = Mathf.Clamp(resolutionScale, 0.05f, 1f);
            int desiredPixelsPerTileW = Mathf.Max(1, Mathf.RoundToInt(srcW * scale));
            int desiredPixelsPerTileH = Mathf.Max(1, Mathf.RoundToInt(srcH * scale));
            int maxTextureSize = Mathf.Max(256, SystemInfo.maxTextureSize);

            int targetW = Mathf.Clamp(mapW * desiredPixelsPerTileW, 1, maxTextureSize);
            int targetH = Mathf.Clamp(mapH * desiredPixelsPerTileH, 1, maxTextureSize);

            // Якщо є маска дірок, гарантуємо мінімум 1 texel на 1 тайл,
            // щоб розмір дірок точно відповідав розміру тайлів незалежно від quality/resolution.
            int holeMaskW = 0;
            int holeMaskH = 0;
            if (holeMask != null)
            {
                holeMaskW = holeMask.GetLength(0);
                holeMaskH = holeMask.GetLength(1);
                targetW = Mathf.Max(targetW, holeMaskW, mapW);
                targetH = Mathf.Max(targetH, holeMaskH, mapH);
            }

            var tex = new Texture2D(targetW, targetH, textureFormat, useMipMaps);
            tex.filterMode = filterMode;
            tex.wrapMode = TextureWrapMode.Clamp;

            var fillPixels = new Color[targetW * targetH];

            for (int texY = 0; texY < targetH; texY++)
            {
                float mapYf = ((texY + 0.5f) / targetH) * mapH;
                float localTileY = mapYf - Mathf.Floor(mapYf);
                int sampleY = Mathf.Clamp(Mathf.FloorToInt(localTileY * srcH), 0, srcH - 1);

                for (int texX = 0; texX < targetW; texX++)
                {
                    float mapXf = ((texX + 0.5f) / targetW) * mapW;
                    float localTileX = mapXf - Mathf.Floor(mapXf);
                    int sampleX = Mathf.Clamp(Mathf.FloorToInt(localTileX * srcW), 0, srcW - 1);
                    fillPixels[texY * targetW + texX] = srcPixels[sampleY * srcW + sampleX];
                }
            }

            // Застосовуємо HoleMask: де true → robimо alpha=0 (діркa)
            if (holeMask != null)
            {
                for (int i = 0; i < fillPixels.Length; i++)
                {
                    // Координати в фіналній текстурі
                    int texX = i % targetW;
                    int texY = i / targetW;

                    // Пропорційне зіставлення texel -> tile маски.
                    // Це дає стабільні межі дірок і не залежить від resolutionScale.
                    int mapX = Mathf.Min(holeMaskW - 1, (texX * holeMaskW) / targetW);
                    int mapY = Mathf.Min(holeMaskH - 1, (texY * holeMaskH) / targetH);

                    // Перевіряємо межі та маску
                    if (mapX >= 0 && mapX < holeMaskW && mapY >= 0 && mapY < holeMaskH)
                    {
                        if (holeMask[mapX, mapY])
                        {
                            // Де маска == true → дірка (alpha=0)
                            fillPixels[i].a = 0f;
                        }
                    }
                }
            }

            // Застосовуємо ObjectMap-дірки: де objectMap[x,y] починається з будь-якого prefix → alpha=0.
            if (objectMap != null && holeObjectPrefixes != null && holeObjectPrefixes.Length > 0)
            {
                int objMapW = objectMap.GetLength(0);
                int objMapH = objectMap.GetLength(1);
                for (int i = 0; i < fillPixels.Length; i++)
                {
                    if (fillPixels[i].a == 0f) continue; // вже прозорий — пропускаємо
                    int texX = i % targetW;
                    int texY = i / targetW;
                    // Пропорційне зіставлення texel → tile (той самий підхід що й у HoleMask).
                    int tileX = Mathf.Clamp((texX * objMapW) / targetW, 0, objMapW - 1);
                    int tileY = Mathf.Clamp((texY * objMapH) / targetH, 0, objMapH - 1);
                    var objId = objectMap[tileX, tileY];
                    if (string.IsNullOrEmpty(objId)) continue;
                    foreach (var prefix in holeObjectPrefixes)
                    {
                        if (!string.IsNullOrEmpty(prefix) && objId.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            fillPixels[i].a = 0f;
                            break;
                        }
                    }
                }
            }

            tex.SetPixels(fillPixels);
            tex.Apply();
            return tex;
        }

        private static WorldLayerData BuildFallbackLayerData(int w, int h)
        {
            int targetW = Mathf.Max(1, Mathf.RoundToInt(w * 0.25f));
            int targetH = Mathf.Max(1, Mathf.RoundToInt(h * 0.25f));
            var tex = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[targetW * targetH];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return new WorldLayerData { LayerTileID = "", TileTexture = tex, LayerShader = null, LayerShaderName = null };
        }
    }
}
