using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.API;
using System;
using UnityEngine;


namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Налаштування сіду", "Генерація", "Налаштування випадковості для генерації. Сід контролює результати всіх випадкових процесів у графі.")]
    [HidePreview]
    [UniqueNode]
    [StaticGraphNode]
    public sealed class SeedNode : NodeBase, ISeedProvider
    {       
        [SerializeField]
        [InlineEditable("сід")]
        private int seed = GlobalSeed.DefaultSeed;

        public int Seed => seed;

        public override string Title => "Налаштування сіду";
        public override string Category => "Генерація";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            GlobalSeed.Set(seed);
            return NodeOutput.Success();
        }
    }
}
