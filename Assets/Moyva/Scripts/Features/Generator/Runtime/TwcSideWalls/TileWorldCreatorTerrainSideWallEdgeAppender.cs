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
            ref TileWorldCreatorTerrainSideWallBuildStats stats,
            ref TileWorldCreatorTerrainSideWallArtifactDiagnostics diagnostics)
        {
            bool hasNeighbour = IsInside(edge.NeighbourX, edge.NeighbourY, stats.Width, stats.Height);
            ObserveBoundaryCandidate(state, edge, currentLevel, edgeLevel, hasNeighbour, config.IncludeMapBoundaryWalls, ref stats, ref diagnostics);
            if (!hasNeighbour && !config.IncludeMapBoundaryWalls)
                return;

            int neighbourLevel = hasNeighbour ? config.TerrainLevelMap[edge.NeighbourX, edge.NeighbourY] : edgeLevel;
            if (currentLevel <= neighbourLevel)
                return;

            AppendWall(state, config, edge, currentLevel, neighbourLevel, hasNeighbour, ref stats, ref diagnostics);
        }

        private static void ObserveBoundaryCandidate(
            TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallEdge edge,
            int currentLevel, int edgeLevel, bool hasNeighbour, bool includeBoundary,
            ref TileWorldCreatorTerrainSideWallBuildStats stats,
            ref TileWorldCreatorTerrainSideWallArtifactDiagnostics diagnostics)
        {
            if (hasNeighbour)
                return;

            diagnostics.BoundaryEdgeChecks++;
            if (currentLevel <= edgeLevel)
                return;

            diagnostics.BoundaryWallCandidates++;
            if (includeBoundary)
                return;

            stats.SkippedBoundaryWalls++;
            diagnostics.SkippedBoundaryWallCandidates++;
            AddArtifactSample(state, $"SKIP boundary cell=({edge.CellX},{edge.CellY}) dir={edge.Direction} outside=({edge.NeighbourX},{edge.NeighbourY}) levels={edgeLevel}->{currentLevel} edge=({TileWorldCreatorTerrainSideWallFormat.Vector3(edge.Start)})->({TileWorldCreatorTerrainSideWallFormat.Vector3(edge.End)})");
        }

        private static void AppendWall(
            TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallConfig config,
            TileWorldCreatorTerrainSideWallEdge edge, int currentLevel, int neighbourLevel, bool hasNeighbour,
            ref TileWorldCreatorTerrainSideWallBuildStats stats,
            ref TileWorldCreatorTerrainSideWallArtifactDiagnostics diagnostics)
        {
            float bottomY = config.BaseY + (neighbourLevel * config.HeightStep);
            float topY = config.BaseY + (currentLevel * config.HeightStep);
            float wallLength = Vector3.Distance(edge.Start, edge.End);
            float wallHeight = topY - bottomY;
            AppendQuad(state, edge, bottomY, topY, wallLength, wallHeight);
            RegisterStats(state, edge, currentLevel, neighbourLevel, hasNeighbour, bottomY, topY, wallLength, wallHeight, ref stats, ref diagnostics);
        }

        private static void RegisterStats(
            TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallEdge edge,
            int currentLevel, int neighbourLevel, bool hasNeighbour, float bottomY, float topY, float wallLength, float wallHeight,
            ref TileWorldCreatorTerrainSideWallBuildStats stats,
            ref TileWorldCreatorTerrainSideWallArtifactDiagnostics diagnostics)
        {
            if (hasNeighbour)
                diagnostics.GeneratedInteriorWalls++;
            else
                diagnostics.GeneratedBoundaryWalls++;

            diagnostics.Observe(edge, currentLevel, neighbourLevel, hasNeighbour, wallLength, wallHeight);
            AddGeneratedArtifactSampleIfNeeded(state, edge, currentLevel, neighbourLevel, hasNeighbour, wallLength, wallHeight, diagnostics);
            IncrementDirection(ref stats, edge.Direction);
            int diff = currentLevel - neighbourLevel;
            stats.TotalLevelDifference += diff;
            stats.MaxLevelDifference = Mathf.Max(stats.MaxLevelDifference, diff);
            stats.DifferenceHistogram.TryGetValue(diff, out int count);
            stats.DifferenceHistogram[diff] = count + 1;
            if (state.Samples.Count < 16)
                state.Samples.Add($"cell=({edge.CellX},{edge.CellY}) dir={edge.Direction} levels={neighbourLevel}->{currentLevel} y={TileWorldCreatorTerrainSideWallFormat.Number(bottomY)}->{TileWorldCreatorTerrainSideWallFormat.Number(topY)} boundary={!hasNeighbour}");
        }

        private static void AddGeneratedArtifactSampleIfNeeded(
            TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallEdge edge,
            int currentLevel, int neighbourLevel, bool hasNeighbour, float wallLength, float wallHeight,
            TileWorldCreatorTerrainSideWallArtifactDiagnostics diagnostics)
        {
            if (hasNeighbour && !diagnostics.IsSuspicious(wallLength, wallHeight))
                return;

            AddArtifactSample(state, $"GEN {(hasNeighbour ? "interior" : "boundary")} cell=({edge.CellX},{edge.CellY}) dir={edge.Direction} neighbour=({edge.NeighbourX},{edge.NeighbourY}) levels={neighbourLevel}->{currentLevel} length={TileWorldCreatorTerrainSideWallFormat.Number(wallLength)} height={TileWorldCreatorTerrainSideWallFormat.Number(wallHeight)} edge=({TileWorldCreatorTerrainSideWallFormat.Vector3(edge.Start)})->({TileWorldCreatorTerrainSideWallFormat.Vector3(edge.End)})");
        }

        private static void AppendQuad(TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallEdge edge, float bottomY, float topY, float wallLength, float wallHeight)
        {
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
            state.Uvs.Add(new Vector2(0f, 0f));
            state.Uvs.Add(new Vector2(0f, wallHeight));
            state.Uvs.Add(new Vector2(wallLength, wallHeight));
            state.Uvs.Add(new Vector2(wallLength, 0f));
        }

        private static void IncrementDirection(ref TileWorldCreatorTerrainSideWallBuildStats stats, string direction)
        {
            if (direction == "East")
                stats.EastWalls++;
            else if (direction == "West")
                stats.WestWalls++;
            else if (direction == "North")
                stats.NorthWalls++;
            else if (direction == "South")
                stats.SouthWalls++;
        }

        private static void AddArtifactSample(TileWorldCreatorTerrainSideWallState state, string sample)
        {
            if (state.ArtifactSamples.Count < 24)
                state.ArtifactSamples.Add(sample);
        }

        private static bool IsInside(int cellX, int cellY, int width, int height)
            => cellX >= 0 && cellY >= 0 && cellX < width && cellY < height;
    }
}
