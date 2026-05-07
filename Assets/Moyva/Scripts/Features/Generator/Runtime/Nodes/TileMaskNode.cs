using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Tile Mask", "Utility",
        "Будує булеву маску з TileMap: клітинка отримує true, якщо її Tile ID входить до вибраного набору. " +
        "Підтримує порівняння по повному ID або по базовому ключу (до першого '-'). " +
        "Вихід: bool[,] Mask — можна передавати у WaterContourGenerator, FlagPaint або інші ноди.")]
    public sealed class TileMaskNode : NodeBase, ICustomEditorNode
    {
        [Tooltip("Набір Tile ID, що позначають цільові клітинки. " +
                 "Клітинка стає true, якщо її тайл збігається з будь-яким із цих значень (або по базовому ключу).")]
        [TileId, SerializeField] private string[] _targetTileIds = Array.Empty<string>();

        [Tooltip("Якщо увімкнено — порівняння по базовому ключу (до '-'). " +
                 "Наприклад, 'water' співпадає з 'water-deep-001', 'water-shallow-002' тощо.")]
        [SerializeField] private bool _matchByBaseKey = false;

        public override string Title    => "Tile Mask";
        public override string Category => "Utility";

        // Read-only for editor
        public string[] TargetTileIds   => _targetTileIds;

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("TileMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs[0] as string[,];
            if (tileMap == null)
                return NodeOutput.Error("TileMap input is required.");

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);

            var matchSet = BuildMatchSet();
            var mask     = new bool[w, h];

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                mask[x, y] = IsMatch(tileMap[x, y], matchSet);

            return NodeOutput.Success(mask);
        }

        private HashSet<string> BuildMatchSet()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_targetTileIds == null) return set;
            foreach (var id in _targetTileIds)
                if (!string.IsNullOrWhiteSpace(id)) set.Add(id.Trim());
            return set;
        }

        private bool IsMatch(string tileId, HashSet<string> matchSet)
        {
            if (string.IsNullOrEmpty(tileId)) return false;
            tileId = tileId.Trim();

            foreach (var target in matchSet)
            {
                if (tileId.Equals(target, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (_matchByBaseKey)
                {
                    string baseKey = GetBaseKey(target);
                    if (tileId.Equals(baseKey, StringComparison.OrdinalIgnoreCase) ||
                        tileId.StartsWith(baseKey + "-", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private static string GetBaseKey(string tileId)
        {
            if (string.IsNullOrEmpty(tileId)) return "";
            int idx = tileId.IndexOf('-');
            return idx > 0 ? tileId.Substring(0, idx) : tileId;
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            UnityEditor.Selection.activeObject = this;
        }
#endif
    }
}
