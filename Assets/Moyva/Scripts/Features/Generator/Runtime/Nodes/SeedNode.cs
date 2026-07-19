using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.API;
using System;
using UnityEngine;


namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Seed",
        "Core",
        "Задає базовий seed графа. Фактичний seed передається через NodeContext і не змінює глобальний random state.",
        StableId = "moyva.core.seed",
        Order = 0,
        Lifecycle = NodeLifecycle.Hidden)]
    [HidePreview]
    [UniqueNode]
    [StaticGraphNode]
    public sealed class SeedNode : NodeBase, ISeedProvider
    {       
        [SerializeField]
        [InlineEditable("сід")]
        private int seed = GlobalSeed.DefaultSeed;

        public int Seed => seed;

        public override string Title => "Seed";
        public override string Category => "Core";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success();
        }
    }
}
