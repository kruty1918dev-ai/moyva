using Kruty1918.Moyva.Generator.Runtime.Noise;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Multi-Layer Noise Composer", "Processing", "Композитор багатошарових шумів з підтримкою clamp/remap/blend/warp/invert.")]
    public sealed class MultiLayerNoiseComposerNode : NodeBase
    {
        [SerializeField, Range(0f, 1f)] private float _perlinWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _simplexWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _worleyWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _fbmWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _ridgedWeight = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _cellularWeight = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _domainWarpWeight = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _blend = 1f;
        [SerializeField] private bool _invert;
        [SerializeField] private bool _clamp = true;
        [SerializeField] private Vector2 _remap = new(0f, 1f);

        public override string Title => "Multi-Layer Noise Composer";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Perlin (optional)"),
            PortDefinition.Input<float[,]>("Simplex (optional)"),
            PortDefinition.Input<float[,]>("Worley (optional)"),
            PortDefinition.Input<float[,]>("FBM (optional)"),
            PortDefinition.Input<float[,]>("Ridged (optional)"),
            PortDefinition.Input<float[,]>("Cellular (optional)"),
            PortDefinition.Input<float[,]>("DomainWarp (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Composed")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int w = Mathf.Max(1, context.MapSize.x);
            int h = Mathf.Max(1, context.MapSize.y);

            var perlin = inputs[0] as float[,];
            var simplex = inputs[1] as float[,];
            var worley = inputs[2] as float[,];
            var fbm = inputs[3] as float[,];
            var ridged = inputs[4] as float[,];
            var cellular = inputs[5] as float[,];
            var domainWarp = inputs[6] as float[,];

            var map = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float warp = Sample(domainWarp, x, y);
                    int wx = Mathf.Clamp(x + Mathf.RoundToInt((warp - 0.5f) * 8f * _domainWarpWeight), 0, w - 1);
                    int wy = Mathf.Clamp(y + Mathf.RoundToInt((warp - 0.5f) * 8f * _domainWarpWeight), 0, h - 1);

                    float p = Sample(perlin, wx, wy);
                    float s = Sample(simplex, wx, wy);
                    float wv = Sample(worley, wx, wy);
                    float f = Sample(fbm, wx, wy);
                    float r = Sample(ridged, wx, wy);
                    float c = Sample(cellular, wx, wy);

                    float weighted =
                        p * _perlinWeight +
                        s * _simplexWeight +
                        wv * _worleyWeight +
                        f * _fbmWeight +
                        r * _ridgedWeight +
                        c * _cellularWeight;

                    float denom = _perlinWeight + _simplexWeight + _worleyWeight + _fbmWeight + _ridgedWeight + _cellularWeight;
                    float baseValue = denom > 0.00001f ? weighted / denom : 0f;

                    float blended = Mathf.Lerp(p, baseValue, _blend);
                    float remapped = Mathf.Lerp(_remap.x, _remap.y, Mathf.Clamp01(blended));
                    if (_invert) remapped = 1f - remapped;
                    map[x, y] = _clamp ? Mathf.Clamp01(remapped) : remapped;

                    context.CountIteration();
                }
            }

            return NodeOutput.Success(map);
        }

        private static float Sample(float[,] map, int x, int y)
        {
            if (map == null) return 0f;
            int w = map.GetLength(0);
            int h = map.GetLength(1);
            if (w == 0 || h == 0) return 0f;

            x = Mathf.Clamp(x, 0, w - 1);
            y = Mathf.Clamp(y, 0, h - 1);
            return map[x, y];
        }
    }
}
