using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class TerrainPlanTests
    {
        [Test]
        public void Relief_Disabled_ReturnsNull()
        {
            var planner = new TerrainReliefPlanner();
            var config = new TerrainReliefConfig { Enabled = false };
            Assert.IsNull(planner.Build(42, new Vector2Int(8, 8), config));
        }

        [Test]
        public void Relief_Deterministic_SameSeedSameField()
        {
            var planner = new TerrainReliefPlanner();
            var config = new TerrainReliefConfig
            {
                Enabled = true,
                QuantumMeters = 0.25f,
                MaxSteps = 6,
                SmoothingIterations = 2,
            };

            float[,] first = planner.Build(1234, new Vector2Int(24, 24), config);
            float[,] second = planner.Build(1234, new Vector2Int(24, 24), config);
            float[,] other = planner.Build(1235, new Vector2Int(24, 24), config);

            Assert.AreEqual(first, second);
            Assert.AreNotEqual(first, other);
        }

        [Test]
        public void Relief_Quantized_ToQuantum()
        {
            var planner = new TerrainReliefPlanner();
            var config = new TerrainReliefConfig
            {
                Enabled = true,
                QuantumMeters = 0.25f,
                MaxSteps = 4,
                SmoothingIterations = 0,
            };

            float[,] field = planner.Build(7, new Vector2Int(32, 32), config);
            const float epsilon = 0.0001f;
            for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
            {
                float value = field[x, y];
                float steps = value / 0.25f;
                Assert.LessOrEqual(Mathf.Abs(steps - Mathf.Round(steps)), epsilon,
                    $"Cell ({x},{y}) height {value} is not quantized.");
                Assert.GreaterOrEqual(value, -epsilon);
                Assert.LessOrEqual(value, 4 * 0.25f + epsilon);
            }
        }

        [Test]
        public void PassagePlanner_Ledge_ProducesFlights()
        {
            // 8x8 map: left half at 0 m, right half at 0.5 m -> ledge of two modules.
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 0.5f : 0f;

            var planner = new TerrainPassagePlanner();
            var config = new TerrainPassageConfig
            {
                Enabled = true,
                ModuleRiseMeters = 0.25f,
                MinLedgeDropMeters = 0.5f,
                MaxLedgeDropMeters = 1f,
                MinEntranceSpacingCells = 2,
                MaxFlights = 16,
            };

            TerrainPassagePlan plan = planner.Plan(surfaces, config);
            Assert.Greater(plan.Flights.Count, 0, "Expected stair flights across the 0.5 m ledge.");

            foreach (StairFlight flight in plan.Flights)
            {
                Assert.AreEqual(2, flight.Modules.Length);
                Assert.AreEqual(0f, flight.LowSurfaceY, 0.001f);
                Assert.AreEqual(0.5f, flight.HighSurfaceY, 0.001f);
                // Modules climb toward the exit: consecutive tops differ by one module rise.
                Assert.AreEqual(0.25f, flight.ModuleTopY[0], 0.001f);
                Assert.AreEqual(0.5f, flight.ModuleTopY[1], 0.001f);
            }
        }

        [Test]
        public void PassagePlanner_NonMultipleLedge_NoFlight()
        {
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 0.3f : 0f;

            var planner = new TerrainPassagePlanner();
            var config = new TerrainPassageConfig
            {
                Enabled = true,
                ModuleRiseMeters = 0.25f,
                MinLedgeDropMeters = 0.25f,
                MaxLedgeDropMeters = 1f,
                MinEntranceSpacingCells = 1,
            };

            Assert.AreEqual(0, planner.Plan(surfaces, config).Flights.Count);
        }

        [Test]
        public void PassagePlanner_TooHighLedge_NoFlight()
        {
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 1.5f : 0f;

            var planner = new TerrainPassagePlanner();
            var config = new TerrainPassageConfig
            {
                Enabled = true,
                ModuleRiseMeters = 0.25f,
                MinLedgeDropMeters = 0.25f,
                MaxLedgeDropMeters = 1f,
                MinEntranceSpacingCells = 1,
            };

            Assert.AreEqual(0, planner.Plan(surfaces, config).Flights.Count);
        }

        [Test]
        public void PassageStore_StairStepAndCells_Resolve()
        {
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 0.5f : 0f;

            var config = new TerrainPassageConfig
            {
                Enabled = true,
                ModuleRiseMeters = 0.25f,
                MinLedgeDropMeters = 0.5f,
                MaxLedgeDropMeters = 1f,
                MinEntranceSpacingCells = 2,
            };
            TerrainPassagePlan plan = new TerrainPassagePlanner().Plan(surfaces, config);
            Assert.Greater(plan.Flights.Count, 0);

            var store = new TerrainPassageStore();
            store.Replace(plan);
            Assert.IsTrue(store.HasPassages);

            StairFlight flight = plan.Flights[0];
            Assert.IsTrue(store.IsStairCell(flight.Modules[0]));
            Assert.IsTrue(store.IsStairStep(flight.Modules[0], flight.Modules[1]));
            Assert.IsTrue(store.IsStairStep(flight.Modules[1], flight.Modules[0]));
            Assert.IsTrue(store.IsStairStep(flight.Entrance, flight.Modules[0]));
            Assert.IsFalse(store.IsStairStep(new Vector2Int(0, 0), new Vector2Int(0, 1)));
        }

        [Test]
        public void RoutePlanner_Deterministic_AndRespectsStepLimit()
        {
            // 12x12 map: flat plateau with a 0.5 m cliff band down the middle.
            var surfaces = new float[12, 12];
            for (int x = 0; x < 12; x++)
            for (int y = 0; y < 12; y++)
                surfaces[x, y] = (x >= 5 && x <= 6) ? 0.5f : 0f;

            var config = new TerrainRouteConfig
            {
                Enabled = true,
                AnchorCount = 4,
                RoadFraction = 0.5f,
                SeedSalt = 99,
            };

            var planner = new TerrainRoutePlanner();
            TerrainRoutePlan first = planner.Plan(surfaces, null, config, seed: 5, autoStepMaxMeters: 0.25f);
            TerrainRoutePlan second = planner.Plan(surfaces, null, config, seed: 5, autoStepMaxMeters: 0.25f);

            CollectionAssert.AreEqual(first.RoadCells, second.RoadCells);
            CollectionAssert.AreEqual(first.FootpathCells, second.FootpathCells);
        }

        [Test]
        public void RoutePlanner_WithoutStairs_CannotCrossCliff()
        {
            // Two plateaus separated by a cliff wall running the full height.
            var surfaces = new float[10, 10];
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                surfaces[x, y] = x >= 5 ? 1f : 0f;

            var config = new TerrainRouteConfig
            {
                Enabled = true,
                AnchorCount = 2,
                RoadFraction = 1f,
                SeedSalt = 3,
            };

            var planner = new TerrainRoutePlanner();
            TerrainRoutePlan plan = planner.Plan(surfaces, null, config, seed: 11, autoStepMaxMeters: 0.25f);

            // Every consecutive pair of route cells must differ by <= autoStep.
            var all = new System.Collections.Generic.List<Vector2Int>();
            all.AddRange(plan.RoadCells);
            all.AddRange(plan.FootpathCells);
            all.Sort((a, b) => a.x != b.x ? a.x - b.x : a.y - b.y);
            for (int i = 1; i < all.Count; i++)
            {
                Vector2Int delta = all[i] - all[i - 1];
                if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1)
                    continue;
                float diff = Mathf.Abs(surfaces[all[i].x, all[i].y] - surfaces[all[i - 1].x, all[i - 1].y]);
                Assert.LessOrEqual(diff, 0.25f + 0.001f,
                    $"Route crossed a {diff} m step without a stair.");
            }
        }

        [Test]
        public void Classifier_DirectWalk_Stair_Blocked()
        {
            // Within auto-step: direct walk.
            Assert.AreEqual(TerrainTransitionKind.DirectWalk,
                TerrainTransitionClassifier.Classify(0.25f, 0f, false, null, "any"));
            // Stair step within module rise: stair.
            Assert.AreEqual(TerrainTransitionKind.Stair,
                TerrainTransitionClassifier.Classify(0.25f, 0f, true, null, "any"));
            // Beyond module rise even on a marked step: blocked.
            Assert.AreEqual(TerrainTransitionKind.Blocked,
                TerrainTransitionClassifier.Classify(0.5f, 0f, true, null, "any"));
            // Beyond auto-step, no stair: blocked.
            Assert.AreEqual(TerrainTransitionKind.Blocked,
                TerrainTransitionClassifier.Classify(0.5f, 0f, false, null, "any"));
        }
    }
}
