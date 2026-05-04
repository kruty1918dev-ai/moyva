using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum HeightBlendCurve
    {
        Linear,
        SmoothStep,
        SmootherStep
    }

    [NodeInfo("Height Smooth Blend", "Processing", "Smoothly blends two height maps with a blend mask and transition controls.")]
    public sealed class HeightSmoothBlendNode : NodeBase
    {
        [Header("Blend")]
        [SerializeField, Range(0f, 1f)] private float _blendStrength = 1f;
        [SerializeField] private HeightBlendCurve _curve = HeightBlendCurve.SmoothStep;
        [SerializeField, Range(0, 4)] private int _maskSmoothRadius = 1;
        [SerializeField, Range(0, 4)] private int _maskSmoothIterations = 1;
        [SerializeField] private bool _preserveHigherPeaks = true;
        [SerializeField, Range(0f, 1f)] private float _peakPreserveFactor = 0.35f;

        public override string Title => "Height Smooth Blend";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Height A"),
            PortDefinition.Input<float[,]>("Height B"),
            PortDefinition.Input<float[,]>("Blend Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Blended HeightMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var a = inputs[0] as float[,];
            var b = inputs[1] as float[,];
            var mask = inputs[2] as float[,];

            if (a == null)
                return NodeOutput.Error("Height A input is required.");
            if (b == null)
                return NodeOutput.Error("Height B input is required.");
            if (mask == null)
                return NodeOutput.Error("Blend Mask input is required.");

            int w = a.GetLength(0);
            int h = a.GetLength(1);

            var preparedMask = (_maskSmoothIterations > 0 && _maskSmoothRadius > 0)
                ? HeightMapMathUtility.BoxBlur(mask, _maskSmoothRadius, _maskSmoothIterations)
                : (float[,])mask.Clone();

            var result = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float av = a[x, y];
                    float bv = HeightMapMathUtility.Sample(b, x, y, av);

                    float m = HeightMapMathUtility.Sample01(preparedMask, x, y, 0f);
                    m = ApplyCurve(m, _curve);
                    m = Mathf.Clamp01(m * _blendStrength);

                    float blended = Mathf.Lerp(av, bv, m);
                    if (_preserveHigherPeaks)
                    {
                        float peak = Mathf.Max(av, bv);
                        float preserveWeight = m * (1f - m) * _peakPreserveFactor * 4f;
                        blended = Mathf.Lerp(blended, peak, Mathf.Clamp01(preserveWeight));
                    }

                    result[x, y] = Mathf.Clamp01(blended);
                    context.CountIteration();
                }
            }

            return NodeOutput.Success(result);
        }

        private static float ApplyCurve(float t, HeightBlendCurve curve)
        {
            t = Mathf.Clamp01(t);
            return curve switch
            {
                HeightBlendCurve.Linear => t,
                HeightBlendCurve.SmoothStep => Mathf.SmoothStep(0f, 1f, t),
                HeightBlendCurve.SmootherStep => t * t * t * (t * (t * 6f - 15f) + 10f),
                _ => t
            };
        }
    }
}