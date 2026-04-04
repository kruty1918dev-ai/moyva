using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("WFC Constraint Polish", "WFC")]
    public sealed class ConstraintPolishNode : NodeBase, ICustomEditorNode
    {
        [Header("WFC Settings")]
        [SerializeField] private WFCDataSettings _wfcSettings;

        public override string Title => "WFC Constraint Polish";
        public override string Category => "WFC";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("PolishedMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            var heightMap = inputs[1] as float[,];
            if (biomeMap == null || heightMap == null)
                return NodeOutput.Error("BiomeMap and HeightMap inputs are required.");
            if (_wfcSettings == null)
                return NodeOutput.Error("WFCDataSettings not assigned.");

            var result = (string[,])biomeMap.Clone();
            var wfcService = new WFCService(_wfcSettings);
            wfcService.Apply(result, heightMap);

            return NodeOutput.Success(result);
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            if (_wfcSettings != null)
                Editor.WFCRulesEditorWindow.OpenWindow(_wfcSettings);
        }
#endif
    }
}
