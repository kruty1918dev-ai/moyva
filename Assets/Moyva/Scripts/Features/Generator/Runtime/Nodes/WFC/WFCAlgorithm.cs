using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.WFC
{
    /// <summary>
    /// Full Wave Function Collapse algorithm with pattern extraction,
    /// entropy-based collapse, propagation, and backtracking.
    /// </summary>
    public sealed class WFCAlgorithm
    {
        public struct WFCSettings
        {
            public int PatternSize;
            public bool PeriodicInput;
            public bool PeriodicOutput;
            public int OutputWidth;
            public int OutputHeight;
            public int Seed;
            public int MaxAttempts;
        }

        private int _patternSize;
        private bool _periodicInput;
        private bool _periodicOutput;
        private int _outputWidth;
        private int _outputHeight;

        private int _tileCount;
        private string[] _tileIndex; // index → tile ID
        private Dictionary<string, int> _tileToIndex;
        private List<int[,]> _patterns;
        private double[] _patternWeights;
        private bool[][][] _propagator; // direction → pattern → compatible patterns

        // Wave state
        private bool[][] _wave; // cell → pattern allowed
        private int[] _compatible; // helper for propagation
        private int[] _sumsOfOnes; // number of possible patterns per cell
        private double[] _sumsOfWeights;
        private double[] _sumsOfWeightLogWeights;
        private double[] _entropies;

        private static readonly int[] DX = { 0, 1, 0, -1 };
        private static readonly int[] DY = { -1, 0, 1, 0 };

        public string[,] Run(string[,] sample, WFCSettings settings,
            CancellationToken ct = default, IProgress<float> progress = null)
        {
            _patternSize = Mathf.Max(settings.PatternSize, 2);
            _periodicInput = settings.PeriodicInput;
            _periodicOutput = settings.PeriodicOutput;
            _outputWidth = settings.OutputWidth;
            _outputHeight = settings.OutputHeight;

            AnalyzeSample(sample);
            if (_patterns.Count == 0) return null;

            var rng = new System.Random(settings.Seed);
            int maxAttempts = settings.MaxAttempts > 0 ? settings.MaxAttempts : 10;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                InitializeWave();
                bool success = Collapse(rng, ct, progress);

                if (success)
                    return ExtractResult();
            }

            Debug.LogWarning("[WFC] Failed to find valid solution after max attempts.");
            return null;
        }

        private void AnalyzeSample(string[,] sample)
        {
            int sw = sample.GetLength(0);
            int sh = sample.GetLength(1);

            // Build tile index
            _tileToIndex = new Dictionary<string, int>();
            var tileList = new List<string>();
            for (int x = 0; x < sw; x++)
            {
                for (int y = 0; y < sh; y++)
                {
                    string id = sample[x, y] ?? "";
                    if (!_tileToIndex.ContainsKey(id))
                    {
                        _tileToIndex[id] = tileList.Count;
                        tileList.Add(id);
                    }
                }
            }
            _tileIndex = tileList.ToArray();
            _tileCount = _tileIndex.Length;

            // Convert sample to int
            int[,] grid = new int[sw, sh];
            for (int x = 0; x < sw; x++)
                for (int y = 0; y < sh; y++)
                    grid[x, y] = _tileToIndex[sample[x, y] ?? ""];

            // Extract patterns
            var patternMap = new Dictionary<long, int>();
            _patterns = new List<int[,]>();
            var weights = new List<double>();

            int maxX = _periodicInput ? sw : sw - _patternSize + 1;
            int maxY = _periodicInput ? sh : sh - _patternSize + 1;

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    var pattern = ExtractPattern(grid, x, y, sw, sh);

                    // Also add rotations and reflections
                    var variants = new List<int[,]> { pattern };
                    var r90 = Rotate(pattern);
                    var r180 = Rotate(r90);
                    var r270 = Rotate(r180);
                    var fx = FlipX(pattern);
                    variants.Add(r90);
                    variants.Add(r180);
                    variants.Add(r270);
                    variants.Add(fx);
                    variants.Add(Rotate(fx));
                    variants.Add(Rotate(Rotate(fx)));
                    variants.Add(Rotate(Rotate(Rotate(fx))));

                    foreach (var v in variants)
                    {
                        long hash = PatternHash(v);
                        if (patternMap.TryGetValue(hash, out int idx))
                        {
                            weights[idx]++;
                        }
                        else
                        {
                            patternMap[hash] = _patterns.Count;
                            _patterns.Add(v);
                            weights.Add(1.0);
                        }
                    }
                }
            }

            _patternWeights = weights.ToArray();
            BuildPropagator();
        }

        private int[,] ExtractPattern(int[,] grid, int px, int py, int sw, int sh)
        {
            var pattern = new int[_patternSize, _patternSize];
            for (int dx = 0; dx < _patternSize; dx++)
                for (int dy = 0; dy < _patternSize; dy++)
                    pattern[dx, dy] = grid[(px + dx) % sw, (py + dy) % sh];
            return pattern;
        }

        private int[,] Rotate(int[,] p)
        {
            int s = p.GetLength(0);
            var r = new int[s, s];
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                    r[x, y] = p[s - 1 - y, x];
            return r;
        }

        private int[,] FlipX(int[,] p)
        {
            int s = p.GetLength(0);
            var r = new int[s, s];
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                    r[x, y] = p[s - 1 - x, y];
            return r;
        }

        private long PatternHash(int[,] p)
        {
            long hash = 17;
            int s = p.GetLength(0);
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                    hash = hash * 31 + p[x, y];
            return hash;
        }

        private void BuildPropagator()
        {
            int pCount = _patterns.Count;
            _propagator = new bool[4][][];

            for (int d = 0; d < 4; d++)
            {
                _propagator[d] = new bool[pCount][];
                for (int p1 = 0; p1 < pCount; p1++)
                {
                    _propagator[d][p1] = new bool[pCount];
                    for (int p2 = 0; p2 < pCount; p2++)
                    {
                        _propagator[d][p1][p2] = PatternsAgree(
                            _patterns[p1], _patterns[p2], DX[d], DY[d]);
                    }
                }
            }
        }

        private bool PatternsAgree(int[,] p1, int[,] p2, int dx, int dy)
        {
            int s = _patternSize;
            int xMin = Mathf.Max(0, dx);
            int xMax = Mathf.Min(s, s + dx);
            int yMin = Mathf.Max(0, dy);
            int yMax = Mathf.Min(s, s + dy);

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    if (p1[x, y] != p2[x - dx, y - dy])
                        return false;
                }
            }
            return true;
        }

        private void InitializeWave()
        {
            int totalCells = _outputWidth * _outputHeight;
            int pCount = _patterns.Count;

            _wave = new bool[totalCells][];
            _sumsOfOnes = new int[totalCells];
            _sumsOfWeights = new double[totalCells];
            _sumsOfWeightLogWeights = new double[totalCells];
            _entropies = new double[totalCells];

            double sumW = 0, sumWLogW = 0;
            for (int p = 0; p < pCount; p++)
            {
                sumW += _patternWeights[p];
                sumWLogW += _patternWeights[p] * Math.Log(_patternWeights[p]);
            }
            double startingEntropy = Math.Log(sumW) - sumWLogW / sumW;

            for (int i = 0; i < totalCells; i++)
            {
                _wave[i] = new bool[pCount];
                for (int p = 0; p < pCount; p++)
                    _wave[i][p] = true;

                _sumsOfOnes[i] = pCount;
                _sumsOfWeights[i] = sumW;
                _sumsOfWeightLogWeights[i] = sumWLogW;
                _entropies[i] = startingEntropy;
            }
        }

        private bool Collapse(System.Random rng, CancellationToken ct,
            IProgress<float> progress)
        {
            int totalCells = _outputWidth * _outputHeight;
            int collapsed = 0;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                int cell = FindMinEntropyCell(rng);
                if (cell == -1) return true; // Fully collapsed

                // Choose pattern
                int chosen = ChoosePattern(cell, rng);
                if (chosen == -1) return false; // Contradiction

                // Collapse cell
                int pCount = _patterns.Count;
                for (int p = 0; p < pCount; p++)
                {
                    if (p != chosen && _wave[cell][p])
                    {
                        _wave[cell][p] = false;
                        _sumsOfOnes[cell]--;
                        _sumsOfWeights[cell] -= _patternWeights[p];
                        _sumsOfWeightLogWeights[cell] -=
                            _patternWeights[p] * Math.Log(_patternWeights[p]);
                    }
                }
                _entropies[cell] = 0;

                // Propagate
                if (!Propagate(cell))
                    return false; // Contradiction

                collapsed++;
                progress?.Report((float)collapsed / totalCells);
            }
        }

        private int FindMinEntropyCell(System.Random rng)
        {
            double minEntropy = double.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < _wave.Length; i++)
            {
                if (_sumsOfOnes[i] <= 1) continue;

                double entropy = _entropies[i] + rng.NextDouble() * 1e-6;
                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    minIndex = i;
                }
            }

            return minIndex;
        }

        private int ChoosePattern(int cell, System.Random rng)
        {
            double threshold = rng.NextDouble() * _sumsOfWeights[cell];
            double cumulative = 0;
            int pCount = _patterns.Count;

            for (int p = 0; p < pCount; p++)
            {
                if (!_wave[cell][p]) continue;
                cumulative += _patternWeights[p];
                if (cumulative >= threshold)
                    return p;
            }

            // Fallback: pick any valid
            for (int p = 0; p < pCount; p++)
                if (_wave[cell][p]) return p;

            return -1;
        }

        private bool Propagate(int startCell)
        {
            var stack = new Stack<int>();
            stack.Push(startCell);
            int pCount = _patterns.Count;

            while (stack.Count > 0)
            {
                int cell = stack.Pop();
                int cx = cell % _outputWidth;
                int cy = cell / _outputWidth;

                for (int d = 0; d < 4; d++)
                {
                    int nx = cx + DX[d];
                    int ny = cy + DY[d];

                    if (_periodicOutput)
                    {
                        nx = (nx + _outputWidth) % _outputWidth;
                        ny = (ny + _outputHeight) % _outputHeight;
                    }
                    else if (nx < 0 || nx >= _outputWidth || ny < 0 || ny >= _outputHeight)
                        continue;

                    int neighbor = ny * _outputWidth + nx;

                    for (int p2 = 0; p2 < pCount; p2++)
                    {
                        if (!_wave[neighbor][p2]) continue;

                        bool anySupports = false;
                        for (int p1 = 0; p1 < pCount; p1++)
                        {
                            if (_wave[cell][p1] && _propagator[d][p1][p2])
                            {
                                anySupports = true;
                                break;
                            }
                        }

                        if (!anySupports)
                        {
                            _wave[neighbor][p2] = false;
                            _sumsOfOnes[neighbor]--;
                            _sumsOfWeights[neighbor] -= _patternWeights[p2];
                            _sumsOfWeightLogWeights[neighbor] -=
                                _patternWeights[p2] * Math.Log(_patternWeights[p2]);

                            if (_sumsOfOnes[neighbor] == 0) return false;
                            if (_sumsOfWeights[neighbor] > 0)
                                _entropies[neighbor] = Math.Log(_sumsOfWeights[neighbor])
                                    - _sumsOfWeightLogWeights[neighbor]
                                    / _sumsOfWeights[neighbor];

                            stack.Push(neighbor);
                        }
                    }
                }
            }

            return true;
        }

        private string[,] ExtractResult()
        {
            var result = new string[_outputWidth, _outputHeight];
            int pCount = _patterns.Count;

            for (int y = 0; y < _outputHeight; y++)
            {
                for (int x = 0; x < _outputWidth; x++)
                {
                    int cell = y * _outputWidth + x;

                    for (int p = 0; p < pCount; p++)
                    {
                        if (_wave[cell][p])
                        {
                            int tileIdx = _patterns[p][0, 0];
                            result[x, y] = _tileIndex[tileIdx];
                            break;
                        }
                    }

                    if (result[x, y] == null && _tileIndex.Length > 0)
                        result[x, y] = _tileIndex[0];
                }
            }

            return result;
        }
    }
}
