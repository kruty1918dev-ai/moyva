using System;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Маска краю", "Розміщення об'єктів", "Створює м’яку зважену смугу біля краю острова або маски рельєфу.")]
    public sealed class EdgeMaskNode : NodeBase, IPreviewableNode
    {
        [SerializeField, Min(0)]
        [InlineEditable("Відстань")]
        [Tooltip("Повна інтенсивність на відстані від краю маски, у клітинах.")]
        private int _distanceFromEdge = 2;

        [SerializeField, Min(0)]
        [InlineEditable("Спад")]
        [Tooltip("Додаткова м’яка відстань спаду, у клітинах.")]
        private int _falloff = 2;

        [SerializeField]
        [Tooltip("Виводить інтерір біля краю замість смуги краю.")]
        private bool _invert;

        [NonSerialized] private bool[,] _lastMask;
        [NonSerialized] private float[,] _lastWeights;

        public override string Title => "Маска краю";
        public override string Category => "Розміщення об'єктів";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Джерело")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Маска"),
            PortDefinition.Output<ScatterMask>("Маска розкиду")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not bool[,] source)
                return NodeOutput.Error("Маска джерела є обов'язковою.");

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var scatterMask = new ScatterMask(source);
            int[] distances = ObjectPlacementScatterUtility.BuildEdgeDistanceMap(scatterMask, out _);
            var mask = new bool[w, h];
            var weights = new float[w, h];
            int fullDistance = Mathf.Max(0, _distanceFromEdge);
            int falloff = Mathf.Max(0, _falloff);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!source[x, y])
                        continue;

                    int distance = distances[y * w + x];
                    float weight;
                    if (distance <= fullDistance)
                    {
                        weight = 1f;
                    }
                    else if (falloff > 0 && distance <= fullDistance + falloff)
                    {
                        weight = 1f - ((distance - fullDistance) / (float)falloff);
                    }
                    else
                    {
                        weight = 0f;
                    }

                    if (_invert)
                        weight = 1f - weight;

                    weights[x, y] = Mathf.Clamp01(weight);
                    mask[x, y] = weights[x, y] > 0.001f;
                }
            }

            _lastMask = mask;
            _lastWeights = weights;
            return NodeOutput.Success(mask, new ScatterMask(mask, null, weights));
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastMask == null)
                return null;

            int w = _lastMask.GetLength(0);
            int h = _lastMask.GetLength(1);
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float t = _lastWeights != null ? _lastWeights[x, y] : (_lastMask[x, y] ? 1f : 0f);
                    texture.SetPixel(x, y, Color.Lerp(
                        new Color(0.04f, 0.05f, 0.07f, 1f),
                        new Color(0.70f, 0.92f, 0.48f, 1f),
                        t));
                }
            }

            texture.Apply(false, false);
            return texture;
        }
    }
}
