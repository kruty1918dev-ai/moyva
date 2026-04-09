using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Кодує та декодує бінарний файл збереження формату .mvs.
    ///
    /// Формат файлу:
    ///   Header  : magic "MVSA" (4 bytes) + version (ushort, 2 bytes) + blockCount (uint, 4 bytes)
    ///   Blocks  : [blockId (uint) + blockSize (uint) + blockCrc32 (uint) + payload (blockSize bytes)] × N
    ///   Footer  : globalCrc32 (uint, 4 bytes) — CRC32 усіх попередніх байтів
    /// </summary>
    internal static class SaveFileCodec
    {
        internal static class FileLayout
        {
            public static readonly byte[] Magic = { (byte)'M', (byte)'V', (byte)'S', (byte)'A' };
            public const ushort CurrentVersion  = 1;
            public const ushort MinVersion      = 1;
            public const int    HeaderSize      = 10; // magic(4) + version(2) + blockCount(4)
            public const int    FooterSize      = 4;  // globalCrc32(4)
            public const int    MinFileSize     = HeaderSize + FooterSize;
            public const int    BlockHeaderSize = 12; // blockId(4) + blockSize(4) + blockCrc32(4)
        }

        /// <summary>
        /// Обчислює детермінований 32-бітний FNV-1a хеш повного імені типу.
        /// Використовується як унікальний blockId.
        /// </summary>
        internal static uint ComputeBlockId(Type moduleType)
            => ComputeBlockId(moduleType.FullName ?? moduleType.Name);

        /// <summary>
        /// Обчислює FNV-1a хеш із рядкового імені типу.
        /// Використовується для пошуку блоків без наявності скомпільованого типу.
        /// </summary>
        internal static uint ComputeBlockId(string fullTypeName)
        {
            uint hash = 2166136261u;
            foreach (char c in fullTypeName)
            {
                hash ^= (uint)c;
                hash *= 16777619u;
            }
            return hash;
        }

        /// <summary>Серіалізує список блоків у повний байтовий масив файлу.</summary>
        internal static byte[] Encode(List<(uint blockId, byte[] payload)> blocks)
        {
            using var body = new MemoryStream();
            using var bw   = new BinaryWriter(body);

            // Header
            bw.Write(FileLayout.Magic);
            bw.Write(FileLayout.CurrentVersion);
            bw.Write((uint)blocks.Count);

            // Blocks
            foreach (var (blockId, payload) in blocks)
            {
                uint crc = Crc32.Compute(payload);
                bw.Write(blockId);
                bw.Write((uint)payload.Length);
                bw.Write(crc);
                bw.Write(payload);
            }

            bw.Flush();
            byte[] bodyBytes = body.ToArray();

            // Footer: global CRC32
            uint globalCrc = Crc32.Compute(bodyBytes);

            var result = new byte[bodyBytes.Length + 4];
            Buffer.BlockCopy(bodyBytes, 0, result, 0, bodyBytes.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(globalCrc), 0, result, bodyBytes.Length, 4);
            return result;
        }

        internal enum DecodeError
        {
            None,
            TooSmall,
            BadMagic,
            UnsupportedVersion,
            CrcMismatch
        }

        /// <summary>
        /// Декодує файл збереження. Блоки з невалідним CRC пропускаються з попередженням.
        /// Невідомі blockId не відкидаються тут — це відповідальність викликача.
        /// </summary>
        internal static DecodeError TryDecode(
            byte[]                               data,
            out ushort                           version,
            out List<(uint blockId, byte[] payload)> blocks,
            out string                           errorMessage)
        {
            version      = 0;
            blocks       = null;
            errorMessage = null;

            if (data == null || data.Length < FileLayout.MinFileSize)
            {
                errorMessage = $"File too small ({data?.Length ?? 0}b < {FileLayout.MinFileSize}b)";
                return DecodeError.TooSmall;
            }

            // Validate magic
            var magic = FileLayout.Magic;
            if (data[0] != magic[0] || data[1] != magic[1] ||
                data[2] != magic[2] || data[3] != magic[3])
            {
                errorMessage = "Invalid magic bytes";
                return DecodeError.BadMagic;
            }

            // Validate global CRC32
            uint storedGlobal   = BitConverter.ToUInt32(data, data.Length - 4);
            uint computedGlobal = Crc32.Compute(data, 0, data.Length - 4);
            if (storedGlobal != computedGlobal)
            {
                errorMessage =
                    $"Global CRC mismatch (stored={storedGlobal:X8}, computed={computedGlobal:X8})";
                return DecodeError.CrcMismatch;
            }

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            ms.Position = 4; // skip magic
            version     = br.ReadUInt16();

            if (version < FileLayout.MinVersion || version > FileLayout.CurrentVersion)
            {
                errorMessage = $"Unsupported version {version}";
                return DecodeError.UnsupportedVersion;
            }

            uint blockCount = br.ReadUInt32();
            blocks = new List<(uint, byte[])>((int)blockCount);

            long dataBodyEnd = data.Length - FileLayout.FooterSize;

            for (uint i = 0; i < blockCount; i++)
            {
                if (ms.Position + FileLayout.BlockHeaderSize > dataBodyEnd)
                    break;

                uint blockId   = br.ReadUInt32();
                uint blockSize = br.ReadUInt32();
                uint blockCrc  = br.ReadUInt32();

                if (ms.Position + blockSize > dataBodyEnd)
                {
                    Debug.LogWarning(
                        $"[SaveFileCodec] Block id={blockId:X8} extends beyond body. Truncated.");
                    break;
                }

                byte[] payload   = br.ReadBytes((int)blockSize);
                uint   actualCrc = Crc32.Compute(payload);

                if (actualCrc != blockCrc)
                {
                    Debug.LogWarning(
                        $"[SaveFileCodec] Block id={blockId:X8} CRC mismatch " +
                        $"(stored={blockCrc:X8}, actual={actualCrc:X8}). Skipped.");
                    continue;
                }

                blocks.Add((blockId, payload));
            }

            return DecodeError.None;
        }
    }
}
