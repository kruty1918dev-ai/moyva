using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Water Normalizer", "Terrain",
        "Normalizes noisy water height ranges into natural water and shore levels for height-to-tile conversion.")]
    public sealed class WaterNormalizerNode : NodeBase, IPreviewableNode, ICustomEditorNode
    {
        private const float MinGap = 0.000001f;

        [Header("Water Range")]
        [SerializeField, Range(0f, 1f)] private float _waterMinHeight = 0f;
        [SerializeField, Range(0f, 1f)] private float _waterMaxHeight = 0.3f;

        [Header("Water Levels")]
        [SerializeField, Range(2, 8)] private int _waterLevels = 4;
        [SerializeField, Range(1, 16)] private int _deepDistance = 5;
        [SerializeField, Range(0f, 1f)] private float _levelBlend = 0.35f;

        [Header("Mask Cleanup")]
        [SerializeField, Range(0, 6)] private int _cleanupIterations = 2;
        [SerializeField, Range(0, 8)] private int _fillNeighborThreshold = 5;
        [SerializeField, Range(0, 8)] private int _removeNeighborThreshold = 1;
        [SerializeField, Range(0, 2048)] private int _minWaterBodySize = 32;

        [Header("Shore")]
        [SerializeField, Range(0, 8)] private int _shoreWidth = 2;
        [SerializeField, Range(0f, 1f)] private float _shoreTargetHeight = 0.36f;
        [SerializeField, Range(0f, 1f)] private float _shoreBlend = 0.65f;
        [SerializeField, Range(0f, 1f)] private float _shoreMaxSourceHeight = 0.45f;

        [Header("Smoothing")]
        [SerializeField, Range(0, 3)] private int _smoothRadius = 1;
        [SerializeField, Range(0, 4)] private int _smoothIterations = 1;

        [NonSerialized] private float[,] _lastOutput;
        [NonSerialized] private bool[,] _lastCandidateMask;
        [NonSerialized] private bool[,] _lastWaterMask;
        [NonSerialized] private bool[,] _lastShoreMask;
        [NonSerialized] private int[,] _lastLevelMap;
        [NonSerialized] private int _lastCandidateCells;
        [NonSerialized] private int _lastWaterCells;
        [NonSerialized] private int _lastRemovedCandidateCells;
        [NonSerialized] private int _lastShoreCells;

        public int LastCandidateCells => _lastCandidateCells;
        public int LastWaterCells => _lastWaterCells;
        public int LastRemovedCandidateCells => _lastRemovedCandidateCells;
        public int LastShoreCells => _lastShoreCells;

        public override string Title => "Water Normalizer";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<bool[,]>("Water Mask"),
            PortDefinition.Output<int[,]>("Water Level Map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            SanitizeSettings();

            var heightMap = inputs.Length > 0 ? inputs[0] as float[,] : null;
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var result = (float[,])heightMap.Clone();
            var waterMask = BuildInitialWaterMask(heightMap, width, height);
            var candidateMask = (bool[,])waterMask.Clone();
            CleanupWaterMask(waterMask, width, height);
            RemoveSmallWaterBodies(waterMask, width, height);

            int[,] distanceToLand = ComputeDistanceToOpposite(waterMask, true, width, height);
            int[,] distanceToWater = ComputeDistanceToOpposite(waterMask, false, width, height);
            var levelMap = new int[width, height];
            var shoreMask = new bool[width, height];
            FillLevelMap(levelMap, -1);

            NormalizeWater(result, waterMask, distanceToLand, levelMap, width, height);
            ApplyShoreBand(result, heightMap, waterMask, distanceToWater, shoreMask, width, height);

            if (_smoothIterations > 0 && _smoothRadius > 0)
                BlendSmoothedWithinInfluence(result, waterMask, shoreMask, width, height);

            _lastOutput = result;
            _lastCandidateMask = candidateMask;
            _lastWaterMask = waterMask;
            _lastShoreMask = shoreMask;
            _lastLevelMap = levelMap;
            CacheStats(candidateMask, waterMask, shoreMask, width, height);

            return NodeOutput.Success(result, waterMask, levelMap);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastOutput == null)
                return null;

            int sourceWidth = _lastOutput.GetLength(0);
            int sourceHeight = _lastOutput.GetLength(1);
            int textureWidth = Mathf.Clamp(width, 32, 256);
            int textureHeight = Mathf.Clamp(height, 32, 256);
            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < textureWidth; x++)
            for (int y = 0; y < textureHeight; y++)
            {
                int sx = Mathf.Clamp(x * sourceWidth / textureWidth, 0, sourceWidth - 1);
                int sy = Mathf.Clamp(y * sourceHeight / textureHeight, 0, sourceHeight - 1);
                texture.SetPixel(x, y, PreviewColor(sx, sy));
            }

            texture.Apply();
            return texture;
        }

        public void SanitizeSettings()
        {
            _waterMinHeight = Mathf.Clamp01(_waterMinHeight);
            _waterMaxHeight = Mathf.Clamp01(_waterMaxHeight);
            if (_waterMaxHeight <= _waterMinHeight + MinGap)
                _waterMaxHeight = Mathf.Min(1f, _waterMinHeight + MinGap);
            if (_waterMaxHeight <= _waterMinHeight)
                _waterMinHeight = Mathf.Max(0f, _waterMaxHeight - MinGap);

            _waterLevels = Mathf.Clamp(_waterLevels, 2, 8);
            _deepDistance = Mathf.Max(1, _deepDistance);
            _cleanupIterations = Mathf.Max(0, _cleanupIterations);
            _minWaterBodySize = Mathf.Max(0, _minWaterBodySize);
            _shoreWidth = Mathf.Max(0, _shoreWidth);
            _shoreTargetHeight = Mathf.Clamp01(_shoreTargetHeight);
            if (_shoreTargetHeight <= _waterMaxHeight + MinGap)
                _shoreTargetHeight = Mathf.Min(1f, _waterMaxHeight + MinGap);
            _shoreMaxSourceHeight = Mathf.Clamp01(_shoreMaxSourceHeight);
        }

        private bool[,] BuildInitialWaterMask(float[,] heightMap, int width, int height)
        {
            var mask = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mask[x, y] = heightMap[x, y] >= _waterMinHeight && heightMap[x, y] <= _waterMaxHeight;
            return mask;
        }

        private void CleanupWaterMask(bool[,] mask, int width, int height)
        {
            if (_cleanupIterations <= 0)
                return;

            var next = new bool[width, height];
            for (int iteration = 0; iteration < _cleanupIterations; iteration++)
            {
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int neighbors = CountWaterNeighbors(mask, x, y, width, height);
                    bool current = mask[x, y];
                    if (!current && neighbors >= _fillNeighborThreshold)
                        next[x, y] = true;
                    else if (current && neighbors <= _removeNeighborThreshold)
                        next[x, y] = false;
                    else
                        next[x, y] = current;
                }

                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    mask[x, y] = next[x, y];
            }
        }

        private int CountWaterNeighbors(bool[,] mask, int centerX, int centerY, int width, int height)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int x = centerX + dx;
                int y = centerY + dy;
                if (x < 0 || x >= width || y < 0 || y >= height)
                    continue;
                if (mask[x, y])
                    count++;
            }

            return count;
        }

        private void RemoveSmallWaterBodies(bool[,] mask, int width, int height)
        {
            if (_minWaterBodySize <= 1)
                return;

            var visited = new bool[width, height];
            var component = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            for (int startX = 0; startX < width; startX++)
            for (int startY = 0; startY < height; startY++)
            {
                if (visited[startX, startY] || !mask[startX, startY])
                    continue;

                component.Clear();
                queue.Enqueue(new Vector2Int(startX, startY));
                visited[startX, startY] = true;

                while (queue.Count > 0)
                {
                    Vector2Int cell = queue.Dequeue();
                    component.Add(cell);

                    TryCollectWater(mask, visited, queue, cell.x + 1, cell.y, width, height);
                    TryCollectWater(mask, visited, queue, cell.x - 1, cell.y, width, height);
                    TryCollectWater(mask, visited, queue, cell.x, cell.y + 1, width, height);
                    TryCollectWater(mask, visited, queue, cell.x, cell.y - 1, width, height);
                }

                if (component.Count >= _minWaterBodySize)
                    continue;

                for (int i = 0; i < component.Count; i++)
                {
                    Vector2Int cell = component[i];
                    mask[cell.x, cell.y] = false;
                }
            }
        }

        private static void TryCollectWater(bool[,] mask, bool[,] visited, Queue<Vector2Int> queue, int x, int y, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            if (visited[x, y] || !mask[x, y])
                return;

            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }

        private static int[,] ComputeDistanceToOpposite(bool[,] waterMask, bool fromWaterToLand, int width, int height)
        {
            var distances = new int[width, height];
            var queue = new Queue<Vector2Int>();

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                distances[x, y] = int.MaxValue;
                bool isSource = fromWaterToLand ? !waterMask[x, y] : waterMask[x, y];
                if (!isSource)
                    continue;

                distances[x, y] = 0;
                queue.Enqueue(new Vector2Int(x, y));
            }

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                int nextDistance = distances[cell.x, cell.y] + 1;

                TryVisit(cell.x + 1, cell.y, nextDistance, distances, queue, width, height);
                TryVisit(cell.x - 1, cell.y, nextDistance, distances, queue, width, height);
                TryVisit(cell.x, cell.y + 1, nextDistance, distances, queue, width, height);
                TryVisit(cell.x, cell.y - 1, nextDistance, distances, queue, width, height);
            }

            return distances;
        }

        private static void TryVisit(int x, int y, int distance, int[,] distances, Queue<Vector2Int> queue, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            if (distances[x, y] <= distance)
                return;

            distances[x, y] = distance;
            queue.Enqueue(new Vector2Int(x, y));
        }

        private void NormalizeWater(float[,] result, bool[,] waterMask, int[,] distanceToLand, int[,] levelMap, int width, int height)
        {
            float maxLevelIndex = Mathf.Max(1, _waterLevels - 1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!waterMask[x, y])
                    continue;

                float depth01 = Mathf.Clamp01(distanceToLand[x, y] / Mathf.Max(1f, _deepDistance));
                depth01 = Mathf.SmoothStep(0f, 1f, depth01);
                float deepToShallow = 1f - depth01;
                int levelIndex = Mathf.Clamp(Mathf.RoundToInt(deepToShallow * maxLevelIndex), 0, _waterLevels - 1);
                float stepped = levelIndex / maxLevelIndex;
                float blended = Mathf.Lerp(deepToShallow, stepped, _levelBlend);

                result[x, y] = Mathf.Lerp(_waterMinHeight, _waterMaxHeight, blended);
                levelMap[x, y] = levelIndex;
            }
        }

        private void ApplyShoreBand(float[,] result, float[,] sourceHeight, bool[,] waterMask, int[,] distanceToWater, bool[,] shoreMask, int width, int height)
        {
            if (_shoreWidth <= 0)
                return;

            float shoreTarget = Mathf.Clamp01(_shoreTargetHeight);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (waterMask[x, y])
                    continue;

                if (sourceHeight[x, y] > _shoreMaxSourceHeight)
                    continue;

                int distance = distanceToWater[x, y];
                if (distance <= 0 || distance > _shoreWidth)
                    continue;

                float edge = 1f - Mathf.Clamp01((distance - 1f) / Mathf.Max(1f, _shoreWidth));
                float blend = Mathf.SmoothStep(0f, 1f, edge) * _shoreBlend;
                result[x, y] = Mathf.Lerp(result[x, y], shoreTarget, blend);
                result[x, y] = Mathf.Clamp(result[x, y], _waterMaxHeight + MinGap, shoreTarget);
                shoreMask[x, y] = true;
            }
        }

        private void BlendSmoothedWithinInfluence(float[,] result, bool[,] waterMask, bool[,] shoreMask, int width, int height)
        {
            var smoothed = HeightMapMathUtility.BoxBlur(result, _smoothRadius, _smoothIterations);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool isInfluencedShore = !waterMask[x, y] && shoreMask[x, y];
                if (!waterMask[x, y] && !isInfluencedShore)
                    continue;

                float blend = waterMask[x, y] ? 0.35f : 0.25f * _shoreBlend;
                float value = Mathf.Lerp(result[x, y], smoothed[x, y], blend);
                result[x, y] = waterMask[x, y]
                    ? Mathf.Clamp(value, _waterMinHeight, _waterMaxHeight)
                    : Mathf.Clamp(value, _waterMaxHeight + MinGap, _shoreTargetHeight);
            }
        }

        private void CacheStats(bool[,] candidateMask, bool[,] waterMask, bool[,] shoreMask, int width, int height)
        {
            int candidate = 0;
            int water = 0;
            int shore = 0;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (candidateMask[x, y]) candidate++;
                if (waterMask[x, y]) water++;
                if (shoreMask[x, y]) shore++;
            }

            _lastCandidateCells = candidate;
            _lastWaterCells = water;
            _lastRemovedCandidateCells = Mathf.Max(0, candidate - water);
            _lastShoreCells = shore;
        }

        private Color PreviewColor(int x, int y)
        {
            if (_lastWaterMask != null && _lastWaterMask[x, y])
            {
                int level = _lastLevelMap != null ? Mathf.Max(0, _lastLevelMap[x, y]) : 0;
                float t = level / Mathf.Max(1f, _waterLevels - 1f);
                return Color.Lerp(new Color(0.02f, 0.08f, 0.25f), new Color(0.15f, 0.55f, 0.75f), t);
            }

            if (_lastShoreMask != null && _lastShoreMask[x, y])
                return new Color(0.92f, 0.78f, 0.38f);

            if (_lastCandidateMask != null && _lastCandidateMask[x, y])
                return new Color(0.85f, 0.18f, 0.75f);

            float value = _lastOutput != null ? Mathf.Clamp01(_lastOutput[x, y]) : 0f;
            return Color.Lerp(new Color(0.06f, 0.06f, 0.06f), TerrainSlopeFilterNode.HeightToColor(value), 0.55f);
        }

        private static void FillLevelMap(int[,] map, int value)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = value;
        }

        private void OnValidate()
        {
            SanitizeSettings();
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            UnityEditor.Selection.activeObject = this;
        }
#endif
    }
}