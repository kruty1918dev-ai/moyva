using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Сервіс для інспектування вмісту файлів збереження без їх завантаження.
    /// Дозволяє перевіряти наявність окремих блоків (модулів) у save-файлі
    /// за типом, повним ім'ям типу або узагальненим типом.
    /// </summary>
    internal sealed class SaveInspectorService : ISaveInspectorService
    {
        public bool HasBlock(int slot, string moduleTypeFullName)
        {
            if (string.IsNullOrWhiteSpace(moduleTypeFullName))
                return false;

            uint targetBlockId = SaveFileCodec.ComputeBlockId(moduleTypeFullName);
            return HasBlockById(slot, targetBlockId);
        }

        public bool HasBlock(int slot, Type moduleType)
        {
            if (moduleType == null)
                return false;

            return HasBlockById(slot, SaveFileCodec.ComputeBlockId(moduleType));
        }

        public bool HasBlock<TModule>(int slot = 0)
            => HasBlock(slot, typeof(TModule));

        public bool TryGetFogSnapshot(int slot, out bool[,] snapshot)
        {
            snapshot = null;
            if (!SavePipelineHelper.ValidateSlot(slot))
                return false;

            string path = SaveService.GetPath(slot);
            if (!File.Exists(path))
                return false;

            byte[] bytes;
            try { bytes = File.ReadAllBytes(path); }
            catch { return false; }

            var result = SaveFileCodec.TryDecode(bytes, out _, out var blocks, out _);
            if (result != SaveFileCodec.DecodeError.None || blocks == null)
                return false;

            uint fogId = SaveFileCodec.ComputeBlockId("Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule");

            foreach (var (blockId, payload) in blocks)
            {
                if (blockId != fogId) continue;

                try
                {
                    using var ms = new MemoryStream(payload);
                    using var br = new BinaryReader(ms);

                    int width = br.ReadInt32();
                    int height = br.ReadInt32();
                    if (width <= 0 || height <= 0)
                        return false;

                    var snap = new bool[width, height];
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            snap[x, y] = br.ReadBoolean();

                    snapshot = snap;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static bool HasBlockById(int slot, uint targetBlockId)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return false;

            string path = SaveService.GetPath(slot);
            if (!File.Exists(path))
                return false;

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch
            {
                return false;
            }

            var result = SaveFileCodec.TryDecode(bytes, out _, out var blocks, out _);
            if (result != SaveFileCodec.DecodeError.None || blocks == null)
                return false;

            foreach (var block in blocks)
            {
                if (block.blockId == targetBlockId)
                    return true;
            }

            return false;
        }
    }
}