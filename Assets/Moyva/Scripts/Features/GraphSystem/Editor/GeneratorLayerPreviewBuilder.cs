using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Будує превʼю шарів генератора без сцени/RenderTexture (CPU-рендер).
    /// </summary>
    internal static class GeneratorLayerPreviewBuilder
    {
        private const int MaxCompositeSize = 768;

        /// <summary>
        /// Обчислює бінарну матрицю кожного шару з результату виконання графа.
        /// Матриця шару = OR усіх його термінальних вузлів (тих, чий вихід Mask
        /// не споживається іншим вузлом того ж шару).
        /// </summary>
        public static Dictionary<string, bool[,]> ComputeLayerMatrices(
            GraphAsset graph, GraphExecutionResult result, out int width, out int height)
        {
            width = 0;
            height = 0;
            var matrices = new Dictionary<string, bool[,]>();
            if (graph == null || result == null)
                return matrices;

            foreach (var layer in graph.Layers)
            {
                if (layer == null)
                    continue;
                if (!layer.Enabled || !GraphLayerRuntimeSemantics.HasRenderableTileOutput(graph, layer.Id))
                    continue;

                var layerNodes = graph.Nodes
                    .Where(n => n != null && n.LayerId == layer.Id)
                    .ToList();
                if (layerNodes.Count == 0)
                    continue;

                var layerNodeIds = new HashSet<string>(layerNodes.Select(n => n.NodeId));

                // Вузол термінальний, якщо його вихід не йде в інший вузол того ж шару.
                var consumed = new HashSet<string>();
                foreach (var c in graph.Connections)
                {
                    if (c != null
                        && layerNodeIds.Contains(c.SourceNodeId)
                        && layerNodeIds.Contains(c.TargetNodeId))
                    {
                        consumed.Add(c.SourceNodeId);
                    }
                }

                bool[,] layerMatrix = null;
                foreach (var node in layerNodes)
                {
                    if (consumed.Contains(node.NodeId))
                        continue;

                    var mask = ExtractLayerOccupancyMatrix(result.GetOutputs(node.NodeId));
                    if (mask == null)
                        continue;

                    int w = mask.GetLength(0);
                    int h = mask.GetLength(1);
                    if (layerMatrix == null)
                    {
                        layerMatrix = new bool[w, h];
                    }

                    int cw = Mathf.Min(w, layerMatrix.GetLength(0));
                    int ch = Mathf.Min(h, layerMatrix.GetLength(1));
                    for (int x = 0; x < cw; x++)
                        for (int y = 0; y < ch; y++)
                            if (mask[x, y])
                                layerMatrix[x, y] = true;
                }

                if (layerMatrix == null)
                    continue;

                layerMatrix = ApplyLayerVisualPadding(layerMatrix, layer);
                matrices[layer.Id] = layerMatrix;
                width = Mathf.Max(width, layerMatrix.GetLength(0));
                height = Mathf.Max(height, layerMatrix.GetLength(1));
            }

            return matrices;
        }

        /// <summary>
        /// Мала кольорова мініатюра однієї матриці шару (заповнені клітинки — колір шару).
        /// </summary>
        public static Texture2D BuildLayerThumbnail(bool[,] matrix, Color color, int maxSize = 48)
        {
            if (matrix == null)
                return null;

            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);
            if (w <= 0 || h <= 0)
                return null;

            int scale = Mathf.Max(1, Mathf.FloorToInt((float)maxSize / Mathf.Max(w, h)));
            int texW = w * scale;
            int texH = h * scale;

            var bg = new Color(0.1f, 0.1f, 0.12f, 1f);
            var pixels = new Color[texW * texH];
            for (int py = 0; py < texH; py++)
            {
                for (int px = 0; px < texW; px++)
                {
                    int mx = px / scale;
                    // Фліп Y, бо текстурний y=0 — знизу.
                    int my = h - 1 - (py / scale);
                    bool on = mx < w && my >= 0 && my < h && matrix[mx, my];
                    pixels[py * texW + px] = on ? color : bg;
                }
            }

            var tex = NewTexture(texW, texH);
            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        public static Texture2D BuildTopDownComposite(
            GraphAsset graph,
            Dictionary<string, bool[,]> matrices,
            int width,
            int height,
            IReadOnlyDictionary<string, Color> layerColorOverrides,
            out string[,] tileMap)
        {
            tileMap = null;
            if (graph == null || matrices == null || matrices.Count == 0 || width <= 0 || height <= 0)
                return null;

            var layers = graph.Layers
                .Where(l => l != null && l.Enabled && matrices.ContainsKey(l.Id))
                .OrderBy(l => l.SortingOrder)
                .ToList();
            if (layers.Count == 0)
                return null;

            tileMap = new string[width, height];
            var colors = new Color[width * height];
            var background = new Color(0.035f, 0.04f, 0.055f, 1f);
            for (int i = 0; i < colors.Length; i++)
                colors[i] = background;

            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var matrix = matrices[layer.Id];
                int matrixWidth = matrix.GetLength(0);
                int matrixHeight = matrix.GetLength(1);
                int drawWidth = Mathf.Min(width, matrixWidth);
                int drawHeight = Mathf.Min(height, matrixHeight);
                Color color = ResolveLayerPreviewColor(layer, layerColorOverrides);
                string tileLabel = !string.IsNullOrWhiteSpace(layer.Name) ? layer.Name : layer.Id;

                for (int x = 0; x < drawWidth; x++)
                {
                    for (int y = 0; y < drawHeight; y++)
                    {
                        if (!matrix[x, y])
                            continue;

                        colors[y * width + x] = color;
                        tileMap[x, y] = tileLabel;
                    }
                }
            }

            var tex = NewTexture(width, height);
            tex.SetPixels(colors);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        /// Складений ізометричний 3D-вид усіх шарів у 2D-текстуру.
        /// Кожна клітинка малюється як ізометричний кубик: верхній ромб (колір
        /// верхнього шару) + бічні грані (тінь), з підняттям за кількістю шарів.
        /// </summary>
        public static Texture2D BuildIsometricComposite(
            GraphAsset graph, Dictionary<string, bool[,]> matrices, int width, int height)
        {
            return BuildIsometricComposite(graph, matrices, width, height, null);
        }

        public static Texture2D BuildIsometricComposite(
            GraphAsset graph,
            Dictionary<string, bool[,]> matrices,
            int width,
            int height,
            IReadOnlyDictionary<string, Color> layerColorOverrides)
        {
            if (graph == null || matrices == null || matrices.Count == 0 || width <= 0 || height <= 0)
                return null;

            // Шари знизу-вверх за SortingOrder, але кожен шар має власну preview-геометрію.
            var layers = graph.Layers
                .Where(l => l != null && l.Enabled && matrices.ContainsKey(l.Id))
                .OrderBy(l => l.SortingOrder)
                .ToList();
            if (layers.Count == 0)
                return null;

            // Геометрія ізометрії — підбираємо розмір тайла під ліміт текстури.
            int tileHalfW = 12;
            while (tileHalfW > 2)
            {
                int estW = (width + height) * tileHalfW + tileHalfW * 2;
                if (estW <= MaxCompositeSize)
                    break;
                tileHalfW--;
            }
            int tileHalfH = Mathf.Max(1, tileHalfW / 2);
            int levelHeight = Mathf.Max(2, tileHalfH);

            var cells = new List<IsoCell>();
            int maxTopLevel = 0;
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var matrix = matrices[layer.Id];
                int baseLevel = ResolveLayerBaseLevel(graph, layer);
                int visualHeight = ResolveLayerVisualHeight(graph, layer);
                int topLevel = baseLevel + visualHeight;
                maxTopLevel = Mathf.Max(maxTopLevel, topLevel);

                int w = matrix.GetLength(0);
                int h = matrix.GetLength(1);
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (!matrix[x, y])
                            continue;

                        cells.Add(new IsoCell
                        {
                            X = x,
                            Y = y,
                            BaseLevel = baseLevel,
                            VisualHeight = visualHeight,
                            SortingOrder = layer.SortingOrder,
                            Color = ResolveLayerPreviewColor(layer, layerColorOverrides)
                        });
                    }
                }
            }

            if (cells.Count == 0)
                return null;

            // Межі полотна. maxTopLevel дає місце для per-layer height/y-offset, а не для однакового stack-count.
            int spanX = (width + height) * tileHalfW;
            int spanY = (width + height) * tileHalfH + Mathf.Max(1, maxTopLevel) * levelHeight;
            int pad = tileHalfW + 2;
            int texW = Mathf.Clamp(spanX + pad * 2, 16, MaxCompositeSize);
            int texH = Mathf.Clamp(spanY + pad * 2, 16, MaxCompositeSize);

            var bg = new Color(0.05f, 0.06f, 0.08f, 1f);
            var pixels = new Color[texW * texH];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = bg;

            int originX = pad + height * tileHalfW;
            int originY = pad + Mathf.Max(1, maxTopLevel) * levelHeight;

            foreach (var cell in cells
                         .OrderBy(c => c.X + c.Y)
                         .ThenBy(c => c.BaseLevel)
                         .ThenBy(c => c.SortingOrder))
            {
                DrawIsoTile(pixels, texW, texH, originX, originY,
                    cell.X, cell.Y, cell.BaseLevel, cell.VisualHeight,
                    tileHalfW, tileHalfH, levelHeight, cell.Color);
            }

            var tex = NewTexture(texW, texH);
            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static Color ResolveLayerPreviewColor(
            GeneratorLayerDefinition layer,
            IReadOnlyDictionary<string, Color> layerColorOverrides)
        {
            if (layer != null
                && layerColorOverrides != null
                && layerColorOverrides.TryGetValue(layer.Id, out var color))
            {
                color.a = Mathf.Approximately(color.a, 0f) ? 1f : color.a;
                return color;
            }

            return layer != null ? layer.Color : Color.white;
        }

        private sealed class IsoCell
        {
            public int X;
            public int Y;
            public int BaseLevel;
            public int VisualHeight;
            public int SortingOrder;
            public Color Color;
        }

        private static void DrawIsoTile(
            Color[] pixels, int texW, int texH, int originX, int originY,
            int gx, int gy, int baseLevel, int visualHeight,
            int tileHalfW, int tileHalfH, int levelHeight, Color color)
        {
            int clampedBase = Mathf.Max(0, baseLevel);
            int clampedHeight = Mathf.Max(1, visualHeight);
            float cx = originX + (gx - gy) * tileHalfW;
            float cy = originY + (gx + gy) * tileHalfH - (clampedBase + clampedHeight) * levelHeight;

            var top = new Vector2(cx, cy - tileHalfH);
            var right = new Vector2(cx + tileHalfW, cy);
            var bottom = new Vector2(cx, cy + tileHalfH);
            var left = new Vector2(cx - tileHalfW, cy);

            int wall = Mathf.Max(levelHeight, clampedHeight * levelHeight);
            var down = new Vector2(0f, wall);

            Color leftFace = color * 0.62f;
            leftFace.a = 1f;
            Color rightFace = color * 0.42f;
            rightFace.a = 1f;
            Color topFace = color;
            topFace.a = 1f;

            // Ліва грань.
            FillPolygon(pixels, texW, texH, new[] { left, bottom, bottom + down, left + down }, leftFace);
            // Права грань.
            FillPolygon(pixels, texW, texH, new[] { bottom, right, right + down, bottom + down }, rightFace);
            // Верхній ромб.
            FillPolygon(pixels, texW, texH, new[] { top, right, bottom, left }, topFace);
        }

        private static void FillPolygon(Color[] pixels, int texW, int texH, Vector2[] poly, Color color)
        {
            if (poly == null || poly.Length < 3)
                return;

            float minXf = float.MaxValue, minYf = float.MaxValue, maxXf = float.MinValue, maxYf = float.MinValue;
            for (int i = 0; i < poly.Length; i++)
            {
                minXf = Mathf.Min(minXf, poly[i].x);
                minYf = Mathf.Min(minYf, poly[i].y);
                maxXf = Mathf.Max(maxXf, poly[i].x);
                maxYf = Mathf.Max(maxYf, poly[i].y);
            }

            int minX = Mathf.Clamp(Mathf.FloorToInt(minXf), 0, texW - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt(minYf), 0, texH - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt(maxXf), 0, texW - 1);
            int maxY = Mathf.Clamp(Mathf.CeilToInt(maxYf), 0, texH - 1);

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    if (!ContainsPoint(poly, new Vector2(px + 0.5f, py + 0.5f)))
                        continue;

                    // Фліп Y: екранна верхівка (менший y) має лягати у верх текстури.
                    int ty = texH - 1 - py;
                    if (ty < 0 || ty >= texH)
                        continue;
                    pixels[ty * texW + px] = color;
                }
            }
        }

        private static bool ContainsPoint(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                bool intersects = ((polygon[i].y > point.y) != (polygon[j].y > point.y))
                    && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y)
                        / Mathf.Max(0.0001f, polygon[j].y - polygon[i].y) + polygon[i].x;
                if (intersects)
                    inside = !inside;
            }

            return inside;
        }

        private static bool[,] ApplyLayerVisualPadding(bool[,] source, GeneratorLayerDefinition layer)
        {
            if (source == null || layer == null)
                return source;

            int extraX = Mathf.Max(0, layer.ExtraWidthCells);
            int extraY = Mathf.Max(0, layer.ExtraLengthCells);
            if (extraX == 0 && extraY == 0)
                return source;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            if (width <= 0 || height <= 0)
                return source;

            int paddedWidth = width + extraX * 2;
            int paddedHeight = height + extraY * 2;
            var padded = new bool[paddedWidth, paddedHeight];

            // TWC border padding is visual/build-layer-local. For preview we extend edge occupancy
            // per layer, instead of forcing all layers into the same unpadded matrix size.
            for (int x = 0; x < paddedWidth; x++)
            {
                int sourceX = Mathf.Clamp(x - extraX, 0, width - 1);
                for (int y = 0; y < paddedHeight; y++)
                {
                    int sourceY = Mathf.Clamp(y - extraY, 0, height - 1);
                    padded[x, y] = source[sourceX, sourceY];
                }
            }

            return padded;
        }

        private static int ResolveLayerBaseLevel(GraphAsset graph, GeneratorLayerDefinition layer)
        {
            if (layer == null)
                return 0;

            float level = Mathf.Max(0f, layer.DefaultHeight);
            var primaryTileSettings = TileSettingsNode.GetNodesForLayer(graph, layer.Id)
                .FirstOrDefault(node => node != null && node.HasRenderableTileOutput);

            if (primaryTileSettings != null)
                level += primaryTileSettings.LayerYOffset + primaryTileSettings.TileLayerHeightOffset;

            return Mathf.Clamp(Mathf.RoundToInt(level), 0, 128);
        }

        private static int ResolveLayerVisualHeight(GraphAsset graph, GeneratorLayerDefinition layer)
        {
            var primaryTileSettings = TileSettingsNode.GetNodesForLayer(graph, layer?.Id)
                .FirstOrDefault(node => node != null && node.HasRenderableTileOutput);

            float height = primaryTileSettings != null
                ? primaryTileSettings.PrimaryTileHeight
                : 1f;

            if (height <= 0.0001f)
                height = 1f;

            return Mathf.Clamp(Mathf.CeilToInt(height), 1, 128);
        }

        private static bool[,] ExtractLayerOccupancyMatrix(object[] outputs)
        {
            if (outputs == null)
                return null;

            foreach (var o in outputs)
                if (o is bool[,] b)
                    return b;

            foreach (var o in outputs)
            {
                if (o is string[,] stringMap)
                    return BuildOccupancyFromStringMap(stringMap);

                if (o is float[,] floatMap)
                    return BuildOccupancyFromFloatMap(floatMap);
            }

            return null;
        }

        private static bool[,] BuildOccupancyFromStringMap(string[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var occupancy = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    occupancy[x, y] = !string.IsNullOrEmpty(source[x, y]);
            }

            return occupancy;
        }

        private static bool[,] BuildOccupancyFromFloatMap(float[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var occupancy = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    occupancy[x, y] = !float.IsNaN(source[x, y]) && !float.IsInfinity(source[x, y]);
            }

            return occupancy;
        }

        private static Texture2D NewTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}
