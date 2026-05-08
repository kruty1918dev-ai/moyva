using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Height Supplement", "Terrain",
        "Applies an incoming noise mask as an additional height layer only inside a selected base-height range.")]
    public sealed class HeightSupplementNode : NodeBase, IPreviewableNode, ICustomEditorNode
    {
        private const float MinRangeGap = 0.001f;

        [Header("Height Range")]
        [SerializeField, Range(0f, 1f)] private float _minHeight = 0f;
        [SerializeField, Range(0f, 1f)] private float _maxHeight = 0.3f;

        [Header("Noise Cut")]
        [SerializeField, Range(0f, 1f)] private float _noiseMin = 0f;
        [SerializeField, Range(0f, 1f)] private float _noiseMax = 1f;
        [SerializeField, Range(0f, 0.5f)] private float _noiseTransition = 0.05f;
        [SerializeField, Range(0.1f, 4f)] private float _peakPower = 1f;

        [Header("Blend")]
        [SerializeField, Range(-1f, 1f)] private float _amount = 0.12f;
        [SerializeField, Range(0f, 0.5f)] private float _rangeTransition = 0.05f;

        [Header("Smoothing")]
        [SerializeField, Range(0, 3)] private int _smoothRadius = 0;
        [SerializeField, Range(0, 4)] private int _smoothIterations = 0;

        [NonSerialized] private float[,] _lastInput;
        [NonSerialized] private float[,] _lastNoiseMask;
        [NonSerialized] private float[,] _lastProcessed;
        [NonSerialized] private float[,] _lastRangeMask;
        [NonSerialized] private float[,] _lastAppliedNoise;

        public override string Title => "Height Supplement";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<float[,]>("Noise Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<float[,]>("Range Mask"),
            PortDefinition.Output<float[,]>("Applied Noise")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            SanitizeSettings();

            var heightMap = inputs.Length > 0 ? inputs[0] as float[,] : null;
            var noiseMask = inputs.Length > 1 ? inputs[1] as float[,] : null;

            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");
            if (noiseMask == null)
                return NodeOutput.Error("Noise Mask input is required.");

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            if (noiseMask.GetLength(0) != width || noiseMask.GetLength(1) != height)
                return NodeOutput.Error("Noise Mask must have the same dimensions as HeightMap.");

            var result = (float[,])heightMap.Clone();
            var rangeMask = new float[width, height];
            var appliedNoise = new float[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float baseHeight = Mathf.Clamp01(heightMap[x, y]);
                if (!IsInsideHeightRange(baseHeight))
                {
                    context.CountIteration();
                    continue;
                }

                float rangeWeight = ComputeRangeWeight(baseHeight);
                float noiseValue = Mathf.Clamp01(noiseMask[x, y]);
                float clippedNoise = ComputeClippedNoise(noiseValue);
                float shapedNoise = Mathf.Pow(clippedNoise, _peakPower);
                float influence = rangeWeight * shapedNoise;

                float modifiedHeight = baseHeight + (_amount * influence);
                result[x, y] = Mathf.Clamp(modifiedHeight, _minHeight, _maxHeight);
                rangeMask[x, y] = 1f;
                appliedNoise[x, y] = influence;

                context.CountIteration();
            }

            if (_smoothIterations > 0 && _smoothRadius > 0)
            {
                var smoothed = HeightMapMathUtility.BoxBlur(result, _smoothRadius, _smoothIterations);
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (rangeMask[x, y] <= 0f)
                        continue;

                    float blend = Mathf.Clamp01(appliedNoise[x, y]);
                    result[x, y] = Mathf.Clamp(Mathf.Lerp(result[x, y], smoothed[x, y], blend), _minHeight, _maxHeight);
                }
            }

            _lastInput = heightMap;
            _lastNoiseMask = noiseMask;
            _lastProcessed = result;
            _lastRangeMask = rangeMask;
            _lastAppliedNoise = appliedNoise;

            return NodeOutput.Success(result, rangeMask, appliedNoise);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastProcessed == null)
                return null;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            int sourceWidth = _lastProcessed.GetLength(0);
            int sourceHeight = _lastProcessed.GetLength(1);

            for (int px = 0; px < width; px++)
            for (int py = 0; py < height; py++)
            {
                int sx = Mathf.Clamp(px * sourceWidth / width, 0, sourceWidth - 1);
                int sy = Mathf.Clamp(py * sourceHeight / height, 0, sourceHeight - 1);

                float inRange = _lastRangeMask != null ? _lastRangeMask[sx, sy] : 0f;
                if (inRange <= 0f)
                {
                    float original = _lastInput != null ? Mathf.Clamp01(_lastInput[sx, sy]) : 0f;
                    float dimmed = Mathf.Lerp(0.04f, 0.22f, original);
                    texture.SetPixel(px, py, new Color(dimmed, dimmed, dimmed, 1f));
                    continue;
                }

                // Show the noise pattern inside the range:
                // white = strong peak contribution, black = no noise applied here
                float noiseVal = _lastAppliedNoise != null ? Mathf.Clamp01(_lastAppliedNoise[sx, sy]) : 0f;
                texture.SetPixel(px, py, new Color(noiseVal, noiseVal, noiseVal, 1f));
            }

            texture.Apply();
            return texture;
        }

        public void SanitizeSettings()
        {
            _minHeight = Mathf.Clamp01(_minHeight);
            _maxHeight = Mathf.Clamp01(_maxHeight);
            if (_maxHeight <= _minHeight + MinRangeGap)
                _maxHeight = Mathf.Min(1f, _minHeight + MinRangeGap);
            if (_maxHeight <= _minHeight)
                _minHeight = Mathf.Max(0f, _maxHeight - MinRangeGap);

            _noiseMin = Mathf.Clamp01(_noiseMin);
            _noiseMax = Mathf.Clamp01(_noiseMax);
            if (_noiseMax <= _noiseMin + MinRangeGap)
                _noiseMax = Mathf.Min(1f, _noiseMin + MinRangeGap);
            if (_noiseMax <= _noiseMin)
                _noiseMin = Mathf.Max(0f, _noiseMax - MinRangeGap);

            _peakPower = Mathf.Max(0.1f, _peakPower);
            _rangeTransition = Mathf.Clamp01(_rangeTransition);
            _noiseTransition = Mathf.Clamp01(_noiseTransition);
        }

        private bool IsInsideHeightRange(float value)
        {
            return value >= _minHeight && value <= _maxHeight;
        }

        private float ComputeRangeWeight(float value)
        {
            if (!IsInsideHeightRange(value))
                return 0f;

            float range = Mathf.Max(MinRangeGap, _maxHeight - _minHeight);
            float edgeSize = range * _rangeTransition;
            if (edgeSize <= 0.00001f)
                return 1f;

            // At the absolute floor (min=0) or ceiling (max=1) there is no adjacent terrain
            // to blend from/to, so no fade-in/out at that edge.
            float fromMin = _minHeight <= 0f ? 1f : Mathf.Clamp01((value - _minHeight) / edgeSize);
            float fromMax = _maxHeight >= 1f ? 1f : Mathf.Clamp01((_maxHeight - value) / edgeSize);
            return Mathf.SmoothStep(0f, 1f, Mathf.Min(fromMin, fromMax));
        }

        private float ComputeClippedNoise(float noiseValue)
        {
            if (noiseValue < _noiseMin || noiseValue > _noiseMax)
                return 0f;

            float normalized = Mathf.InverseLerp(_noiseMin, _noiseMax, noiseValue);
            float range = Mathf.Max(MinRangeGap, _noiseMax - _noiseMin);
            float edgeSize = range * _noiseTransition;
            if (edgeSize <= 0.00001f)
                return normalized;

            float fromMin = Mathf.Clamp01((noiseValue - _noiseMin) / edgeSize);
            float fromMax = Mathf.Clamp01((_noiseMax - noiseValue) / edgeSize);
            float gate = Mathf.SmoothStep(0f, 1f, Mathf.Min(fromMin, fromMax));
            return normalized * gate;
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