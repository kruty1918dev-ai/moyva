using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    public enum ScaleMode
    {
        NearestNeighbour,
        Bilinear
    }

    [NodeInfo("Map Scale", "Utility")]
    public sealed class MapScaleNode : NodeBase
    {
        [Header("Scale Settings")]
        [SerializeField, Range(0.1f, 4f)] private float _scaleFactor = 2f;
        [SerializeField] private ScaleMode _interpolation = ScaleMode.Bilinear;

        public override string Title => "Map Scale";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Scaled")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as float[,];
            if (source == null)
                return NodeOutput.Error("HeightMap input is required.");

            int sw = source.GetLength(0);
            int sh = source.GetLength(1);
            int tw = Mathf.Max(1, Mathf.RoundToInt(sw * _scaleFactor));
            int th = Mathf.Max(1, Mathf.RoundToInt(sh * _scaleFactor));
            var result = new float[tw, th];

            for (int x = 0; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {
                    float srcX = x / _scaleFactor;
                    float srcY = y / _scaleFactor;

                    if (_interpolation == ScaleMode.Bilinear)
                        result[x, y] = SampleBilinear(source, srcX, srcY, sw, sh);
                    else
                        result[x, y] = SampleNearest(source, srcX, srcY, sw, sh);
                }
            }

            return NodeOutput.Success(result);
        }

        private static float SampleNearest(float[,] src, float x, float y, int w, int h)
        {
            int ix = Mathf.Clamp(Mathf.RoundToInt(x), 0, w - 1);
            int iy = Mathf.Clamp(Mathf.RoundToInt(y), 0, h - 1);
            return src[ix, iy];
        }

        private static float SampleBilinear(float[,] src, float x, float y, int w, int h)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt(x), 0, w - 1);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(y), 0, h - 1);
            int x1 = Mathf.Min(x0 + 1, w - 1);
            int y1 = Mathf.Min(y0 + 1, h - 1);
            float fx = x - x0;
            float fy = y - y0;

            float a = src[x0, y0];
            float b = src[x1, y0];
            float c = src[x0, y1];
            float d = src[x1, y1];

            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }
    }
}
