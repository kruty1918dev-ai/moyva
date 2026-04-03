using NUnit.Framework;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for CRC-32 implementation.
    /// Pure algorithm tests — no Unity/Zenject dependencies.
    /// </summary>
    [TestFixture]
    public class Crc32Tests
    {
        // ─── 1. Known-vector: CRC32 of "123456789" = 0xCBF43926 ────────────

        [Test]
        public void Compute_KnownVector_ReturnsExpectedValue()
        {
            byte[] input    = System.Text.Encoding.ASCII.GetBytes("123456789");
            uint   expected = 0xCBF43926u;

            Assert.AreEqual(expected, Crc32.Compute(input));
        }

        // ─── 2. Empty array returns specific constant ────────────────────

        [Test]
        public void Compute_EmptyArray_ReturnsCrc32Empty()
        {
            // CRC32 of empty input = 0x00000000
            Assert.AreEqual(0x00000000u, Crc32.Compute(new byte[0]));
        }

        // ─── 3. Single byte ──────────────────────────────────────────────

        [Test]
        public void Compute_SingleByte_IsDeterministic()
        {
            uint first  = Crc32.Compute(new byte[] { 0xAB });
            uint second = Crc32.Compute(new byte[] { 0xAB });
            Assert.AreEqual(first, second);
        }

        // ─── 4. Different inputs → different CRC ────────────────────────

        [Test]
        public void Compute_DifferentInputs_ReturnDifferentCrc()
        {
            uint a = Crc32.Compute(new byte[] { 0x01 });
            uint b = Crc32.Compute(new byte[] { 0x02 });
            Assert.AreNotEqual(a, b);
        }

        // ─── 5. Offset+length variant matches full-array variant ─────────

        [Test]
        public void Compute_WithOffsetAndLength_MatchesSlice()
        {
            var data = new byte[] { 0xFF, 0x01, 0x02, 0x03, 0xFF };
            uint full  = Crc32.Compute(new byte[] { 0x01, 0x02, 0x03 });
            uint slice = Crc32.Compute(data, 1, 3);
            Assert.AreEqual(full, slice);
        }

        // ─── 6. All-zeros buffer is deterministic ───────────────────────

        [Test]
        public void Compute_AllZeros_IsDeterministic()
        {
            uint a = Crc32.Compute(new byte[16]);
            uint b = Crc32.Compute(new byte[16]);
            Assert.AreEqual(a, b);
        }
    }
}
