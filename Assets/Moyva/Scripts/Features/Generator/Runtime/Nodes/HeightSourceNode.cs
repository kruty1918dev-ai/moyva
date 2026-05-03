using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    /// <summary>
    /// Стартова нода графа генерації: створює карту висот з noise settings.
    /// Реалізує ISeedProvider для безпечного доступу до seed без рефлексії.
    /// </summary>
    [NodeInfo("Height Source", "Generators", "Генерує початкову карту висот із noise settings. Це стартова точка більшості графів генерації, від якої далі будуються біоми, вода, річки, ліси та інші шари світу.")]
    public sealed class HeightSourceNode : NodeBase
    {
       
        public override string Title => "Height Source";
        public override string Category => "Generators";

        private NoiseSettings noiseSettings;

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<int>("Seed"),
            PortDefinition.Input<NoiseSettings>("NoiseSettings")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs[1] == null)
                return NodeOutput.Error("NoiseSettings not assigned.");

            noiseSettings = (NoiseSettings)inputs[1];
            var noiseProvider = context.GetService<INoiseProvider>();
            var heightMap = noiseProvider.GenerateNoiseMap(noiseSettings, context.MapSize.x, context.MapSize.y, (int)inputs[0]);

            return NodeOutput.Success(heightMap);
        }
    }
}
