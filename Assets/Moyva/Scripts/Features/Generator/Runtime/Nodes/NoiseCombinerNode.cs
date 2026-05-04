using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum CombineMode
    {
        Add,
        Multiply,
        Max,
        Min,
        Lerp
    }

    [NodeInfo("Noise Combiner", "Noise", "Комбінує дві карти шуму або висот в одну. Дозволяє нашаровувати великі форми й дрібні деталі, змішувати континенти з локальними особливостями або готувати складніші маски.")]
    public sealed class NoiseCombinerNode : NodeBase
    {
        [Header("Combine Settings")]
        [Tooltip("Режим комбінування двох вхідних карт. Визначає, чи карти додаються, перемножуються, змішуються лінійно або порівнюються через мінімум/максимум.")]
        [SerializeField] private CombineMode _mode = CombineMode.Add;
        [Tooltip("Коефіцієнт змішування для режиму Lerp. 0 залишає лише першу карту, 1 повністю переходить на другу.")]
        [SerializeField, Range(0f, 1f)] private float _blend = 0.5f;

        public override string Title => "Noise Combiner";
        public override string Category => "Noise";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<object>("HeightMap A"),
            PortDefinition.Input<object>("HeightMap B")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Combined")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (!TryGetFloatMap(inputs[0], out var a))
                return NodeOutput.Error("HeightMap A input is required.");

            if (!TryGetFloatMap(inputs[1], out var b))
                return NodeOutput.Warning("HeightMap B is not connected. Returning HeightMap A unchanged.", (float[,])a.Clone());

            int w = a.GetLength(0);
            int h = a.GetLength(1);
            int bw = b.GetLength(0);
            int bh = b.GetLength(1);
            var result = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float va = a[x, y];
                    float vb = (x < bw && y < bh) ? b[x, y] : 0f;

                    result[x, y] = _mode switch
                    {
                        CombineMode.Add => Mathf.Clamp01(va + vb),
                        CombineMode.Multiply => va * vb,
                        CombineMode.Max => Mathf.Max(va, vb),
                        CombineMode.Min => Mathf.Min(va, vb),
                        CombineMode.Lerp => Mathf.Lerp(va, vb, _blend),
                        _ => va
                    };
                }
            }

            return NodeOutput.Success(result);
        }

        private static bool TryGetFloatMap(object input, out float[,] map)
        {
            map = null;
            if (input is float[,] floatMap)
            {
                map = floatMap;
                return true;
            }

            if (input is int[,] intMap)
            {
                map = ConvertIntMapToNormalizedFloat(intMap);
                return true;
            }

            return false;
        }

        private static float[,] ConvertIntMapToNormalizedFloat(int[,] source)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var result = new float[width, height];

            int maxValue = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = source[x, y];
                    if (value > maxValue)
                        maxValue = value;
                }
            }

            if (maxValue <= 0)
                return result;

            float invMax = 1f / maxValue;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = Mathf.Clamp01(source[x, y] * invMax);
                }
            }

            return result;
        }
    }
}
