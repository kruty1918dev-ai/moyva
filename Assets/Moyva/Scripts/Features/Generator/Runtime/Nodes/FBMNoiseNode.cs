using Kruty1918.Moyva.Generator.Runtime.Noise;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum FbmBaseNoise
    {
        Perlin,
        Simplex
    }

    [NodeInfo("FBM Noise", "Generators", "Генерує fractal Brownian motion (FBM) noise з підтримкою Perlin або Simplex бази.")]
    public sealed class FBMNoiseNode : NodeBase
    {
        [SerializeField] private FbmBaseNoise _baseNoise = FbmBaseNoise.Perlin;
        [SerializeField, Min(0.0001f)] private float _scale = 0.03f;
        [SerializeField, Range(1, 10)] private int _octaves = 5;
        [SerializeField, Min(1f)] private float _lacunarity = 2f;
        [SerializeField, Range(0.01f, 1f)] private float _persistence = 0.5f;
        [SerializeField] private Vector2 _offset;
        [SerializeField] private int _seedOffset = 911;

        public override string Title => "FBM Noise";
        public override string Category => "Generators";
        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[] { PortDefinition.Output<float[,]>("Noise") };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int w = Mathf.Max(1, context.MapSize.x);
            int h = Mathf.Max(1, context.MapSize.y);
            int seed = context.Seed ^ _seedOffset;
            bool simplexBase = _baseNoise == FbmBaseNoise.Simplex;
            var map = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float sx = (x + _offset.x) * _scale;
                    float sy = (y + _offset.y) * _scale;
                    map[x, y] = ProceduralNoiseUtility.SampleFbm(
                        sx,
                        sy,
                        _octaves,
                        _lacunarity,
                        _persistence,
                        seed,
                        simplexBase);
                    context.CountIteration();
                }
            }

            return NodeOutput.Success(map);
        }
    }
}
