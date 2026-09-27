using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Produces the macro-scale land/ocean mask for the chosen archetype:
    /// seeded super-ellipse continent centres + domain-warped coastline noise.
    /// Output: landMask[x,y] in [0,1] — &gt;=<see cref="LandThreshold"/> ≈ land.
    /// After the ratio normalization, isolated speck components smaller than the
    /// archetype's minimum island size are sunk back to ocean and the mask is
    /// re-normalized so the land ratio stays on target.
    /// </summary>
    internal sealed class LandmassStage
    {
        /// <summary>Mask value at/above which a cell counts as land (matches TerrainLevelStage).</summary>
        internal const float LandThreshold = 0.5f;

        /// <summary>Macro-land metrics: land ratio, largest component, island area distribution.</summary>
        internal sealed class Stats
        {
            public int LandCells;
            public float LandFraction;
            public int ComponentCount;
            public int LargestComponentCells;
            /// <summary>Component areas in cells, sorted descending.</summary>
            public int[] ComponentSizes = System.Array.Empty<int>();
        }

        internal float[,] Generate(WorldGenerationRequest request)
        {
            int w = request.Width;
            int h = request.Height;
            var p = request.Config.FindArchetype(request.ArchetypeId());

            var mask = BuildRawMask(request);
            PruneInsignificantComponents(mask, w, h, p);
            // Restoring the target ratio only grows surviving coastlines —
            // sunk specks sit at 0 and cannot re-emerge. A second prune pass
            // removes anything the re-normalization pushed over the threshold.
            NormalizeToTarget(mask, w, h, p.LandRatio);
            PruneInsignificantComponents(mask, w, h, p);
            return mask;
        }

        /// <summary>
        /// Centres + coastline noise normalized to the archetype land ratio,
        /// before speck-component pruning. Exposed for stage-level audits/tests.
        /// </summary>
        internal float[,] BuildRawMask(WorldGenerationRequest request)
        {
            int w = request.Width;
            int h = request.Height;
            var p = request.Config.FindArchetype(request.ArchetypeId());
            int seed = request.Seed;

            var centres = BuildCentres(w, h, seed, p);
            float halfExtent = Mathf.Min(w, h) * 0.5f;
            float baseRadius = halfExtent * p.CentreRadius;
            float warp = p.CoastWarp;
            float noiseScale = Mathf.Max(6f, halfExtent * 0.35f);

            var mask = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float best = 0f;
                for (int c = 0; c < centres.Length; c++)
                {
                    ref readonly var centre = ref centres[c];
                    float dx = (x - centre.x) / (baseRadius * centre.rx);
                    float dy = (y - centre.y) / (baseRadius * centre.ry);
                    // super-ellipse-ish falloff (exponent ~2.7 → rounded-square landmasses)
                    float d = Mathf.Pow(Mathf.Abs(dx), 2.7f) + Mathf.Pow(Mathf.Abs(dy), 2.7f);
                    d = Mathf.Pow(d, 1f / 2.7f);
                    float local = Mathf.Clamp01(1f - d);
                    local = Smooth(local);
                    if (local > best)
                        best = local;
                }

                float coast = DeterministicNoise.WarpedFbm(
                    seed + 31, x / noiseScale, y / noiseScale, warp / noiseScale, 4);
                mask[x, y] = Mathf.Clamp01(best * 0.75f + (coast - 0.5f) * 0.55f + best * coast * 0.35f);
            }

            NormalizeToTarget(mask, w, h, p.LandRatio);
            return mask;
        }

        /// <summary>Measures land fraction and 8-connected component areas of the mask.</summary>
        internal static Stats Measure(float[,] mask)
        {
            int w = mask.GetLength(0);
            int h = mask.GetLength(1);
            LabelComponents(mask, w, h, out int[] sizes);
            var stats = new Stats { ComponentCount = sizes.Length - 1 };
            for (int i = 1; i < sizes.Length; i++)
            {
                stats.LandCells += sizes[i];
                if (sizes[i] > stats.LargestComponentCells)
                    stats.LargestComponentCells = sizes[i];
            }
            stats.LandFraction = w * h > 0 ? (float)stats.LandCells / (w * h) : 0f;
            var sorted = new int[stats.ComponentCount];
            System.Array.Copy(sizes, 1, sorted, 0, sorted.Length);
            System.Array.Sort(sorted);
            System.Array.Reverse(sorted);
            stats.ComponentSizes = sorted;
            return stats;
        }

        /// <summary>
        /// Sinks land components too small to be intentional for this archetype.
        /// Single-continent configs (pangaea/desert) get a large floor, archipelago
        /// configs (islands) a small one — the threshold follows CentreCount so the
        /// same specks that are noise on a supercontinent stay legal on an
        /// archipelago map. Cells are zeroed so re-normalization cannot revive them.
        /// </summary>
        private static void PruneInsignificantComponents(
            float[,] mask, int w, int h, API.WorldGenerationConfig.ArchetypeParameters p)
        {
            int minCells = MinComponentCells(w, h, p);
            var labels = LabelComponents(mask, w, h, out int[] sizes);
            if (sizes.Length <= 2)
                return; // zero or one landmass — pruning it would hollow out the map

            var pruned = new bool[sizes.Length];
            bool any = false;
            for (int i = 1; i < sizes.Length; i++)
            {
                if (sizes[i] >= minCells) continue;
                pruned[i] = true;
                any = true;
            }
            if (!any) return;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (pruned[labels[x, y]])
                    mask[x, y] = 0f;
        }

        /// <summary>
        /// Minimum island area in cells: ~2% of the map shared between the seeded
        /// centres. TODO(I01): expose as per-archetype JSON parameter once config
        /// ownership allows it; current derivation is provisional.
        /// </summary>
        private static int MinComponentCells(int w, int h, API.WorldGenerationConfig.ArchetypeParameters p)
        {
            const float baseFraction = 0.02f;
            const float minFraction = 0.0005f;
            float fraction = Mathf.Clamp(
                baseFraction / Mathf.Max(1, p.CentreCount), minFraction, baseFraction);
            return Mathf.Max(2, Mathf.RoundToInt(w * h * fraction));
        }

        /// <summary>8-connected labelling of land cells; sizes[0] stays 0 for non-land.</summary>
        private static int[,] LabelComponents(float[,] mask, int w, int h, out int[] sizes)
        {
            var labels = new int[w, h];
            var sizeList = new List<int> { 0 };
            var queue = new int[w * h];
            for (int sx = 0; sx < w; sx++)
            for (int sy = 0; sy < h; sy++)
            {
                if (mask[sx, sy] < LandThreshold || labels[sx, sy] != 0)
                    continue;

                int id = sizeList.Count;
                sizeList.Add(1);
                labels[sx, sy] = id;
                int head = 0, tail = 0;
                queue[tail++] = sx * h + sy;
                while (head < tail)
                {
                    int cell = queue[head++];
                    int cx = cell / h, cy = cell % h;
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = cx + NeighbourDx[d];
                        int ny = cy + NeighbourDy[d];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        if (labels[nx, ny] != 0 || mask[nx, ny] < LandThreshold) continue;
                        labels[nx, ny] = id;
                        sizeList[id]++;
                        queue[tail++] = nx * h + ny;
                    }
                }
            }
            sizes = sizeList.ToArray();
            return labels;
        }

        /// <summary>Rescale the mask so ~landRatio of cells exceed 0.5.</summary>
        private static void NormalizeToTarget(float[,] mask, int w, int h, float targetLand)
        {
            // Flatten + sort to find the threshold giving the target land ratio.
            int n = w * h;
            var values = new float[n];
            int i = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                values[i++] = mask[x, y];
            System.Array.Sort(values);
            int landIndex = Mathf.Clamp(n - Mathf.RoundToInt(n * targetLand), 0, n - 1);
            float threshold = Mathf.Max(0.02f, values[landIndex]);
            float inv = 1f / Mathf.Max(0.001f, 1f - threshold);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float v = mask[x, y];
                mask[x, y] = v >= threshold
                    ? 0.5f + (v - threshold) * inv * 0.5f
                    : v / threshold * 0.5f;
            }
        }

        private struct Centre { public float x, y, rx, ry; }

        private static Centre[] BuildCentres(int w, int h, int seed, API.WorldGenerationConfig.ArchetypeParameters p)
        {
            int count = Mathf.Max(1, p.CentreCount);
            var centres = new Centre[count];
            float cx = w * 0.5f;
            float cy = h * 0.5f;

            if (count == 1)
            {
                centres[0] = JitteredCentre(cx, cy, 1.15f, seed, 0);
                return centres;
            }

            // Jittered grid with shuffled cell assignment: pitch ~sqrt(area/count)
            // keeps neighbouring centres far enough apart to form distinct
            // continents/islands instead of collapsing into one merged blob.
            int cols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count * (float)w / h)));
            int rows = Mathf.Max(1, Mathf.CeilToInt((float)count / cols));
            float cellW = w / (float)cols;
            float cellH = h / (float)rows;

            var order = new int[cols * rows];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            DeterministicNoise.Shuffle(seed, order);

            for (int i = 0; i < count; i++)
            {
                int cell = order[i % order.Length];
                int gx = cell % cols;
                int gy = cell / cols;
                float x = (gx + 0.5f) * cellW + DeterministicNoise.Jitter(seed, i, 11, 3) * cellW * 0.3f;
                float y = (gy + 0.5f) * cellH + DeterministicNoise.Jitter(seed, i, 23, 3) * cellH * 0.3f;
                centres[i] = JitteredCentre(x, y, 0.85f + DeterministicNoise.Hash01(seed, i, 37) * 0.55f, seed, i + 1);
            }
            return centres;
        }

        private static Centre JitteredCentre(float x, float y, float scale, int seed, int salt)
        {
            return new Centre
            {
                x = x + DeterministicNoise.Jitter(seed, salt, 3, 5) * 4f,
                y = y + DeterministicNoise.Jitter(seed, salt, 5, 7) * 4f,
                rx = scale * (0.85f + DeterministicNoise.Hash01(seed, salt, 51) * 0.5f),
                ry = scale * (0.85f + DeterministicNoise.Hash01(seed, salt, 67) * 0.5f),
            };
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);

        private static readonly int[] NeighbourDx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] NeighbourDy = { -1, -1, -1, 0, 0, 1, 1, 1 };
    }

    internal static class WorldGenerationRequestArchetypeExt
    {
        internal static string ArchetypeId(this WorldGenerationRequest request)
            => WorldArchetypeResolver.IdOf(request.Archetype);
    }
}
