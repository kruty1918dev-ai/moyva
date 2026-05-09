using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Base Noise Settings", "Generators", "Базові налаштування для генерації шуму.")]
    [HidePreview]
    public sealed class BaseNoiseSettings : NodeBase
    {
        
        [SerializeField, Min(0.0001f)]
        [InlineEditable("scale")]
        [Tooltip("Масштаб шуму. Визначає, наскільки 'розтягнутий' або 'стиснутий' буде шум. Великі значення — плавні області, малі — дрібні деталі. Приклад: 50 — великі континенти, 5 — дрібні острови.")]
        private float scale = 20f;

        [SerializeField, Range(1, 12)]
        [InlineEditable("octaves")]
        [Tooltip("Кількість октав. Визначає, скільки шарів шуму буде накладено. 1 — гладко, 8 — багато деталей. Приклад: 4 — баланс між деталізацією та продуктивністю.")]
        private int octaves = 4;

        [SerializeField, Range(0.01f, 1f)]
        [InlineEditable("persistance")]
        [Tooltip("Persistance — як швидко зменшується амплітуда шуму для кожної октави. 0.3 — плавно, 0.8 — багато дрібних деталей. Приклад: 0.5 — природний рельєф.")]
        private float persistance = 0.5f;

        [SerializeField, Min(1f)]
        [InlineEditable("lacunarity")]
        [Tooltip("Lacunarity — як швидко зростає частота шуму для кожної октави. 2 — типовий для природних карт, 3+ — дуже 'шумно'. Приклад: 2 — класика для перлинного шуму.")]
        private float lacunarity = 2f;

        [SerializeField]
        [InlineEditable("offset")]
        [Tooltip("Offset — зсув карти шуму по X та Y. Дозволяє зміщувати карту без зміни інших параметрів. Приклад: (100, 200) — карта зміщена праворуч і вгору.")]
        private Vector2 Offset = Vector2.zero;

        private NoiseSettings noiseSettings;

        public override string Title => "BaseNoiseSettings";

        public override string Category => "Generators";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<NoiseSettings>("NoiseSettings")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            noiseSettings = new NoiseSettings(scale, octaves, persistance, lacunarity, Offset); 
            return NodeOutput.Success(noiseSettings);
        }
    }

    public struct NoiseSettings
    {
        public float scale;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public Vector2 offset;

        public NoiseSettings(float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            this.scale = scale;
            this.octaves = octaves;
            this.persistance = persistance;
            this.lacunarity = lacunarity;
            this.offset = offset;
        }
    }
}
