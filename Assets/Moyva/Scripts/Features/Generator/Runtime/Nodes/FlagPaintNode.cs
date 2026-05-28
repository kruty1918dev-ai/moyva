using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Paint Flags", "Flags", "Записує віртуальний flag ID у клітинки за bool або float маскою. Це зручно для побудови проміжної FlagMap, яку потім можна накласти на TileMap перед WFC.")]
    public sealed class FlagPaintNode : NodeBase
    {
        [Header("Flag Settings")]
        [Tooltip("ID прапорця, який буде записано в FlagMap. Рекомендований формат: flag:road, flag:river, flag:wall.")]
        [SerializeField] private string _flagId = "flag:line";

        [Tooltip("Поріг для float маски. Клітинки зі значенням >= порога отримають прапорець.")]
        [SerializeField, Range(0f, 1f)] private float _threshold = 0.5f;

        [Tooltip("Якщо вимкнено, існуючий flag у клітинці не буде перезаписано.")]
        [SerializeField] private bool _overwriteExisting = true;

        public override string Title => "Paint Flags";
        public override string Category => "Flags";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BaseFlagMap (optional)"),
            PortDefinition.Input<bool[,]>("BoolMask (optional)"),
            PortDefinition.Input<float[,]>("FloatMask (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("FlagMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var baseMap = inputs.Length > 0 ? inputs[0] as string[,] : null;
            var boolMask = inputs.Length > 1 ? inputs[1] as bool[,] : null;
            var floatMask = inputs.Length > 2 ? inputs[2] as float[,] : null;

            if (baseMap == null && boolMask == null && floatMask == null)
                return NodeOutput.Error("At least one of BaseFlagMap, BoolMask or FloatMask is required.");

            ResolveSize(baseMap, boolMask, floatMask, out int w, out int h);
            if (!ValidateSize(baseMap, boolMask, floatMask, w, h))
                return NodeOutput.Error("All inputs must have the same size.");

            var result = MapArrayUtils.CloneStringMapOrCreate(baseMap, w, h);
            float threshold = Mathf.Clamp01(_threshold);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool active = (boolMask != null && boolMask[x, y])
                        || (floatMask != null && floatMask[x, y] >= threshold);

                    if (!active)
                        continue;

                    if (!_overwriteExisting && !string.IsNullOrEmpty(result[x, y]))
                        continue;

                    result[x, y] = _flagId;
                }
            }

            return NodeOutput.Success(result);
        }

        private static void ResolveSize(string[,] baseMap, bool[,] boolMask, float[,] floatMask, out int w, out int h)
        {
            if (baseMap != null)
            {
                w = baseMap.GetLength(0);
                h = baseMap.GetLength(1);
                return;
            }

            if (boolMask != null)
            {
                w = boolMask.GetLength(0);
                h = boolMask.GetLength(1);
                return;
            }

            w = floatMask.GetLength(0);
            h = floatMask.GetLength(1);
        }

        private static bool ValidateSize(string[,] baseMap, bool[,] boolMask, float[,] floatMask, int w, int h)
        {
            if (baseMap != null && (baseMap.GetLength(0) != w || baseMap.GetLength(1) != h)) return false;
            if (boolMask != null && (boolMask.GetLength(0) != w || boolMask.GetLength(1) != h)) return false;
            if (floatMask != null && (floatMask.GetLength(0) != w || floatMask.GetLength(1) != h)) return false;
            return true;
        }
    }
}