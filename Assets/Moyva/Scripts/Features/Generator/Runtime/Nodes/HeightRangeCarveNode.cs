using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Height Range Carve", "Terrain", "Carves river-like channels inside a selected height range using a noise guide and smooth banks.")]
    public sealed class HeightRangeCarveNode : NodeBase
    {
        [Header("Range")]
        [SerializeField, Range(0f, 1f)] private float _targetMinHeight = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _targetMaxHeight = 0.45f;

        [Header("Channel")]
        [SerializeField, Range(0f, 1f)] private float _channelCenter = 0.5f;
        [SerializeField, Range(0.01f, 1f)] private float _channelWidth = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _depth = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;
        [SerializeField, Range(0f, 1f)] private float _bankSoftness = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _minClampHeight = 0f;

        [Header("Auto Smoothing")]
        [SerializeField, Range(0, 3)] private int _smoothRadius = 1;
        [SerializeField, Range(0, 4)] private int _smoothIterations = 1;

        public override string Title => "Height Range Carve";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<float[,]>("Carve Noise"),
            PortDefinition.Input<float[,]>("Control Mask (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Carved HeightMap"),
            PortDefinition.Output<float[,]>("Carve Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var carveNoise = inputs[1] as float[,];
            var controlMask = inputs[2] as float[,];

            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");
            if (carveNoise == null)
                return NodeOutput.Error("Carve Noise input is required.");

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            float minH = Mathf.Min(_targetMinHeight, _targetMaxHeight);
            float maxH = Mathf.Max(_targetMinHeight, _targetMaxHeight);
            float range = Mathf.Max(0.0001f, maxH - minH);

            var carved = (float[,])heightMap.Clone();
            var carveMask = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float baseHeight = heightMap[x, y];
                    float heightWeight = ComputeHeightBandWeight(baseHeight, minH, maxH, range);
                    if (heightWeight <= 0f)
                    {
                        context.CountIteration();
                        continue;
                    }

                    float noiseValue = HeightMapMathUtility.Sample01(carveNoise, x, y, 0.5f);
                    float halfWidth = Mathf.Max(0.005f, _channelWidth * 0.5f);
                    float distance = Mathf.Abs(noiseValue - _channelCenter);
                    float channel = Mathf.Clamp01(1f - (distance / halfWidth));
                    channel = Mathf.SmoothStep(0f, 1f, channel);

                    float bankPower = Mathf.Lerp(2.2f, 0.7f, _bankSoftness);
                    channel = Mathf.Pow(channel, bankPower);

                    float control = controlMask != null
                        ? HeightMapMathUtility.Sample01(controlMask, x, y, 0f)
                        : 1f;

                    float influence = Mathf.Clamp01(heightWeight * channel * control * _strength);
                    carveMask[x, y] = influence;

                    if (influence > 0f)
                    {
                        float lowered = baseHeight - (_depth * influence);
                        carved[x, y] = Mathf.Max(_minClampHeight, lowered);
                    }

                    context.CountIteration();
                }
            }

            if (_smoothIterations > 0 && _smoothRadius > 0)
            {
                var smoothed = HeightMapMathUtility.BoxBlur(carved, _smoothRadius, _smoothIterations);
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        float localBlend = Mathf.Clamp01(carveMask[x, y]);
                        if (localBlend <= 0f)
                            continue;

                        carved[x, y] = Mathf.Lerp(carved[x, y], smoothed[x, y], localBlend);
                    }
                }
            }

            return NodeOutput.Success(carved, carveMask);
        }

        private static float ComputeHeightBandWeight(float value, float minH, float maxH, float range)
        {
            if (value < minH || value > maxH)
                return 0f;

            float center = (minH + maxH) * 0.5f;
            float halfRange = Mathf.Max(0.0001f, range * 0.5f);
            float distance = Mathf.Abs(value - center);
            float linear = Mathf.Clamp01(1f - (distance / halfRange));
            return Mathf.SmoothStep(0f, 1f, linear);
        }
    }
}