using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITerrainPlanApplier
    {
        /// <summary>
        /// Applies the recipe's terrain plan to the exported logical map:
        /// per-cell relief heights, river/lake channel carving, stair
        /// corridors, road/footpath routes. Returns the generated passage
        /// plan (possibly empty).
        /// </summary>
        TerrainPassagePlan Apply(
            LogicalTileMap map,
            GeneratorMapRecipe recipe,
            float[,] reliefField,
            int seed,
            RecipeHydrologyPlan hydrology = null);
    }

    /// <summary>
    /// Single authority for terrain-plan mutations of the logical tile map.
    /// Runs after TWC layer export, before grid publication and mesh building.
    /// </summary>
    internal sealed class TerrainPlanApplicationService : ITerrainPlanApplier
    {
        private readonly ITerrainPassagePlanner _passagePlanner;
        private readonly ITerrainRoutePlanner _routePlanner;
        private readonly ITerrainShorePlanner _shorePlanner;

        public TerrainPlanApplicationService(
            ITerrainPassagePlanner passagePlanner,
            ITerrainRoutePlanner routePlanner,
            [Zenject.InjectOptional] ITerrainShorePlanner shorePlanner = null)
        {
            _passagePlanner = passagePlanner;
            _routePlanner = routePlanner;
            _shorePlanner = shorePlanner;
        }

        public TerrainPassagePlan Apply(
            LogicalTileMap map,
            GeneratorMapRecipe recipe,
            float[,] reliefField,
            int seed,
            RecipeHydrologyPlan hydrology = null)
        {
            if (map == null || recipe == null)
                return null;

            if (reliefField != null)
                ApplyRelief(map, recipe, reliefField);

            // Channel carving must precede the shore pass: river/lake cells
            // whose flood level never rose above raw terrain carry a water
            // surface just below the land surface, so the land winner would
            // hide the channel. Dropping their land columns to the bed makes
            // the water sheet the winner and lets the shoreline measure the
            // true water contour.
            CarveWaterChannels(map, recipe, hydrology);

            // Shore runs on the post-relief surfaces so the band follows the
            // real waterline; passages and routes read the graded map.
            _shorePlanner?.Apply(
                map,
                recipe.Shore,
                recipe.SharedGeneratorSettings?.WaterLikeTileIds);

            TerrainPassagePlan passages = _passagePlanner.Plan(
                map.SurfaceHeights,
                recipe.Passages);
            if (passages != null && passages.Flights.Count > 0)
            {
                // A stair module replaces every terrain sample on its cell —
                // a flight through a river/lake would erase the water sheet
                // and read as a hole in the channel, so water-crossing
                // flights are dropped whole.
                passages.Flights.RemoveAll(flight => FlightTouchesWater(map, flight));
            }
            if (passages != null && passages.Flights.Count > 0)
                CarveFlights(map, passages, recipe.Passages);

            TerrainRoutePlan routes = _routePlanner.Plan(
                map.SurfaceHeights,
                passages,
                recipe.Routes,
                seed,
                MovementHeightLimits.Default.AutoStepMaxMeters);
            if (routes != null)
                ApplyRoutes(map, routes, recipe.Routes, recipe.Passages);

            map.ReprojectAll();
            return passages;
        }

        private static void ApplyRelief(
            LogicalTileMap map,
            GeneratorMapRecipe recipe,
            float[,] field)
        {
            int width = Mathf.Min(map.Width, field.GetLength(0));
            int height = Mathf.Min(map.Height, field.GetLength(1));
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                TileStackCell stack = map.GetCellStack(x, y);
                if (stack == null || stack.IsEmpty)
                    continue;

                float surface = field[x, y];
                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    TileLayerSample sample = stack.Samples[i];
                    if (!sample.IsTerrainLike
                        || IsWaterLike(recipe, sample.TileId))
                    {
                        continue;
                    }

                    stack.SetAt(i, sample.WithSurfaceHeight(surface));
                }
            }
        }

        /*
         * On river/lake cells the water surface sits just below the
         * surrounding land (fill level minus a few centimetres). Without a
         * carve, the land sample keeps the raw terrain height and wins the
         * visual vote by millimetres — the channel renders as grass. Lower
         * every land column to the planned bed so the water sheet wins and
         * the bed column below stays consistent with the provider geometry.
         */
        private static void CarveWaterChannels(
            LogicalTileMap map,
            GeneratorMapRecipe recipe,
            RecipeHydrologyPlan hydrology)
        {
            if (hydrology?.RiverMask == null || hydrology.LakeMask == null
                || hydrology.BedHeight == null)
            {
                return;
            }

            int width = Mathf.Min(map.Width, hydrology.RiverMask.GetLength(0));
            int height = Mathf.Min(map.Height, hydrology.RiverMask.GetLength(1));
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!hydrology.RiverMask[x, y] && !hydrology.LakeMask[x, y])
                    continue;

                float surface = hydrology.WaterSurface != null
                    ? hydrology.WaterSurface[x, y]
                    : float.NaN;
                float bed = hydrology.BedHeight[x, y];
                float cap = IsFinite(bed)
                    ? bed
                    : IsFinite(surface) ? surface - 0.01f : float.NaN;
                if (!IsFinite(cap))
                    continue;

                TileStackCell stack = map.GetCellStack(x, y);
                if (stack == null || stack.IsEmpty)
                    continue;

                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    TileLayerSample sample = stack.Samples[i];
                    if (!sample.IsTerrainLike
                        || sample.TileGeometryMode == TileGeometryMode.SurfaceOnly
                        || IsWaterLike(recipe, sample.TileId))
                    {
                        continue;
                    }

                    float current = IsFinite(sample.SurfaceHeight)
                        ? sample.SurfaceHeight
                        : sample.Height;
                    if (!IsFinite(current) || current <= cap)
                        continue;

                    stack.SetAt(i, sample.WithSurfaceHeight(cap));
                }
            }
        }

        private static void CarveFlights(
            LogicalTileMap map,
            TerrainPassagePlan plan,
            TerrainPassageConfig config)
        {
            foreach (StairFlight flight in plan.Flights)
            {
                for (int i = 0; i < flight.Modules.Length; i++)
                {
                    Vector2Int cell = flight.Modules[i];
                    TileStackCell stack = map.GetCellStack(cell.x, cell.y);
                    if (stack == null)
                        continue;

                    // The corridor removes the terrain tile: its neighbours
                    // open borders/skirts toward the trench. Non-terrain
                    // samples (decorations, spawns) are kept. The module's
                    // top edge is the walkable surface of the stair cell.
                    stack.RemoveAll(sample => sample.IsTerrainLike);
                    float top = flight.ModuleTopY[i];
                    stack.Add(new TileLayerSample(
                        layerId: "passage:stair",
                        layerName: "StairPassage",
                        blueprintLayerGuid: null,
                        buildLayerGuid: null,
                        tileId: config.StairTileId,
                        presetId: flight.ThemeId,
                        layerKind: LayerKind.StairPassage,
                        sortingOrder: 0,
                        layerOrder: 0,
                        terrainPriority: 0,
                        height: top,
                        surfaceHeight: top,
                        sourceLayerId: null,
                        tileGeometryMode: TileGeometryMode.SolidTerrain,
                        authoredClosurePolicy: AuthoredClosurePolicy.PreserveAuthored));
                }
            }
        }

        private static bool FlightTouchesWater(LogicalTileMap map, StairFlight flight)
        {
            if (flight?.Modules == null)
                return false;
            for (int i = 0; i < flight.Modules.Length; i++)
            {
                Vector2Int cell = flight.Modules[i];
                if (IsWaterCell(map.GetCellStack(cell.x, cell.y)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The cell renders a water sheet: a surface-only terrain sample is
        /// the channel/lake/sea marker, matching the spawner's water test.
        /// </summary>
        private static bool IsWaterCell(TileStackCell stack)
        {
            if (stack == null)
                return false;
            for (int i = 0; i < stack.Samples.Count; i++)
            {
                TileLayerSample sample = stack.Samples[i];
                if (sample.IsTerrainLike
                    && sample.TileGeometryMode == TileGeometryMode.SurfaceOnly)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ApplyRoutes(
            LogicalTileMap map,
            TerrainRoutePlan routes,
            TerrainRouteConfig config,
            TerrainPassageConfig passages)
        {
            AddRouteCells(map, routes.RoadCells, config, config.RoadTileId, "route:road");
            AddRouteCells(map, routes.FootpathCells, config, config.FootpathTileId, "route:footpath");
        }

        private static void AddRouteCells(
            LogicalTileMap map,
            List<Vector2Int> cells,
            TerrainRouteConfig config,
            string tileId,
            string layerId)
        {
            if (cells == null || string.IsNullOrWhiteSpace(tileId))
                return;

            foreach (Vector2Int cell in cells)
            {
                TileStackCell stack = map.GetCellStack(cell.x, cell.y);
                // A road/footpath overlay on a water cell would cover the
                // sheet it sits above — routes never enter open water.
                if (stack == null || stack.IsEmpty || IsWaterCell(stack))
                    continue;

                float surface = map.SurfaceHeights[cell.x, cell.y];
                if (float.IsNaN(surface) || float.IsInfinity(surface))
                    continue;

                // Never overwrite generated stair modules or an existing route.
                bool occupied = false;
                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    LayerKind kind = stack.Samples[i].LayerKind;
                    if (kind == LayerKind.StairPassage || kind == LayerKind.Road)
                    {
                        occupied = true;
                        break;
                    }
                }
                if (occupied)
                    continue;

                // Road/footpath are full dual-grid tile themes: the route
                // sample rides a few millimetres above the terrain surface so
                // it wins the visual composition while traversal stays flat.
                stack.Add(new TileLayerSample(
                    layerId: layerId,
                    layerName: tileId,
                    blueprintLayerGuid: null,
                    buildLayerGuid: null,
                    tileId: tileId,
                    presetId: tileId,
                    layerKind: LayerKind.Road,
                    sortingOrder: 0,
                    layerOrder: 0,
                    terrainPriority: 0,
                    height: surface,
                    surfaceHeight: surface + Mathf.Max(0f, config.OverlaySurfaceOffsetMeters),
                    sourceLayerId: null,
                    tileGeometryMode: TileGeometryMode.SolidTerrain,
                    authoredClosurePolicy: AuthoredClosurePolicy.PreserveAuthored));
            }
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool IsWaterLike(GeneratorMapRecipe recipe, string tileId)
        {
            string[] waterIds = recipe.SharedGeneratorSettings?.WaterLikeTileIds;
            if (waterIds == null || string.IsNullOrWhiteSpace(tileId))
                return false;
            foreach (string id in waterIds)
            {
                if (string.Equals(id, tileId, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
