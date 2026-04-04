using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Debug Preview", "Debug")]
    public sealed class DebugPreviewNode : NodeBase
    {
        [Header("Debug Settings")]
        [SerializeField] private string _label = "Debug";
        [SerializeField] private bool _logToConsole = true;

        public override string Title => $"Debug: {_label}";
        public override string Category => "Debug";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap Pass"),
            PortDefinition.Output<string[,]>("TileMap Pass"),
            PortDefinition.Output<bool[,]>("Mask Pass")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var tileMap = inputs[1] as string[,];
            var mask = inputs[2] as bool[,];

            if (_logToConsole)
            {
                if (heightMap != null)
                    LogMapStats($"[{_label}] HeightMap", heightMap);
                if (tileMap != null)
                    LogTileStats($"[{_label}] TileMap", tileMap);
                if (mask != null)
                    LogMaskStats($"[{_label}] Mask", mask);

                if (heightMap == null && tileMap == null && mask == null)
                    Debug.Log($"[DebugPreview:{_label}] No inputs connected.");
            }

            return NodeOutput.Success(heightMap, tileMap, mask);
        }

        private static void LogMapStats(string label, float[,] map)
        {
            int w = map.GetLength(0);
            int h = map.GetLength(1);
            float min = float.MaxValue, max = float.MinValue, sum = 0;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float v = map[x, y];
                    if (v < min) min = v;
                    if (v > max) max = v;
                    sum += v;
                }
            }

            float avg = sum / (w * h);
            Debug.Log($"{label} [{w}x{h}] min={min:F3} max={max:F3} avg={avg:F3}");
        }

        private static void LogTileStats(string label, string[,] map)
        {
            int w = map.GetLength(0);
            int h = map.GetLength(1);
            var counts = new System.Collections.Generic.Dictionary<string, int>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string tile = map[x, y] ?? "(null)";
                    counts.TryGetValue(tile, out var c);
                    counts[tile] = c + 1;
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{label} [{w}x{h}] unique tiles: {counts.Count}");
            foreach (var kvp in counts)
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            Debug.Log(sb.ToString());
        }

        private static void LogMaskStats(string label, bool[,] mask)
        {
            int w = mask.GetLength(0);
            int h = mask.GetLength(1);
            int trueCount = 0;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (mask[x, y]) trueCount++;

            Debug.Log($"{label} [{w}x{h}] true={trueCount} false={w * h - trueCount} ({100f * trueCount / (w * h):F1}%)");
        }
    }
}
