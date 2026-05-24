using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Tile Replace", "Utility", "Точково замінює один Tile ID на інший по всій карті або лише в межах маски. Зручна для локальних правок після генерації, наприклад перетворення трави на пісок чи болото.")]
    public sealed class TileReplaceNode : NodeBase
    {
        [Header("Replace Settings")]
        [Tooltip("Tile ID, який потрібно знайти у вхідній карті. Лише клітинки з повним збігом цього ID будуть замінені.")]
        [SerializeField] private string _findTile = "";
        [Tooltip("Tile ID, який буде записано замість знайденого значення. Дозволяє масово перефарбувати потрібну категорію клітинок.")]
        [SerializeField] private string _replaceTile = "";

        public override string Title => "Tile Replace";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("Result")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs[0] as string[,];
            if (tileMap == null)
                return NodeOutput.Error("TileMap input is required.");

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);
            var result = MapArrayUtils.CloneStringMap(tileMap);
            var mask = inputs[1] as bool[,];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (mask != null && !mask[x, y]) continue;
                    if (result[x, y] == _findTile)
                        result[x, y] = _replaceTile;
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
