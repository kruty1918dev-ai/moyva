using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Height Source", "Generators")]
    public sealed class HeightSourceNode : NodeBase
    {
        [Header("Noise Settings")]
        [SerializeField] private DataNoiseSettings _noiseSettings;

        public override string Title => "Height Source";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (_noiseSettings == null)
                return NodeOutput.Error("DataNoiseSettings not assigned.");

            var noiseProvider = context.GetService<INoiseProvider>();
            var heightMap = noiseProvider.GenerateNoiseMap(
                _noiseSettings, context.MapSize.x, context.MapSize.y);

            return NodeOutput.Success(heightMap);
        }
    }
}
