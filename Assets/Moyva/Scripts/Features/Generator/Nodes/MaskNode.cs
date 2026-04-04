using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    public enum MaskComparison
    {
        GreaterThan,
        LessThan,
        Between
    }

    [NodeInfo("Mask", "Utility")]
    public sealed class MaskNode : NodeBase
    {
        [Header("Mask Settings")]
        [SerializeField] private MaskComparison _comparison = MaskComparison.GreaterThan;
        [SerializeField] private float _threshold = 0.5f;
        [SerializeField] private float _upperThreshold = 0.8f;

        public override string Title => "Mask";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var mask = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float v = heightMap[x, y];
                    mask[x, y] = _comparison switch
                    {
                        MaskComparison.GreaterThan => v > _threshold,
                        MaskComparison.LessThan => v < _threshold,
                        MaskComparison.Between => v >= _threshold && v <= _upperThreshold,
                        _ => false
                    };
                }
            }

            return NodeOutput.Success(mask);
        }
    }
}
