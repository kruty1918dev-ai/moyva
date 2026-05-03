using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.API;
using System;
using UnityEngine;


namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Seed Settings", "Generators", "Налаштування сіду")]
    [HidePreview]
    [UniqueNode]
    public sealed class SeedNode : NodeBase, ISeedProvider
    {       
        [SerializeField]
        [InlineEditable("Seed")]
        private int seed = GlobalSeed.DefaultSeed;

        public int Seed => seed;

        public override string Title => "Seed Settings";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<int>("Seed")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            GlobalSeed.Set(seed);
            return NodeOutput.Success(seed);
        }
    }
}
