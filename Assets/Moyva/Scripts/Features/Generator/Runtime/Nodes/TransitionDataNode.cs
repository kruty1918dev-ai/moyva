using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Transition Data", "Rendering", "Генерує маску переходів між тайлами для шейдера edge-blending. Для кожної клітини визначає, які кардинальні сусіди мають інший тип тайлу (N=1, E=2, S=4, W=8).")]
    public sealed class TransitionDataNode : NodeBase
    {
        public override string Title => "Transition Data";
        public override string Category => "Rendering";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("TileMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<int[,]>("NeighborMask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs[0] as string[,];
            if (tileMap == null)
                return NodeOutput.Error("TileMap input is required.");

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);
            var mask = new int[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (IsWaterTile(tileMap[x, y]))
                        continue;

                    string current = GetBaseTileType(tileMap[x, y]);
                    int bits = 0;
                    string nb;

                    // N (y+1)
                    nb = (y + 1 < h) ? tileMap[x, y + 1] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current)
                        bits |= 1;
                    // E (x+1)
                    nb = (x + 1 < w) ? tileMap[x + 1, y] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current)
                        bits |= 2;
                    // S (y-1)
                    nb = (y - 1 >= 0) ? tileMap[x, y - 1] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current)
                        bits |= 4;
                    // W (x-1)
                    nb = (x - 1 >= 0) ? tileMap[x - 1, y] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current)
                        bits |= 8;

                    mask[x, y] = bits;
                }
            }

            return NodeOutput.Success(mask);
        }

        private static string GetBaseTileType(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
                return string.Empty;

            int dashIndex = tileId.IndexOf('-');
            return dashIndex > 0 ? tileId.Substring(0, dashIndex) : tileId;
        }

        private static bool IsWaterTile(string tileId)
        {
            return GetBaseTileType(tileId) == "water";
        }
    }
}
