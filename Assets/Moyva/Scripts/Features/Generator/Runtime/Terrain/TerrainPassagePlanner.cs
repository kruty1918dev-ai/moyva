using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITerrainPassagePlanner
    {
        /// <summary>
        /// Plans stair flights across ledges of the authoritative surface map.
        /// A flight needs `drop / moduleRise` consecutive low-plateau cells
        /// ending at the ledge, plus a same-level entry cell.
        /// </summary>
        TerrainPassagePlan Plan(float[,] surfaces, TerrainPassageConfig config);
    }

    /// <summary>
    /// Deterministic stair placement: iterate ledges in cell order, place a
    /// corridor when spacing and corridor constraints hold. No randomness.
    /// </summary>
    internal sealed class TerrainPassagePlanner : ITerrainPassagePlanner
    {
        private const float Epsilon = 0.01f;

        private static readonly Vector2Int[] Directions =
        {
            new(0, 1),   // +Z
            new(1, 0),   // +X
            new(0, -1),  // -Z
            new(-1, 0),  // -X
        };

        public TerrainPassagePlan Plan(float[,] surfaces, TerrainPassageConfig config)
        {
            var plan = new TerrainPassagePlan();
            if (surfaces == null || config == null || !config.Enabled)
                return plan;

            int width = surfaces.GetLength(0);
            int height = surfaces.GetLength(1);
            float moduleRise = Mathf.Max(0.01f, config.ModuleRiseMeters);
            var occupied = new HashSet<Vector2Int>();
            var entrances = new List<Vector2Int>();
            int spacing = Mathf.Max(1, config.MinEntranceSpacingCells);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var lowCell = new Vector2Int(x, y);
                float low = surfaces[x, y];
                if (!IsFinite(low))
                    continue;

                for (int d = 0; d < Directions.Length; d++)
                {
                    Vector2Int dir = Directions[d];
                    var highCell = lowCell + dir;
                    if (!InBounds(highCell, width, height))
                        continue;

                    float high = surfaces[highCell.x, highCell.y];
                    if (!IsFinite(high))
                        continue;

                    float drop = high - low;
                    if (drop < config.MinLedgeDropMeters - Epsilon
                        || drop > config.MaxLedgeDropMeters + Epsilon)
                        continue;

                    int modules = Mathf.RoundToInt(drop / moduleRise);
                    if (modules < 1 || Mathf.Abs(modules * moduleRise - drop) > Epsilon)
                        continue;

                    if (plan.Flights.Count >= Mathf.Max(0, config.MaxFlights))
                        return plan;

                    Vector2Int entranceCandidate = lowCell - dir * modules;
                    if (!EntranceIsSpaced(entranceCandidate, entrances, spacing))
                        continue;

                    if (!TryBuildFlight(
                            surfaces, width, height, lowCell, dir, d,
                            modules, low, high, moduleRise, occupied, config,
                            out StairFlight flight))
                        continue;

                    plan.Flights.Add(flight);
                    entrances.Add(flight.Entrance);
                }
            }

            return plan;
        }

        private static bool TryBuildFlight(
            float[,] surfaces,
            int width,
            int height,
            Vector2Int lowCell,
            Vector2Int dir,
            int directionIndex,
            int modules,
            float low,
            float high,
            float moduleRise,
            HashSet<Vector2Int> occupied,
            TerrainPassageConfig config,
            out StairFlight flight)
        {
            flight = null;
            var cells = new Vector2Int[modules];
            var tops = new float[modules];

            // Modules[0] is the lowest: corridor extends from the ledge back
            // into the low plateau. Module i sits at cell lowCell - (K-1-i)*dir.
            for (int i = 0; i < modules; i++)
            {
                Vector2Int cell = lowCell - dir * (modules - 1 - i);
                if (!InBounds(cell, width, height)
                    || occupied.Contains(cell)
                    || !IsFinite(surfaces[cell.x, cell.y])
                    || Mathf.Abs(surfaces[cell.x, cell.y] - low) > Epsilon)
                    return false;

                cells[i] = cell;
                tops[i] = low + (i + 1) * moduleRise;
            }

            Vector2Int entrance = lowCell - dir * modules;
            if (!InBounds(entrance, width, height)
                || occupied.Contains(entrance)
                || !IsFinite(surfaces[entrance.x, entrance.y])
                || Mathf.Abs(surfaces[entrance.x, entrance.y] - low) > Epsilon)
                return false;

            flight = new StairFlight
            {
                Entrance = entrance,
                ExitModule = lowCell,
                Exit = lowCell + dir,
                DirectionIndex = directionIndex,
                Modules = cells,
                ModuleTopY = tops,
                LowSurfaceY = low,
                HighSurfaceY = high,
                ThemeId = config.StairThemeId,
            };

            foreach (Vector2Int cell in cells)
                occupied.Add(cell);
            return true;
        }

        private static bool EntranceIsSpaced(
            Vector2Int candidate,
            List<Vector2Int> entrances,
            int spacing)
        {
            for (int i = 0; i < entrances.Count; i++)
            {
                Vector2Int delta = entrances[i] - candidate;
                if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) < spacing)
                    return false;
            }
            return true;
        }

        private static bool InBounds(Vector2Int cell, int width, int height)
            => cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
