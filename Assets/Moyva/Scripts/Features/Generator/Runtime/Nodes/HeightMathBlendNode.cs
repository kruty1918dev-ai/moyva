using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum HeightMathMode
    {
        Add,
        Subtract,
        Multiply,
        Min,
        Max,
        Replace,
        FlattenToLevel
    }

    [NodeInfo("Height Math Blend", "Processing", "Combines base height with a modifier map using math operations, optional mask and smooth transitions.")]
    public sealed class HeightMathBlendNode : NodeBase
    {
        [Header("Blend")]
        [SerializeField] private HeightMathMode _mode = HeightMathMode.Add;
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;
        [SerializeField] private bool _invertModifier;
        [SerializeField, Range(-1f, 1f)] private float _modifierBias;
        [SerializeField, Range(0f, 1f)] private float _flattenLevel = 0f;
        [SerializeField] private bool _clamp01 = true;

        [Header("Mask")]
        [SerializeField, Range(0f, 0.5f)] private float _edgeSoftness = 0.1f;
        [SerializeField, Range(0, 4)] private int _maskSmoothRadius = 1;
        [SerializeField, Range(0, 4)] private int _maskSmoothIterations = 0;

        public override string Title => "Height Math Blend";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Base Height"),
            PortDefinition.Input<float[,]>("Modifier Height"),
            PortDefinition.Input<float[,]>("Mask (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var baseMap = inputs[0] as float[,];
            var modifierMap = inputs[1] as float[,];
            var maskMap = inputs[2] as float[,];

            if (baseMap == null)
                return NodeOutput.Error("Base Height input is required.");
            if (modifierMap == null)
                return NodeOutput.Error("Modifier Height input is required.");

            int w = baseMap.GetLength(0);
            int h = baseMap.GetLength(1);

            var preparedMask = maskMap;
            if (preparedMask != null && _maskSmoothIterations > 0 && _maskSmoothRadius > 0)
                preparedMask = HeightMapMathUtility.BoxBlur(preparedMask, _maskSmoothRadius, _maskSmoothIterations);

            var result = new float[w, h];
            float edgeMin = 0.5f - _edgeSoftness;
            float edgeMax = 0.5f + _edgeSoftness;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float baseValue = baseMap[x, y];
                    float modifierValue = HeightMapMathUtility.Sample01(modifierMap, x, y, 0f);

                    if (_invertModifier)
                        modifierValue = 1f - modifierValue;

                    modifierValue = Mathf.Clamp01(modifierValue + _modifierBias);

                    float operationValue = ApplyMode(baseValue, modifierValue);

                    float maskWeight = preparedMask != null
                        ? HeightMapMathUtility.Sample01(preparedMask, x, y, 0f)
                        : 1f;

                    if (_edgeSoftness > 0f)
                    {
                        float t = Mathf.InverseLerp(edgeMin, edgeMax, maskWeight);
                        maskWeight = Mathf.SmoothStep(0f, 1f, t);
                    }

                    float weight = Mathf.Clamp01(maskWeight * _strength);
                    float blended = Mathf.Lerp(baseValue, operationValue, weight);
                    result[x, y] = _clamp01 ? Mathf.Clamp01(blended) : blended;
                    context.CountIteration();
                }
            }

            return NodeOutput.Success(result);
        }

        private float ApplyMode(float baseValue, float modifierValue)
        {
            return _mode switch
            {
                HeightMathMode.Add => baseValue + modifierValue,
                HeightMathMode.Subtract => baseValue - modifierValue,
                HeightMathMode.Multiply => baseValue * modifierValue,
                HeightMathMode.Min => Mathf.Min(baseValue, modifierValue),
                HeightMathMode.Max => Mathf.Max(baseValue, modifierValue),
                HeightMathMode.Replace => modifierValue,
                HeightMathMode.FlattenToLevel => _flattenLevel,
                _ => baseValue
            };
        }
    }
}