using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallMeshBuilder : ITileWorldCreatorTerrainSideWallMeshBuilder
    {
        private readonly ITileWorldCreatorTerrainSideWallEdgeAppender _edgeAppender;

        public TileWorldCreatorTerrainSideWallMeshBuilder(
            ITileWorldCreatorTerrainSideWallEdgeAppender edgeAppender)
        {
            _edgeAppender = edgeAppender;
        }

        /// <summary>Перебудовує бічні стінки для всіх перепадів висоти в мапі.</summary>
        public void Build(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config)
        {
            state.ClearBuildBuffers();
            state.Mesh.Clear();
            if (!TryResolveGrid(config, out int width, out int height, out int edgeLevel))
                return;

            int wallCount = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                AppendCellWalls(state, config, x, y, width, height, edgeLevel, ref wallCount);

            if (wallCount == 0)
                return;

            state.Mesh.indexFormat = state.Vertices.Count > 65000
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;
            state.Mesh.SetVertices(state.Vertices);
            state.Mesh.SetTriangles(state.Triangles, 0);
            state.Mesh.SetUVs(0, state.Uvs);
            state.Mesh.RecalculateNormals();
            state.Mesh.RecalculateTangents();
            state.Mesh.RecalculateBounds();
        }

        private static bool TryResolveGrid(
            TileWorldCreatorTerrainSideWallConfig config,
            out int width,
            out int height,
            out int edgeLevel)
        {
            width = config.TerrainLevelMap?.GetLength(0) ?? 0;
            height = config.TerrainLevelMap?.GetLength(1) ?? 0;
            edgeLevel = 0;
            if (width <= 0 || height <= 0)
                return false;

            int minimum = int.MaxValue;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                minimum = Mathf.Min(minimum, config.TerrainLevelMap[x, y]);

            edgeLevel = Mathf.Min(0, minimum);
            return true;
        }

        private void AppendCellWalls(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config,
            int cellX,
            int cellY,
            int width,
            int height,
            int edgeLevel,
            ref int wallCount)
        {
            int level = config.TerrainLevelMap[cellX, cellY];
            float minX = (cellX - 0.5f) * config.CellSize;
            float maxX = minX + config.CellSize;
            float minZ = (cellY - 0.5f) * config.CellSize;
            float maxZ = minZ + config.CellSize;

            AppendEdge(new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX + 1, cellY, "East", new Vector3(maxX, 0f, minZ), new Vector3(maxX, 0f, maxZ)), ref wallCount);
            AppendEdge(new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX - 1, cellY, "West", new Vector3(minX, 0f, maxZ), new Vector3(minX, 0f, minZ)), ref wallCount);
            AppendEdge(new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX, cellY + 1, "North", new Vector3(maxX, 0f, maxZ), new Vector3(minX, 0f, maxZ)), ref wallCount);
            AppendEdge(new TileWorldCreatorTerrainSideWallEdge(cellX, cellY, cellX, cellY - 1, "South", new Vector3(minX, 0f, minZ), new Vector3(maxX, 0f, minZ)), ref wallCount);

            void AppendEdge(TileWorldCreatorTerrainSideWallEdge edge, ref int count)
            {
                _edgeAppender.TryAppend(
                    state,
                    config,
                    edge,
                    level,
                    edgeLevel,
                    width,
                    height,
                    ref count);
            }
        }
    }
}
