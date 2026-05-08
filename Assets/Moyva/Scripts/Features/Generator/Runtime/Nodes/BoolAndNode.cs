using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Bool And", "Math", "Об'єднання двох масок із пріоритетом істини. Результат true там, де true хоча б в одній з масок (A або B).")]
    public sealed class BoolAndNode : NodeBase, IPreviewableNode
    {
        [NonSerialized] private bool[,] _lastA;
        [NonSerialized] private bool[,] _lastB;
        [NonSerialized] private bool[,] _lastResult;

        public override string Title => "Bool And";
        public override string Category => "Math";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("A"),
            PortDefinition.Input<bool[,]>("B")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var a = inputs[0] as bool[,];
            var b = inputs[1] as bool[,];
            if (!BoolMaskMathUtility.ValidatePair(a, b, out int w, out int h, out var error))
                return NodeOutput.Error(error);

            var result = new bool[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                    result[x, y] = a[x, y] || b[x, y];
            }

            _lastA = a;
            _lastB = b;
            _lastResult = result;

            return NodeOutput.Success(result);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            int tw = Mathf.Max(16, width);
            int th = Mathf.Max(16, height);
            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (_lastResult == null)
            {
                DrawNoDataPattern(tex, tw, th);
                return tex;
            }

            int sw = _lastResult.GetLength(0);
            int sh = _lastResult.GetLength(1);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    bool a = _lastA != null && _lastA[sx, sy];
                    bool b = _lastB != null && _lastB[sx, sy];
                    bool both = _lastResult[sx, sy];

                    Color color;
                    if (both)
                        color = new Color(0.22f, 0.90f, 0.34f, 1f);
                    else if (a)
                        color = new Color(0.12f, 0.42f, 0.95f, 1f);
                    else if (b)
                        color = new Color(0.95f, 0.66f, 0.16f, 1f);
                    else
                        color = new Color(0.08f, 0.09f, 0.12f, 1f);

                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        private static void DrawNoDataPattern(Texture2D tex, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool stripe = ((x / 8) + (y / 8)) % 2 == 0;
                tex.SetPixel(x, y, stripe
                    ? new Color(0.12f, 0.14f, 0.20f, 1f)
                    : new Color(0.22f, 0.18f, 0.12f, 1f));
            }

            tex.Apply(false, false);
        }
    }
}
