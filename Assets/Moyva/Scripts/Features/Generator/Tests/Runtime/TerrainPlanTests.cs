using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class TerrainPlanTests
    {
        [TestCase(0.3f, false)]
        [TestCase(3f, false)]
        [TestCase(0f, true)]
        public void SameBiome_MergesOnlyAtEqualSurfaceHeight(float drop, bool matches)
        {
            var high = new TileStackCell();
            var low = new TileStackCell();
            var sample = new TileLayerSample("grass", "Grass", null, null,
                "grass", null, LayerKind.BaseTerrain, 0, 0, 0, 3f, 3f, null);
            high.Add(sample);
            low.Add(sample.WithSurfaceHeight(3f - drop));
            var neighborhood = new TileNeighborhood(high, low, low, low, low,
                low, low, low, low);
            var result = new ResolvedTileCompositionResolver().Resolve(Vector2Int.zero, neighborhood);

            Assert.AreEqual(matches, result.NorthMatches);
            Assert.AreEqual(matches, result.EastMatches);
            Assert.AreEqual(matches, result.SouthMatches);
            Assert.AreEqual(matches, result.WestMatches);
            Assert.AreEqual(matches, result.NorthEastMatches);
            Assert.AreEqual(matches, result.SouthEastMatches);
            Assert.AreEqual(matches, result.SouthWestMatches);
            Assert.AreEqual(matches, result.NorthWestMatches);
            Assert.AreEqual(matches ? TileMeshOccludedSides.North : TileMeshOccludedSides.None,
                TwcTileMeshSourceProvider.ResolveDualOccludedSides(true, result.NorthMatches, false, false));
        }

        [Test]
        public void Relief_PeakBias_PreservesRangeAndFavorsLowGround()
        {
            var config = new TerrainReliefConfig
            {
                Enabled = true, QuantumMeters = 0.3f, MaxSteps = 10,
                SmoothingIterations = 0
            };
            var planner = new TerrainReliefPlanner();
            var linear = planner.Build(42, new Vector2Int(32, 32), config);
            config.HeightExponent = 2f;
            var biased = planner.Build(42, new Vector2Int(32, 32), config);
            float maximum = 0f;
            int lowCells = 0;
            for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
            {
                float value = biased[x, y];
                Assert.That(value, Is.InRange(0f, 3f));
                Assert.LessOrEqual(value, linear[x, y]);
                Assert.AreEqual(Mathf.Round(value / 0.3f), value / 0.3f, 0.0001f);
                maximum = Mathf.Max(maximum, value);
                if (value <= 1f) lowCells++;
            }
            Assert.AreEqual(3f, maximum, 0.0001f);
            Assert.Greater(lowCells, 32 * 32 / 2);
        }

        [TestCase(0.3f)]
        [TestCase(3f)]
        public void FlatTerrain_GeneratesSideGeometryDownToSupport(float drop)
        {
            var top = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f),
                    new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f)
                },
                triangles = new[] { 0, 1, 2, 0, 2, 3 },
                uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }
            };
            var material = new Material(Shader.Find("Hidden/InternalErrorShader"));
            Mesh closed = null;
            try
            {
                var source = new TileMeshSource(top, new[] { material },
                    Matrix4x4.Translate(Vector3.up * drop), visibleBottomY: 0f,
                    tileHalfExtent: 0.5f);
                Assert.IsTrue(TileVerticalFillMeshUtility.TryCreate(source, out closed));
                Assert.Greater(closed.triangles.Length, top.triangles.Length);
                Assert.AreEqual(-drop, closed.bounds.min.y, 0.0001f);
                Assert.AreEqual(0f, closed.bounds.max.y, 0.0001f);
            }
            finally
            {
                if (closed != null) Object.DestroyImmediate(closed);
                Object.DestroyImmediate(top);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void WaterPreset_UsesStylizedWater3Material()
        {
            var preset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Moyva/TileWater.asset");
            Assert.IsNotNull(preset);
            using var serialized = new UnityEditor.SerializedObject(preset);
            var material = serialized.FindProperty("materialOverride").objectReferenceValue as Material;
            Assert.IsNotNull(material);
            Assert.AreEqual("Assets/ThirdParty/Stylized Water 3/Materials/StylizedWater3_Toon.mat",
                UnityEditor.AssetDatabase.GetAssetPath(material));
            Assert.IsNotNull(material.shader);
            Assert.IsFalse(material.shader.name.Contains("InternalErrorShader"));
        }

        [TestCase("PC_Renderer")]
        [TestCase("Mobile_Renderer")]
        public void WorldRenderer_EnablesStylizedWaterFeature(string rendererName)
        {
            var renderer = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                $"Assets/Moyva/Settings/URP/{rendererName}.asset");
            Assert.IsNotNull(renderer);
            using var serialized = new UnityEditor.SerializedObject(renderer);
            var features = serialized.FindProperty("m_RendererFeatures");
            for (int i = 0; i < features.arraySize; i++)
            {
                var feature = features.GetArrayElementAtIndex(i).objectReferenceValue;
                if (feature == null || feature.GetType().FullName != "StylizedWater3.StylizedWaterRenderFeature")
                    continue;
                using var settings = new UnityEditor.SerializedObject(feature);
                Assert.IsTrue(settings.FindProperty("m_Active").boolValue);
                return;
            }
            Assert.Fail("Stylized Water 3 renderer feature is missing.");
        }

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

        [TestCase(0.25f)]
        [TestCase(0.3f)]
        public void PassagePlanner_Ledge_ProducesFlights(float rise)
        {
            // 8x8 map: left half at 0 m, right half at twice the rise -> ledge of two modules.
            var surfaces = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                surfaces[x, y] = x >= 4 ? 2f * rise : 0f;

            var planner = new TerrainPassagePlanner();
            var config = new TerrainPassageConfig
            {
                Enabled = true,
                ModuleRiseMeters = rise,
                MinLedgeDropMeters = 0.5f,
                MaxLedgeDropMeters = 1f,
                MinEntranceSpacingCells = 2,
                MaxFlights = 16,
            };

            TerrainPassagePlan plan = planner.Plan(surfaces, config);
            Assert.Greater(plan.Flights.Count, 0, "Expected stair flights across the ledge.");
            var store = new TerrainPassageStore();
            store.Replace(plan);

            foreach (StairFlight flight in plan.Flights)
            {
                Assert.AreEqual(2, flight.Modules.Length);
                Assert.AreEqual(0f, flight.LowSurfaceY, 0.001f);
                Assert.AreEqual(2f * rise, flight.HighSurfaceY, 0.001f);
                // Modules climb toward the exit: consecutive tops differ by one module rise.
                Assert.AreEqual(rise, flight.ModuleTopY[0], 0.001f);
                Assert.AreEqual(2f * rise, flight.ModuleTopY[1], 0.001f);
                foreach (var cell in flight.Modules)
                {
                    Assert.IsTrue(store.TryGetModule(cell, out var module));
                    Assert.AreEqual(rise, module.RiseMeters, 0.001f);
                }
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
