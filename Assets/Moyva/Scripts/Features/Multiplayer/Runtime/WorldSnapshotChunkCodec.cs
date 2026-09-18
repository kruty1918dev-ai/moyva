using System;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Wire codec for chunked world-state snapshots.
    /// Layout: version(1) transferId(4 LE) index(2 LE) count(2 LE)
    /// totalSize(4 LE) data(n). Chunks stay below the transport frame limit.
    /// </summary>
    internal static class WorldSnapshotChunkCodec
    {
        public const byte Version = 1;
        public const int HeaderBytes = 13;
        public const int DataBytes = 32 * 1024;

        public static int ChunkCountFor(int totalSize)
        {
            if (totalSize <= 0)
                return 1;
            return (totalSize + DataBytes - 1) / DataBytes;
        }

        public static byte[] Build(
            uint transferId,
            ushort index,
            ushort count,
            int totalSize,
            byte[] data,
            int dataOffset,
            int dataLength)
        {
            var payload = new byte[HeaderBytes + Math.Max(0, dataLength)];
            payload[0] = Version;
            WriteUInt32(payload, 1, transferId);
            WriteUInt16(payload, 5, index);
            WriteUInt16(payload, 7, count);
            WriteUInt32(payload, 9, (uint)totalSize);
            if (dataLength > 0)
                Buffer.BlockCopy(data, dataOffset, payload, HeaderBytes, dataLength);
            return payload;
        }

        public static bool TryParse(
            byte[] payload,
            int maxDataBytes,
            int maxTotalSize,
            out uint transferId,
            out ushort index,
            out ushort count,
            out int totalSize,
            out int dataOffset,
            out int dataLength)
        {
            transferId = 0;
            index = 0;
            count = 0;
            totalSize = 0;
            dataOffset = 0;
            dataLength = 0;

            if (payload == null || payload.Length < HeaderBytes || payload[0] != Version)
                return false;

            transferId = ReadUInt32(payload, 1);
            index = ReadUInt16(payload, 5);
            count = ReadUInt16(payload, 7);
            totalSize = (int)ReadUInt32(payload, 9);
            dataLength = payload.Length - HeaderBytes;

            if (transferId == 0
                || count == 0
                || index >= count
                || totalSize <= 0
                || totalSize > maxTotalSize
                || dataLength > maxDataBytes)
            {
                return false;
            }

            dataOffset = HeaderBytes;
            return true;
        }

        private static void WriteUInt16(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return buffer[offset]
                | ((uint)buffer[offset + 1] << 8)
                | ((uint)buffer[offset + 2] << 16)
                | ((uint)buffer[offset + 3] << 24);
        }
    }
}
