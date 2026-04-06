using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Road / Path", "Features", "Будує дороги між точками інтересу з урахуванням рельєфу та води. Корисна для формування логічної мережі шляхів між селами, замками, шахтами та іншими POI.")]
    public sealed class RoadPathNode : NodeBase
    {
        [Header("Road Settings")]
        [Tooltip("Tile ID, яким буде позначено прокладений шлях у біомній карті. Вибирай тайл, який відповідає візуальному стилю дороги або стежки.")]
        [SerializeField, TileId] private string _roadTile = "road";
        [Tooltip("Штраф за рух через різкі перепади висот. Більше значення змушує дорогу обходити круті схили та шукати більш пологі маршрути.")]
        [SerializeField, Range(0.5f, 20f)] private float _hillPenalty = 5f;
        [Tooltip("Штраф за проходження через водні клітинки за даними WaterMask. Дозволяє робити дороги, які уникають води, якщо ти не хочеш частих мостів або бродів.")]
        [SerializeField, Range(0.5f, 50f)] private float _waterPenalty = 30f;
        [Tooltip("Список Tile ID, які вважаються водними для логіки маршруту. Може використовуватись для додаткових перевірок або узгодження з візуальними типами води.")]
        [SerializeField, TileId] private string[] _waterTiles = { "water-deep", "water-shallow", "sea" };
        [Tooltip("Якщо увімкнено, нода намагатиметься побудувати зв'язну мережу між усіма POI через мінімальний остов. Якщо вимкнено, з'єднуватиме точки в послідовному порядку.")]
        [SerializeField] private bool _connectAllPOIs = true;
        [Tooltip("Ширина дороги в клітинках. Дає змогу робити вузькі стежки або ширші магістралі, що займають кілька сусідніх клітинок.")]
        [SerializeField, Range(1, 3)] private int _roadWidth = 1;

        public override string Title => "Road / Path";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<int[,]>("POIMap"),
            PortDefinition.Input<bool[,]>("WaterMask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<bool[,]>("RoadMask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var biomeMap = inputs[1] as string[,];
            var poiMap = inputs[2] as int[,];

            if (heightMap == null || biomeMap == null)
                return NodeOutput.Error("HeightMap and BiomeMap inputs are required.");
            if (poiMap == null)
                return NodeOutput.Error("POIMap input is required (connect POI Placement node).");

            var waterMask = inputs[3] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);
            var result = (string[,])biomeMap.Clone();
            var roadMask = new bool[w, h];

            // Find all POI positions
            var pois = new List<Vector2Int>();
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (poiMap[x, y] > 0)
                        pois.Add(new Vector2Int(x, y));
                }
            }

            if (pois.Count < 2)
                return NodeOutput.Warning("Need at least 2 POIs for roads.", result, roadMask);

            // Build cost map for A*
            var costMap = BuildCostMap(heightMap, waterMask, w, h);

            if (_connectAllPOIs)
            {
                // Connect POIs via minimum spanning tree (Prim's)
                var connected = new List<int> { 0 };
                var remaining = new HashSet<int>();
                for (int i = 1; i < pois.Count; i++) remaining.Add(i);

                while (remaining.Count > 0)
                {
                    float bestDist = float.MaxValue;
                    int bestFrom = -1, bestTo = -1;

                    foreach (int from in connected)
                    {
                        foreach (int to in remaining)
                        {
                            float dist = Vector2Int.Distance(pois[from], pois[to]);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestFrom = from;
                                bestTo = to;
                            }
                        }
                    }

                    if (bestTo < 0) break;

                    // A* from pois[bestFrom] to pois[bestTo]
                    var path = FindPath(pois[bestFrom], pois[bestTo], costMap, w, h);
                    ApplyRoad(result, roadMask, path, w, h);

                    connected.Add(bestTo);
                    remaining.Remove(bestTo);
                }
            }
            else
            {
                // Connect sequentially
                for (int i = 0; i < pois.Count - 1; i++)
                {
                    var path = FindPath(pois[i], pois[i + 1], costMap, w, h);
                    ApplyRoad(result, roadMask, path, w, h);
                }
            }

            return NodeOutput.Success(result, roadMask);
        }

        private float[,] BuildCostMap(float[,] heightMap, bool[,] waterMask, int w, int h)
        {
            var cost = new float[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float baseCost = 1f;
                    // Height penalty (steep = expensive)
                    if (x > 0 && x < w - 1 && y > 0 && y < h - 1)
                    {
                        float slope = 0f;
                        slope += Mathf.Abs(heightMap[x, y] - heightMap[x - 1, y]);
                        slope += Mathf.Abs(heightMap[x, y] - heightMap[x + 1, y]);
                        slope += Mathf.Abs(heightMap[x, y] - heightMap[x, y - 1]);
                        slope += Mathf.Abs(heightMap[x, y] - heightMap[x, y + 1]);
                        baseCost += slope * _hillPenalty;
                    }

                    if (waterMask != null && waterMask[x, y])
                        baseCost += _waterPenalty;

                    cost[x, y] = baseCost;
                }
            }
            return cost;
        }

        private static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end,
            float[,] costMap, int w, int h)
        {
            var gScore = new float[w, h];
            var cameFrom = new Vector2Int[w, h];
            var visited = new bool[w, h];

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    gScore[x, y] = float.MaxValue;

            gScore[start.x, start.y] = 0;

            // Simple priority queue via sorted list
            var open = new SortedList<float, Vector2Int>(new DuplicateKeyComparer());
            open.Add(Heuristic(start, end), start);

            int[] dx = { 0, 0, 1, -1, 1, -1, 1, -1 };
            int[] dy = { 1, -1, 0, 0, 1, 1, -1, -1 };
            float[] dCost = { 1f, 1f, 1f, 1f, 1.41f, 1.41f, 1.41f, 1.41f };

            while (open.Count > 0)
            {
                var current = open.Values[0];
                open.RemoveAt(0);

                if (current == end) break;

                if (visited[current.x, current.y]) continue;
                visited[current.x, current.y] = true;

                for (int d = 0; d < 8; d++)
                {
                    int nx = current.x + dx[d];
                    int ny = current.y + dy[d];
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    if (visited[nx, ny]) continue;

                    float tentative = gScore[current.x, current.y] + costMap[nx, ny] * dCost[d];
                    if (tentative < gScore[nx, ny])
                    {
                        gScore[nx, ny] = tentative;
                        cameFrom[nx, ny] = current;
                        float priority = tentative + Heuristic(new Vector2Int(nx, ny), end);
                        open.Add(priority, new Vector2Int(nx, ny));
                    }
                }
            }

            // Reconstruct path
            var path = new List<Vector2Int>();
            var pos = end;
            int maxIter = w * h;
            while (pos != start && maxIter-- > 0)
            {
                path.Add(pos);
                if (gScore[pos.x, pos.y] >= float.MaxValue) break;
                pos = cameFrom[pos.x, pos.y];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        private void ApplyRoad(string[,] biomeMap, bool[,] roadMask, List<Vector2Int> path, int w, int h)
        {
            foreach (var cell in path)
            {
                for (int dx = -(_roadWidth / 2); dx <= _roadWidth / 2; dx++)
                {
                    for (int dy = -(_roadWidth / 2); dy <= _roadWidth / 2; dy++)
                    {
                        int nx = cell.x + dx;
                        int ny = cell.y + dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        biomeMap[nx, ny] = _roadTile;
                        roadMask[nx, ny] = true;
                    }
                }
            }
        }

        private static float Heuristic(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private class DuplicateKeyComparer : IComparer<float>
        {
            public int Compare(float x, float y)
            {
                int result = x.CompareTo(y);
                return result == 0 ? 1 : result; // Allow duplicates
            }
        }
    }
}
