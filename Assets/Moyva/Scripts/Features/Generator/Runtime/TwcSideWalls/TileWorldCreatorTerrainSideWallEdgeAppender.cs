using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallEdgeAppender : ITileWorldCreatorTerrainSideWallEdgeAppender
    {
        public void TryAppend(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config,
            TileWorldCreatorTerrainSideWallEdge edge,
            int currentLevel,
            int edgeLevel,
            int width,
            int height,
            ref int wallCount)
        {
            bool hasNeighbour = edge.NeighbourX >= 0
                                && edge.NeighbourY >= 0
                                && edge.NeighbourX < width
                                && edge.NeighbourY < height;
            if (!hasNeighbour && !config.IncludeMapBoundaryWalls)
                return;

            int neighbourLevel = hasNeighbour
                ? config.TerrainLevelMap[edge.NeighbourX, edge.NeighbourY]
                : edgeLevel;
            if (currentLevel <= neighbourLevel)
                return;

            AppendQuad(
                state,
                edge,
                config.BaseY + neighbourLevel * config.HeightStep,
                config.BaseY + currentLevel * config.HeightStep);
            wallCount++;
        }

        private static void AppendQuad(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallEdge edge,
            float bottomY,
            float topY)
        {
            float wallLength = Vector3.Distance(edge.Start, edge.End);
            float wallHeight = topY - bottomY;
            int start = state.Vertices.Count;
            state.Vertices.Add(new Vector3(edge.Start.x, bottomY, edge.Start.z));
            state.Vertices.Add(new Vector3(edge.Start.x, topY, edge.Start.z));
            state.Vertices.Add(new Vector3(edge.End.x, topY, edge.End.z));
            state.Vertices.Add(new Vector3(edge.End.x, bottomY, edge.End.z));
            state.Triangles.Add(start);
            state.Triangles.Add(start + 1);
            state.Triangles.Add(start + 2);
            state.Triangles.Add(start);
            state.Triangles.Add(start + 2);
            state.Triangles.Add(start + 3);
            state.Uvs.Add(Vector2.zero);
            state.Uvs.Add(new Vector2(0f, wallHeight));
            state.Uvs.Add(new Vector2(wallLength, wallHeight));
            state.Uvs.Add(new Vector2(wallLength, 0f));
        }
    }
}
