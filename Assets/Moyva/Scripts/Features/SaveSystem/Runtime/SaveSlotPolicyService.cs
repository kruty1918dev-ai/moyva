using System;
using System.IO;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveSlotPolicyService : ISaveSlotPolicyService
    {
        public bool HasSave(int slot)
        {
            return SavePipelineHelper.ValidateSlot(slot) && File.Exists(SaveService.GetPath(slot));
        }

        public void Delete(int slot)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return;

            SavePipelineHelper.TryDelete(SaveService.GetPath(slot));
            SavePipelineHelper.TryDelete(SaveService.GetPath(slot) + ".bak");
            SavePipelineHelper.TryDelete(SaveService.GetPath(slot) + ".tmp");
        }

        public SaveSlotInfo GetSlotInfo(int slot)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            string path = SaveService.GetPath(slot);
            if (!File.Exists(path))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            var fi = new FileInfo(path);
            string worldName = TryReadWorldName(path, out var name) ? name : null;
            return new SaveSlotInfo(slot, true, fi.Length, fi.LastWriteTimeUtc, worldName);
        }

        private static bool TryReadWorldName(string path, out string worldName)
        {
            worldName = null;

            byte[] bytes;
            try { bytes = File.ReadAllBytes(path); }
            catch { return false; }

            var result = SaveFileCodec.TryDecode(bytes, out _, out var blocks, out _);
            if (result != SaveFileCodec.DecodeError.None || blocks == null)
                return false;

            uint generatedWorldBlockId = SaveFileCodec.ComputeBlockId("Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule");
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].blockId != generatedWorldBlockId)
                    continue;

                return TryReadWorldNameFromGeneratedWorldPayload(blocks[i].payload, out worldName);
            }

            return false;
        }

        private static bool TryReadWorldNameFromGeneratedWorldPayload(byte[] payload, out string worldName)
        {
            worldName = null;

            try
            {
                using var ms = new MemoryStream(payload);
                using var reader = new BinaryReader(ms);
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                if (width <= 0 || height <= 0)
                    return false;

                SkipStringMap(reader, width, height);
                SkipStringMap(reader, width, height);
                SkipFloatMap(reader, width, height);
                SkipStringMap(reader, width, height);

                if (ms.Position >= ms.Length)
                    return false;

                string value = reader.ReadString();
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                worldName = value.Trim();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SkipStringMap(BinaryReader reader, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    reader.ReadString();
        }

        private static void SkipFloatMap(BinaryReader reader, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    reader.ReadSingle();
        }
    }
}
