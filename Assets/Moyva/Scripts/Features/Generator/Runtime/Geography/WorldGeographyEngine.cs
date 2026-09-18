using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Deterministic world-geography pipeline:
    /// landmass → elevation → level quantization → hydrology → coast →
    /// biomes/objects → fairness validation. Failed attempts retry with a
    /// hashed sub-seed; the best candidate wins.
    /// </summary>
    internal sealed class WorldGeographyEngine
    {
        private readonly LandmassStage _landmass = new LandmassStage();
        private readonly ElevationStage _elevation = new ElevationStage();
        private readonly TerrainLevelStage _terrain = new TerrainLevelStage();
        private readonly HydrologyStage _hydrology = new HydrologyStage();
        private readonly CoastStage _coast = new CoastStage();
        private readonly BiomeStage _biomes = new BiomeStage();
        private readonly FairnessStage _fairness = new FairnessStage();

        /// <summary>
        /// Runs up to MaxGenerationAttempts deterministic attempts and returns
        /// the first accepted world, otherwise the highest-scoring candidate.
        /// </summary>
        internal WorldGeographyResult Generate(WorldGenerationRequest baseRequest)
        {
            int maxAttempts = Mathf.Max(1, baseRequest.Config?.MaxGenerationAttempts ?? 1);
            WorldGeographyResult best = null;
            float bestScore = float.NegativeInfinity;
            var reports = new List<WorldGenerationReport>(maxAttempts);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int attemptSeed = attempt == 0
                    ? baseRequest.Seed
                    : DeterministicNoise.AttemptSeed(baseRequest.Seed, attempt);
                var request = ReSeed(baseRequest, attemptSeed);
                var result = GenerateOnce(request);
                result.Attempt = attempt;
                reports.Add(result.Report);

                if (result.Report.Accepted)
                {
                    result.Report.Attempt = attempt;
                    AttachHistory(result, reports);
                    return result;
                }

                float score = ScoreCandidate(result.Report);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = result;
                }
            }

            best.Report.GateFailures.Add("accepted-as-best-effort");
            AttachHistory(best, reports);
            return best;
        }

        private void AttachHistory(WorldGeographyResult result, List<WorldGenerationReport> reports)
        {
            if (result?.Report == null) return;
            foreach (var r in reports)
                if (r != null && r != result.Report)
                    result.Report.GateFailures.Add($"prior-attempt[{r.Attempt}]: {r.ToSummary()}");
        }

        internal WorldGeographyResult GenerateOnce(WorldGenerationRequest request)
        {
            var result = new WorldGeographyResult
            {
                Width = request.Width,
                Height = request.Height,
                Seed = request.Seed,
                Archetype = request.Archetype,
                Report = new WorldGenerationReport
                {
                    Seed = request.Seed,
                    Archetype = request.ArchetypeId(),
                },
            };

            float[,] landMask = _landmass.Generate(request);
            float[,] elevation = _elevation.Generate(request, landMask);
            int[,] levels = _terrain.Quantize(request, elevation, landMask);
            var hydro = _hydrology.Generate(request, levels);
            bool[,] beach = _coast.Generate(
                request, hydro.Levels, hydro.LakeMask, hydro.RiverMask, hydro.WaterSurface);
            var biome = _biomes.Generate(
                request, hydro.Levels, hydro.RiverMask, hydro.LakeMask,
                beach, hydro.WaterSurface, landMask, elevation);
            var fair = _fairness.Evaluate(
                request, biome.TileMap, hydro.Levels, hydro.RiverMask,
                hydro.LakeMask, hydro.WaterSurface, biome.ForestField,
                biome.HeightMap);

            result.TileMap = biome.TileMap;
            result.HeightMap = biome.HeightMap;
            result.TerrainLevelMap = hydro.Levels;
            result.ObjectMap = biome.ObjectMap;
            result.RiverMask = hydro.RiverMask;
            result.LakeMask = hydro.LakeMask;
            result.BeachMask = beach;
            result.FlowParent = hydro.FlowParent;
            result.FlowAccumulation = hydro.Accumulation;
            result.MoistureField = biome.MoistureField;
            result.ForestField = biome.ForestField;
            result.SpawnHints = ToCells(fair.Spawns);

            var report = result.Report;
            report.RiverCells = hydro.RiverCellCount;
            report.LakeCells = hydro.LakeCellCount;
            report.MountainCells = biome.MountainCells;
            report.ForestCells = biome.ForestCells;
            report.ObjectCells = biome.ObjectCells;
            report.SpawnCount = fair.Spawns?.Length ?? 0;
            report.MinOpportunityScore = fair.MinOpportunityScore;
            report.MaxOpportunityScore = fair.MaxOpportunityScore;
            report.OpportunityGap = fair.OpportunityGap;
            report.MinPairwiseSpawnDistance = fair.MinPairwiseDistance;
            report.ConnectivityOk = fair.ConnectivityOk;
            report.LargestLandComponent = fair.LargestLandComponent;
            foreach (var failure in fair.GateFailures)
                report.GateFailures.Add(failure);

            int land = 0, water = 0, beachCells = 0;
            for (int x = 0; x < request.Width; x++)
            for (int y = 0; y < request.Height; y++)
            {
                if (result.TileMap[x, y] == "water") water++;
                else land++;
                if (beach[x, y]) beachCells++;
            }
            report.LandCells = land;
            report.WaterCells = water;
            report.BeachCells = beachCells;
            report.LandFraction = (float)land / (request.Width * request.Height);

            return result;
        }

        private static float ScoreCandidate(WorldGenerationReport report)
        {
            if (report == null) return float.NegativeInfinity;
            float score = 0f;
            score += report.ConnectivityOk ? 4f : 0f;
            score += report.SpawnCount * 2f;
            score -= report.OpportunityGap;
            score += Mathf.Clamp01(report.LandFraction) * 2f;
            score -= report.GateFailures.Count * 0.5f;
            return score;
        }

        private static Vector2Int[] ToCells(SpawnHint[] hints)
        {
            if (hints == null || hints.Length == 0)
                return System.Array.Empty<Vector2Int>();
            var cells = new Vector2Int[hints.Length];
            for (int i = 0; i < hints.Length; i++)
                cells[i] = hints[i].Cell;
            return cells;
        }

        private static WorldGenerationRequest ReSeed(WorldGenerationRequest source, int seed)
            => new WorldGenerationRequest(
                seed,
                source.Width,
                source.Height,
                source.Archetype,
                source.PlayerCount,
                source.Config,
                source.WaterLevel,
                source.ShoreLevel,
                source.LandLevel,
                source.HillLevel,
                source.MaxLevel,
                source.HeightStep,
                source.WaterSurfaceOffset);
    }
}
