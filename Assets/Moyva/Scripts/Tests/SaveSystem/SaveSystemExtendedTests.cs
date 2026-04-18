using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    // ====================================================================
    // SaveFileCodecExtendedTests — 10 tests
    // ====================================================================
    [TestFixture]
    public sealed class SaveFileCodecExtendedTests
    {
        [Test]
        public void Encode_EmptyBlocks_DecodesCorrectly()
        {
            var blocks = new List<(uint, byte[])>();
            byte[] data = SaveFileCodec.Encode(blocks);
            var err = SaveFileCodec.TryDecode(data, out var v, out var decoded, out _);
            Assert.AreEqual(SaveFileCodec.DecodeError.None, err);
            Assert.AreEqual(0, decoded.Count);
        }

        [Test]
        public void Encode_LargeBlock_Roundtrip()
        {
            var payload = new byte[10000];
            for (int i = 0; i < payload.Length; i++)
                payload[i] = (byte)(i % 256);
            var blocks = new List<(uint, byte[])> { (42u, payload) };
            byte[] data = SaveFileCodec.Encode(blocks);
            var err = SaveFileCodec.TryDecode(data, out _, out var decoded, out _);
            Assert.AreEqual(SaveFileCodec.DecodeError.None, err);
            Assert.AreEqual(1, decoded.Count);
            Assert.AreEqual(payload.Length, decoded[0].payload.Length);
        }

        [Test]
        public void Encode_MultipleBlocks_OrderPreserved()
        {
            var blocks = new List<(uint, byte[])>
            {
                (1u, new byte[] { 10 }),
                (2u, new byte[] { 20 }),
                (3u, new byte[] { 30 })
            };
            byte[] data = SaveFileCodec.Encode(blocks);
            var err = SaveFileCodec.TryDecode(data, out _, out var decoded, out _);
            Assert.AreEqual(SaveFileCodec.DecodeError.None, err);
            Assert.AreEqual(3, decoded.Count);
            Assert.AreEqual(1u, decoded[0].blockId);
            Assert.AreEqual(2u, decoded[1].blockId);
            Assert.AreEqual(3u, decoded[2].blockId);
        }

        [Test]
        public void TryDecode_EmptyArray_ReturnsTooSmall()
        {
            var err = SaveFileCodec.TryDecode(System.Array.Empty<byte>(), out _, out _, out _);
            Assert.AreEqual(SaveFileCodec.DecodeError.TooSmall, err);
        }

        [Test]
        public void TryDecode_WrongMagic_ReturnsBadMagic()
        {
            byte[] data = SaveFileCodec.Encode(new List<(uint, byte[])>());
            data[0] = 0xFF;
            var err = SaveFileCodec.TryDecode(data, out _, out _, out _);
            Assert.AreEqual(SaveFileCodec.DecodeError.BadMagic, err);
        }

        [Test]
        public void Version_IsCurrent()
        {
            byte[] data = SaveFileCodec.Encode(new List<(uint, byte[])>());
            SaveFileCodec.TryDecode(data, out var version, out _, out _);
            Assert.AreEqual(SaveFileCodec.FileLayout.CurrentVersion, version);
        }

        [Test]
        public void ComputeBlockId_EmptyString_NonZero()
        {
            Assert.AreNotEqual(0u, SaveFileCodec.ComputeBlockId(""));
        }

        [Test]
        public void ComputeBlockId_Deterministic()
        {
            uint a = SaveFileCodec.ComputeBlockId("TestModule");
            uint b = SaveFileCodec.ComputeBlockId("TestModule");
            Assert.AreEqual(a, b);
        }

        [Test]
        public void ComputeBlockId_Different_ForDifferentNames()
        {
            uint a = SaveFileCodec.ComputeBlockId("Alpha");
            uint b = SaveFileCodec.ComputeBlockId("Beta");
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Encode_ZeroPayload_Block_Roundtrip()
        {
            var blocks = new List<(uint, byte[])> { (99u, System.Array.Empty<byte>()) };
            byte[] data = SaveFileCodec.Encode(blocks);
            var err = SaveFileCodec.TryDecode(data, out _, out var decoded, out _);
            Assert.AreEqual(SaveFileCodec.DecodeError.None, err);
            Assert.AreEqual(1, decoded.Count);
            Assert.AreEqual(0, decoded[0].payload.Length);
        }
    }

    // ====================================================================
    // Crc32ExtendedTests — 8 tests
    // ====================================================================
    [TestFixture]
    public sealed class Crc32ExtendedTests
    {
        [Test]
        public void Compute_EmptyArray_Deterministic()
        {
            uint a = Crc32.Compute(System.Array.Empty<byte>());
            uint b = Crc32.Compute(System.Array.Empty<byte>());
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Compute_OneByte_Deterministic()
        {
            uint a = Crc32.Compute(new byte[] { 42 });
            uint b = Crc32.Compute(new byte[] { 42 });
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Compute_DifferentInputs_DifferentResults()
        {
            uint a = Crc32.Compute(new byte[] { 0 });
            uint b = Crc32.Compute(new byte[] { 1 });
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Compute_WithOffset_MatchesSubarray()
        {
            byte[] full = { 0xFF, 1, 2, 3, 0xFF };
            byte[] sub = { 1, 2, 3 };
            Assert.AreEqual(Crc32.Compute(sub), Crc32.Compute(full, 1, 3));
        }

        [Test]
        public void Compute_LargeInput_NonZero()
        {
            byte[] data = new byte[65536];
            Assert.AreNotEqual(0u, Crc32.Compute(data));
        }

        [Test]
        public void Compute_AllOnes_NonZero()
        {
            byte[] data = new byte[100];
            for (int i = 0; i < data.Length; i++) data[i] = 0xFF;
            Assert.AreNotEqual(0u, Crc32.Compute(data));
        }

        [Test]
        public void Compute_Sequential_IsDeterministic()
        {
            byte[] data = new byte[256];
            for (int i = 0; i < 256; i++) data[i] = (byte)i;
            uint a = Crc32.Compute(data);
            uint b = Crc32.Compute(data);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Compute_FullRange_VsSlice_Matching()
        {
            byte[] data = { 10, 20, 30, 40, 50 };
            Assert.AreEqual(Crc32.Compute(data), Crc32.Compute(data, 0, data.Length));
        }
    }

    // ====================================================================
    // SaveSlotInfoTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class SaveSlotInfoTests
    {
        [Test]
        public void Constructor_SetsAll()
        {
            var dt = new System.DateTime(2025, 1, 1);
            var info = new SaveSlotInfo(1, true, 1024, dt);
            Assert.AreEqual(1, info.Slot);
            Assert.IsTrue(info.Exists);
            Assert.AreEqual(1024, info.FileSizeBytes);
            Assert.AreEqual(dt, info.LastWriteTimeUtc);
        }

        [Test]
        public void NonExistent_HasZeroSize()
        {
            var info = new SaveSlotInfo(0, false, 0, System.DateTime.MinValue);
            Assert.IsFalse(info.Exists);
            Assert.AreEqual(0, info.FileSizeBytes);
        }

        [Test]
        public void Slot_NegativeValue_Stores()
        {
            var info = new SaveSlotInfo(-1, false, 0, System.DateTime.MinValue);
            Assert.AreEqual(-1, info.Slot);
        }

        [Test]
        public void LargeSize_Stores()
        {
            var info = new SaveSlotInfo(0, true, long.MaxValue, System.DateTime.MinValue);
            Assert.AreEqual(long.MaxValue, info.FileSizeBytes);
        }

        [Test]
        public void DateTime_MinValue_IsDefault()
        {
            var info = new SaveSlotInfo(0, false, 0, System.DateTime.MinValue);
            Assert.AreEqual(System.DateTime.MinValue, info.LastWriteTimeUtc);
        }
    }
}
