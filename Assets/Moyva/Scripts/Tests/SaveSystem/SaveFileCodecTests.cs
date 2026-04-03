using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for SaveFileCodec (Encode / TryDecode) and related utilities.
    /// Pure algorithm tests — no Unity/Zenject dependencies.
    /// </summary>
    [TestFixture]
    public class SaveFileCodecTests
    {
        // ─── Helpers ─────────────────────────────────────────────────────

        private static byte[] EncodeOne(byte[] payload)
        {
            uint blockId = 0xDEADBEEFu;
            var  blocks  = new List<(uint, byte[])> { (blockId, payload) };
            return SaveFileCodec.Encode(blocks);
        }

        private static byte[] EncodeEmpty()
            => SaveFileCodec.Encode(new List<(uint, byte[])>());

        // ─── 1. Encode → TryDecode roundtrip (no modules) ────────────────

        [Test]
        public void Encode_NoBlocks_ProducesValidFile()
        {
            byte[] data   = EncodeEmpty();
            var    result = SaveFileCodec.TryDecode(data, out ushort ver, out var blocks, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.None, result);
            Assert.AreEqual(SaveFileCodec.FileLayout.CurrentVersion, ver);
            Assert.IsNotNull(blocks);
            Assert.AreEqual(0, blocks.Count);
        }

        // ─── 2. Header magic present ─────────────────────────────────────

        [Test]
        public void Encode_Header_HasCorrectMagic()
        {
            byte[] data  = EncodeEmpty();
            var    magic = SaveFileCodec.FileLayout.Magic;

            Assert.AreEqual(magic[0], data[0]);
            Assert.AreEqual(magic[1], data[1]);
            Assert.AreEqual(magic[2], data[2]);
            Assert.AreEqual(magic[3], data[3]);
        }

        // ─── 3. Single block encode+decode roundtrip ─────────────────────

        [Test]
        public void Encode_SingleBlock_DecodesCorrectly()
        {
            byte[] payload = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            uint   id      = 0xDEADBEEFu;
            byte[] data    = SaveFileCodec.Encode(new List<(uint, byte[])> { (id, payload) });

            var result = SaveFileCodec.TryDecode(data, out _, out var blocks, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.None, result);
            Assert.AreEqual(1, blocks.Count);
            Assert.AreEqual(id, blocks[0].blockId);
            CollectionAssert.AreEqual(payload, blocks[0].payload);
        }

        // ─── 4. Corrupted global CRC → CrcMismatch ───────────────────────

        [Test]
        public void TryDecode_CorruptedGlobalCrc_ReturnsCrcMismatch()
        {
            byte[] data = EncodeEmpty();
            data[data.Length - 1] ^= 0xFF; // flip last byte of global CRC

            var result = SaveFileCodec.TryDecode(data, out _, out _, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.CrcMismatch, result);
        }

        // ─── 5. Bad magic → BadMagic ─────────────────────────────────────

        [Test]
        public void TryDecode_BadMagic_ReturnsBadMagic()
        {
            byte[] data = EncodeEmpty();
            data[0] = 0x00; // corrupt magic

            var result = SaveFileCodec.TryDecode(data, out _, out _, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.BadMagic, result);
        }

        // ─── 6. Too small → TooSmall ─────────────────────────────────────

        [Test]
        public void TryDecode_TooSmall_ReturnsTooSmall()
        {
            var result = SaveFileCodec.TryDecode(new byte[3], out _, out _, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.TooSmall, result);
        }

        // ─── 7. Null data → TooSmall ─────────────────────────────────────

        [Test]
        public void TryDecode_NullData_ReturnsTooSmall()
        {
            var result = SaveFileCodec.TryDecode(null, out _, out _, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.TooSmall, result);
        }

        // ─── 8. Block with corrupted CRC is skipped ──────────────────────

        [Test]
        public void TryDecode_CorruptedBlockCrc_SkipsBlock()
        {
            byte[] payload = new byte[] { 0xAA, 0xBB };
            byte[] data    = EncodeOne(payload);

            // The block CRC is at byte offset:
            // Header(10) + blockId(4) + blockSize(4) = offset 18, then crc32 is 4 bytes at 18..21
            // Corrupt one byte of the block CRC
            data[20] ^= 0xFF;

            // Also fix global CRC to pass global check
            uint newGlobal = Crc32.Compute(data, 0, data.Length - 4);
            byte[] gcrcBytes = System.BitConverter.GetBytes(newGlobal);
            System.Buffer.BlockCopy(gcrcBytes, 0, data, data.Length - 4, 4);

            var result = SaveFileCodec.TryDecode(data, out _, out var blocks, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.None, result);
            Assert.AreEqual(0, blocks.Count, "Corrupted block should be skipped");
        }

        // ─── 9. FNV-1a blockId is deterministic ──────────────────────────

        [Test]
        public void ComputeBlockId_IsDeterministic()
        {
            uint first  = SaveFileCodec.ComputeBlockId(typeof(string));
            uint second = SaveFileCodec.ComputeBlockId(typeof(string));
            Assert.AreEqual(first, second);
        }

        // ─── 10. Different types produce different blockIds ───────────────

        [Test]
        public void ComputeBlockId_DifferentTypes_ProduceDifferentIds()
        {
            uint a = SaveFileCodec.ComputeBlockId(typeof(int));
            uint b = SaveFileCodec.ComputeBlockId(typeof(string));
            Assert.AreNotEqual(a, b);
        }

        // ─── 11. Multiple blocks roundtrip ───────────────────────────────

        [Test]
        public void Encode_MultipleBlocks_AllDecodeCorrectly()
        {
            var blocks = new List<(uint, byte[])>
            {
                (0x00000001u, new byte[] { 0x01 }),
                (0x00000002u, new byte[] { 0x02, 0x03 }),
                (0x00000003u, new byte[] { 0x04, 0x05, 0x06 }),
            };

            byte[] data   = SaveFileCodec.Encode(blocks);
            var    result = SaveFileCodec.TryDecode(data, out _, out var decoded, out _);

            Assert.AreEqual(SaveFileCodec.DecodeError.None, result);
            Assert.AreEqual(3, decoded.Count);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(blocks[i].Item1, decoded[i].blockId);
                CollectionAssert.AreEqual(blocks[i].Item2, decoded[i].payload);
            }
        }
    }
}
