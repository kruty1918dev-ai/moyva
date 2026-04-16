using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Water Visual Data", "Rendering", "Генерує дані для водяного шейдера: відстань до берега (BFS) та маску берегових країв для кожного водного тайлу.")]
    public sealed class WaterVisualNode : NodeBase
    {
        [SerializeField] private string[] _waterTileIds = { "water" };
        [SerializeField] private int _maxDepthTiles = 10;

        public override string Title => "Water Visual Data";
        public override string Category => "Rendering";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("ShoreDistanceMap"),
            PortDefinition.Output<int[,]>("ShoreMask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            if (biomeMap == null)
                return NodeOutput.Error("BiomeMap input is required.");

            int w = biomeMap.GetLength(0);
            int h = biomeMap.GetLength(1);

            var waterTileSet = new HashSet<string>(_waterTileIds);

            // Determine water cells
            var isWater = new bool[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    isWater[x, y] = IsWaterTile(biomeMap[x, y], waterTileSet);

            // Multi-Source BFS from all non-water cells
            var shoreDistance = new float[w, h];
            var queue = new Queue<(int x, int y)>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!isWater[x, y])
                    {
                        shoreDistance[x, y] = 0f;
                        queue.Enqueue((x, y));
                    }
                    else
                    {
                        shoreDistance[x, y] = float.MaxValue;
                    }
                }
            }

            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                float nextDist = shoreDistance[cx, cy] + 1f;

                if (nextDist > _maxDepthTiles)
                    continue;

                for (int d = 0; d < 4; d++)
                {
                    int nx = cx + dx[d];
                    int ny = cy + dy[d];

                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;

                    if (nextDist < shoreDistance[nx, ny])
                    {
                        shoreDistance[nx, ny] = nextDist;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            // Clamp non-water to 0
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (!isWater[x, y])
                        shoreDistance[x, y] = 0f;

            // Shore mask: for each water tile, check if cardinal neighbor is non-water
            var shoreMask = new int[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!isWater[x, y])
                        continue;

                    int bits = 0;
                    // N (y+1)
                    if (y + 1 >= h || !isWater[x, y + 1]) bits |= 1;
                    // E (x+1)
                    if (x + 1 >= w || !isWater[x + 1, y]) bits |= 2;
                    // S (y-1)
                    if (y - 1 < 0 || !isWater[x, y - 1]) bits |= 4;
                    // W (x-1)
                    if (x - 1 < 0 || !isWater[x - 1, y]) bits |= 8;

                    shoreMask[x, y] = bits;
                }
            }

            return NodeOutput.Success(shoreDistance, shoreMask);
        }

        private static bool IsWaterTile(string tileId, HashSet<string> waterIds)
        {
            if (string.IsNullOrEmpty(tileId))
                return false;

            if (waterIds.Contains(tileId))
                return true;

            // Check base type (e.g. "water-deep" → "water")
            int dashIndex = tileId.IndexOf('-');
            return dashIndex > 0 && waterIds.Contains(tileId.Substring(0, dashIndex));
        }
    }
}
