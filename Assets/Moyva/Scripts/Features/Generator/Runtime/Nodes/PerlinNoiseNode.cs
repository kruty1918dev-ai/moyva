using Kruty1918.Moyva.Generator.Runtime.Noise;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Perlin Noise", "Generators", "Генерує карту шуму Perlin у діапазоні 0..1.")]
    public sealed class PerlinNoiseNode : NodeBase
    {
        [SerializeField, Min(0.0001f)] private float _scale = 0.05f;
        [SerializeField] private Vector2 _offset;

        public override string Title => "Perlin Noise";
        public override string Category => "Generators";
        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[] { PortDefinition.Output<float[,]>("Noise") };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int w = Mathf.Max(1, context.MapSize.x);
            int h = Mathf.Max(1, context.MapSize.y);
            var map = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float sx = (x + _offset.x) * _scale;
                    float sy = (y + _offset.y) * _scale;
                    map[x, y] = ProceduralNoiseUtility.SamplePerlin(sx, sy);
                    context.CountIteration();
                }
            }

            return NodeOutput.Success(map);
        }
    }
}
