using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [Serializable]
    public sealed class FlagObjectRule
    {
        [Tooltip("Який flag/tile ID шукати в TileMap після WFC. Наприклад: flag:road, flag:river-bend.")]
        public string FlagTileId;

        [Tooltip("Який Object ID записати в ObjectMap, якщо flag знайдено.")]
        [MapObjectId] public string ObjectId;

        [Tooltip("Яким звичайним Tile ID замінити flag у TileMap після переносу в ObjectMap.")]
        [TileId] public string ReplaceTileId;
    }

    [NodeInfo("Resolve Flag Objects", "Flags", "Шукає в TileMap flag ids після WFC, переносить їх у ObjectMap і за потреби замінює підкладку на звичайний Tile ID. Це міст між віртуальними прапорцями і справжніми ігровими об'єктами.")]
    public sealed class ResolveFlagObjectsNode : NodeBase
    {
        [Header("Resolve Rules")]
        [SerializeField] private FlagObjectRule[] _rules;

        [Tooltip("Якщо вимкнено, існуючі Object ID не будуть перезаписані.")]
        [SerializeField] private bool _overwriteExistingObjects;

        public override string Title => "Resolve Flag Objects";
        public override string Category => "Flags";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<string[,]>("ObjectMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap"),
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs.Length > 0 ? inputs[0] as string[,] : null;
            var objectMap = inputs.Length > 1 ? inputs[1] as string[,] : null;

            if (tileMap == null)
                return NodeOutput.Error("TileMap input is required.");

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);
            if (objectMap != null && (objectMap.GetLength(0) != w || objectMap.GetLength(1) != h))
                return NodeOutput.Error("TileMap and ObjectMap must have the same size.");

            var tileResult = MapArrayUtils.CloneStringMap(tileMap);
            var objectResult = MapArrayUtils.CloneStringMapOrCreate(objectMap, w, h);

            var lookup = new Dictionary<string, FlagObjectRule>(StringComparer.OrdinalIgnoreCase);
            if (_rules != null)
            {
                foreach (var rule in _rules)
                {
                    if (rule == null || string.IsNullOrWhiteSpace(rule.FlagTileId))
                        continue;

                    lookup[rule.FlagTileId.Trim()] = rule;
                }
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string currentTile = tileResult[x, y];
                    if (string.IsNullOrWhiteSpace(currentTile))
                        continue;

                    if (!lookup.TryGetValue(currentTile.Trim(), out var rule))
                        continue;

                    if (_overwriteExistingObjects || string.IsNullOrEmpty(objectResult[x, y]))
                        objectResult[x, y] = rule.ObjectId;

                    if (!string.IsNullOrWhiteSpace(rule.ReplaceTileId))
                        tileResult[x, y] = rule.ReplaceTileId.Trim();
                }
            }

            return NodeOutput.Success(tileResult, objectResult);
        }
    }
}