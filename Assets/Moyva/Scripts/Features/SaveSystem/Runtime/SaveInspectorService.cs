using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveInspectorService : ISaveInspectorService
    {
        public bool HasBlock(int slot, string moduleTypeFullName)
        {
            if (string.IsNullOrWhiteSpace(moduleTypeFullName))
                return false;

            uint targetBlockId = ComputeBlockId(moduleTypeFullName);
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

        private static bool HasBlockById(int slot, uint targetBlockId)
        {
            if (slot < 0 || slot > 99)
                return false;

            string path = Path.Combine(Application.persistentDataPath, "saves", $"slot{slot:D2}.mvs");
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

            var result = SaveFileCodec.TryDecode(bytes, out _, out List<(uint blockId, byte[] payload)> blocks, out _);
            if (result != SaveFileCodec.DecodeError.None || blocks == null)
                return false;

            foreach (var block in blocks)
            {
                if (block.blockId == targetBlockId)
                    return true;
            }

            return false;
        }

        private static uint ComputeBlockId(string fullTypeName)
        {
            uint hash = 2166136261u;
            foreach (char c in fullTypeName)
            {
                hash ^= c;
                hash *= 16777619u;
            }
            return hash;
        }
    }
}