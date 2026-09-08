using System;
using System.Collections.Generic;
using System.IO;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveSlotPolicyService : ISaveSlotPolicyService
    {
        private readonly Dictionary<int, SaveSlotInfo> _metadata = new();

        public bool HasSave(int slot)
        {
            if (!SavePipelineHelper.ValidateSlot(slot)) return false;
            string path = SaveService.GetPath(slot);
            return File.Exists(path) || File.Exists(path + ".bak");
        }

        public void Delete(int slot)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return;

            _metadata.Remove(slot);
            SavePipelineHelper.TryDelete(SaveService.GetPath(slot));
            SavePipelineHelper.TryDelete(SaveService.GetPath(slot) + ".bak");
            SavePipelineHelper.TryDelete(SaveService.GetPath(slot) + ".tmp");
        }

        public SaveSlotInfo GetSlotInfo(int slot)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            string path = SaveService.GetPath(slot);
            if (!File.Exists(path)) path += ".bak";
            if (!File.Exists(path))
            {
                _metadata.Remove(slot);
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);
            }

            var fi = new FileInfo(path);
            if (_metadata.TryGetValue(slot, out var cached)
                && cached.FileSizeBytes == fi.Length && cached.LastWriteTimeUtc == fi.LastWriteTimeUtc)
                return cached;

            string worldName = TryReadWorldName(path, out var name) ? name : null;
            var info = new SaveSlotInfo(slot, true, fi.Length, fi.LastWriteTimeUtc, worldName);
            _metadata[slot] = info;
            return info;
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
                int markerOrWidth = reader.ReadInt32();
                bool versioned = markerOrWidth < 0;
                int version = versioned ? -markerOrWidth : 0;
                if (versioned && version != 2 && version != 3 && version != 4)
                    return false;
                int width = versioned ? reader.ReadInt32() : markerOrWidth;
                int height = reader.ReadInt32();
                if (width <= 0 || height <= 0)
                    return false;

                if (versioned) reader.ReadString(); // Configuration fingerprint.
                int minimumBytesPerCell = versioned ? 8 : 7;
                if ((long)width * height > (ms.Length - ms.Position) / minimumBytesPerCell)
                    return false;

                if (versioned) SkipStringMap(reader, width, height); // Gameplay tiles precede visual tiles.
                SkipStringMap(reader, width, height);
                SkipStringMap(reader, width, height);
                SkipFloatMap(reader, width, height);
                SkipStringMap(reader, width, height);
                if (version >= 3)
                {
                    SkipIntMap(reader, width, height);
                    SkipWorldSettings(reader);
                    if (!SkipCompiledLayers(reader))
                        return false;
                    if (!SkipLogicalTileMap(reader, width, height, version >= 4))
                        return false;
                }

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

        private static void SkipIntMap(BinaryReader reader, int width, int height)
        {
            if (!reader.ReadBoolean())
                return;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    reader.ReadInt32();
        }

        private static void SkipWorldSettings(BinaryReader reader)
        {
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadBoolean();
            reader.ReadSingle();
            bool hasBounds = reader.ReadBoolean();
            if (!hasBounds)
                return;

            SkipVector3(reader);
            SkipVector3(reader);
        }

        private static bool SkipCompiledLayers(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > 1024)
                return false;

            for (int i = 0; i < count; i++)
            {
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                reader.ReadBoolean();
            }

            return true;
        }

        private static bool SkipLogicalTileMap(BinaryReader reader, int width, int height, bool compact)
        {
            if (!reader.ReadBoolean())
                return true;

            if (reader.ReadInt32() != width || reader.ReadInt32() != height)
                return false;

            int uniqueCount = 0;
            if (compact)
            {
                uniqueCount = reader.ReadInt32();
                if (uniqueCount < 0 || uniqueCount > (long)width * height * 64)
                    return false;

                for (int i = 0; i < uniqueCount; i++)
                    SkipLogicalSample(reader);
            }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int count = reader.ReadInt32();
                    if (count < 0 || count > 64)
                        return false;

                    for (int i = 0; i < count; i++)
                    {
                        if (compact)
                        {
                            int sampleIndex = reader.ReadInt32();
                            if (sampleIndex < 0 || sampleIndex >= uniqueCount)
                                return false;
                        }
                        else
                        {
                            SkipLogicalSample(reader);
                        }
                    }
                }

            return true;
        }

        private static void SkipLogicalSample(BinaryReader reader)
        {
            reader.ReadString();
            reader.ReadString();
            reader.ReadString();
            reader.ReadString();
            reader.ReadString();
            reader.ReadString();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadSingle();
            reader.ReadSingle();
            reader.ReadString();
            reader.ReadInt32();
            reader.ReadInt32();
        }

        private static void SkipVector3(BinaryReader reader)
        {
            reader.ReadSingle();
            reader.ReadSingle();
            reader.ReadSingle();
        }
    }
}
