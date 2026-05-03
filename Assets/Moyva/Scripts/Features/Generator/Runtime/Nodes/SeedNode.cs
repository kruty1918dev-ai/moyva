using Kruty1918.Moyva.GraphSystem.API;
using System;
using UnityEngine;


namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Seed Settings", "Generators", "Налаштування сіду")]
    [HidePreview]
    public sealed class SeedNode : NodeBase
    {       
        [SerializeField]
        [InlineEditable("seed")]
        private int seed = 0;
        
        [SerializeField]
        [InlineEditable("isRandomSeed")]
        private bool isRandomSeed = true;
        private static System.Random rng = new System.Random();
        public override string Title => "Seed Settings";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<int>("Seed")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (isRandomSeed)
            {
                seed = GetRandomSeed();
            }

            return NodeOutput.Success(seed);
        }

        private int GetRandomSeed()
        {
            return rng.Next(int.MinValue, int.MaxValue);
        }
    }
}
