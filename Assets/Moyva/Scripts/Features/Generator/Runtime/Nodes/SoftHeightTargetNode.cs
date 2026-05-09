using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Soft Height Target", "Terrain",
        "Softly moves heights inside a selected range toward a target height while preserving natural neighbor transitions.")]
    public sealed class SoftHeightTargetNode : NodeBase, IPreviewableNode, ICustomEditorNode
    {
        private const float MinGap = 0.000001f;

        [Header("Height Range")]
        [SerializeField, Range(0f, 1f)] private float _minHeight = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _maxHeight = 0.45f;

        [Header("Target")]
        [SerializeField, Range(0f, 1f)] private float _targetHeight = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _strength = 0.65f;
        [SerializeField, Range(0f, 0.5f)] private float _rangeTransition = 0.12f;

        [Header("Neighbor Softness")]
        [SerializeField, Range(0f, 1f)] private float _softness = 0.45f;
        [SerializeField, Range(0, 6)] private int _neighborRadius = 2;
        [SerializeField, Range(0, 6)] private int _softnessIterations = 1;

        [NonSerialized] private float[,] _lastInput;
        [NonSerialized] private float[,] _lastOutput;
        [NonSerialized] private float[,] _lastInfluenceMask;
        [NonSerialized] private float[,] _lastDeltaMap;

        public override string Title => "Soft Height Target";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<float[,]>("Influence Mask"),
            PortDefinition.Output<float[,]>("Delta Map")
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
            var influenceMask = new float[width, height];
            var deltaMap = new float[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float source = Mathf.Clamp01(heightMap[x, y]);
                float influence = ComputeRangeWeight(source) * _strength;
                influenceMask[x, y] = influence;

                if (influence > 0f)
                    result[x, y] = Mathf.Lerp(source, _targetHeight, influence);

                context.CountIteration();
            }

            if (_softness > 0f && _neighborRadius > 0 && _softnessIterations > 0)
            {
                var smoothed = HeightMapMathUtility.BoxBlur(result, _neighborRadius, _softnessIterations);
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    float influence = influenceMask[x, y];
                    if (influence <= 0f)
                        continue;

                    float blend = Mathf.Clamp01(_softness * influence);
                    result[x, y] = Mathf.Lerp(result[x, y], smoothed[x, y], blend);
                }
            }

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                deltaMap[x, y] = result[x, y] - heightMap[x, y];

            _lastInput = heightMap;
            _lastOutput = result;
            _lastInfluenceMask = influenceMask;
            _lastDeltaMap = deltaMap;

            return NodeOutput.Success(result, influenceMask, deltaMap);
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
                texture.SetPixel(x, y, PreviewColor(sx, sy, sourceWidth, sourceHeight));
            }

            texture.Apply();
            return texture;
        }

        public void SanitizeSettings()
        {
            _minHeight = Mathf.Clamp01(_minHeight);
            _maxHeight = Mathf.Clamp01(_maxHeight);
            if (_maxHeight <= _minHeight + MinGap)
                _maxHeight = Mathf.Min(1f, _minHeight + MinGap);
            if (_maxHeight <= _minHeight)
                _minHeight = Mathf.Max(0f, _maxHeight - MinGap);

            _targetHeight = Mathf.Clamp01(_targetHeight);
            _strength = Mathf.Clamp01(_strength);
            _rangeTransition = Mathf.Clamp01(_rangeTransition);
            _softness = Mathf.Clamp01(_softness);
            _neighborRadius = Mathf.Max(0, _neighborRadius);
            _softnessIterations = Mathf.Max(0, _softnessIterations);
        }

        private float ComputeRangeWeight(float value)
        {
            if (value < _minHeight || value > _maxHeight)
                return 0f;

            float range = Mathf.Max(MinGap, _maxHeight - _minHeight);
            float edgeSize = range * _rangeTransition;
            if (edgeSize <= 0.00001f)
                return 1f;

            float fromMin = _minHeight <= 0f ? 1f : Mathf.Clamp01((value - _minHeight) / edgeSize);
            float fromMax = _maxHeight >= 1f ? 1f : Mathf.Clamp01((_maxHeight - value) / edgeSize);
            return Mathf.SmoothStep(0f, 1f, Mathf.Min(fromMin, fromMax));
        }

        private Color PreviewColor(int x, int y, int width, int height)
        {
            float source = _lastInput != null ? Mathf.Clamp01(_lastInput[x, y]) : 0f;
            Color baseColor = Color.Lerp(new Color(0.08f, 0.08f, 0.08f), TerrainSlopeFilterNode.HeightToColor(source), 0.45f);

            float influence = _lastInfluenceMask != null ? Mathf.Clamp01(_lastInfluenceMask[x, y]) : 0f;
            if (influence <= 0f)
                return baseColor;

            float delta = _lastDeltaMap != null ? _lastDeltaMap[x, y] : 0f;
            float deltaStrength = Mathf.Clamp01(Mathf.Abs(delta) / 0.25f);
            Color changeColor = delta >= 0f
                ? new Color(1f, 0.48f, 0.12f)
                : new Color(0.08f, 0.65f, 1f);

            Color regionColor = Color.Lerp(new Color(1f, 0.9f, 0.25f), changeColor, deltaStrength);
            Color finalColor = Color.Lerp(baseColor, regionColor, Mathf.Clamp01(0.25f + influence * 0.75f));

            if (IsRegionEdge(x, y, width, height))
                finalColor = Color.Lerp(finalColor, Color.white, 0.65f);

            return finalColor;
        }

        private bool IsRegionEdge(int x, int y, int width, int height)
        {
            if (_lastInfluenceMask == null || _lastInfluenceMask[x, y] <= 0f)
                return false;

            return IsOutsideRegion(x + 1, y, width, height)
                || IsOutsideRegion(x - 1, y, width, height)
                || IsOutsideRegion(x, y + 1, width, height)
                || IsOutsideRegion(x, y - 1, width, height);
        }

        private bool IsOutsideRegion(int x, int y, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return true;
            return _lastInfluenceMask == null || _lastInfluenceMask[x, y] <= 0f;
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