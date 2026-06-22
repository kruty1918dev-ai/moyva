using System;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum LayerOutputKind
    {
        Tiles,
        Objects,
        Masks,
        InternalData,
        Other
    }

    [NodeInfo("Output", "Core", "Фінальна нода шару, яка явно позначає результат: tiles, objects, masks, internal data або other. Кожен активний шар має мати один підключений Output Node.")]
    public sealed class OutputNode : NodeBase
    {
        public const int BiomeMapInputIndex = 0;
        public const int ObjectMapInputIndex = 1;
        public const int HeightMapInputIndex = 2;
        public const int BuildingMapInputIndex = 3;
        public const int MaskInputIndex = 4;
        public const int DataInputIndex = 5;

        [UnityEngine.SerializeField, InlineEditable("Output Kind")]
        private LayerOutputKind _outputKind = LayerOutputKind.Tiles;

        public override string Title => "Output";
        public override string Category => "Core";
        public LayerOutputKind OutputKind
        {
            get => _outputKind;
            set => _outputKind = value;
        }

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<string[,]>("ObjectMap"),
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BuildingMap (optional)"),
            PortDefinition.Input<bool[,]>("Mask"),
            PortDefinition.Input<object>("Data (optional)")
        };

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            // Перші 4 значення залишаються legacy map outputs для SubgraphNode/Menu preview.
            var biomeMap = GetInput<string[,]>(inputs, BiomeMapInputIndex);
            var objectMap = GetInput<string[,]>(inputs, ObjectMapInputIndex);
            var heightMap = GetInput<float[,]>(inputs, HeightMapInputIndex);
            var buildingMap = GetInput<string[,]>(inputs, BuildingMapInputIndex);
            var mask = GetInput<bool[,]>(inputs, MaskInputIndex);
            var data = GetInput<object>(inputs, DataInputIndex);

            ResolveSize(context, new object[] { biomeMap, objectMap, heightMap, buildingMap, mask }, out int w, out int h);

            biomeMap ??= new string[w, h];
            objectMap ??= new string[w, h];
            heightMap ??= new float[w, h];
            buildingMap ??= new string[w, h];

            return NodeOutput.Success(biomeMap, objectMap, heightMap, buildingMap, mask, data, _outputKind);
        }

        private static T GetInput<T>(object[] inputs, int index) where T : class
        {
            if (inputs == null || index < 0 || index >= inputs.Length)
                return null;

            return inputs[index] as T;
        }

        private static void ResolveSize(NodeContext context, object[] maps, out int width, out int height)
        {
            width = Math.Max(1, context?.MapSize.x ?? 0);
            height = Math.Max(1, context?.MapSize.y ?? 0);

            if (maps == null)
                return;

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i] is not Array array || array.Rank != 2)
                    continue;

                width = Math.Max(1, array.GetLength(0));
                height = Math.Max(1, array.GetLength(1));
                return;
            }
        }
    }
}
