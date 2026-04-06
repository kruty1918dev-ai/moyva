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

    [NodeInfo("Noise Combiner", "Processing", "Комбінує дві карти шуму або висот в одну. Дозволяє нашаровувати великі форми й дрібні деталі, змішувати континенти з локальними особливостями або готувати складніші маски.")]
    public sealed class NoiseCombinerNode : NodeBase
    {
        [Header("Combine Settings")]
        [Tooltip("Режим комбінування двох вхідних карт. Визначає, чи карти додаються, перемножуються, змішуються лінійно або порівнюються через мінімум/максимум.")]
        [SerializeField] private CombineMode _mode = CombineMode.Add;
        [Tooltip("Коефіцієнт змішування для режиму Lerp. 0 залишає лише першу карту, 1 повністю переходить на другу.")]
        [SerializeField, Range(0f, 1f)] private float _blend = 0.5f;

        public override string Title => "Noise Combiner";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap A"),
            PortDefinition.Input<float[,]>("HeightMap B")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Combined")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var a = inputs[0] as float[,];
            var b = inputs[1] as float[,];
            if (a == null)
                return NodeOutput.Error("HeightMap A input is required.");
            if (b == null)
                return NodeOutput.Error("HeightMap B input is required.");

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
    }
}
