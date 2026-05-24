using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    internal static class ProjectedMapPreviewRenderer
    {
        private const int MinTextureSize = 64;
        private const int MaxTextureSize = 512;
        private const float PaddingUnits = 0.75f;
        private static readonly Color Background = new Color(0.035f, 0.04f, 0.05f, 1f);

        public static bool ShouldProject(GraphSharedSettings settings)
        {
            if (settings == null)
                return false;

            return settings.ProjectionMode == GridProjectionMode.Isometric2D
                || settings.ProjectionMode == GridProjectionMode.Isometric3DPreview
                || settings.ProjectionMode == GridProjectionMode.HexPointy2D
                || settings.ProjectionMode == GridProjectionMode.HexFlat2D;
        }

        public static Texture2D Render(
            int mapWidth,
            int mapHeight,
            int requestedWidth,
            int requestedHeight,
            GraphSharedSettings settings,
            Func<int, int, Color> resolveBaseColor,
            Func<int, int, Color> resolveMarkerColor = null)
        {
            if (mapWidth <= 0 || mapHeight <= 0 || !ShouldProject(settings) || resolveBaseColor == null)
                return null;

            var cells = BuildCells(mapWidth, mapHeight, settings.ProjectionMode);
            if (cells.Count == 0)
                return null;

            Bounds2D bounds = CalculateBounds(cells);
            bounds.Expand(PaddingUnits);

            int targetWidth = Mathf.Clamp(requestedWidth > 0 ? requestedWidth : 128, MinTextureSize, MaxTextureSize);
            int targetHeight = Mathf.Clamp(requestedHeight > 0 ? requestedHeight : 128, MinTextureSize, MaxTextureSize);
            float scale = Mathf.Min(targetWidth / Mathf.Max(0.01f, bounds.Width), targetHeight / Mathf.Max(0.01f, bounds.Height));
            scale = Mathf.Max(1f, scale);

            int texWidth = Mathf.Clamp(Mathf.CeilToInt(bounds.Width * scale), MinTextureSize, MaxTextureSize);
            int texHeight = Mathf.Clamp(Mathf.CeilToInt(bounds.Height * scale), MinTextureSize, MaxTextureSize);

            var pixels = new Color[texWidth * texHeight];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Background;

            cells.Sort((a, b) =>
            {
                int yCompare = a.Grid.y.CompareTo(b.Grid.y);
                return yCompare != 0 ? yCompare : a.Grid.x.CompareTo(b.Grid.x);
            });

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                DrawPolygon(pixels, texWidth, texHeight, bounds, scale, cell.Vertices, resolveBaseColor(cell.Grid.x, cell.Grid.y));

                Color marker = resolveMarkerColor != null ? resolveMarkerColor(cell.Grid.x, cell.Grid.y) : Color.clear;
                if (marker.a > 0.01f)
                    DrawMarker(pixels, texWidth, texHeight, bounds, scale, cell.Center, cell.MarkerRadius, marker);
            }

            var texture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static List<ProjectedCell> BuildCells(int mapWidth, int mapHeight, GridProjectionMode mode)
        {
            var cells = new List<ProjectedCell>(mapWidth * mapHeight);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    Vector2 center = GetCenter(x, y, mode);
                    Vector2[] vertices = mode == GridProjectionMode.HexPointy2D || mode == GridProjectionMode.HexFlat2D
                        ? BuildHex(center, mode == GridProjectionMode.HexPointy2D)
                        : BuildDiamond(center);

                    cells.Add(new ProjectedCell
                    {
                        Grid = new Vector2Int(x, y),
                        Center = center,
                        Vertices = vertices,
                        MarkerRadius = mode == GridProjectionMode.HexPointy2D || mode == GridProjectionMode.HexFlat2D ? 0.32f : 0.24f
                    });
                }
            }

            return cells;
        }

        private static Vector2 GetCenter(int x, int y, GridProjectionMode mode)
        {
            return mode switch
            {
                GridProjectionMode.HexPointy2D => new Vector2(Mathf.Sqrt(3f) * (x + y * 0.5f), 1.5f * y),
                GridProjectionMode.HexFlat2D => new Vector2(1.5f * x, Mathf.Sqrt(3f) * (y + x * 0.5f)),
                _ => new Vector2(x - y, (x + y) * 0.5f),
            };
        }

        private static Vector2[] BuildDiamond(Vector2 center)
        {
            return new[]
            {
                center + new Vector2(0f, 0.5f),
                center + new Vector2(1f, 0f),
                center + new Vector2(0f, -0.5f),
                center + new Vector2(-1f, 0f),
            };
        }

        private static Vector2[] BuildHex(Vector2 center, bool pointy)
        {
            var vertices = new Vector2[6];
            float startDegrees = pointy ? 30f : 0f;
            for (int i = 0; i < vertices.Length; i++)
            {
                float radians = Mathf.Deg2Rad * (startDegrees + 60f * i);
                vertices[i] = center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            }

            return vertices;
        }

        private static Bounds2D CalculateBounds(List<ProjectedCell> cells)
        {
            var bounds = new Bounds2D
            {
                MinX = float.MaxValue,
                MinY = float.MaxValue,
                MaxX = float.MinValue,
                MaxY = float.MinValue
            };

            for (int i = 0; i < cells.Count; i++)
            {
                var vertices = cells[i].Vertices;
                for (int v = 0; v < vertices.Length; v++)
                    bounds.Include(vertices[v]);
            }

            return bounds;
        }

        private static void DrawPolygon(Color[] pixels, int texWidth, int texHeight, Bounds2D bounds, float scale, Vector2[] vertices, Color color)
        {
            if (vertices == null || vertices.Length < 3)
                return;

            int minX = texWidth - 1;
            int minY = texHeight - 1;
            int maxX = 0;
            int maxY = 0;

            var pixelVertices = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                pixelVertices[i] = ToPixel(vertices[i], bounds, scale, texHeight);
                minX = Mathf.Min(minX, Mathf.FloorToInt(pixelVertices[i].x));
                minY = Mathf.Min(minY, Mathf.FloorToInt(pixelVertices[i].y));
                maxX = Mathf.Max(maxX, Mathf.CeilToInt(pixelVertices[i].x));
                maxY = Mathf.Max(maxY, Mathf.CeilToInt(pixelVertices[i].y));
            }

            minX = Mathf.Clamp(minX, 0, texWidth - 1);
            minY = Mathf.Clamp(minY, 0, texHeight - 1);
            maxX = Mathf.Clamp(maxX, 0, texWidth - 1);
            maxY = Mathf.Clamp(maxY, 0, texHeight - 1);

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    if (!ContainsPoint(pixelVertices, new Vector2(px + 0.5f, py + 0.5f)))
                        continue;

                    pixels[py * texWidth + px] = color.a >= 0.99f ? color : AlphaBlend(pixels[py * texWidth + px], color);
                }
            }
        }

        private static void DrawMarker(Color[] pixels, int texWidth, int texHeight, Bounds2D bounds, float scale, Vector2 center, float radius, Color color)
        {
            Vector2 pixelCenter = ToPixel(center, bounds, scale, texHeight);
            float pixelRadius = Mathf.Max(2f, radius * scale);
            float radiusSqr = pixelRadius * pixelRadius;

            int minX = Mathf.Clamp(Mathf.FloorToInt(pixelCenter.x - pixelRadius), 0, texWidth - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt(pixelCenter.y - pixelRadius), 0, texHeight - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt(pixelCenter.x + pixelRadius), 0, texWidth - 1);
            int maxY = Mathf.Clamp(Mathf.CeilToInt(pixelCenter.y + pixelRadius), 0, texHeight - 1);

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float dx = px + 0.5f - pixelCenter.x;
                    float dy = py + 0.5f - pixelCenter.y;
                    if (dx * dx + dy * dy > radiusSqr)
                        continue;

                    pixels[py * texWidth + px] = AlphaBlend(pixels[py * texWidth + px], color);
                }
            }
        }

        private static Vector2 ToPixel(Vector2 world, Bounds2D bounds, float scale, int texHeight)
        {
            float x = (world.x - bounds.MinX) * scale;
            float y = (world.y - bounds.MinY) * scale;
            return new Vector2(x, texHeight - 1 - y);
        }

        private static bool ContainsPoint(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                bool intersects = ((polygon[i].y > point.y) != (polygon[j].y > point.y))
                    && point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / Mathf.Max(0.0001f, polygon[j].y - polygon[i].y) + polygon[i].x;
                if (intersects)
                    inside = !inside;
            }

            return inside;
        }

        private static Color AlphaBlend(Color dst, Color src)
        {
            float srcAlpha = src.a;
            float dstAlpha = dst.a * (1f - srcAlpha);
            float outAlpha = srcAlpha + dstAlpha;
            if (outAlpha < 0.001f)
                return Color.clear;

            return new Color(
                (src.r * srcAlpha + dst.r * dstAlpha) / outAlpha,
                (src.g * srcAlpha + dst.g * dstAlpha) / outAlpha,
                (src.b * srcAlpha + dst.b * dstAlpha) / outAlpha,
                outAlpha);
        }

        private struct ProjectedCell
        {
            public Vector2Int Grid;
            public Vector2 Center;
            public Vector2[] Vertices;
            public float MarkerRadius;
        }

        private struct Bounds2D
        {
            public float MinX;
            public float MinY;
            public float MaxX;
            public float MaxY;
            public float Width => Mathf.Max(0.01f, MaxX - MinX);
            public float Height => Mathf.Max(0.01f, MaxY - MinY);

            public void Include(Vector2 point)
            {
                MinX = Mathf.Min(MinX, point.x);
                MinY = Mathf.Min(MinY, point.y);
                MaxX = Mathf.Max(MaxX, point.x);
                MaxY = Mathf.Max(MaxY, point.y);
            }

            public void Expand(float amount)
            {
                MinX -= amount;
                MinY -= amount;
                MaxX += amount;
                MaxY += amount;
            }
        }
    }
}
