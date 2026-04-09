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
    public sealed class HeightSourceNode : NodeBase, ISeedProvider
    {
        [Header("Noise Settings")]
        [Tooltip("Налаштування шуму, з якого буде побудована карта висот. Від цього asset залежить загальний характер рельєфу: великі форми, деталізація, seed і масштаб.")]
        [SerializeField] private DataNoiseSettings _noiseSettings;

        public override string Title => "Height Source";
        public override string Category => "Generators";

        /// <summary>Seed генерації з поточних noise settings (0 якщо settings відсутні).</summary>
        public int Seed => _noiseSettings != null ? _noiseSettings.Seed : 0;

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
