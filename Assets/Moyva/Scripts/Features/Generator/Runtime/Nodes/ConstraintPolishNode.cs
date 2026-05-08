using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("WFC Constraint Polish", "WFC", "Полірує вже згенеровану карту за набором локальних WFC-правил. Це легший етап, ніж повний WFC, і він зручний для виправлення проблемних сусідств або приведення карти до потрібних шаблонів.")]
    public sealed class ConstraintPolishNode : NodeBase, ICustomEditorNode
    {
        [Header("WFC Settings")]
        [Tooltip("Набір правил сумісності тайлів, які будуть застосовані до карти під час полірування. Саме цей asset визначає, які сусідства вважаються правильними або помилковими.")]
        [SerializeField] private WFCDataSettings _wfcSettings;

        [Header("Flags")]
        [Tooltip("Якщо увімкнено, WFC-полірування застосовується лише до клітинок із FlagMap.")]
        [SerializeField] private bool _applyOnlyOnFlags;

        [Tooltip("Список flag ID, які дозволяють змінювати клітинку. Якщо список порожній — підходить будь-який непорожній flag.")]
        [SerializeField] private string[] _targetFlagIds;

        public override string Title => "WFC Constraint Polish";
        public override string Category => "WFC";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("FlagMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("PolishedMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            var heightMap = inputs[1] as float[,];
            var flagMap = inputs.Length > 2 ? inputs[2] as string[,] : null;
            if (biomeMap == null || heightMap == null)
                return NodeOutput.Error("BiomeMap and HeightMap inputs are required.");
            if (_wfcSettings == null)
                return NodeOutput.Error("WFCDataSettings not assigned.");
            if (_applyOnlyOnFlags && flagMap == null)
                return NodeOutput.Error("FlagMap input is required when flag-based mode is enabled.");

            int w = biomeMap.GetLength(0);
            int h = biomeMap.GetLength(1);
            if (heightMap.GetLength(0) != w || heightMap.GetLength(1) != h)
                return NodeOutput.Error("BiomeMap and HeightMap must have the same size.");
            if (flagMap != null && (flagMap.GetLength(0) != w || flagMap.GetLength(1) != h))
                return NodeOutput.Error("BiomeMap and FlagMap must have the same size.");

            var result = (string[,])biomeMap.Clone();
            var wfcService = new WFCService(_wfcSettings);
            wfcService.Apply(result, heightMap);

            if (_applyOnlyOnFlags)
            {
                var targetFlags = FlagMapSelectionUtility.BuildFilterSet(_targetFlagIds);
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (!FlagMapSelectionUtility.IsSelected(flagMap, x, y, targetFlags))
                            result[x, y] = biomeMap[x, y];
                    }
                }
            }

            return NodeOutput.Success(result);
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            if (_wfcSettings == null) return;
            var windowType = System.Type.GetType(
                "Kruty1918.Moyva.Generator.Editor.WFCRulesEditorWindow, Kruty1918.Moyva.Generator.Editor");
            if (windowType != null)
            {
                var method = windowType.GetMethod(
                    "OpenWindow",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                method?.Invoke(null, new object[] { _wfcSettings });
            }
        }
#endif
    }
}
