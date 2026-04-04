using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("AutoTile Transitions", "Terrain")]
    public sealed class AutoTileTransitionNode : NodeBase
    {
        [Header("Transition Settings")]
        [SerializeField] private string _separator = "_";
        [SerializeField] private bool _diagonalEdges = true;

        public override string Title => "AutoTile Transitions";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TransitionMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs[0] as string[,];
            if (tileMap == null)
                return NodeOutput.Error("TileMap input is required.");

            var heightMap = inputs[1] as float[,];

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);
            var result = new string[w, h];

            // Direction offsets: N, NE, E, SE, S, SW, W, NW
            int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] dy = { 1, 1, 0, -1, -1, -1, 0, 1 };
            string[] dirNames = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string baseTile = tileMap[x, y] ?? "";
                    if (string.IsNullOrEmpty(baseTile))
                    {
                        result[x, y] = baseTile;
                        continue;
                    }

                    // Analyze height tier if heightmap available
                    int tier = 0;
                    if (heightMap != null)
                        tier = GetHeightTier(heightMap[x, y]);

                    // Find edges — where neighbor is different tile type
                    int edgeMask = 0;
                    int dirCount = _diagonalEdges ? 8 : 4;
                    int dirStep = _diagonalEdges ? 1 : 2;

                    for (int d = 0; d < dirCount; d += dirStep)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];

                        if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        {
                            edgeMask |= (1 << d);
                            continue;
                        }

                        string neighborTile = tileMap[nx, ny] ?? "";
                        if (GetBaseTileType(neighborTile) != GetBaseTileType(baseTile))
                            edgeMask |= (1 << d);
                    }

                    // Build tile ID with transition info
                    string tileId = GetBaseTileType(baseTile);

                    if (tier > 0)
                        tileId += _separator + tier;

                    if (edgeMask != 0)
                    {
                        string edgeSuffix = BuildEdgeSuffix(edgeMask, dirNames, dirCount, dirStep);
                        if (!string.IsNullOrEmpty(edgeSuffix))
                            tileId += _separator + edgeSuffix;
                    }

                    result[x, y] = tileId;
                }
            }

            return NodeOutput.Success(result);
        }

        private static int GetHeightTier(float height)
        {
            if (height < 0.2f) return 0;
            if (height < 0.4f) return 1;
            if (height < 0.6f) return 2;
            if (height < 0.8f) return 3;
            return 4;
        }

        private static string GetBaseTileType(string tileId)
        {
            if (string.IsNullOrEmpty(tileId)) return "";
            int idx = tileId.IndexOf('_');
            return idx > 0 ? tileId.Substring(0, idx) : tileId;
        }

        private static string BuildEdgeSuffix(int mask, string[] dirNames, int dirCount, int dirStep)
        {
            // Check cardinal directions first for common patterns
            bool n = (mask & 1) != 0;
            bool e = (mask & 4) != 0;
            bool s = (mask & 16) != 0;
            bool w = (mask & 64) != 0;

            // Cliff edge naming: if only one side is different
            int cardinalCount = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);

            if (cardinalCount == 1)
            {
                if (n) return "cliff_N";
                if (e) return "cliff_E";
                if (s) return "cliff_S";
                if (w) return "cliff_W";
            }

            if (cardinalCount == 2)
            {
                if (n && e) return "cliff_NE";
                if (n && w) return "cliff_NW";
                if (s && e) return "cliff_SE";
                if (s && w) return "cliff_SW";
                if (n && s) return "cliff_NS";
                if (e && w) return "cliff_EW";
            }

            if (cardinalCount == 3)
            {
                if (!n) return "cliff_SEW";
                if (!e) return "cliff_NSW";
                if (!s) return "cliff_NEW";
                if (!w) return "cliff_NSE";
            }

            if (cardinalCount == 4)
                return "cliff_ALL";

            // For diagonal-only edges, check corners
            bool ne = (mask & 2) != 0;
            bool se = (mask & 8) != 0;
            bool sw = (mask & 32) != 0;
            bool nw = (mask & 128) != 0;

            var sb = new System.Text.StringBuilder("corner");
            if (ne) sb.Append("_NE");
            if (se) sb.Append("_SE");
            if (sw) sb.Append("_SW");
            if (nw) sb.Append("_NW");

            return sb.Length > 6 ? sb.ToString() : "";
        }
    }
}
