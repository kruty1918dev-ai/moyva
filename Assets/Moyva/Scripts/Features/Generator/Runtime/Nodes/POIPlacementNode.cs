using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum POIType
    {
        Castle,
        Village,
        Tower,
        Mine,
        Port,
        Temple
    }

    [Serializable]
    public class POIRule
    {
        [Tooltip("Тип точки інтересу для логічної класифікації. Допомагає групувати правила для замків, сіл, шахт, храмів та інших важливих об'єктів карти.")]
        public POIType Type;
        [Tooltip("ID будівлі або об'єкта, який буде поставлено в клітинку POI. Має існувати у відповідному реєстрі, щоб точка інтересу могла з'явитися у світі.")]
        [BuildingId] public string ObjectId = "village";
        [Tooltip("Скільки екземплярів цього типу POI нода намагатиметься поставити на мапі. Фактична кількість може бути меншою, якщо не знайдеться достатньо валідних позицій.")]
        [Range(0, 10)] public int Count = 3;
        [Tooltip("Мінімальна висота для розміщення цього POI. Дозволяє, наприклад, не будувати села в низинних озерах або замки в болоті.")]
        [Range(0f, 1f)] public float MinHeight = 0.3f;
        [Tooltip("Максимальна висота для цього POI. Разом із MinHeight задає допустимий висотний пояс для типу об'єкта.")]
        [Range(0f, 1f)] public float MaxHeight = 0.6f;
        [Tooltip("Якщо увімкнено, під час оцінки позицій нода віддаватиме перевагу місцям ближче до води. Добре підходить для портів, сіл або храмів біля берегів.")]
        public bool NearWater;
        [Tooltip("Мінімальна мангеттенська відстань до інших уже поставлених POI. Допомагає уникати скупчення поселень або споруд в одній точці.")]
        [Range(0, 20)] public int MinDistanceFromOthers = 10;
        [Tooltip("Якщо увімкнено, правило віддає перевагу рівнішим ділянкам. Корисно для будівель, які мають стояти на відносно пласкій місцевості.")]
        public bool PreferFlat = true;
    }

    [NodeInfo("POI Placement", "Features", "Розставляє точки інтересу на основі висоти, близькості до води та рівності місцевості. Підходить для поселень, фортець, шахт, храмів та іншої ключової інфраструктури карти.")]
    public sealed class POIPlacementNode : NodeBase
    {
        [Header("POI Rules")]
        [Tooltip("Список правил розміщення точок інтересу. Кожне правило визначає тип об'єкта, його кількість і критерії пошуку придатних позицій.")]
        [SerializeField] private POIRule[] _rules = new[]
        {
            new POIRule { Type = POIType.Castle, ObjectId = "castle", Count = 1,
                MinHeight = 0.5f, MaxHeight = 0.75f, MinDistanceFromOthers = 15, PreferFlat = true },
            new POIRule { Type = POIType.Village, ObjectId = "village", Count = 4,
                MinHeight = 0.3f, MaxHeight = 0.55f, MinDistanceFromOthers = 10, NearWater = true }
        };
        [Tooltip("Seed випадковості для сортування та вибору позицій. Дозволяє відтворювати однакове розташування POI при незмінних вхідних даних.")]
        [SerializeField] private int _seed = 42;

        public override string Title => "POI Placement";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<bool[,]>("WaterMask"),
            PortDefinition.Input<string[,]>("ObjectMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BuildingMap"),
            PortDefinition.Output<int[,]>("POIMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            var waterMask = inputs[1] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            var objectMap = inputs[2] as string[,];
            var buildingMap = new string[w, h];
            var poiMap = new int[w, h]; // 0 = no POI, 1+ = POI id

            // Precompute flatness map (variance in local area)
            var flatness = ComputeFlatness(heightMap, w, h);

            // Precompute water distance if needed
            int[,] waterDist = waterMask != null ? ComputeDistanceField(waterMask, w, h) : null;

            var placedPOIs = new List<Vector2Int>();
            var rng = new System.Random(_seed);
            int poiId = 1;

            if (_rules == null) return NodeOutput.Success(buildingMap, poiMap);

            foreach (var rule in _rules)
            {
                // Build candidate list
                var candidates = new List<(Vector2Int pos, float score)>();

                for (int x = 2; x < w - 2; x++)
                {
                    for (int y = 2; y < h - 2; y++)
                    {
                        float height = heightMap[x, y];
                        if (height < rule.MinHeight || height > rule.MaxHeight) continue;
                        if (waterMask != null && waterMask[x, y]) continue;
                        if (objectMap != null && !string.IsNullOrEmpty(objectMap[x, y])) continue;
                        if (!string.IsNullOrEmpty(buildingMap[x, y])) continue;

                        // Check distance from already placed POIs
                        bool tooClose = false;
                        foreach (var placed in placedPOIs)
                        {
                            int dist = Mathf.Abs(placed.x - x) + Mathf.Abs(placed.y - y);
                            if (dist < rule.MinDistanceFromOthers)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        if (tooClose) continue;

                        // Score this position
                        float score = 0f;
                        if (rule.PreferFlat)
                            score += (1f - flatness[x, y]) * 3f;
                        if (rule.NearWater && waterDist != null)
                            score += Mathf.Max(0f, 1f - waterDist[x, y] / 15f) * 5f;

                        // Small random factor
                        score += (float)rng.NextDouble() * 0.5f;

                        candidates.Add((new Vector2Int(x, y), score));
                    }
                }

                // Sort by score (descending)
                candidates.Sort((a, b) => b.score.CompareTo(a.score));

                // Place top N candidates
                int placed_count = 0;
                foreach (var (pos, _) in candidates)
                {
                    if (placed_count >= rule.Count) break;

                    // Double-check distance (candidates may overlap from other rules)
                    bool valid = true;
                    foreach (var already in placedPOIs)
                    {
                        int dist = Mathf.Abs(already.x - pos.x) + Mathf.Abs(already.y - pos.y);
                        if (dist < rule.MinDistanceFromOthers)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) continue;

                    buildingMap[pos.x, pos.y] = rule.ObjectId;
                    poiMap[pos.x, pos.y] = poiId;
                    placedPOIs.Add(pos);
                    placed_count++;
                    poiId++;
                }
            }

            return NodeOutput.Success(buildingMap, poiMap);
        }

        private static float[,] ComputeFlatness(float[,] heightMap, int w, int h)
        {
            var flatness = new float[w, h];
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    float center = heightMap[x, y];
                    float variance = 0f;
                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            float diff = center - heightMap[x + dx, y + dy];
                            variance += diff * diff;
                            count++;
                        }
                    }
                    flatness[x, y] = Mathf.Clamp01(variance / count * 100f);
                }
            }
            return flatness;
        }

        private static int[,] ComputeDistanceField(bool[,] mask, int w, int h)
        {
            var dist = new int[w, h];
            var queue = new Queue<Vector2Int>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (mask[x, y])
                    {
                        dist[x, y] = 0;
                        queue.Enqueue(new Vector2Int(x, y));
                    }
                    else
                    {
                        dist[x, y] = int.MaxValue;
                    }
                }
            }

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int nx = cell.x + dx[d];
                    int ny = cell.y + dy[d];
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    int newDist = dist[cell.x, cell.y] + 1;
                    if (newDist < dist[nx, ny])
                    {
                        dist[nx, ny] = newDist;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            return dist;
        }
    }
}
