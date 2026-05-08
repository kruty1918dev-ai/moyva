using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Steppe / Mountain Balance", "Terrain",
        "Below the steppe start the noise is passed through. Above it, the height range above the steppe start is limited to a percentage of [steppeStart..1].")]
    public sealed class SteppeMountainBalanceNode : NodeBase, IPreviewableNode, ICustomEditorNode
    {
        private const float MinGap = 0.000001f;

        [Header("Distribution")]
        [SerializeField, Range(0f, 100f)] private float _mountainCoveragePercent = 20f;

        [Header("Steppe Range")]
        [SerializeField, Range(0f, 1f)] private float _steppeStartHeight = 0.25f;

        [Header("Shape")]
        [SerializeField, Range(0.1f, 4f)] private float _liftCurve = 1f;

        [NonSerialized] private float[,] _lastInput;
        [NonSerialized] private float[,] _lastOutput;
        [NonSerialized] private float[,] _lastMountainMask;
        [NonSerialized] private float _lastMaxOutputHeight;
        [NonSerialized] private float _lastMountainInputThreshold;
        [NonSerialized] private int _lastSteppeCells;
        [NonSerialized] private int _lastMountainCells;
        [NonSerialized] private int _lastPassThroughCells;

        public int LastSteppeCells => _lastSteppeCells;
        public int LastMountainCells => _lastMountainCells;
        public int LastPassThroughCells => _lastPassThroughCells;
        public float LastMaxOutputHeight => _lastMaxOutputHeight;
        public float LastMountainInputThreshold => _lastMountainInputThreshold;

        public override string Title => "Steppe / Mountain Balance";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Noise")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<float[,]>("Mountain Mask"),
            PortDefinition.Output<float[,]>("Steppe Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            SanitizeSettings();

            var noise = inputs.Length > 0 ? inputs[0] as float[,] : null;
            if (noise == null)
                return NodeOutput.Error("Noise input is required.");

            int width = noise.GetLength(0);
            int height = noise.GetLength(1);
            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Noise map is empty.");

            float coverage01 = Mathf.Clamp01(_mountainCoveragePercent / 100f);
            float aboveSteppe = Mathf.Max(MinGap, 1f - _steppeStartHeight);
            float maxOutputHeight = _steppeStartHeight + coverage01 * aboveSteppe;
            float mountainInputThreshold = 1f - coverage01 * aboveSteppe;

            var output = new float[width, height];
            var mountainMask = new float[width, height];
            var steppeMask = new float[width, height];
            int mountainCells = 0;
            int steppeCells = 0;
            int passThroughCells = 0;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float inputValue = Mathf.Clamp01(noise[x, y]);

                if (inputValue < _steppeStartHeight)
                {
                    output[x, y] = inputValue;
                    mountainMask[x, y] = 0f;
                    steppeMask[x, y] = 0f;
                    passThroughCells++;
                    context.CountIteration();
                    continue;
                }

                float inputAbove01 = Mathf.Clamp01((inputValue - _steppeStartHeight) / aboveSteppe);
                float curved = Mathf.Pow(Mathf.SmoothStep(0f, 1f, inputAbove01), _liftCurve);
                float lifted = _steppeStartHeight + curved * coverage01 * aboveSteppe;
                output[x, y] = Mathf.Clamp01(lifted);

                float mountainWeight = (coverage01 <= 0f || inputValue < mountainInputThreshold) ? 0f : 1f;
                mountainMask[x, y] = mountainWeight;
                steppeMask[x, y] = 1f - mountainWeight;

                if (mountainWeight >= 0.5f) mountainCells++;
                else steppeCells++;

                context.CountIteration();
            }

            _lastInput = noise;
            _lastOutput = output;
            _lastMountainMask = mountainMask;
            _lastMaxOutputHeight = maxOutputHeight;
            _lastMountainInputThreshold = mountainInputThreshold;
            _lastMountainCells = mountainCells;
            _lastSteppeCells = steppeCells;
            _lastPassThroughCells = passThroughCells;

            return NodeOutput.Success(output, mountainMask, steppeMask);
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
            _mountainCoveragePercent = Mathf.Clamp(_mountainCoveragePercent, 0f, 100f);
            _steppeStartHeight = Mathf.Clamp01(_steppeStartHeight);
            if (_steppeStartHeight >= 1f)
                _steppeStartHeight = 1f - MinGap;

            _liftCurve = Mathf.Max(0.1f, _liftCurve);
        }

        private Color PreviewColor(int x, int y)
        {
            float value = _lastOutput != null ? Mathf.Clamp01(_lastOutput[x, y]) : 0f;
            float mountain = _lastMountainMask != null ? Mathf.Clamp01(_lastMountainMask[x, y]) : 0f;

            if (value < _steppeStartHeight)
            {
                float belowT = value / Mathf.Max(MinGap, _steppeStartHeight);
                return Color.Lerp(new Color(0.08f, 0.12f, 0.22f), new Color(0.18f, 0.30f, 0.38f), belowT);
            }

            float steppeTopForView = Mathf.Max(_steppeStartHeight + MinGap, _lastMaxOutputHeight);
            float steppeT = Mathf.InverseLerp(_steppeStartHeight, steppeTopForView, value);
            Color steppeColor = Color.Lerp(new Color(0.45f, 0.55f, 0.18f), new Color(0.78f, 0.68f, 0.32f), steppeT);

            if (mountain <= 0f)
                return steppeColor;

            Color mountainColor = TerrainSlopeFilterNode.HeightToColor(value);
            return Color.Lerp(steppeColor, mountainColor, mountain);
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
