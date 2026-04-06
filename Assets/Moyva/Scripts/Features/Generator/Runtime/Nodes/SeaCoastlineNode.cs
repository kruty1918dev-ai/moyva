using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum CoastSide
    {
        South,
        North,
        East,
        West,
        SouthEast,
        SouthWest,
        Island
    }

    [NodeInfo("Sea / Coastline", "Terrain", "Нода формує море, берегову лінію та пляж уздовж вибраного краю карти. Дає змогу швидко додати узбережжя або острівну карту без ручного малювання води.")]
    public sealed class SeaCoastlineNode : NodeBase
    {
        [Header("Sea Settings")]
        [Tooltip("Сторона карти, з якої наступатиме море. Визначає базовий напрямок побудови узбережжя: з півдня, півночі, сходу, заходу або у вигляді острова.")]
        [SerializeField] private CoastSide _coastSide = CoastSide.South;
        [Tooltip("Базовий рівень моря. Використовується для висотної інтерполяції в прибережній зоні, щоб край води мав логічний перехід по рельєфу.")]
        [SerializeField, Range(0f, 0.5f)] private float _seaLevel = 0.25f;
        [Tooltip("Ширина прибережної смуги в клітинках. Чим більше значення, тим глибше море заходить у карту і тим ширший буде береговий перехід.")]
        [SerializeField, Range(0, 30)] private int _coastWidth = 8;
        [Tooltip("Tile ID для глибокого моря. Саме цей тайл буде ставитись у найдальшій від суші частині узбережжя.")]
        [SerializeField, TileId] private string _seaTile = "sea";
        [Tooltip("Tile ID для мілководної прибережної смуги між морем і пляжем. Дає проміжний пояс, щоб море не обривалося занадто різко.")]
        [SerializeField, TileId] private string _coastTile = "coast";
        [Tooltip("Tile ID для пляжу або вузької берегової смуги. Застосовується в зоні переходу між водою та сушею.")]
        [SerializeField, TileId] private string _beachTile = "beach";

        [Header("Coast Noise")]
        [Tooltip("Налаштування шуму, який спотворює ідеально рівний берег. Дозволяє зробити лінію узбережжя природнішою, з бухтами та виступами.")]
        [SerializeField] private DataNoiseSettings _coastNoise;
        [Tooltip("Сила впливу шуму на берегову лінію. Чим більше значення, тим сильніше берег відхиляється від простого рівного краю.")]
        [SerializeField, Range(0f, 15f)] private float _noiseInfluence = 5f;

        public override string Title => "Sea / Coastline";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BiomeMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<bool[,]>("SeaMask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var biomeMap = inputs[1] as string[,];
            if (heightMap == null || biomeMap == null)
                return NodeOutput.Error("HeightMap and BiomeMap inputs are required.");

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);
            var resultBiome = (string[,])biomeMap.Clone();
            var resultHeight = (float[,])heightMap.Clone();
            var seaMask = new bool[w, h];

            // Generate coastal noise for natural-looking edge
            float[,] noiseMap = null;
            if (_coastNoise != null)
            {
                var noiseProvider = context.GetService<INoiseProvider>();
                noiseMap = noiseProvider.GenerateNoiseMap(_coastNoise, w, h);
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float distToCoast = GetDistanceToCoast(x, y, w, h);
                    float noiseOffset = noiseMap != null ? (noiseMap[x, y] - 0.5f) * _noiseInfluence : 0f;
                    float effectiveDist = distToCoast + noiseOffset;

                    if (effectiveDist < _coastWidth)
                    {
                        float t = effectiveDist / _coastWidth;

                        if (t < 0.3f)
                        {
                            // Deep sea
                            resultBiome[x, y] = _seaTile;
                            resultHeight[x, y] = 0f;
                            seaMask[x, y] = true;
                        }
                        else if (t < 0.6f)
                        {
                            // Shallow coast
                            resultBiome[x, y] = _coastTile;
                            resultHeight[x, y] = Mathf.Lerp(0f, _seaLevel, (t - 0.3f) / 0.3f);
                            seaMask[x, y] = true;
                        }
                        else
                        {
                            // Beach
                            resultBiome[x, y] = _beachTile;
                            resultHeight[x, y] = Mathf.Lerp(_seaLevel, resultHeight[x, y], (t - 0.6f) / 0.4f);
                        }
                    }
                }
            }

            return NodeOutput.Success(resultBiome, resultHeight, seaMask);
        }

        private float GetDistanceToCoast(int x, int y, int w, int h)
        {
            return _coastSide switch
            {
                CoastSide.South => y,
                CoastSide.North => h - 1 - y,
                CoastSide.West => x,
                CoastSide.East => w - 1 - x,
                CoastSide.SouthEast => Mathf.Min(y, w - 1 - x),
                CoastSide.SouthWest => Mathf.Min(y, x),
                CoastSide.Island => Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y)),
                _ => float.MaxValue
            };
        }
    }
}
