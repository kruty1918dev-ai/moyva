using System;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum NoiseMapAlgorithm
    {
        Perlin,
        Value
    }

    [NodeInfo(
        "Noise Map",
        "Generators",
        "Створює детерміновану карту шуму точного розміру контексту для висот, масок і природних форм.",
        StableId = "moyva.generators.noise-map",
        Order = 10,
        PreviewOutput = "out.map")]
    public sealed class NoiseMapNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Алгоритм базового шуму.")]
        private NoiseMapAlgorithm _algorithm = NoiseMapAlgorithm.Perlin;

        [SerializeField, Min(0.0001f)]
        [Tooltip("Розмір характерних деталей у клітинах.")]
        private float _scale = 20f;

        [SerializeField, Range(1, 12)]
        [Tooltip("Кількість октав фрактального шуму.")]
        private int _octaves = 4;

        [SerializeField, Range(0.01f, 1f)]
        [Tooltip("Частка амплітуди кожної наступної октави.")]
        private float _persistence = 0.5f;

        [SerializeField, Min(1f)]
        [Tooltip("Множник частоти кожної наступної октави.")]
        private float _lacunarity = 2f;

        [SerializeField]
        [Tooltip("Просторовий зсув вибірки шуму.")]
        private Vector2 _offset;

        public override string Title => "Noise Map";
        public override string Category => "Generators";
        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Map", "out.map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            MapNodeUtility.ResolveSize(context, out int width, out int height);
            int seed = MapNodeUtility.ResolveSeed(context, NodeId);
            float seedX = ProceduralNoiseUtility.Hash01(seed, 17, seed) * 8192f;
            float seedY = ProceduralNoiseUtility.Hash01(31, seed, seed ^ 0x51ed270b) * 8192f;
            float scale = Mathf.Max(0.0001f, _scale);
            var result = new float[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float sampleX = (x + _offset.x) / scale + seedX;
                float sampleY = (y + _offset.y) / scale + seedY;
                result[x, y] = ProceduralNoiseUtility.SampleFbm(
                    sampleX,
                    sampleY,
                    Mathf.Max(1, _octaves),
                    Mathf.Max(1f, _lacunarity),
                    Mathf.Clamp01(_persistence),
                    seed,
                    _algorithm == NoiseMapAlgorithm.Value);
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
