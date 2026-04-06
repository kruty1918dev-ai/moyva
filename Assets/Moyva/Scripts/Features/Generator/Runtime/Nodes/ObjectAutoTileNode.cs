using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Object AutoTile", "Objects", "Підбирає варіант об'єкта за сусідами, наприклад для річок, доріг або інших лінійних структур. Нода перетворює базові ID в кути, прямі сегменти, хрести та трійники за таблицею правил.")]
    public sealed class ObjectAutoTileNode : NodeBase
    {
        [Tooltip("Набір правил, який описує відповідність між маскою сусідів та конкретними варіантами об'єкта. Без цього asset нода просто поверне вхідну ObjectMap без змін.")]
        [SerializeField] private ObjectConnectionRulesSO _rules;

        [Header("Exclusions")]
        [Tooltip("Префікси Object ID, які треба пропустити при автотайлінгу (наприклад river-outlet). Об'єкти з такими префіксами залишаться без змін.")]
        [SerializeField] private string[] _excludeObjectPrefixes;

        [Tooltip("Роздільник для базового типу Object ID.")]
        [SerializeField] private char _separator = '-';

        public override string Title => "Object AutoTile";
        public override string Category => "Objects";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("ObjectMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        // N=1, E=2, S=4, W=8
        private static readonly int[] DX = { 0, 1, 0, -1 };
        private static readonly int[] DY = { 1, 0, -1, 0 };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var objectMap = inputs[0] as string[,];
            if (objectMap == null)
                return NodeOutput.Error("ObjectMap input is required.");

            if (_rules == null)
                return NodeOutput.Success(objectMap);

            var excludeSet = BuildExcludeSet();
            int w = objectMap.GetLength(0);
            int h = objectMap.GetLength(1);
            var result = new string[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string objId = objectMap[x, y];
                    if (string.IsNullOrEmpty(objId))
                    {
                        result[x, y] = objId;
                        continue;
                    }

                    if (IsExcluded(objId, excludeSet))
                    {
                        result[x, y] = objId;
                        continue;
                    }

                    string baseId = GetBaseId(objId);
                    int mask = 0;
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + DX[d];
                        int ny = y + DY[d];

                        if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        {
                            string neighbor = objectMap[nx, ny];
                            if (!string.IsNullOrEmpty(neighbor) && GetBaseId(neighbor) == baseId)
                                mask |= (1 << d);
                        }
                    }

                    if (_rules.TryResolve(baseId, mask, out string variantId))
                        result[x, y] = variantId;
                    else
                        result[x, y] = objId;
                }
            }

            return NodeOutput.Success(result);
        }

        private HashSet<string> BuildExcludeSet()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_excludeObjectPrefixes != null)
            {
                foreach (var prefix in _excludeObjectPrefixes)
                {
                    if (!string.IsNullOrWhiteSpace(prefix))
                        set.Add(prefix.Trim());
                }
            }
            return set;
        }

        private bool IsExcluded(string objectId, HashSet<string> excludeSet)
        {
            if (excludeSet.Count == 0) return false;

            foreach (var prefix in excludeSet)
            {
                if (objectId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private string GetBaseId(string id)
        {
            int sep = id.IndexOf(_separator);
            return sep >= 0 ? id.Substring(0, sep) : id;
        }
    }
}
