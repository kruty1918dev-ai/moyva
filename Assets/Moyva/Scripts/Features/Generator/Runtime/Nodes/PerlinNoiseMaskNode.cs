using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Perlin Noise Mask",
        "Generators",
        "Створює булеву маску на основі перлінового шуму. Поріг визначає, які клітини беруть участь у результаті.",
        StableId = "moyva.generators.perlin-noise-mask",
        Order = 20,
        PreviewOutput = "out.mask")]
    public sealed class PerlinNoiseMaskNode : NodeBase
    {
        [SerializeField, Min(0.0001f)]
        [InlineEditable("масштаб")]
        [Tooltip("Масштаб шуму. Великі значення — плавні області, малі — дрібні деталі. Приклад: 50 — великі континенти, 5 — дрібні острови.")]
        private float _scale = 20f;

        [SerializeField, Range(1, 12)]
        [InlineEditable("октави")]
        [Tooltip("Кількість октав. Визначає, скільки шарів шуму буде накладено. 1 — гладко, 8 — багато деталей. Приклад: 4 — баланс між деталізацією та продуктивністю.")]
        private int _octaves = 4;

        [SerializeField, Range(0.01f, 1f)]
        [InlineEditable("амплітуда")]
        [Tooltip("Як швидко зменшується амплітуда шуму для кожної октави. 0.3 — плавно, 0.8 — багато дрібних деталей. Приклад: 0.5 — природний рельєф.")]
        private float _persistence = 0.5f;

        [SerializeField, Min(1f)]
        [InlineEditable("частота")]
        [Tooltip("Як швидко зростає частота шуму для кожної октави. 2 — типовий для природних карт, 3+ — дуже 'шумно'. Приклад: 2 — класика для перлинного шуму.")]
        private float _lacunarity = 2f;

        [SerializeField]
        [InlineEditable("зсув")]
        [Tooltip("Зсув карти шуму по X та Y. Дозволяє зміщувати карту без зміни інших параметрів. Приклад: (100, 200) — карта зміщена праворуч і вгору.")]
        private Vector2 _offset = Vector2.zero;

        [SerializeField, Range(0f, 1f)]
        [InlineEditable("поріг")]
        [Tooltip("Поріг для перетворення значення шуму у булеву маску. 0.5 — середнє значення. Нижче порігу буде тайл, выше — ні. Тести: 0.3 — більше та, 0.7 — менше та.")]
        private float _threshold = 0.5f;

        public override string Title => "Perlin Noise Mask";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask"),
            PortDefinition.Output<float[,]>("Noise", "out.noise")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int w = Mathf.Max(1, context?.MapSize.x ?? 0);
            int h = Mathf.Max(1, context?.MapSize.y ?? 0);

            var noiseValues = new float[w, h];
            var mask = new bool[w, h];

            int seed = GlobalSeed.Combine(
                context?.Seed ?? GlobalSeed.DefaultSeed,
                GlobalSeed.StableHash(NodeId));
            float seedX = ProceduralNoiseUtility.Hash01(seed, 17, seed) * 8192f;
            float seedY = ProceduralNoiseUtility.Hash01(31, seed, seed ^ 0x51ed270b) * 8192f;
            float scale = Mathf.Max(0.0001f, _scale);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + _offset.x) / scale + seedX;
                    float ny = (y + _offset.y) / scale + seedY;

                    float noise = ProceduralNoiseUtility.SampleFbm(
                        nx, ny,
                        _octaves, _lacunarity, _persistence,
                        seed, false);

                    noiseValues[x, y] = noise;
                    mask[x, y] = noise >= _threshold;
                }
            }

            context?.CountIteration(w * h);
            return NodeOutput.Success(mask, noiseValues);
        }
    }
}
