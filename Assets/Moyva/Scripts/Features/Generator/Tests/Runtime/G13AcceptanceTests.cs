using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G13 acceptance: planned stair passages must stay consistent with the
    /// traversal classification a unit actually gets, in both directions.
    /// </summary>
    public sealed class G13AcceptanceTests
    {
        private const float Rise = 0.5f;

        private static TerrainPassageConfig Config(
            float moduleRise = Rise,
            float minDrop = Rise,
            float maxDrop = 1.5f,
            int spacing = 1,
            int maxFlights = 32)
        {
            return new TerrainPassageConfig
            {
                Enabled = true,
                ModuleRiseMeters = moduleRise,
                MinLedgeDropMeters = minDrop,
                MaxLedgeDropMeters = maxDrop,
                MinEntranceSpacingCells = spacing,
                MaxFlights = maxFlights,
                StairTileId = "stair",
                StairThemeId = "stone",
            };
        }

        /*
         * What traversal sees per cell after carving: a stair module replaces
         * the terrain sample, so the effective surface is the module top.
         * Plain cells keep the authored surface.
         */
        private static float EffectiveSurface(
            TerrainPassageStore store,
            float[,] surfaces,
            Vector2Int cell)
        {
            return store.TryGetModule(cell, out TerrainPassageModule module)
                ? module.TopY
                : surfaces[cell.x, cell.y];
        }

        private static TerrainTransitionKind ClassifyStep(
            TerrainPassageStore store,
            float[,] surfaces,
            Vector2Int from,
            Vector2Int to)
        {
            float rise = EffectiveSurface(store, surfaces, to)
                         - EffectiveSurface(store, surfaces, from);
            return TerrainTransitionClassifier.Classify(
                rise,
                Mathf.Max(0f, -rise),
                store.IsStairStep(from, to),
                null,
                "ground");
        }

        [Test]
        public void PlannedFlight_TraversesBothDirections_AsStair()
        {
            // 8x8: x<4 low (0), x>=4 high (1.0) -> two-module stair corridor.
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 2f * Rise : 0f;

            TerrainPassagePlan plan =
                new TerrainPassagePlanner().Plan(surfaces, Config(maxDrop: 1f));
            Assert.Greater(plan.Flights.Count, 0,
                "fixture must produce at least one stair flight");

            var store = new TerrainPassageStore();
            store.Replace(plan);

            foreach (StairFlight flight in plan.Flights)
            {
                var chain = new List<Vector2Int> { flight.Entrance };
                chain.AddRange(flight.Modules);
                chain.Add(flight.Exit);

                for (int i = 0; i + 1 < chain.Count; i++)
                {
                    Vector2Int a = chain[i];
                    Vector2Int b = chain[i + 1];
                    Assert.IsTrue(store.IsStairStep(a, b),
                        $"stair step {a}->{b} must be registered");
                    Assert.IsTrue(store.IsStairStep(b, a),
                        $"stair step {b}->{a} must be registered (unordered)");

                    float delta = Mathf.Abs(
                        EffectiveSurface(store, surfaces, b)
                        - EffectiveSurface(store, surfaces, a));
                    Assert.LessOrEqual(delta, Rise + 0.001f,
                        $"step {a}->{b} exceeds one module rise");

                    Assert.AreEqual(TerrainTransitionKind.Stair,
                        ClassifyStep(store, surfaces, a, b),
                        $"climb step {a}->{b} rejected");
                    Assert.AreEqual(TerrainTransitionKind.Stair,
                        ClassifyStep(store, surfaces, b, a),
                        $"descend step {b}->{a} rejected");
                }
            }
        }

        [Test]
        public void LedgeWithoutCorridorRoom_NoFlight_TransitionBlocked()
        {
            // 6x6: only two low columns before a 1.0 ledge — a two-module
            // flight needs a third cell for the entrance, so none may appear.
            var surfaces = new float[6, 6];
            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                surfaces[x, y] = x >= 2 ? 2f * Rise : 0f;

            TerrainPassagePlan plan =
                new TerrainPassagePlanner().Plan(surfaces, Config(maxDrop: 1f));
            Assert.AreEqual(0, plan.Flights.Count,
                "ledge without corridor depth must not produce a flight");

            var store = new TerrainPassageStore();
            store.Replace(plan);

            // The raw ledge step must be rejected in both directions.
            var low = new Vector2Int(1, 3);
            var high = new Vector2Int(2, 3);
            Assert.AreEqual(TerrainTransitionKind.Blocked,
                ClassifyStep(store, surfaces, low, high));
            Assert.AreEqual(TerrainTransitionKind.Blocked,
                ClassifyStep(store, surfaces, high, low));
        }

        [Test]
        public void NonMultipleDrop_NoFlight_TransitionBlocked()
        {
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 0.7f : 0f;

            TerrainPassagePlan plan =
                new TerrainPassagePlanner().Plan(surfaces, Config());
            Assert.AreEqual(0, plan.Flights.Count,
                "drop that is not an integer module count must not produce a flight");

            var store = new TerrainPassageStore();
            store.Replace(plan);
            var low = new Vector2Int(3, 4);
            var high = new Vector2Int(4, 4);
            Assert.AreEqual(TerrainTransitionKind.Blocked,
                ClassifyStep(store, surfaces, low, high));
            Assert.AreEqual(TerrainTransitionKind.Blocked,
                ClassifyStep(store, surfaces, high, low));
        }

        [Test]
        public void FlightsOnSharedPlateau_DoNotCorruptEachOthersCells()
        {
            /*
             * 6x10 fixture:
             *   y>=8, x>=2 -> 1.5  (north high wall)
             *   (0,7)      -> 0.5  (single west step)
             *   rest       -> 0    (low plateau)
             *
             * B climbs west:  module (1,7), entrance (2,7).
             * A climbs north: lowCell (2,7) is scanned after B and its
             * three-module corridor ends on (2,7) — exactly B's entrance.
             * If the planner does not reserve entrance cells, A's module
             * is carved on B's entrance at top 1.5, and B's entry step
             * becomes a 1.0 m jump that traversal rejects.
             */
            var surfaces = new float[6, 10];
            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 10; y++)
                surfaces[x, y] = x >= 2 && y >= 8 ? 1.5f : 0f;
            surfaces[0, 7] = Rise;

            TerrainPassagePlan plan = new TerrainPassagePlanner()
                .Plan(surfaces, Config(maxDrop: 1.5f));
            Assert.GreaterOrEqual(plan.Flights.Count, 2,
                "fixture must produce the west step and at least one north flight");

            var store = new TerrainPassageStore();
            store.Replace(plan);

            StairFlight west = plan.Flights.Find(
                f => f.Modules.Length == 1
                     && f.Modules[0] == new Vector2Int(1, 7));
            Assert.IsNotNull(west, "west single-module flight must be planned");

            // Invariant: a flight's entrance and exit stay plain plateau
            // cells — no other flight may carve a module onto them.
            foreach (StairFlight flight in plan.Flights)
            {
                Assert.IsFalse(store.IsStairCell(flight.Entrance),
                    $"entrance {flight.Entrance} of flight to {flight.Exit} "
                    + "was carved by another flight's module");
                Assert.IsFalse(store.IsStairCell(flight.Exit),
                    $"exit {flight.Exit} of flight from {flight.Entrance} "
                    + "was carved by another flight's module");
            }

            // West flight must be traversable in both directions.
            var entry = west.Modules[0];
            Assert.AreEqual(TerrainTransitionKind.Stair,
                ClassifyStep(store, surfaces, west.Entrance, entry),
                "west flight entry step must be traversable");
            Assert.AreEqual(TerrainTransitionKind.Stair,
                ClassifyStep(store, surfaces, entry, west.Entrance),
                "west flight exit-to-entrance step must be traversable");
        }

        [Test]
        public void TerracedLedges_LegitFlights_NotOverRejected()
        {
            /*
             * 10x6 terraces: x<2 -> 2.0, x<6 -> 1.0, x>=6 -> 0.
             * The mid band is deep enough for a legitimate west-climbing
             * flight (2 modules + entrance at mid level), and the low
             * plateau hosts a second flight up to mid. Both must be
             * planned and stay traversable — the reservation fix must not
             * reject flights that merely share a plateau.
             */
            var surfaces = new float[10, 6];
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 6; y++)
                surfaces[x, y] = x < 2 ? 2f : x < 6 ? 1f : 0f;

            TerrainPassagePlan plan = new TerrainPassagePlanner()
                .Plan(surfaces, Config(maxDrop: 1.5f));

            int westFlights = plan.Flights.FindAll(
                f => f.Exit.x == 1).Count;
            int eastFlights = plan.Flights.FindAll(
                f => f.Exit.x == 5).Count;
            Assert.Greater(westFlights, 0, "mid->high flights expected");
            Assert.Greater(eastFlights, 0, "low->mid flights expected");

            var store = new TerrainPassageStore();
            store.Replace(plan);

            var moduleCells = new HashSet<Vector2Int>();
            foreach (StairFlight flight in plan.Flights)
            {
                foreach (Vector2Int cell in flight.Modules)
                    Assert.IsTrue(moduleCells.Add(cell),
                        $"module cell {cell} claimed by two flights");

                Assert.IsFalse(moduleCells.Contains(flight.Entrance),
                    $"entrance {flight.Entrance} overlaps another module");

                var chain = new List<Vector2Int> { flight.Entrance };
                chain.AddRange(flight.Modules);
                chain.Add(flight.Exit);
                for (int i = 0; i + 1 < chain.Count; i++)
                {
                    Assert.AreEqual(TerrainTransitionKind.Stair,
                        ClassifyStep(store, surfaces, chain[i], chain[i + 1]),
                        $"flight {flight.Entrance}->{flight.Exit} blocked at "
                        + $"{chain[i]}->{chain[i + 1]}");
                    Assert.AreEqual(TerrainTransitionKind.Stair,
                        ClassifyStep(store, surfaces, chain[i + 1], chain[i]),
                        $"flight {flight.Entrance}->{flight.Exit} blocked at "
                        + $"{chain[i + 1]}->{chain[i]}");
                }
            }
        }

        [Test]
        public void SameLevelWalk_NotStair_StillAllowed()
        {
            var surfaces = new float[6, 6];
            var store = new TerrainPassageStore();
            store.Replace(new TerrainPassagePlan());

            var a = new Vector2Int(2, 2);
            var b = new Vector2Int(3, 2);
            Assert.AreEqual(TerrainTransitionKind.DirectWalk,
                ClassifyStep(store, surfaces, a, b));
            Assert.AreEqual(TerrainTransitionKind.DirectWalk,
                ClassifyStep(store, surfaces, b, a));
        }
    }
}
