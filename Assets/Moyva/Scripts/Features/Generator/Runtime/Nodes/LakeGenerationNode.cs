using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Lake Generation", "Features", "Шукає низини на карті й перетворює їх на озера, формуючи також водну маску для подальших систем. Дозволяє додати локальні водойми без ручного розписування води.")]
    public sealed class LakeGenerationNode : NodeBase
    {
        [Header("Lake Settings")]
        [Tooltip("Рівень води, нижче якого низини вважаються кандидатами на озеро. Підвищення цього значення збільшує кількість і площу водойм.")]
        [SerializeField, Range(0f, 0.5f)] private float _waterLevel = 0.3f;
        [Tooltip("Мінімальний розмір басейну в клітинках, щоб він вважався повноцінним озером. Менші скупчення низин будуть проігноровані як шум.")]
        [SerializeField, Range(1, 10)] private int _minLakeSize = 4;
        [Tooltip("Якщо увімкнено, озера можуть з'являтися лише на суші. Це захищає існуючі моря, річки та інші водні тайли від повторного перетворення.")]
        [SerializeField] private bool _onlyGenerateOnLand = true;
        [Tooltip("Tile ID для глибокої частини озера. Використовується там, де глибина низини значно нижча за рівень води.")]
        [SerializeField, TileId] private string _deepWaterTile = "water-deep";
        [Tooltip("Tile ID для мілководдя озера. Застосовується біля країв або в неглибоких водоймах для більш плавного переходу.")]
        [SerializeField, TileId] private string _shallowWaterTile = "water-shallow";
        [Tooltip("Поріг, який розділяє мілку та глибоку воду. Дозволяє керувати тим, наскільки широкою буде смуга мілководдя біля берегів.")]
        [SerializeField, Range(0f, 0.1f)] private float _shallowDepth = 0.05f;
        [Tooltip("Tile ID для берегової смуги навколо озера. Якщо поле порожнє, окремий береговий тайл створюватися не буде.")]
        [SerializeField, TileId] private string _shoreTile = "shore";

        public override string Title => "Lake Generation";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BiomeMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<bool[,]>("WaterMask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var biomeMap = inputs[1] as string[,];
            if (heightMap == null || biomeMap == null)
                return NodeOutput.Error("HeightMap and BiomeMap inputs are required.");

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);
            var result = (string[,])biomeMap.Clone();
            var waterMask = new bool[w, h];
            var visited = new bool[w, h];

            // Find all connected low regions (below water level) via flood-fill
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (visited[x, y]) continue;
                    if (heightMap[x, y] >= _waterLevel) continue;
                    if (_onlyGenerateOnLand && !CanGenerateLakeAt(biomeMap[x, y]))
                    {
                        visited[x, y] = true;
                        continue;
                    }

                    // Flood-fill to find this basin
                    var basin = FloodFill(heightMap, biomeMap, visited, x, y, w, h, _waterLevel, _onlyGenerateOnLand);

                    if (basin.Count < _minLakeSize) continue;

                    // Mark this lake
                    foreach (var cell in basin)
                    {
                        float depth = _waterLevel - heightMap[cell.x, cell.y];
                        waterMask[cell.x, cell.y] = true;

                        if (depth > _shallowDepth)
                            result[cell.x, cell.y] = _deepWaterTile;
                        else
                            result[cell.x, cell.y] = _shallowWaterTile;
                    }
                }
            }

            // Add shore tiles around water
            if (!string.IsNullOrEmpty(_shoreTile))
            {
                var shorePositions = new List<Vector2Int>();
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (waterMask[x, y]) continue;
                        if (_onlyGenerateOnLand && !CanGenerateLakeAt(result[x, y])) continue;
                        if (HasWaterNeighbor(waterMask, x, y, w, h))
                            shorePositions.Add(new Vector2Int(x, y));
                    }
                }

                foreach (var pos in shorePositions)
                    result[pos.x, pos.y] = _shoreTile;
            }

            return NodeOutput.Success(result, waterMask);
        }

        private static List<Vector2Int> FloodFill(float[,] heightMap, string[,] biomeMap, bool[,] visited,
            int startX, int startY, int w, int h, float threshold, bool onlyGenerateOnLand)
        {
            var basin = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                basin.Add(cell);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1) continue; // 4-directional

                        int nx = cell.x + dx;
                        int ny = cell.y + dy;

                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        if (visited[nx, ny]) continue;
                        if (heightMap[nx, ny] >= threshold) continue;
                        if (onlyGenerateOnLand && !CanGenerateLakeAt(biomeMap[nx, ny])) continue;

                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            return basin;
        }

        private static bool HasWaterNeighbor(bool[,] waterMask, int x, int y, int w, int h)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && waterMask[nx, ny])
                        return true;
                }
            }
            return false;
        }

        private static bool CanGenerateLakeAt(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return true;

            string normalized = tileId.ToLowerInvariant();
            return !normalized.Contains("water")
                && !normalized.Contains("sea")
                && !normalized.Contains("lake")
                && !normalized.Contains("river");
        }
    }
}
