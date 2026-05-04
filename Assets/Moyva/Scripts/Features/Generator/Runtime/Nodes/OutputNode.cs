using System;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Output", "Core", "Фінальна нода графа, яка збирає підсумкові карти біомів, об'єктів, висот і будівель у єдиний результат. Без неї граф не поверне готовий набір даних для побудови світу.")]
    public sealed class OutputNode : NodeBase
    {
        public override string Title => "Output";
        public override string Category => "Core";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<string[,]>("ObjectMap"),
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BuildingMap (optional)")
        };

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            // Усі входи необов'язкові: якщо BiomeMap не підключено — повертаємо
            // порожні мапи, щоб граф міг працювати лише через side-effects (шари тощо).
            var biomeMap = inputs[0] as string[,];

            int w = biomeMap?.GetLength(0) ?? context.MapSize.x;
            int h = biomeMap?.GetLength(1) ?? context.MapSize.y;

            biomeMap ??= new string[w, h];
            var objectMap = inputs[1] as string[,] ?? new string[w, h];
            var heightMap = inputs[2] as float[,] ?? new float[w, h];
            var buildingMap = inputs[3] as string[,] ?? new string[w, h];

            return NodeOutput.Success(biomeMap, objectMap, heightMap, buildingMap);
        }
    }
}
