using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallMeshBuilder : ITileWorldCreatorTerrainSideWallMeshBuilder
    {
        private readonly ITileWorldCreatorTerrainSideWallEdgeAppender _edgeAppender;

        public TileWorldCreatorTerrainSideWallMeshBuilder(ITileWorldCreatorTerrainSideWallEdgeAppender edgeAppender)
        {
            _edgeAppender = edgeAppender;
        }

        public TileWorldCreatorTerrainSideWallBuildResult Build(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config)
        {
            state.ClearBuildBuffers();
            state.Mesh.Clear();
            if (!TryCreateStats(config, out var stats, out string skipReason))
                return new TileWorldCreatorTerrainSideWallBuildResult(true, skipReason, stats, default, IndexFormat.UInt16);

            var artifacts = new TileWorldCreatorTerrainSideWallArtifactDiagnostics(config.CellSize, config.HeightStep);
            for (int x = 0; x < stats.Width; x++)
            for (int y = 0; y < stats.Height; y++)
                AppendCellWalls(state, config, x, y, stats.EdgeLevel, ref stats, ref artifacts);

            if (stats.WallCount == 0)
                return new TileWorldCreatorTerrainSideWallBuildResult(false, "no side walls produced", stats, artifacts, IndexFormat.UInt16);

            IndexFormat indexFormat = state.Vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            ApplyMesh(state, indexFormat);
            stats.VertexCount = state.Vertices.Count;
            stats.TriangleCount = state.Triangles.Count / 3;
            return new TileWorldCreatorTerrainSideWallBuildResult(false, null, stats, artifacts, indexFormat);
        }

        private static bool TryCreateStats(
            TileWorldCreatorTerrainSideWallConfig config,
            out TileWorldCreatorTerrainSideWallBuildStats stats,
            out string skipReason)
        {
            stats = new TileWorldCreatorTerrainSideWallBuildStats
            {
                DifferenceHistogram = new SortedDictionary<int, int>()
            };
            skipReason = null;
            if (config.TerrainLevelMap == null)
            {
                skipReason = "TerrainLevelMap is null.";
                return false;
            }

            stats.Width = config.TerrainLevelMap.GetLength(0);
            stats.Height = config.TerrainLevelMap.GetLength(1);
            if (stats.Width <= 0 || stats.Height <= 0)
            {
                skipReason = $"TerrainLevelMap size is {stats.Width}x{stats.Height}.";
                return false;
            }

            FindLevelRange(config.TerrainLevelMap, stats.Width, stats.Height, out stats.MinLevel, out stats.MaxLevel);
            stats.EdgeLevel = Mathf.Min(0, stats.MinLevel);
            return true;
        }

        private void AppendCellWalls(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config,
            int cellX,
            int cellY,
            int edgeLevel,
            ref TileWorldCreatorTerrainSideWallBuildStats stats,
            ref TileWorldCreatorTerrainSideWallArtifactDiagnostics artifacts)
        {
            int level = config.TerrainLevelMap[cellX, cellY];
            float minX = (cellX * config.CellSize) - (config.CellSize * 0.5f);
            float maxX = minX + config.CellSize;
            float minZ = (cellY * config.CellSize) - (config.CellSize * 0.5f);
            float maxZ = minZ + config.CellSize;
            AppendEdge(state, config, new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX + 1, cellY, "East", new Vector3(maxX, 0f, minZ), new Vector3(maxX, 0f, maxZ)), level, edgeLevel, ref stats, ref artifacts);
            AppendEdge(state, config, new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX - 1, cellY, "West", new Vector3(minX, 0f, maxZ), new Vector3(minX, 0f, minZ)), level, edgeLevel, ref stats, ref artifacts);
            AppendEdge(state, config, new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX, cellY + 1, "North", new Vector3(maxX, 0f, maxZ), new Vector3(minX, 0f, maxZ)), level, edgeLevel, ref stats, ref artifacts);
            AppendEdge(state, config, new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX, cellY - 1, "South", new Vector3(minX, 0f, minZ), new Vector3(maxX, 0f, minZ)), level, edgeLevel, ref stats, ref artifacts);
        }

        private void AppendEdge(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config,
            TileWorldCreatorTerrainSideWallEdge edge,
            int level,
            int edgeLevel,
            ref TileWorldCreatorTerrainSideWallBuildStats stats,
            ref TileWorldCreatorTerrainSideWallArtifactDiagnostics artifacts)
        {
            _edgeAppender.TryAppend(state, config, edge, level, edgeLevel, ref stats, ref artifacts);
        }

        private static void ApplyMesh(TileWorldCreatorTerrainSideWallState state, IndexFormat indexFormat)
        {
            state.Mesh.indexFormat = indexFormat;
            state.Mesh.SetVertices(state.Vertices);
            state.Mesh.SetTriangles(state.Triangles, 0);
            state.Mesh.SetUVs(0, state.Uvs);
            state.Mesh.RecalculateNormals();
            state.Mesh.RecalculateTangents();
            state.Mesh.RecalculateBounds();
        }

        private static void FindLevelRange(int[,] levels, int width, int height, out int min, out int max)
        {
            min = int.MaxValue;
            max = int.MinValue;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int level = levels[x, y];
                min = Mathf.Min(min, level);
                max = Mathf.Max(max, level);
            }
        }
    }
}
