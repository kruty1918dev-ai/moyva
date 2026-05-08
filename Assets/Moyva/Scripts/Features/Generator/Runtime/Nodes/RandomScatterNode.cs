using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Random Scatter", "Features", "Розкидає один тип об'єкта по карті випадковим чином. Підходить для швидкого тестового наповнення, простих ресурсів або шумового декору без складних правил.")]
    public sealed class RandomScatterNode : NodeBase
    {
        [Header("Scatter Settings")]
        [Tooltip("Щільність заповнення клітинок об'єктом. 0 означає майже повну відсутність спавну, 1 — спробу поставити об'єкт у кожну доступну клітинку.")]
        [SerializeField, Range(0f, 1f)] private float _density = 0.1f;
        [Tooltip("ID об'єкта, який буде розкидатися по карті. Має бути присутній у реєстрі об'єктів, інакше генератор не зможе його візуалізувати.")]
        [SerializeField, MapObjectId] private string _objectId = "tree";

        public override string Title => "Random Scatter";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Mask"),
            PortDefinition.Input<int>("MapWidth"),
            PortDefinition.Input<int>("MapHeight")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var mask = inputs[0] as bool[,];
            int width = 0, height = 0;

            if (mask != null)
            {
                width = mask.GetLength(0);
                height = mask.GetLength(1);
            }
            else
            {
                if (inputs[1] is int w) width = w;
                if (inputs[2] is int h) height = h;
            }

            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Cannot determine map size. Provide Mask or MapWidth+MapHeight.");

            var rng = context.CreateRandom();
            var result = new string[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mask != null && !mask[x, y]) continue;

                    if (rng.NextDouble() < _density)
                        result[x, y] = _objectId;
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
