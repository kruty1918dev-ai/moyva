using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Будує превʼю шарів генератора без сцени/RenderTexture (CPU-рендер),
    /// тому не тягне типи TileWorldCreator у GraphSystem.Editor:
    /// <list type="bullet">
    /// <item>матриця кожного шару як кольорова мініатюра (принцип blueprint-preview з TWC);</item>
    /// <item>складений ізометричний 3D-вид (стек шарів за SortingOrder) у 2D-текстуру.</item>
    /// </list>
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
                        width = Mathf.Max(width, w);
                        height = Mathf.Max(height, h);
                    }

                    int cw = Mathf.Min(w, layerMatrix.GetLength(0));
                    int ch = Mathf.Min(h, layerMatrix.GetLength(1));
                    for (int x = 0; x < cw; x++)
                        for (int y = 0; y < ch; y++)
                            if (mask[x, y])
                                layerMatrix[x, y] = true;
                }

                if (layerMatrix != null)
                    matrices[layer.Id] = layerMatrix;
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

        /// <summary>
        /// Складений ізометричний 3D-вид усіх шарів у 2D-текстуру.
        /// Кожна клітинка малюється як ізометричний кубик: верхній ромб (колір
        /// верхнього шару) + бічні грані (тінь), з підняттям за кількістю шарів.
        /// </summary>
        public static Texture2D BuildIsometricComposite(
            GraphAsset graph, Dictionary<string, bool[,]> matrices, int width, int height)
        {
            if (graph == null || matrices == null || matrices.Count == 0 || width <= 0 || height <= 0)
                return null;

            // Шари знизу-вверх за SortingOrder.
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

            // Рахуємо для кожної клітинки: висоту стека та колір верхнього шару.
            int[,] stack = new int[width, height];
            Color[,] topColor = new Color[width, height];
            int maxStack = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int count = 0;
                    Color top = Color.clear;
                    foreach (var layer in layers)
                    {
                        var m = matrices[layer.Id];
                        if (x < m.GetLength(0) && y < m.GetLength(1) && m[x, y])
                        {
                            count++;
                            top = layer.Color;
                        }
                    }

                    stack[x, y] = count;
                    topColor[x, y] = top;
                    if (count > maxStack)
                        maxStack = count;
                }
            }

            if (maxStack == 0)
                return null;

            // Межі полотна.
            int spanX = (width + height) * tileHalfW;
            int spanY = (width + height) * tileHalfH + maxStack * levelHeight;
            int pad = tileHalfW + 2;
            int texW = Mathf.Clamp(spanX + pad * 2, 16, MaxCompositeSize);
            int texH = Mathf.Clamp(spanY + pad * 2, 16, MaxCompositeSize);

            var bg = new Color(0.05f, 0.06f, 0.08f, 1f);
            var pixels = new Color[texW * texH];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = bg;

            int originX = pad + height * tileHalfW;
            int originY = pad + maxStack * levelHeight;

            // Малюємо ззаду наперед: менший (x+y) — далі (вгорі), малюється першим.
            for (int sum = 0; sum <= (width - 1) + (height - 1); sum++)
            {
                for (int x = 0; x < width; x++)
                {
                    int y = sum - x;
                    if (y < 0 || y >= height)
                        continue;
                    if (stack[x, y] <= 0)
                        continue;

                    DrawIsoTile(pixels, texW, texH, originX, originY,
                        x, y, stack[x, y], tileHalfW, tileHalfH, levelHeight, topColor[x, y]);
                }
            }

            var tex = NewTexture(texW, texH);
            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static void DrawIsoTile(
            Color[] pixels, int texW, int texH, int originX, int originY,
            int gx, int gy, int elevation, int tileHalfW, int tileHalfH, int levelHeight, Color color)
        {
            float cx = originX + (gx - gy) * tileHalfW;
            float cy = originY + (gx + gy) * tileHalfH - elevation * levelHeight;

            var top = new Vector2(cx, cy - tileHalfH);
            var right = new Vector2(cx + tileHalfW, cy);
            var bottom = new Vector2(cx, cy + tileHalfH);
            var left = new Vector2(cx - tileHalfW, cy);

            int wall = Mathf.Max(levelHeight, elevation * levelHeight);
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