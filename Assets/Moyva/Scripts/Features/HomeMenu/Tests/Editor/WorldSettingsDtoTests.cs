using System.IO;
using Kruty1918.Moyva.WorldCreation.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>
    /// WorldSettingsDto wire format: v4 carries the host's turn-flow/timer
    /// selection to clients, and older payloads (v1-v3) still deserialize with
    /// -1 placeholders so the JSON turn-rules preset resolves the defaults.
    /// </summary>
    public sealed class WorldSettingsDtoTests
    {
        [Test]
        public void RoundTrip_V4_PreservesTurnSettings()
        {
            var dto = new WorldSettingsDto(
                "Host World", 424242, 1, 64, 64, MapType.Islands, Difficulty.Hard,
                4, true, turnMode: 1, turnTimerSeconds: 90);

            Assert.IsTrue(WorldSettingsDto.TryFromBytes(dto.ToBytes(), out var parsed));
            Assert.AreEqual("Host World", parsed.WorldName);
            Assert.AreEqual(424242, parsed.Seed);
            Assert.AreEqual(4, parsed.MaxPlayers);
            Assert.AreEqual(1, parsed.TurnMode);
            Assert.AreEqual(90, parsed.TurnTimerSeconds);
        }

        [Test]
        public void RoundTrip_V4_PreservesSequentialAndTimerOff()
        {
            var dto = new WorldSettingsDto(
                "Classic", 7, 0, 32, 32, MapType.Continents, Difficulty.Normal,
                2, false, turnMode: 0, turnTimerSeconds: 0);

            Assert.IsTrue(WorldSettingsDto.TryFromBytes(dto.ToBytes(), out var parsed));
            Assert.AreEqual(0, parsed.TurnMode);
            Assert.AreEqual(0, parsed.TurnTimerSeconds);
        }

        [Test]
        public void TryFromBytes_V3Payload_DefaultsTurnSettingsToPreset()
        {
            // v3 wire format: header + world fields + width/height, no turn block.
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(3);
            w.Write(99);
            w.Write(2);
            w.Write((int)MapType.Highlands);
            w.Write((int)Difficulty.Easy);
            w.Write(3);
            w.Write(false);
            w.Write("Legacy World");
            w.Write(128);
            w.Write(128);

            Assert.IsTrue(WorldSettingsDto.TryFromBytes(ms.ToArray(), out var parsed));
            Assert.AreEqual("Legacy World", parsed.WorldName);
            Assert.AreEqual(128, parsed.Width);
            Assert.AreEqual(-1, parsed.TurnMode,
                "Old payloads must defer to the JSON turn-rules preset.");
            Assert.AreEqual(-1, parsed.TurnTimerSeconds);
        }

        [Test]
        public void TryFromBytes_TruncatedV4_Fails()
        {
            var dto = new WorldSettingsDto(
                "Host World", 1, 1, 64, 64, MapType.Desert, Difficulty.Normal,
                2, false, turnMode: 1, turnTimerSeconds: 90);
            byte[] bytes = dto.ToBytes();
            var truncated = new byte[bytes.Length - 4];
            System.Array.Copy(bytes, truncated, truncated.Length);

            Assert.IsFalse(WorldSettingsDto.TryFromBytes(truncated, out _));
        }
    }
}
