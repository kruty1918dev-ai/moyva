using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Water Contour Generator", "Terrain",
        "Генерує контурні тайли вздовж краю водойм у top-down 2D. " +
        "Приймає булеву маску води (bool[,]) та TileMap; за замовчуванням замінює сухопутні тайли на межі вода/суша " +
        "(можна перемкнути на заміну водяних) " +
        "відповідними контурними тайлами. Підтримує 4 кардинальні напрямки, " +
        "4 зовнішніх та 4 внутрішніх кути. Вихід ContourMask позначає клітинки, де контурний тайл був фактично розміщений.")]
    public sealed class WaterContourGeneratorNode : ContourGeneratorNodeBase
    {
        [Header("Contour Tiles")]
        [Tooltip("Прив'язка напрямків контуру до Tile ID. " +
                 "Нода замінює тайл лише тоді, коли є відповідний запис для визначеного напрямку.")]
        [SerializeField] private HillTileEntry[] _contourTiles = Array.Empty<HillTileEntry>();

        [Header("Replacement Target")]
        [Tooltip("Якщо увімкнено, нода змінює сушу біля води (берег). Якщо вимкнено — змінює воду біля суші (legacy-режим).")]
        [SerializeField] private bool _replaceLandNearWater = true;

        [NonSerialized] private bool[,] _lastWaterMask;

        public override string Title    => "Water Contour Generator";
        public override string Category => "Terrain";

        protected override int TotalLevels => 2;

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("WaterMask"),
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<string[,]>("FlagMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap"),
            PortDefinition.Output<bool[,]>("ContourMask")
        };

        // WaterContour не виводить LevelMap — лише TileMap і фінальну маску розміщених контурів.
        protected override NodeOutput BuildOutput(string[,] tileMap, int[,] levelMap)
        {
            var contourMask = _lastEdgeMask != null
                ? (bool[,])_lastEdgeMask.Clone()
                : new bool[tileMap.GetLength(0), tileMap.GetLength(1)];

            return NodeOutput.Success(tileMap, contourMask);
        }

        // ── ContourGeneratorNodeBase overrides ──

        protected override string ValidatePrimaryInput(object[] inputs, int w, int h)
        {
            var waterMask = inputs[0] as bool[,];
            if (waterMask == null)
                return "WaterMask input is required.";
            if (waterMask.GetLength(0) != w || waterMask.GetLength(1) != h)
                return "WaterMask and TileMap must have the same dimensions.";
            return null;
        }

        protected override int[,] BuildLevelMap(object[] inputs, int w, int h)
        {
            var waterMask = (bool[,])inputs[0];
            _lastWaterMask = waterMask;
            var map = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                map[x, y] = _replaceLandNearWater
                    ? (waterMask[x, y] ? 0 : 1)
                    : (waterMask[x, y] ? 1 : 0);
            return map;
        }

        // У режимі replaceLandNearWater кандидати — суша (рівень 1), а "нижчий" рівень — вода (0).
        // У legacy-режимі навпаки: кандидати — вода (1), а "нижчий" рівень — суша (0).
        protected override bool IsCandidateLevel(int level) => level > 0;

        protected override Dictionary<HillDirection, string> BuildTileLookup()
        {
            var lookup = new Dictionary<HillDirection, string>();
            if (_contourTiles == null) return lookup;

            IContourDirectionMapper mapper = _replaceLandNearWater
                ? (IContourDirectionMapper)new InvertedContourDirectionMapper()
                : new IdentityContourDirectionMapper();

            foreach (var entry in _contourTiles)
                if (!string.IsNullOrEmpty(entry.TileId))
                    lookup[mapper.Map(entry.Direction)] = entry.TileId;

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

            var waterColor = new Color(0.15f, 0.35f, 0.70f, 1f);
            var landColor  = new Color(0.30f, 0.55f, 0.25f, 1f);
            var edgeColor  = new Color(0.90f, 0.75f, 0.30f, 1f);

            for (int x = 0; x < tw; x++)
            {
                int sx = x * sw / tw;
                for (int y = 0; y < th; y++)
                {
                    int sy    = y * sh / th;
                    bool edge = _lastEdgeMask != null && _lastEdgeMask[sx, sy];
                    bool isWater = _lastWaterMask != null
                        ? _lastWaterMask[sx, sy]
                        : (!_replaceLandNearWater && _lastLevelMap[sx, sy] > 0) || (_replaceLandNearWater && _lastLevelMap[sx, sy] == 0);
                    Color c = edge
                        ? edgeColor
                        : isWater ? waterColor : landColor;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
