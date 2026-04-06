using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("AutoTile Transitions", "Terrain", "Автоматично формує перехідні Tile ID на основі сусідів та, за бажанням, висотного рівня. Потрібна, коли одному базовому тайлу треба підбирати варіанти на кшталт cliff-N, corner-NE тощо.")]
    public sealed class AutoTileTransitionNode : NodeBase
    {
        [Header("Transition Settings")]
        [Tooltip("Роздільник, яким з'єднуються частини згенерованого Tile ID. Має збігатися з іменуванням тайлів у твоєму реєстрі або атласі.")]
        [SerializeField] private string _separator = "-";
        [Tooltip("Чи враховувати діагональних сусідів під час визначення країв. Увімкнення дає багатші переходи, але вимагає більш повного набору тайлів.")]
        [SerializeField] private bool _diagonalEdges = true;
        [Tooltip("Базові типи тайлів, для яких НЕ виконувати автоперехід. Наприклад: water, grass. Tile ID зберігається без змін.")]
        [SerializeField] private string[] _excludedTileTypes = System.Array.Empty<string>();

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

            var excluded = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase);
            if (_excludedTileTypes != null)
                foreach (var t in _excludedTileTypes)
                    if (!string.IsNullOrWhiteSpace(t)) excluded.Add(t.Trim());

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

                    // Skip excluded tile types — pass through unchanged
                    if (excluded.Contains(GetBaseTileType(baseTile)))
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
                    int dirStep = _diagonalEdges ? 1 : 2;

                    for (int d = 0; d < 8; d += dirStep)
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
                        string edgeSuffix = BuildEdgeSuffix(edgeMask, dirNames);
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
            int idx = tileId.IndexOf('-');
            return idx > 0 ? tileId.Substring(0, idx) : tileId;
        }

        private static string BuildEdgeSuffix(int mask, string[] dirNames)
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
                if (n) return "cliff-N";
                if (e) return "cliff-E";
                if (s) return "cliff-S";
                if (w) return "cliff-W";
            }

            if (cardinalCount == 2)
            {
                if (n && e) return "cliff-NE";
                if (n && w) return "cliff-NW";
                if (s && e) return "cliff-SE";
                if (s && w) return "cliff-SW";
                if (n && s) return "cliff-NS";
                if (e && w) return "cliff-EW";
            }

            if (cardinalCount == 3)
            {
                if (!n) return "cliff-SEW";
                if (!e) return "cliff-NSW";
                if (!s) return "cliff-NEW";
                if (!w) return "cliff-NSE";
            }

            if (cardinalCount == 4)
                return "cliff-ALL";

            // For diagonal-only edges, check corners
            bool ne = (mask & 2) != 0;
            bool se = (mask & 8) != 0;
            bool sw = (mask & 32) != 0;
            bool nw = (mask & 128) != 0;

            var sb = new System.Text.StringBuilder("corner");
            if (ne) sb.Append("-NE");
            if (se) sb.Append("-SE");
            if (sw) sb.Append("-SW");
            if (nw) sb.Append("-NW");

            return sb.Length > 6 ? sb.ToString() : "";
        }
    }
}
