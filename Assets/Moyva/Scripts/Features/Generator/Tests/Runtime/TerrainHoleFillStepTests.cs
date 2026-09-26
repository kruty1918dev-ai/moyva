using System.Collections.Generic;
using System.Linq;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Jsonization;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Focused contract for <see cref="TerrainHoleFillStep"/>: small interior
    /// mask holes that sit on already-elevated relief are filled, while
    /// border-connected water, large inland bodies, lowland ponds and real
    /// terrain depressions are preserved.
    /// </summary>
    public sealed class TerrainHoleFillStepTests
    {
        private static TerrainHoleFillStep Step() => new TerrainHoleFillStep
        {
            MaxComponentCells = 32,
            MinRingMedianMeters = 1.5f,
            MaxReliefDropMeters = 1.0f,
        };

        private static GeneratorMaskContext Context(float[,] field, int w, int h)
            => new GeneratorMaskContext(1, new Vector2Int(w, h), null, field);

        private static bool[,] Filled(int w, int h)
        {
            var mask = new bool[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                mask[x, y] = true;
            return mask;
        }

        private static void Clear(bool[,] mask, params (int x, int y)[] cells)
        {
            foreach (var (x, y) in cells)
                mask[x, y] = false;
        }

        private static float[,] UniformField(int w, int h, float meters)
        {
            var field = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                field[x, y] = meters;
            return field;
        }

        private static int CountTrue(bool[,] mask)
        {
            int n = 0;
            foreach (bool b in mask)
                if (b) n++;
            return n;
        }

        [Test]
        public void SmallInteriorHole_OnHighland_IsFilled()
        {
            int w = 12, h = 12;
            var mask = Filled(w, h);
            Clear(mask, (5, 5), (6, 5), (5, 6), (6, 6));
            var field = UniformField(w, h, 3f);

            var result = Step().TransformMask(mask, Context(field, w, h));

            Assert.AreEqual(w * h, CountTrue(result));
        }

        [Test]
        public void BorderTouchingHole_IsPreserved()
        {
            int w = 12, h = 12;
            var mask = Filled(w, h);
            Clear(mask, (0, 5), (1, 5), (2, 5));
            var field = UniformField(w, h, 3f);

            var result = Step().TransformMask(mask, Context(field, w, h));

            Assert.IsFalse(result[1, 5]);
        }

        [Test]
        public void LargeInteriorHole_IsPreserved()
        {
            int w = 16, h = 16;
            var mask = Filled(w, h);
            // 8x8 interior lake = 64 cells > cap.
            for (int x = 4; x < 12; x++)
            for (int y = 4; y < 12; y++)
                mask[x, y] = false;
            var field = UniformField(w, h, 3f);

            var result = Step().TransformMask(mask, Context(field, w, h));

            Assert.IsFalse(result[8, 8]);
        }

        [Test]
        public void LowlandHole_IsPreserved()
        {
            int w = 12, h = 12;
            var mask = Filled(w, h);
            Clear(mask, (5, 5), (6, 5));
            var field = UniformField(w, h, 0.5f);

            var result = Step().TransformMask(mask, Context(field, w, h));

            Assert.IsFalse(result[5, 5]);
            Assert.IsFalse(result[6, 5]);
        }

        [Test]
        public void DeepBasinHole_IsPreserved()
        {
            int w = 12, h = 12;
            var mask = Filled(w, h);
            Clear(mask, (5, 5), (6, 5));
            var field = UniformField(w, h, 3f);
            // The hole's own relief is a real 2m-deep basin.
            field[5, 5] = 1f;
            field[6, 5] = 1f;

            var result = Step().TransformMask(mask, Context(field, w, h));

            Assert.IsFalse(result[5, 5]);
            Assert.IsFalse(result[6, 5]);
        }

        [Test]
        public void ControlCase_OnlyTheUnwantedPitIsFilled()
        {
            // Synthetic world: plateau at 3m with a small noise hole (pit),
            // a large interior lake, a lowland pond and a sea outlet to border.
            int w = 24, h = 24;
            var mask = Filled(w, h);
            var field = UniformField(w, h, 3f);

            // (a) unwanted pit: 3x3 hole on plateau.
            for (int x = 10; x < 13; x++)
            for (int y = 10; y < 13; y++)
                mask[x, y] = false;

            // (b) intentional lake: 6x6 interior hole, also on plateau (big).
            for (int x = 3; x < 9; x++)
            for (int y = 3; y < 9; y++)
                mask[x, y] = false;

            // (c) lowland pond: 2x2 hole where terrain is low.
            for (int x = 15; x < 17; x++)
            for (int y = 15; y < 17; y++)
            {
                mask[x, y] = false;
                field[x, y] = 0.5f;
            }
            for (int x = 13; x < 19; x++)
            for (int y = 13; y < 19; y++)
                field[x, y] = 0.5f;

            // (d) sea outlet: border-connected channel of holes.
            for (int y = 20; y < 24; y++)
                mask[20, y] = false;

            var result = Step().TransformMask(mask, Context(field, w, h));

            // pit filled; lake, pond and sea outlet preserved.
            Assert.IsTrue(result[11, 11], "pit should be filled");
            Assert.IsFalse(result[5, 5], "interior lake should remain");
            Assert.IsFalse(result[16, 16], "lowland pond should remain");
            Assert.IsFalse(result[20, 22], "sea outlet should remain");
        }

        [Test]
        public void NullTerrainField_ReturnsMaskUnchanged()
        {
            int w = 8, h = 8;
            var mask = Filled(w, h);
            Clear(mask, (3, 3));

            var result = Step().TransformMask(mask, Context(null, w, h));

            Assert.AreSame(mask, result);
            Assert.IsFalse(result[3, 3]);
        }

        [Test]
        public void Deterministic_SameInputSameOutput()
        {
            int w = 12, h = 12;
            var field = UniformField(w, h, 3f);
            var a = Filled(w, h);
            var b = Filled(w, h);
            Clear(a, (5, 5), (6, 5));
            Clear(b, (5, 5), (6, 5));

            var ra = Step().TransformMask(a, Context(field, w, h));
            var rb = Step().TransformMask(b, Context(field, w, h));

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                Assert.AreEqual(ra[x, y], rb[x, y]);
        }

        [Test]
        public void InputMask_IsNotMutated()
        {
            int w = 8, h = 8;
            var mask = Filled(w, h);
            Clear(mask, (3, 3));
            var field = UniformField(w, h, 3f);

            Step().TransformMask(mask, Context(field, w, h));

            Assert.IsFalse(mask[3, 3], "input mask must stay untouched");
        }

        // ---- real-recipe regression scans ----------------------------------

        private static readonly int[] Dx8 = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] Dy8 = { -1, -1, -1, 0, 0, 1, 1, 1 };

        private static GeneratorMapRecipe LoadProductionRecipe()
        {
            if (!JsonConfigRuntime.IsLoaded)
            {
                JsonConfigRuntime.Configure(MoyvaJsonRuntimeSettings.Create());
                JsonConfigRuntime.EnsureLoaded();
            }
            return JsonConfigRuntime.GetAll<GeneratorMapRecipe>()
                .FirstOrDefault(r => r != null && r.Layers != null && r.Layers.Count > 0);
        }

        /// <summary>
        /// Counts interior mask holes (empty components that never touch the
        /// border) whose surrounding ring and own relief are already elevated —
        /// exactly the unwanted pit class the step removes.
        /// </summary>
        private static int CountClassifiedPits(bool[,] land, float[,] field, TerrainHoleFillStep step)
        {
            int w = land.GetLength(0), h = land.GetLength(1);
            var visited = new bool[w, h];
            var queue = new Queue<Vector2Int>();
            var cells = new List<Vector2Int>();
            var ring = new List<float>();
            int pits = 0;
            for (int sy = 0; sy < h; sy++)
            for (int sx = 0; sx < w; sx++)
            {
                if (visited[sx, sy] || land[sx, sy]) continue;
                cells.Clear(); ring.Clear();
                bool border = false;
                visited[sx, sy] = true;
                queue.Enqueue(new Vector2Int(sx, sy));
                while (queue.Count > 0)
                {
                    var c = queue.Dequeue();
                    cells.Add(c);
                    if (c.x == 0 || c.y == 0 || c.x == w - 1 || c.y == h - 1) border = true;
                    for (int i = 0; i < 8; i++)
                    {
                        int nx = c.x + Dx8[i], ny = c.y + Dy8[i];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        if (land[nx, ny]) ring.Add(field[nx, ny]);
                        else if (!visited[nx, ny])
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
                if (border || cells.Count > step.MaxComponentCells || ring.Count == 0) continue;
                ring.Sort();
                float ringMed = ring[ring.Count / 2];
                if (ringMed < step.MinRingMedianMeters) continue;
                var comp = cells.Select(c => field[c.x, c.y]).OrderBy(v => v).ToList();
                if (ringMed - comp[comp.Count / 2] > step.MaxReliefDropMeters) continue;
                pits++;
            }
            return pits;
        }

        /// <summary>
        /// Production-regression: the recipe's _baseLayer must contain no
        /// classified pits on the production relief field across seeds and
        /// profiles. Large lakes, border-connected water and real basins are
        /// counted separately and must still exist (relief not flattened).
        /// </summary>
        [Test]
        public void ProductionMask_NoClassifiedPits_AcrossSeedsAndProfiles()
        {
            var recipe = LoadProductionRecipe();
            Assert.IsNotNull(recipe, "no generator recipe loaded");
            var baseLayer = recipe.Layers.FirstOrDefault(l => l.Name == "_baseLayer");
            Assert.IsNotNull(baseLayer, "_baseLayer missing");
            var fill = baseLayer.Steps.OfType<TerrainHoleFillStep>().FirstOrDefault();
            Assert.IsNotNull(fill, "terrain-hole-fill-step missing from _baseLayer");

            var mountain = new TerrainReliefConfig
            {
                Enabled = true, QuantumMeters = 0.5f, MaxSteps = 14, NoiseScale = 22f,
                Octaves = 5, Persistence = 0.55f, Lacunarity = 2.1f, HeightExponent = 1.4f,
                SmoothingIterations = 2, MinPlateauCells = 3, BumpMinSpacingCells = 8,
                MaxBumpCount = 30, RidgeErosionIterations = 1, SeedSalt = 9137,
            };
            var profiles = new (string name, Vector2Int size, TerrainReliefConfig relief, int seeds)[]
            {
                ("default48", new Vector2Int(48, 48), recipe.TerrainRelief, 12),
                ("wide96", new Vector2Int(96, 96), recipe.TerrainRelief, 6),
                ("mountain48", new Vector2Int(48, 48), mountain, 6),
            };

            var planner = new TerrainReliefPlanner();
            int keptHoles = 0;
            foreach (var (name, size, relief, seeds) in profiles)
            {
                for (int seed = 1; seed <= seeds; seed++)
                {
                    float[,] field = planner.Build(seed, size, relief);
                    var session = new GeneratorMaskSession(recipe);
                    var masks = GeneratorMaskEvaluator.EvaluateMasks(
                        recipe, seed, size, null, field, session);
                    bool[,] land = masks[baseLayer.Id];
                    int pits = CountClassifiedPits(land, field, fill);
                    Assert.AreEqual(0, pits,
                        $"{name} seed={seed}: {pits} classified interior pits remain");
                    keptHoles += InteriorHoleCount(land);
                }
            }
            Assert.Greater(keptHoles, 0,
                "all interior holes were removed; legitimate water bodies must be preserved");
        }

        private static int InteriorHoleCount(bool[,] land)
        {
            int w = land.GetLength(0), h = land.GetLength(1);
            var visited = new bool[w, h];
            var queue = new Queue<Vector2Int>();
            int interior = 0;
            for (int sy = 0; sy < h; sy++)
            for (int sx = 0; sx < w; sx++)
            {
                if (visited[sx, sy] || land[sx, sy]) continue;
                bool border = false;
                visited[sx, sy] = true;
                queue.Enqueue(new Vector2Int(sx, sy));
                while (queue.Count > 0)
                {
                    var c = queue.Dequeue();
                    if (c.x == 0 || c.y == 0 || c.x == w - 1 || c.y == h - 1) border = true;
                    for (int i = 0; i < 8; i++)
                    {
                        int nx = c.x + Dx8[i], ny = c.y + Dy8[i];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h || land[nx, ny] || visited[nx, ny])
                            continue;
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
                if (!border) interior++;
            }
            return interior;
        }
    }
}
