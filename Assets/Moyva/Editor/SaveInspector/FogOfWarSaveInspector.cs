using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.SaveInspector
{
    /// <summary>
    /// Editor helper: читає .mvs файл, знаходить блок FogOfWar і виводить координати розблокованих тайлів.
    /// Виклик: Moyva / Inspect Save / Fog of War...
    /// </summary>
    public static class FogOfWarSaveInspector
    {
        private const string FogModuleTypeName = "Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule";

        public static void InspectFogOfWar()
        {
            string path = EditorUtility.OpenFilePanel("Select .mvs save file", Application.dataPath, "mvs");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                using var fs = File.OpenRead(path);
                using var br = new BinaryReader(fs);

                // Header
                var magic = br.ReadBytes(4);
                if (magic.Length < 4 || magic[0] != (byte)'M' || magic[1] != (byte)'V' || magic[2] != (byte)'S' || magic[3] != (byte)'A')
                {
                    Debug.LogError("Selected file is not a valid .mvs (bad magic)");
                    return;
                }

                ushort version = br.ReadUInt16();
                uint blockCount = br.ReadUInt32();

                uint targetId = ComputeBlockId(FogModuleTypeName);
                bool found = false;
                byte[] payload = null;

                for (uint i = 0; i < blockCount; i++)
                {
                    if (fs.Position + 12 > fs.Length)
                    {
                        Debug.LogWarning("Unexpected EOF while reading block headers.");
                        break;
                    }

                    uint blockId = br.ReadUInt32();
                    uint blockSize = br.ReadUInt32();
                    uint blockCrc = br.ReadUInt32();

                    if (blockSize > int.MaxValue || fs.Position + blockSize > fs.Length)
                    {
                        Debug.LogWarning($"Block {blockId:X8} has invalid size {blockSize}, stopping scan.");
                        break;
                    }

                    if (blockId == targetId)
                    {
                        payload = br.ReadBytes((int)blockSize);
                        found = true;
                        break;
                    }

                    fs.Seek(blockSize, SeekOrigin.Current);
                }

                if (!found || payload == null)
                {
                    Debug.LogWarning("FogOfWar block not found in the selected save file.");
                    return;
                }

                using var ms = new MemoryStream(payload);
                using var pr = new BinaryReader(ms);

                int width = pr.ReadInt32();
                int height = pr.ReadInt32();

                if (width <= 0 || height <= 0)
                {
                    Debug.Log("FogOfWar snapshot is empty (width/height = 0)");
                    return;
                }

                var explored = new List<(int x, int y)>();
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        bool b = pr.ReadBoolean();
                        if (b) explored.Add((x, y));
                    }
                }

                Debug.Log($"FogOfWar: width={width}, height={height}, exploredCount={explored.Count}");

                int limit = 200;
                if (explored.Count > 0)
                {
                    int show = Math.Min(limit, explored.Count);
                    var parts = new List<string>(show);
                    for (int i = 0; i < show; i++)
                        parts.Add($"({explored[i].x},{explored[i].y})");

                    Debug.Log($"Explored coords (first {show}): {string.Join(", ", parts)}");
                    if (explored.Count > show)
                        Debug.Log($"... and {explored.Count - show} more.");
                }

                // Export full list to CSV next to the save file for easier inspection
                string csvPath = path + ".fog.csv";
                using (var sw = new StreamWriter(csvPath, false))
                {
                    sw.WriteLine("x,y");
                    for (int i = 0; i < explored.Count; i++)
                        sw.WriteLine($"{explored[i].x},{explored[i].y}");
                }

                Debug.Log($"Full explored list exported to: {csvPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error inspecting FogOfWar block: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static uint ComputeBlockId(string fullTypeName)
        {
            uint hash = 2166136261u;
            foreach (char c in fullTypeName)
            {
                hash ^= (uint)c;
                hash *= 16777619u;
            }
            return hash;
        }
    }
}
