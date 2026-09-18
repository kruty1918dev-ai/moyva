using Kruty1918.Moyva.Multiplayer.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    public sealed class WorldSnapshotChunkCodecTests
    {
        private const int MaxPayload = 16 * 1024 * 1024;

        [Test]
        public void Build_Parse_Roundtrip_PreservesAllFields()
        {
            var data = new byte[100];
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 251);

            var payload = WorldSnapshotChunkCodec.Build(
                transferId: 0xDEADBEEF,
                index: 3,
                count: 7,
                totalSize: 123456,
                data: data,
                dataOffset: 10,
                dataLength: 50);

            Assert.AreEqual(
                WorldSnapshotChunkCodec.HeaderBytes + 50,
                payload.Length);

            var parsed = WorldSnapshotChunkCodec.TryParse(
                payload,
                WorldSnapshotChunkCodec.DataBytes,
                MaxPayload,
                out var transferId,
                out var index,
                out var count,
                out var totalSize,
                out var dataOffset,
                out var dataLength);

            Assert.IsTrue(parsed);
            Assert.AreEqual(0xDEADBEEFu, transferId);
            Assert.AreEqual((ushort)3, index);
            Assert.AreEqual((ushort)7, count);
            Assert.AreEqual(123456, totalSize);
            Assert.AreEqual(50, dataLength);
            Assert.AreEqual(
                WorldSnapshotChunkCodec.HeaderBytes,
                dataOffset);
            for (var i = 0; i < 50; i++)
                Assert.AreEqual(data[10 + i], payload[dataOffset + i]);
        }

        [Test]
        public void TryParse_Rejects_TruncatedHeader()
        {
            var payload = new byte[WorldSnapshotChunkCodec.HeaderBytes - 1];
            payload[0] = WorldSnapshotChunkCodec.Version;
            Assert.IsFalse(TryParseDefault(payload));
        }

        [Test]
        public void TryParse_Rejects_WrongVersion()
        {
            var payload = WorldSnapshotChunkCodec.Build(
                1, 0, 1, 10, new byte[10], 0, 10);
            payload[0] = 99;
            Assert.IsFalse(TryParseDefault(payload));
        }

        [Test]
        public void TryParse_Rejects_IndexBeyondCount()
        {
            var payload = WorldSnapshotChunkCodec.Build(
                1, 5, 3, 10, new byte[10], 0, 10);
            Assert.IsFalse(TryParseDefault(payload));
        }

        [Test]
        public void TryParse_Rejects_ZeroTransferId()
        {
            var payload = WorldSnapshotChunkCodec.Build(
                0, 0, 1, 10, new byte[10], 0, 10);
            Assert.IsFalse(TryParseDefault(payload));
        }

        [Test]
        public void TryParse_Rejects_OversizedData()
        {
            var big = new byte[WorldSnapshotChunkCodec.DataBytes + 1];
            var payload = WorldSnapshotChunkCodec.Build(
                1, 0, 2, big.Length * 2, big, 0, big.Length);
            Assert.IsFalse(TryParseDefault(payload));
        }

        [Test]
        public void TryParse_Rejects_TotalSizeAboveLimit()
        {
            var payload = WorldSnapshotChunkCodec.Build(
                1, 0, 1, MaxPayload + 1, new byte[4], 0, 4);
            Assert.IsFalse(TryParseDefault(payload));
        }

        [Test]
        public void ChunkCountFor_CoversWholePayload()
        {
            Assert.AreEqual(1, WorldSnapshotChunkCodec.ChunkCountFor(0));
            Assert.AreEqual(1, WorldSnapshotChunkCodec.ChunkCountFor(1));
            Assert.AreEqual(
                1,
                WorldSnapshotChunkCodec.ChunkCountFor(
                    WorldSnapshotChunkCodec.DataBytes));
            Assert.AreEqual(
                2,
                WorldSnapshotChunkCodec.ChunkCountFor(
                    WorldSnapshotChunkCodec.DataBytes + 1));
        }

        [Test]
        public void ChunkStaysBelowTransportFrameLimit()
        {
            // LAN/Relay transport frame limit is ~60 KiB; a max-size chunk
            // (32 KiB data + header) must fit with margin.
            var payload = WorldSnapshotChunkCodec.Build(
                1, 0, 1, 1024, new byte[WorldSnapshotChunkCodec.DataBytes], 0,
                WorldSnapshotChunkCodec.DataBytes);
            Assert.LessOrEqual(payload.Length, 60 * 1024);
        }

        private static bool TryParseDefault(byte[] payload)
        {
            return WorldSnapshotChunkCodec.TryParse(
                payload,
                WorldSnapshotChunkCodec.DataBytes,
                MaxPayload,
                out _, out _, out _, out _, out _, out _);
        }
    }
}
