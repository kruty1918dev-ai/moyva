using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Hill Generator", "Terrain",
        "Генерує пагорби у top-down 2D на основі карти висот. " +
        "Розбиває heightmap на рівні, визначає краї кожного рівня і замінює тайли на краях " +
        "відповідними hill-тайлами. Підтримує 4 кардинальні напрямки, " +
        "4 зовнішніх та 4 внутрішніх кути. " +
        "Виходи: змінена TileMap, LevelMap та HillLevelData для кожної клітинки.")]
    public sealed class HillGeneratorNode : ContourGeneratorNodeBase
    {
        [Header("Level Settings")]
        [Tooltip("Кількість висотних рівнів, на які ділиться карта (2–10). " +
                 "Кожна сусідня пара рівнів утворює один шар схилів пагорба.")]
        [SerializeField, Range(2, 10)] private int _levels = 3;

        [Tooltip("Якщо увімкнено — межі між рівнями задаються вручну масивом _levelThresholds. " +
                 "Вимкнено — рівні ділять діапазон [0,1] рівномірно.")]
        [SerializeField] private bool _useCustomThresholds;

        [Tooltip("Межі між рівнями у діапазоні [0,1], розміром (_levels − 1), відсортовані за зростанням. " +
                 "Значення i = поріг між рівнем i та рівнем i+1.")]
        [SerializeField] private float[] _levelThresholds = Array.Empty<float>();

        [Header("Hill Tiles")]
        [Tooltip("Прив'язка напрямків пагорба до Tile ID. " +
                 "Нода замінює тайл лише тоді, коли є відповідний запис для визначеного напрямку.")]
        [SerializeField] private HillTileEntry[] _hillTiles = Array.Empty<HillTileEntry>();

        [Header("Mask Override")]
        [Tooltip("Якщо увімкнено, у клітинках маски використовується окремий профіль: власні рівні та власні hill-тайли.")]
        [SerializeField] private bool _useMaskOverride;

        [Tooltip("Список індексів шарів HeightLayers, які формують маску, якщо зовнішній Mask input не підключений.")]
        [SerializeField] private int[] _maskLayerIndices = Array.Empty<int>();

        [Tooltip("Кількість рівнів у зоні маски (2–10).")]
        [SerializeField, Range(2, 10)] private int _maskOverrideLevels = 3;

        [Tooltip("Якщо увімкнено — межі рівнів у зоні маски беруться з _maskOverrideThresholds.")]
        [SerializeField] private bool _maskOverrideUseCustomThresholds;

        [Tooltip("Пороги між рівнями у зоні маски (розмір = _maskOverrideLevels - 1).")]
        [SerializeField] private float[] _maskOverrideThresholds = Array.Empty<float>();

        [Tooltip("Альтернативний набір hill-тайлів, що застосовується в зоні маски.")]
        [SerializeField] private HillTileEntry[] _maskOverrideHillTiles = Array.Empty<HillTileEntry>();

        [NonSerialized] private float[,] _lastHeightMap;
        [NonSerialized] private string[,] _lastSourceTileMap;
        [NonSerialized] private bool[,] _activeOverrideMask;
        [NonSerialized] private Dictionary<HillDirection, string> _baseTileLookup = new();
        [NonSerialized] private Dictionary<HillDirection, string> _maskTileLookup = new();

        public override string Title    => "Hill Generator";
        public override string Category => "Terrain";

        // Read-only accessors for editor — thresholds data needed by DrawLevelPreview
        public bool    UseCustomThresholds => _useCustomThresholds;
        public int     LevelCount          => _levels;
        public float[] LevelThresholds     => _levelThresholds;

        protected override int TotalLevels => _levels;

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<string[,]>("FlagMap (optional)"),
            PortDefinition.Input<bool[,]>("Mask (optional)"),
            PortDefinition.Input<int[,]>("LayerIndexMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap"),
            PortDefinition.Output<int[,]>("LevelMap"),
            PortDefinition.Output<HillLevelDataMap>("HillLevelData")
        };

        // Palette used for both inline preview and IPreviewableNode — matches editor palette
        internal static readonly Color[] LevelColors =
        {
            new(0.10f, 0.18f, 0.35f, 1f),
            new(0.22f, 0.45f, 0.20f, 1f),
            new(0.38f, 0.62f, 0.28f, 1f),
            new(0.60f, 0.75f, 0.35f, 1f),
            new(0.75f, 0.68f, 0.38f, 1f),
            new(0.65f, 0.50f, 0.30f, 1f),
            new(0.75f, 0.58f, 0.45f, 1f),
            new(0.80f, 0.80f, 0.80f, 1f),
            new(0.92f, 0.92f, 0.95f, 1f),
            new(1.00f, 1.00f, 1.00f, 1f),
        };

        // ── ContourGeneratorNodeBase overrides ──

        protected override string ValidatePrimaryInput(object[] inputs, int w, int h)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return "HeightMap input is required.";
            if (heightMap.GetLength(0) != w || heightMap.GetLength(1) != h)
                return "HeightMap and TileMap must have the same dimensions.";
            return null;
        }

        protected override int[,] BuildLevelMap(object[] inputs, int w, int h)
        {
            var heightMap = (float[,])inputs[0];
            _lastHeightMap = heightMap;
            _lastSourceTileMap = inputs.Length > 1 ? inputs[1] as string[,] : null;
            _activeOverrideMask = BuildOverrideMask(inputs, w, h);

            var map = new int[w, h];

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                bool useOverrideProfile = IsInOverrideMask(x, y);
                map[x, y] = ResolveLevel(
                    heightMap[x, y],
                    useOverrideProfile ? _maskOverrideLevels : _levels,
                    useOverrideProfile ? _maskOverrideUseCustomThresholds : _useCustomThresholds,
                    useOverrideProfile ? _maskOverrideThresholds : _levelThresholds);
            }

            return map;
        }

        protected override bool IsCandidateLevel(int level) => level > 0;

        protected override Dictionary<HillDirection, string> BuildTileLookup()
        {
            var lookup = new Dictionary<HillDirection, string>();
            if (_hillTiles == null) return lookup;
            foreach (var entry in _hillTiles)
                if (!string.IsNullOrEmpty(entry.TileId))
                    lookup[entry.Direction] = entry.TileId;

            _baseTileLookup = lookup;
            _maskTileLookup = BuildLookup(_maskOverrideHillTiles);
            return lookup;
        }

        protected override bool TryResolveTileIdForCell(
            int x,
            int y,
            HillDirection direction,
            Dictionary<HillDirection, string> defaultLookup,
            out string tileId)
        {
            if (IsInOverrideMask(x, y) && _maskTileLookup != null && _maskTileLookup.Count > 0)
            {
                if (_maskTileLookup.TryGetValue(direction, out tileId) && !string.IsNullOrEmpty(tileId))
                    return true;
            }

            if (_baseTileLookup != null && _baseTileLookup.TryGetValue(direction, out tileId) && !string.IsNullOrEmpty(tileId))
                return true;

            return defaultLookup.TryGetValue(direction, out tileId) && !string.IsNullOrEmpty(tileId);
        }

        protected override NodeOutput BuildOutput(string[,] tileMap, int[,] levelMap)
            => NodeOutput.Success(tileMap, levelMap, BuildHillLevelData(tileMap, levelMap));

        private HillLevelDataMap BuildHillLevelData(string[,] tileMap, int[,] levelMap)
        {
            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);
            var data = new HillLevelTileData[w, h];

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                string sourceTileId = HasSameSize(_lastSourceTileMap, w, h)
                    ? _lastSourceTileMap[x, y]
                    : tileMap[x, y];
                float height = HasSameSize(_lastHeightMap, w, h)
                    ? _lastHeightMap[x, y]
                    : 0f;
                int level = HasSameSize(levelMap, w, h)
                    ? levelMap[x, y]
                    : 0;
                string directionId = HasSameSize(_lastDirectionMap, w, h) && _lastDirectionMap[x, y].HasValue
                    ? _lastDirectionMap[x, y].Value.ToString()
                    : string.Empty;
                bool wasModified = !string.Equals(sourceTileId, tileMap[x, y], StringComparison.Ordinal);

                data[x, y] = new HillLevelTileData(
                    x,
                    y,
                    tileMap[x, y],
                    sourceTileId,
                    directionId,
                    height,
                    level,
                    wasModified);
            }

            return new HillLevelDataMap(data);
        }

        private static bool HasSameSize(Array map, int width, int height)
            => map != null
               && map.Rank == 2
               && map.GetLength(0) == width
               && map.GetLength(1) == height;

        private bool[,] BuildOverrideMask(object[] inputs, int width, int height)
        {
            if (!_useMaskOverride)
                return null;

            var externalMask = inputs.Length > 3 ? inputs[3] as bool[,] : null;
            if (HasSameSize(externalMask, width, height))
                return (bool[,])externalMask.Clone();

            var layerIndexMap = inputs.Length > 4 ? inputs[4] as int[,] : null;
            if (!HasSameSize(layerIndexMap, width, height))
                return null;

            var selected = BuildMaskLayerIndexSet();
            if (selected.Count == 0)
                return null;

            var mask = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mask[x, y] = selected.Contains(layerIndexMap[x, y]);

            return mask;
        }

        private HashSet<int> BuildMaskLayerIndexSet()
        {
            var set = new HashSet<int>();
            if (_maskLayerIndices == null)
                return set;

            for (int i = 0; i < _maskLayerIndices.Length; i++)
                if (_maskLayerIndices[i] >= 0)
                    set.Add(_maskLayerIndices[i]);

            return set;
        }

        private bool IsInOverrideMask(int x, int y)
            => _activeOverrideMask != null
               && x >= 0 && y >= 0
               && x < _activeOverrideMask.GetLength(0)
               && y < _activeOverrideMask.GetLength(1)
               && _activeOverrideMask[x, y];

        private static int ResolveLevel(float height, int levels, bool useCustomThresholds, float[] thresholds)
        {
            int safeLevels = Mathf.Clamp(levels, 2, 10);
            if (useCustomThresholds && thresholds != null && thresholds.Length >= safeLevels - 1)
            {
                int level = 0;
                for (int i = 0; i < safeLevels - 1; i++)
                    if (height >= thresholds[i])
                        level = i + 1;

                return level;
            }

            return Mathf.Clamp(Mathf.FloorToInt(height * safeLevels), 0, safeLevels - 1);
        }

        private static Dictionary<HillDirection, string> BuildLookup(HillTileEntry[] entries)
        {
            var lookup = new Dictionary<HillDirection, string>();
            if (entries == null)
                return lookup;

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.TileId))
                    continue;
                lookup[entry.Direction] = entry.TileId;
            }

            return lookup;
        }

        // ── IPreviewableNode ──

        public override Texture2D GeneratePreview(int width, int height)
        {
            if (_lastLevelMap == null) return null;

            int sw = _lastLevelMap.GetLength(0);
            int sh = _lastLevelMap.GetLength(1);
            int tw = Mathf.Clamp(width,  32, 256);
            int th = Mathf.Clamp(height, 32, 256);

            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < tw; x++)
            {
                int sx = x * sw / tw;
                for (int y = 0; y < th; y++)
                {
                    int sy    = y * sh / th;
                    int lvl   = _lastLevelMap[sx, sy];
                    var baseC = LevelIndexToColor(lvl, _levels);
                    var final = (_lastEdgeMask != null && _lastEdgeMask[sx, sy])
                        ? Color.Lerp(baseC, new Color(1f, 0.85f, 0f, 1f), 0.65f)
                        : baseC;
                    tex.SetPixel(x, y, final);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Color LevelIndexToColor(int level, int totalLevels)
        {
            if (totalLevels <= 1) return LevelColors[0];
            float t  = (float)level / (totalLevels - 1);
            float fi = t * (LevelColors.Length - 1);
            int   lo = Mathf.FloorToInt(fi);
            int   hi = Mathf.Min(lo + 1, LevelColors.Length - 1);
            return Color.Lerp(LevelColors[lo], LevelColors[hi], fi - lo);
        }
    }
}
