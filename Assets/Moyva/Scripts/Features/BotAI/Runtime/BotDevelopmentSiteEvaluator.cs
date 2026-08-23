using System;
using Kruty1918.Moyva.BotAI.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal readonly struct BotDevelopmentSiteScore
    {
        public BotDevelopmentSiteScore(
            int utility,
            string reason)
        {
            Utility = utility;
            Reason = reason ?? string.Empty;
        }

        public int Utility { get; }
        public string Reason { get; }
    }

    internal sealed class BotDevelopmentSiteEvaluator
    {
        private readonly IBotTerrainKnowledge _terrain;

        [Inject]
        public BotDevelopmentSiteEvaluator(
            [InjectOptional] IBotTerrainKnowledge terrain = null)
        {
            _terrain = terrain;
        }

        public BotDevelopmentSiteScore Evaluate(
            BotWorldSnapshot snapshot,
            Vector2Int anchor,
            Vector2Int cell)
        {
            int distance = Manhattan(anchor, cell);
            int utility = 520 - distance * 18;

            if (_terrain == null ||
                !_terrain.IsReady ||
                !_terrain.TryGetCell(
                    cell,
                    out BotTerrainCellSnapshot center))
            {
                return new BotDevelopmentSiteScore(
                    utility - 40,
                    $"distance={distance}; terrain=unavailable");
            }

            int open = 0;
            int water = 0;
            int muchHigher = 0;

            var neighbors =
                _terrain.GetNeighbors(cell, 2);

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (!_terrain.TryGetCell(
                        neighbors[i],
                        out BotTerrainCellSnapshot sample))
                {
                    continue;
                }

                if (IsWater(sample))
                {
                    water++;
                    continue;
                }

                if (!sample.HasObject)
                    open++;

                if (sample.TerrainLevel >
                        center.TerrainLevel + 1 ||
                    sample.Height >
                        center.Height + 0.75f)
                {
                    muchHigher++;
                }
            }

            // Development wants open land more than a fortress does.
            utility += Mathf.Clamp(open * 9, 0, 260);

            // Some water is useful for protected edges / future economy, but too
            // much water compresses the city footprint.
            utility += water <= 4
                ? water * 15
                : 60 - (water - 4) * 22;

            utility -= muchHigher * 20;

            int visibleThreatDistance =
                ClosestVisibleThreatDistance(
                    snapshot,
                    cell);

            if (visibleThreatDistance >= 0)
            {
                utility += visibleThreatDistance switch
                {
                    <= 2 => -280,
                    <= 4 => -180,
                    <= 7 => -80,
                    _ => 20,
                };
            }

            string reason =
                $"distance={distance}; open={open}; water={water}; " +
                $"higherThreatTerrain={muchHigher}; " +
                $"visibleThreatDistance={visibleThreatDistance}; " +
                $"terrainLevel={center.TerrainLevel}; height={center.Height:0.##}";

            return new BotDevelopmentSiteScore(
                utility,
                reason);
        }

        private static int ClosestVisibleThreatDistance(
            BotWorldSnapshot snapshot,
            Vector2Int cell)
        {
            if (snapshot == null)
                return -1;

            int best = int.MaxValue;

            for (int i = 0;
                 i < snapshot.VisibleEnemyUnits.Count;
                 i++)
            {
                best = Math.Min(
                    best,
                    Manhattan(
                        cell,
                        snapshot.VisibleEnemyUnits[i].Position));
            }

            for (int i = 0;
                 i < snapshot.VisibleEnemyBuildings.Count;
                 i++)
            {
                best = Math.Min(
                    best,
                    Manhattan(
                        cell,
                        snapshot.VisibleEnemyBuildings[i].Position));
            }

            return best == int.MaxValue
                ? -1
                : best;
        }

        private static bool IsWater(
            BotTerrainCellSnapshot cell)
        {
            string value =
                $"{cell.TileId} {cell.ObjectId}";

            return
                value.IndexOf(
                    "water",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf(
                    "river",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf(
                    "lake",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf(
                    "sea",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf(
                    "ocean",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int Manhattan(
            Vector2Int left,
            Vector2Int right)
            => Mathf.Abs(left.x - right.x) +
               Mathf.Abs(left.y - right.y);
    }
}
