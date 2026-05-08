using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Island Noise", "Noise", "Generates an island-shaped height noise map with varied island sizes and irregular coastlines.")]
    public sealed class IslandNoiseNode : NodeBase, IPreviewableNode, ICustomEditorNode
    {
        [Header("Layout")]
        [SerializeField, Range(1, 80)] private int _islandCount = 18;
        [SerializeField, Range(0.01f, 0.5f)] private float _minRadius = 0.035f;
        [SerializeField, Range(0.02f, 0.75f)] private float _maxRadius = 0.22f;
        [SerializeField, Range(0.2f, 4f)] private float _sizeBias = 1.8f;
        [SerializeField, Range(0f, 1f)] private float _spacing = 0.25f;
        [SerializeField] private Vector2 _offset;

        [Header("Shape")]
        [SerializeField, Range(0f, 1.5f)] private float _aspectVariance = 0.55f;
        [SerializeField, Range(0.001f, 0.45f)] private float _shoreSoftness = 0.12f;
        [SerializeField, Range(0.2f, 4f)] private float _falloffPower = 1.35f;
        [SerializeField, Range(0f, 0.5f)] private float _shoreCutoff = 0.12f;

        [Header("Coast Distortion")]
        [SerializeField, Min(0.001f)] private float _coastNoiseScale = 7.5f;
        [SerializeField, Range(0f, 0.45f)] private float _coastNoiseStrength = 0.16f;
        [SerializeField, Range(1, 6)] private int _coastOctaves = 3;

        [Header("Interior Detail")]
        [SerializeField, Min(0.001f)] private float _detailNoiseScale = 18f;
        [SerializeField, Range(0f, 0.75f)] private float _detailStrength = 0.22f;
        [SerializeField, Range(1, 6)] private int _detailOctaves = 4;
        [SerializeField, Range(0.2f, 4f)] private float _heightPower = 1.1f;

        [Header("Output")]
        [SerializeField, Range(0f, 1f)] private float _outputMin = 0f;
        [SerializeField, Range(0f, 1f)] private float _outputMax = 1f;
        [SerializeField, Range(0f, 1f)] private float _heightReduction = 0f;
        [SerializeField] private bool _normalize = true;

        [NonSerialized] private float[,] _lastNoise;
        [NonSerialized] private int _lastSeed;
        [NonSerialized] private bool _hasLastSeed;

        public override string Title => "Island Noise";
        public override string Category => "Noise";
        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[] { PortDefinition.Output<float[,]>("Noise") };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            SanitizeSettings();

            int width = Mathf.Max(1, context.MapSize.x);
            int height = Mathf.Max(1, context.MapSize.y);
            int seed = context.Seed;

            _lastSeed = seed;
            _hasLastSeed = true;
            _lastNoise = GenerateMap(width, height, seed, context);
            return NodeOutput.Success(_lastNoise);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            SanitizeSettings();

            float[,] preview = _lastNoise;
            if (preview == null || preview.GetLength(0) != width || preview.GetLength(1) != height)
            {
                if (!_hasLastSeed)
                    return null;

                preview = GenerateMap(Mathf.Max(1, width), Mathf.Max(1, height), _lastSeed, null);
            }

            int sourceWidth = preview.GetLength(0);
            int sourceHeight = preview.GetLength(1);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            for (int previewX = 0; previewX < width; previewX++)
            for (int previewY = 0; previewY < height; previewY++)
            {
                int sourceX = Mathf.Clamp(previewX * sourceWidth / width, 0, sourceWidth - 1);
                int sourceY = Mathf.Clamp(previewY * sourceHeight / height, 0, sourceHeight - 1);
                float value = Mathf.Clamp01(preview[sourceX, sourceY]);
                texture.SetPixel(previewX, previewY, new Color(value, value, value, 1f));
            }

            texture.Apply();
            return texture;
        }

        public void ClearPreviewCache()
        {
            _lastNoise = null;
        }

        public void SanitizeSettings()
        {
            _islandCount = Mathf.Clamp(_islandCount, 1, 80);
            _minRadius = Mathf.Clamp(_minRadius, 0.001f, 0.75f);
            _maxRadius = Mathf.Clamp(_maxRadius, 0.001f, 0.75f);
            if (_maxRadius < _minRadius)
                _maxRadius = _minRadius;

            _sizeBias = Mathf.Max(0.2f, _sizeBias);
            _shoreSoftness = Mathf.Max(0.001f, _shoreSoftness);
            _falloffPower = Mathf.Max(0.2f, _falloffPower);
            _coastNoiseScale = Mathf.Max(0.001f, _coastNoiseScale);
            _detailNoiseScale = Mathf.Max(0.001f, _detailNoiseScale);
            _heightPower = Mathf.Max(0.2f, _heightPower);

            _outputMin = Mathf.Clamp01(_outputMin);
            _outputMax = Mathf.Clamp01(_outputMax);
            if (_outputMax < _outputMin)
                _outputMax = _outputMin;
            _heightReduction = Mathf.Clamp01(_heightReduction);
        }

        private float[,] GenerateMap(int width, int height, int seed, NodeContext context)
        {
            var islands = BuildIslands(seed);
            var map = new float[width, height];
            float maxValue = 0f;

            float aspectCorrection = width / Mathf.Max(1f, (float)height);

            for (int pixelX = 0; pixelX < width; pixelX++)
            for (int pixelY = 0; pixelY < height; pixelY++)
            {
                float sampleX = ((pixelX + 0.5f) / width) + _offset.x;
                float sampleY = ((pixelY + 0.5f) / height) + _offset.y;
                float value = 0f;

                for (int islandIndex = 0; islandIndex < islands.Count; islandIndex++)
                {
                    float islandValue = SampleIsland(islands[islandIndex], sampleX, sampleY, aspectCorrection, seed);
                    if (islandValue > value)
                        value = islandValue;
                }

                value = ApplyShoreCutoff(value);
                value = Mathf.Pow(Mathf.Clamp01(value), _heightPower);
                map[pixelX, pixelY] = value;
                if (value > maxValue)
                    maxValue = value;

                context?.CountIteration();
            }

            if (_normalize && maxValue > 0.00001f)
            {
                for (int pixelX = 0; pixelX < width; pixelX++)
                for (int pixelY = 0; pixelY < height; pixelY++)
                    map[pixelX, pixelY] = Mathf.Clamp01(map[pixelX, pixelY] / maxValue);
            }

            if (_outputMin > 0f || _outputMax < 1f)
            {
                for (int pixelX = 0; pixelX < width; pixelX++)
                for (int pixelY = 0; pixelY < height; pixelY++)
                    map[pixelX, pixelY] = Mathf.Lerp(_outputMin, _outputMax, map[pixelX, pixelY]);
            }

            if (_heightReduction > 0f)
            {
                for (int pixelX = 0; pixelX < width; pixelX++)
                for (int pixelY = 0; pixelY < height; pixelY++)
                    map[pixelX, pixelY] = Mathf.Clamp01(map[pixelX, pixelY] - _heightReduction);
            }

            return map;
        }

        private List<IslandSpec> BuildIslands(int seed)
        {
            var random = new System.Random(seed);
            var islands = new List<IslandSpec>(_islandCount);

            for (int islandIndex = 0; islandIndex < _islandCount; islandIndex++)
            {
                IslandSpec island = default;
                bool placed = false;

                for (int attempt = 0; attempt < 32; attempt++)
                {
                    island = CreateIsland(random, islandIndex);
                    if (IsValidPlacement(islands, island))
                    {
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                    island = CreateIsland(random, islandIndex);

                islands.Add(island);
            }

            return islands;
        }

        private IslandSpec CreateIsland(System.Random random, int islandIndex)
        {
            float sizeT = Mathf.Pow(Random01(random), _sizeBias);
            float radius = Mathf.Lerp(_maxRadius, _minRadius, sizeT);
            float aspect = Mathf.Lerp(1f - _aspectVariance * 0.5f, 1f + _aspectVariance, Random01(random));
            aspect = Mathf.Max(0.15f, aspect);
            float angle = Random01(random) * Mathf.PI * 2f;

            return new IslandSpec
            {
                Center = new Vector2(RandomRange(random, 0.04f, 0.96f), RandomRange(random, 0.04f, 0.96f)),
                RadiusX = radius * aspect,
                RadiusY = radius / Mathf.Sqrt(aspect),
                AngleCos = Mathf.Cos(angle),
                AngleSin = Mathf.Sin(angle),
                Height = RandomRange(random, 0.75f, 1f),
                Salt = islandIndex * 137 + random.Next(10000, 99999)
            };
        }

        private bool IsValidPlacement(List<IslandSpec> islands, IslandSpec candidate)
        {
            if (_spacing <= 0f)
                return true;

            float candidateRadius = Mathf.Max(candidate.RadiusX, candidate.RadiusY);
            for (int islandIndex = 0; islandIndex < islands.Count; islandIndex++)
            {
                IslandSpec existing = islands[islandIndex];
                float existingRadius = Mathf.Max(existing.RadiusX, existing.RadiusY);
                float minDistance = (candidateRadius + existingRadius) * _spacing;
                if (Vector2.Distance(candidate.Center, existing.Center) < minDistance)
                    return false;
            }

            return true;
        }

        private float SampleIsland(IslandSpec island, float sampleX, float sampleY, float aspectCorrection, int seed)
        {
            float deltaX = (sampleX - island.Center.x) * aspectCorrection;
            float deltaY = sampleY - island.Center.y;

            float localX = island.AngleCos * deltaX + island.AngleSin * deltaY;
            float localY = -island.AngleSin * deltaX + island.AngleCos * deltaY;

            float normalizedX = localX / Mathf.Max(0.0001f, island.RadiusX);
            float normalizedY = localY / Mathf.Max(0.0001f, island.RadiusY);
            float distance = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);

            if (distance > 1f + _coastNoiseStrength)
                return 0f;

            float coastNoise = ProceduralNoiseUtility.SampleFbm(
                (sampleX + island.Salt * 0.013f) * _coastNoiseScale,
                (sampleY - island.Salt * 0.017f) * _coastNoiseScale,
                _coastOctaves,
                2f,
                0.55f,
                seed + island.Salt,
                true);

            float warpedDistance = distance + ((coastNoise - 0.5f) * 2f * _coastNoiseStrength);
            float rawBody = 1f - warpedDistance;
            if (rawBody <= 0f)
                return 0f;

            float edge = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(rawBody / _shoreSoftness));
            float dome = Mathf.Pow(Mathf.Clamp01(rawBody), _falloffPower);

            float detailNoise = ProceduralNoiseUtility.SampleFbm(
                (sampleX + island.Salt * 0.031f) * _detailNoiseScale,
                (sampleY + island.Salt * 0.029f) * _detailNoiseScale,
                _detailOctaves,
                2.1f,
                0.5f,
                seed ^ island.Salt,
                true);

            float detail = 1f + ((detailNoise - 0.5f) * 2f * _detailStrength);
            return Mathf.Clamp01(dome * edge * detail * island.Height);
        }

        private float ApplyShoreCutoff(float value)
        {
            if (value <= _shoreCutoff)
                return 0f;

            float normalized = Mathf.InverseLerp(_shoreCutoff, 1f, value);
            return Mathf.SmoothStep(0f, 1f, normalized);
        }

        private static float Random01(System.Random random)
        {
            return (float)random.NextDouble();
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return Mathf.Lerp(min, max, Random01(random));
        }

        private void OnValidate()
        {
            SanitizeSettings();
            _lastNoise = null;
        }

        private struct IslandSpec
        {
            public Vector2 Center;
            public float RadiusX;
            public float RadiusY;
            public float AngleCos;
            public float AngleSin;
            public float Height;
            public int Salt;
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            UnityEditor.Selection.activeObject = this;
        }
#endif
    }
}