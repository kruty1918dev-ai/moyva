using System;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Шум Пerlін Mask",
        "Генерація",
        "Створює булеву маску на основі перлінового шуму. Поріг визначає, які клітини будуть марковані як 'true' (дають участь у результаті). Корисний для створення випадкових ділянок, континентів, островів або інших природних структур.")]
    public sealed class PerlinNoiseMaskNode : NodeBase, IPreviewableNode
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

        [NonSerialized] private bool[,] _lastMask;
        [NonSerialized] private float[,] _lastNoiseValues;

        public override string Title => "Шум Пerlін Mask";
        public override string Category => "Генерація";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Маска"),
            PortDefinition.Output<float[,]>("Значення шуму")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int w = Mathf.Max(1, context?.MapSize.x ?? 0);
            int h = Mathf.Max(1, context?.MapSize.y ?? 0);

            _lastNoiseValues = new float[w, h];
            _lastMask = new bool[w, h];

            int seed = GlobalSeed.Combine(
                context?.Seed ?? GlobalSeed.DefaultSeed,
                GlobalSeed.StableHash(NodeId));

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + _offset.x) / w / _scale;
                    float ny = (y + _offset.y) / h / _scale;

                    float noise = ProceduralNoiseUtility.SampleFbm(
                        nx, ny,
                        _octaves, _lacunarity, _persistence,
                        seed, false);

                    _lastNoiseValues[x, y] = noise;
                    _lastMask[x, y] = noise >= _threshold;
                }
            }

            return NodeOutput.Success(_lastMask, _lastNoiseValues);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastMask == null || _lastNoiseValues == null)
                return null;

            int w = _lastMask.GetLength(0);
            int h = _lastMask.GetLength(1);

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            float min = float.MaxValue;
            float max = float.MinValue;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float v = _lastNoiseValues[x, y];
                if (v < min) min = v;
                if (v > max) max = v;
            }
            float range = Mathf.Max(0.0001f, max - min);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float t = (_lastNoiseValues[x, y] - min) / range;
                    texture.SetPixel(x, y, new Color(t, t, t, 1f));
                }
            }

            texture.Apply(false, false);
            return texture;
        }
    }
}
