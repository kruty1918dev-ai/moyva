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
        [SerializeField] private DataNoiseSettings _noiseSettings;
       
        public override string Title => "Height Source";
        public override string Category => "Generators";

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
            int seed = ResolveSeed(inputs, context);
            var noiseProvider = context.GetService<INoiseProvider>();

            if (inputs.Length > 1 && inputs[1] is NoiseSettings inputNoiseSettings)
            {
                var heightMap = noiseProvider.GenerateNoiseMap(inputNoiseSettings, context.MapSize.x, context.MapSize.y, seed);
                return NodeOutput.Success(heightMap);
            }

            if (_noiseSettings == null)
                return NodeOutput.Error("NoiseSettings not assigned.");

            var legacyNoiseSettings = new NoiseSettings(
                Mathf.Max(0.0001f, _noiseSettings.Scale),
                Mathf.Max(1, _noiseSettings.Octaves),
                _noiseSettings.Persistance,
                Mathf.Max(1f, _noiseSettings.Lacunarity),
                _noiseSettings.Offset);

            var legacyHeightMap = noiseProvider.GenerateNoiseMap(legacyNoiseSettings, context.MapSize.x, context.MapSize.y, seed);

            return NodeOutput.Success(legacyHeightMap);
        }

        private int ResolveSeed(object[] inputs, NodeContext context)
        {
            if (inputs.Length > 0 && inputs[0] is int inputSeed)
                return inputSeed;

            return context.Seed;
        }
    }
}
