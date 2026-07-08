using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMap
    {
        public GraphLogicalTileMap(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            TileIds = new string[Width, Height];
            GraphLayerIds = new string[Width, Height];
            LayerNames = new string[Width, Height];
            LayerHeights = new float[Width, Height];
            SurfaceHeights = new float[Width, Height];
            CellStacks = new TileStackCell[Width, Height];
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                CellStacks[x, y] = new TileStackCell();
        }

        public int Width { get; }
        public int Height { get; }
        public string[,] TileIds { get; }
        public string[,] GraphLayerIds { get; }
        public string[,] LayerNames { get; }
        public float[,] LayerHeights { get; }
        public float[,] SurfaceHeights { get; }
        public TileStackCell[,] CellStacks { get; }

        public TileStackCell GetCellStack(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;

            return CellStacks[x, y];
        }

        public void AddSample(int x, int y, GraphTileLayerSample sample)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            CellStacks[x, y].Add(sample);
            ApplyCompatibilityProjection(x, y);
        }

        private void ApplyCompatibilityProjection(int x, int y)
        {
            if (!CellStacks[x, y].TryGetTopCompatibilitySample(out var sample))
                return;

            TileIds[x, y] = sample.TileId;
            GraphLayerIds[x, y] = sample.GraphLayerId;
            LayerNames[x, y] = sample.GraphLayerName;
            LayerHeights[x, y] = sample.Height;
            SurfaceHeights[x, y] = sample.SurfaceHeight;
        }

        public Dictionary<string, bool[,]> BuildLayerMatrices()
        {
            var matrices = new Dictionary<string, bool[,]>(StringComparer.Ordinal);
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                AddCell(matrices, x, y);
            return matrices;
        }

        private void AddCell(Dictionary<string, bool[,]> matrices, int x, int y)
        {
            var stack = CellStacks[x, y];
            if (stack == null || stack.IsEmpty)
                return;

            for (int i = 0; i < stack.Samples.Count; i++)
            {
                string layerId = stack.Samples[i].GraphLayerId;
                if (string.IsNullOrEmpty(layerId))
                    continue;

                if (!matrices.TryGetValue(layerId, out var matrix))
                {
                    matrix = new bool[Width, Height];
                    matrices[layerId] = matrix;
                }

                matrix[x, y] = true;
            }
        }
    }
}
