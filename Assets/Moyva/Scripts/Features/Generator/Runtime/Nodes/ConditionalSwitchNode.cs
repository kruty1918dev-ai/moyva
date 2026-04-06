using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Conditional Switch", "Logic", "Покомірково вибирає значення з однієї з двох карт за булевою маскою. Це базовий логічний вузол для змішування альтернативних варіантів рельєфу або шуму.")]
    public sealed class ConditionalSwitchNode : NodeBase
    {
        public override string Title => "Conditional Switch";
        public override string Category => "Logic";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("A"),
            PortDefinition.Input<float[,]>("B"),
            PortDefinition.Input<bool[,]>("Condition")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Result")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var a = inputs[0] as float[,];
            var b = inputs[1] as float[,];
            var condition = inputs[2] as bool[,];

            if (a == null)
                return NodeOutput.Error("Input A is required.");
            if (b == null)
                return NodeOutput.Error("Input B is required.");
            if (condition == null)
                return NodeOutput.Error("Condition mask is required.");

            int w = a.GetLength(0);
            int h = a.GetLength(1);
            int bw = b.GetLength(0);
            int bh = b.GetLength(1);
            var result = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool cond = x < condition.GetLength(0) && y < condition.GetLength(1)
                                && condition[x, y];
                    float aVal = a[x, y];
                    float bVal = (x < bw && y < bh) ? b[x, y] : 0f;
                    result[x, y] = cond ? aVal : bVal;
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
