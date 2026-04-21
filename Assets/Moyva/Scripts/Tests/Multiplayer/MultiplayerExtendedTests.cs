using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using NUnit.Framework;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    // ====================================================================
    // ParticipantIdentityTests — 12 tests
    // ====================================================================
    [TestFixture]
    public sealed class ParticipantIdentityTests
    {
        [Test]
        public void Constructor_SetsProperties()
        {
            var id = new ParticipantIdentity("p1", "Oleks");
            Assert.AreEqual("p1", id.PlayerId);
            Assert.AreEqual("Oleks", id.Nickname);
        }

        [Test]
        public void Constructor_NullPlayerId_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new ParticipantIdentity(null, "nick"));
        }

        [Test]
        public void Constructor_NullNickname_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new ParticipantIdentity("p1", null));
        }

        [Test]
        public void Equals_SamePlayerId_ReturnsTrue()
        {
            var a = new ParticipantIdentity("p1", "Alice");
            var b = new ParticipantIdentity("p1", "Bob");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentPlayerId_ReturnsFalse()
        {
            var a = new ParticipantIdentity("p1", "Alice");
            var b = new ParticipantIdentity("p2", "Alice");
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var a = new ParticipantIdentity("p1", "Alice");
            Assert.IsFalse(a.Equals((ParticipantIdentity)null));
        }

        [Test]
        public void Equals_Object_Works()
        {
            var a = new ParticipantIdentity("p1", "Alice");
            var b = new ParticipantIdentity("p1", "Alice");
            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void GetHashCode_SamePlayerId_Same()
        {
            var a = new ParticipantIdentity("p1", "A");
            var b = new ParticipantIdentity("p1", "B");
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToString_ContainsPlayerIdAndNickname()
        {
            var id = new ParticipantIdentity("p1", "Nick");
            var str = id.ToString();
            Assert.IsTrue(str.Contains("p1"));
            Assert.IsTrue(str.Contains("Nick"));
        }

        [Test]
        public void BotIdPrefix_IsCorrect()
        {
            Assert.AreEqual("BOT_", ParticipantIdentity.BotIdPrefix);
        }

        [Test]
        public void Equals_NullObject_ReturnsFalse()
        {
            var a = new ParticipantIdentity("p1", "X");
            Assert.IsFalse(a.Equals((object)null));
        }

        [Test]
        public void Equals_Self_ReturnsTrue()
        {
            var a = new ParticipantIdentity("p1", "X");
            Assert.IsTrue(a.Equals(a));
        }
    }

    // ====================================================================
    // ParticipantTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class ParticipantTests
    {
        [Test]
        public void Constructor_SetsAll()
        {
            var id = new ParticipantIdentity("p1", "A");
            var p = new Participant(id, false, true);
            Assert.AreEqual(id, p.Identity);
            Assert.IsFalse(p.IsBot);
            Assert.IsTrue(p.IsHost);
        }

        [Test]
        public void AsHost_ReturnsNewWithIsHostTrue()
        {
            var id = new ParticipantIdentity("p1", "A");
            var p = new Participant(id, false, false);
            var host = p.AsHost();
            Assert.IsTrue(host.IsHost);
            Assert.AreEqual(id, host.Identity);
        }

        [Test]
        public void AsHost_PreservesIsBot()
        {
            var id = new ParticipantIdentity("b1", "Bot");
            var p = new Participant(id, true, false);
            var host = p.AsHost();
            Assert.IsTrue(host.IsBot);
        }

        [Test]
        public void Bot_CreatesCorrectly()
        {
            var id = new ParticipantIdentity("BOT_1", "BotName");
            var p = new Participant(id, true, false);
            Assert.IsTrue(p.IsBot);
            Assert.IsFalse(p.IsHost);
        }

        [Test]
        public void IsHost_FalseByDefault()
        {
            var id = new ParticipantIdentity("p1", "X");
            var p = new Participant(id, false, false);
            Assert.IsFalse(p.IsHost);
        }
    }

    // ====================================================================
    // ParticipantSlotTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class ParticipantSlotTests
    {
        [Test]
        public void Constructor_SetsAll()
        {
            var s = new ParticipantSlot(0, "Red", "Player 1", true);
            Assert.AreEqual(0, s.SlotIndex);
            Assert.AreEqual("Red", s.ColorName);
            Assert.AreEqual("Player 1", s.DisplayName);
            Assert.IsTrue(s.IsOccupied);
        }

        [Test]
        public void WithOccupied_True_ChangesFlag()
        {
            var s = new ParticipantSlot(0, "Red", "P", false);
            var s2 = s.WithOccupied(true);
            Assert.IsTrue(s2.IsOccupied);
        }

        [Test]
        public void WithOccupied_PreservesOtherProperties()
        {
            var s = new ParticipantSlot(2, "Blue", "P2", false);
            var s2 = s.WithOccupied(true);
            Assert.AreEqual(2, s2.SlotIndex);
            Assert.AreEqual("Blue", s2.ColorName);
            Assert.AreEqual("P2", s2.DisplayName);
        }

        [Test]
        public void WithOccupied_False_ClearsFlag()
        {
            var s = new ParticipantSlot(0, "R", "P", true);
            var s2 = s.WithOccupied(false);
            Assert.IsFalse(s2.IsOccupied);
        }

        [Test]
        public void Immutable_OriginalUnchanged()
        {
            var s = new ParticipantSlot(0, "R", "P", false);
            s.WithOccupied(true);
            Assert.IsFalse(s.IsOccupied);
        }
    }

    // ====================================================================
    // HostMigrationServiceTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class HostMigrationServiceTests
    {
        private sealed class FakeLogger : IMultiplayerLogger
        {
            public void Info(string m) { }
            public void Warn(string m) { }
            public void Error(string m) { }
            public void Trace(string m) { }
        }

        private IHostMigrationService _service;

        [SetUp]
        public void SetUp()
        {
            var type = typeof(IHostMigrationService).Assembly
                .GetType("Kruty1918.Moyva.Multiplayer.Runtime.HostMigrationService");
            _service = (IHostMigrationService)System.Activator.CreateInstance(type, new FakeLogger());
        }

        [Test]
        public void ChooseNewHost_EmptyList_ReturnsNull()
        {
            var result = _service.ChooseNewHost(new List<Participant>());
            Assert.IsNull(result);
        }

        [Test]
        public void ChooseNewHost_SingleHuman_ReturnsAsHost()
        {
            var id = new ParticipantIdentity("p1", "Human");
            var p = new Participant(id, false, false);
            var result = _service.ChooseNewHost(new List<Participant> { p });
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsHost);
            Assert.AreEqual("p1", result.Identity.PlayerId);
        }

        [Test]
        public void ChooseNewHost_AllBots_ReturnsNull()
        {
            var b1 = new Participant(new ParticipantIdentity("b1", "Bot1"), true, false);
            var b2 = new Participant(new ParticipantIdentity("b2", "Bot2"), true, false);
            var result = _service.ChooseNewHost(new List<Participant> { b1, b2 });
            Assert.IsNull(result);
        }

        [Test]
        public void ChooseNewHost_MixedBotHuman_ReturnsHuman()
        {
            var bot = new Participant(new ParticipantIdentity("b1", "Bot"), true, false);
            var human = new Participant(new ParticipantIdentity("p1", "Human"), false, false);
            var result = _service.ChooseNewHost(new List<Participant> { bot, human });
            Assert.IsNotNull(result);
            Assert.AreEqual("p1", result.Identity.PlayerId);
        }

        [Test]
        public void ChooseNewHost_FirstHumanWins()
        {
            var h1 = new Participant(new ParticipantIdentity("p1", "H1"), false, false);
            var h2 = new Participant(new ParticipantIdentity("p2", "H2"), false, false);
            var result = _service.ChooseNewHost(new List<Participant> { h1, h2 });
            Assert.AreEqual("p1", result.Identity.PlayerId);
        }
    }

    // ====================================================================
    // WorldConsistencyExtendedTests — 6 tests
    // ====================================================================
    [TestFixture]
    public sealed class WorldConsistencyExtendedTests
    {
        private sealed class FakeLogger : IMultiplayerLogger
        {
            public void Info(string m) { }
            public void Warn(string m) { }
            public void Error(string m) { }
            public void Trace(string m) { }
        }

        private IWorldConsistencyService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new WorldConsistencyService(new FakeLogger());
        }

        [Test]
        public void Compare_BothNull_ReturnsWorldMismatch()
        {
            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(null, null));
        }

        [Test]
        public void Compare_HostNull_ReturnsWorldMismatch()
        {
            var client = new WorldSnapshot("w1", 1, 123u);
            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(null, client));
        }

        [Test]
        public void Compare_ClientNull_ReturnsWorldMismatch()
        {
            var host = new WorldSnapshot("w1", 1, 123u);
            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(host, null));
        }

        [Test]
        public void Compare_SameIdSameChecksum_ReturnsEqual()
        {
            var host = new WorldSnapshot("map1", 1, 999u);
            var client = new WorldSnapshot("map1", 1, 999u);
            Assert.AreEqual(ConsistencyCheckResult.Equal, _service.Compare(host, client));
        }

        [Test]
        public void Compare_DifferentId_ReturnsMismatch()
        {
            var host = new WorldSnapshot("map1", 1, 999u);
            var client = new WorldSnapshot("map2", 1, 999u);
            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(host, client));
        }

        [Test]
        public void Compare_SameIdDifferentChecksum_ReturnsMismatch()
        {
            var host = new WorldSnapshot("map1", 1, 100u);
            var client = new WorldSnapshot("map1", 1, 200u);
            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(host, client));
        }
    }

    // ====================================================================
    // WorldSnapshotTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class WorldSnapshotTests
    {
        [Test]
        public void Constructor_SetsAll()
        {
            var ws = new WorldSnapshot("w1", 3, 12345u);
            Assert.AreEqual("w1", ws.WorldId);
            Assert.AreEqual(3, ws.Version);
            Assert.AreEqual(12345u, ws.Checksum);
        }

        [Test]
        public void Constructor_NullWorldId_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new WorldSnapshot(null, 1, 0u));
        }

        [Test]
        public void ToString_ContainsWorldId()
        {
            var ws = new WorldSnapshot("myWorld", 1, 42u);
            Assert.IsTrue(ws.ToString().Contains("myWorld"));
        }

        [Test]
        public void ToString_ContainsVersion()
        {
            var ws = new WorldSnapshot("w", 5, 0u);
            Assert.IsTrue(ws.ToString().Contains("5"));
        }

        [Test]
        public void ToString_ContainsCrcHex()
        {
            var ws = new WorldSnapshot("w", 1, 0xDEADBEEFu);
            Assert.IsTrue(ws.ToString().Contains("DEADBEEF"));
        }
    }

    // ====================================================================
    // MultiplayerConfigTests — 8 tests
    // ====================================================================
    [TestFixture]
    public sealed class MultiplayerConfigTests
    {
        [Test]
        public void Default_ReturnsNonNull()
        {
            Assert.IsNotNull(MultiplayerConfig.Default());
        }

        [Test]
        public void Default_SchemaVersion_IsCurrent()
        {
            Assert.AreEqual(MultiplayerConfig.CurrentSchemaVersion, MultiplayerConfig.Default().SchemaVersion);
        }

        [Test]
        public void Default_ProviderType_IsRelay()
        {
            Assert.AreEqual(NetworkProviderType.Relay, MultiplayerConfig.Default().ProviderType);
        }

        [Test]
        public void Default_StrictLock_IsFalse()
        {
            Assert.IsFalse(MultiplayerConfig.Default().StrictParticipantLock);
        }

        [Test]
        public void Default_EnforceConsistency_IsTrue()
        {
            Assert.IsTrue(MultiplayerConfig.Default().EnforceConfigConsistency);
        }

        [Test]
        public void Default_Matchmaking_IsFalse()
        {
            Assert.IsFalse(MultiplayerConfig.Default().MatchmakingEnabled);
        }

        [Test]
        public void Constructor_SetsAllProperties()
        {
            var rules = SessionRules.Default();
            var cfg = new MultiplayerConfig(
                2, NetworkProviderType.WebSocket, rules,
                true, true, true);
            Assert.AreEqual(2, cfg.SchemaVersion);
            Assert.AreEqual(NetworkProviderType.WebSocket, cfg.ProviderType);
            Assert.IsTrue(cfg.StrictParticipantLock);
            Assert.IsTrue(cfg.MatchmakingEnabled);
        }

        [Test]
        public void Constructor_NullRelay_DefaultsToRelayDefault()
        {
            var cfg = new MultiplayerConfig(
                2, NetworkProviderType.Offline, SessionRules.Default(),
                false, false, false, null, null);
            Assert.IsNotNull(cfg.RelaySettings);
            Assert.IsNotNull(cfg.WebSocketSettings);
        }
    }

    // ====================================================================
    // SessionRulesExtendedTests — 6 tests
    // ====================================================================
    [TestFixture]
    public sealed class SessionRulesExtendedTests
    {
        [Test]
        public void Default_Mode_IsMultiplayerHumans()
        {
            Assert.AreEqual(SessionMode.MultiplayerHumans, SessionRules.Default().Mode);
        }

        [Test]
        public void Default_MaxParticipants_Is4()
        {
            Assert.AreEqual(4, SessionRules.Default().MaxParticipants);
        }

        [Test]
        public void Default_AllowBotsFallback_IsFalse()
        {
            Assert.IsFalse(SessionRules.Default().AllowBotsFallbackOnLeave);
        }

        [Test]
        public void Constructor_SetsAllFields()
        {
            var r = new SessionRules(SessionMode.SoloWithBots, 2, 1, 1, true, true, true);
            Assert.AreEqual(SessionMode.SoloWithBots, r.Mode);
            Assert.AreEqual(2, r.MaxParticipants);
            Assert.AreEqual(1, r.MaxHumans);
            Assert.AreEqual(1, r.MaxBots);
            Assert.IsTrue(r.AllowBotsFallbackOnLeave);
            Assert.IsTrue(r.AllowMatchSaveForAnalysis);
            Assert.IsTrue(r.StrictParticipantLock);
        }

        [Test]
        public void Default_StrictLock_IsFalse()
        {
            Assert.IsFalse(SessionRules.Default().StrictParticipantLock);
        }

        [Test]
        public void PeacefulSolo_EnumValue_IsZero()
        {
            Assert.AreEqual(0, (int)SessionMode.PeacefulSolo);
        }
    }

    // ====================================================================
    // ProviderSettingsTests — 8 tests
    // ====================================================================
    [TestFixture]
    public sealed class ProviderSettingsTests
    {
        [Test]
        public void RelayDefault_HasCorrectDefaults()
        {
            var r = RelayProviderSettings.Default();
            Assert.AreEqual(string.Empty, r.ProjectId);
            Assert.AreEqual("production", r.Environment);
            Assert.AreEqual(4, r.MaxConnections);
        }

        [Test]
        public void Relay_NullProjectId_DefaultsToEmpty()
        {
            var r = new RelayProviderSettings(null, null, null, 0);
            Assert.AreEqual(string.Empty, r.ProjectId);
        }

        [Test]
        public void Relay_ZeroMaxConnections_DefaultsTo4()
        {
            var r = new RelayProviderSettings("", "", "", 0);
            Assert.AreEqual(4, r.MaxConnections);
        }

        [Test]
        public void Relay_NullEnvironment_DefaultsProduction()
        {
            var r = new RelayProviderSettings("", null, "", 4);
            Assert.AreEqual("production", r.Environment);
        }

        [Test]
        public void WebSocketDefault_HasCorrectDefaults()
        {
            var ws = WebSocketProviderSettings.Default();
            Assert.AreEqual("ws://localhost", ws.ServerUrl);
            Assert.AreEqual(9999, ws.Port);
            Assert.AreEqual(3, ws.ReconnectAttempts);
            Assert.AreEqual(2f, ws.ReconnectDelaySeconds);
        }

        [Test]
        public void WebSocket_NullUrl_DefaultsToLocalhost()
        {
            var ws = new WebSocketProviderSettings(null, 0, null, -1, -1);
            Assert.AreEqual("ws://localhost", ws.ServerUrl);
        }

        [Test]
        public void WebSocket_ZeroPort_DefaultsTo9999()
        {
            var ws = new WebSocketProviderSettings("", 0, "", 0, 0);
            Assert.AreEqual(9999, ws.Port);
        }

        [Test]
        public void WebSocket_NegativeReconnect_DefaultsTo3()
        {
            var ws = new WebSocketProviderSettings("", 1, "", -1, -1);
            Assert.AreEqual(3, ws.ReconnectAttempts);
        }
    }

    // ====================================================================
    // SessionConnectOptionsTests — 3 tests
    // ====================================================================
    [TestFixture]
    public sealed class SessionConnectOptionsTests
    {
        [Test]
        public void Constructor_SetsAllProperties()
        {
            var id = new ParticipantIdentity("p1", "Nick");
            var rules = SessionRules.Default();
            var opts = new SessionConnectOptions(id, "room1", true, rules, 42u);
            Assert.AreEqual(id, opts.LocalIdentity);
            Assert.AreEqual("room1", opts.RoomId);
            Assert.IsTrue(opts.CreateIfNotExists);
            Assert.AreEqual(rules, opts.Rules);
            Assert.AreEqual(42u, opts.ConfigChecksum);
        }

        [Test]
        public void NullIdentity_StoresNull()
        {
            var opts = new SessionConnectOptions(null, "r", false, null, 0u);
            Assert.IsNull(opts.LocalIdentity);
        }

        [Test]
        public void Checksum_Zero_IsValid()
        {
            var opts = new SessionConnectOptions(null, "", false, null, 0u);
            Assert.AreEqual(0u, opts.ConfigChecksum);
        }
    }

    // ====================================================================
    // SlotMappingTests — 3 tests
    // ====================================================================
    [TestFixture]
    public sealed class SlotMappingTests
    {
        [Test]
        public void Constructor_SetsArray()
        {
            var mapping = new SlotMapping(new[] { 0, 2, 1 });
            Assert.AreEqual(3, mapping.OldToNewSlotIndices.Length);
            Assert.AreEqual(2, mapping.OldToNewSlotIndices[1]);
        }

        [Test]
        public void EmptyArray_IsValid()
        {
            var mapping = new SlotMapping(System.Array.Empty<int>());
            Assert.AreEqual(0, mapping.OldToNewSlotIndices.Length);
        }

        [Test]
        public void NullArray_StoresNull()
        {
            var mapping = new SlotMapping(null);
            Assert.IsNull(mapping.OldToNewSlotIndices);
        }
    }

    // ====================================================================
    // SessionResultTests — 4 tests
    // ====================================================================
    [TestFixture]
    public sealed class SessionResultTests
    {
        [Test]
        public void Ok_ReturnsSuccessTrue()
        {
            var r = SessionResult.Ok("sess1");
            Assert.IsTrue(r.Success);
            Assert.AreEqual("sess1", r.SessionId);
            Assert.IsNull(r.ErrorMessage);
        }

        [Test]
        public void Fail_ReturnsSuccessFalse()
        {
            var r = SessionResult.Fail("timeout");
            Assert.IsFalse(r.Success);
            Assert.IsNull(r.SessionId);
            Assert.AreEqual("timeout", r.ErrorMessage);
        }

        [Test]
        public void Constructor_SetsAll()
        {
            var r = new SessionResult(true, "s1", "warn");
            Assert.IsTrue(r.Success);
            Assert.AreEqual("s1", r.SessionId);
            Assert.AreEqual("warn", r.ErrorMessage);
        }

        [Test]
        public void NetworkMessage_Constructor_SetsFields()
        {
            var payload = new byte[] { 1, 2, 3 };
            var msg = new NetworkMessage("sender1", payload);
            Assert.AreEqual("sender1", msg.SenderId);
            Assert.AreEqual(payload, msg.Payload);
        }
    }

    // ====================================================================
    // GameCommandTypeEnumTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class GameCommandTypeEnumTests
    {
        [Test]
        public void UnitMove_Is1() => Assert.AreEqual(1, (int)GameCommandType.UnitMove);

        [Test]
        public void BuildingPlace_Is2() => Assert.AreEqual(2, (int)GameCommandType.BuildingPlace);

        [Test]
        public void BuildingDemolish_Is3() => Assert.AreEqual(3, (int)GameCommandType.BuildingDemolish);

        [Test]
        public void UnitSpawn_Is4() => Assert.AreEqual(4, (int)GameCommandType.UnitSpawn);

        [Test]
        public void GameStateChange_Is5() => Assert.AreEqual(5, (int)GameCommandType.GameStateChange);
    }

    // ====================================================================
    // FailureCategoryEnumTests — 3 tests
    // ====================================================================
    [TestFixture]
    public sealed class FailureCategoryEnumTests
    {
        [Test]
        public void Unknown_IsZero() => Assert.AreEqual(0, (int)FailureCategory.Unknown);

        [Test]
        public void AllValuesUnique()
        {
            var values = (FailureCategory[])System.Enum.GetValues(typeof(FailureCategory));
            var set = new HashSet<int>();
            foreach (var v in values)
                Assert.IsTrue(set.Add((int)v), $"Duplicate: {v}");
        }

        [Test]
        public void Has8Values() => Assert.AreEqual(8, System.Enum.GetValues(typeof(FailureCategory)).Length);
    }

    // ====================================================================
    // ConsistencyCheckResultEnumTests — 2 tests
    // ====================================================================
    [TestFixture]
    public sealed class ConsistencyCheckResultEnumTests
    {
        [Test]
        public void Equal_IsZero() => Assert.AreEqual(0, (int)ConsistencyCheckResult.Equal);

        [Test]
        public void Has3Values() => Assert.AreEqual(3, System.Enum.GetValues(typeof(ConsistencyCheckResult)).Length);
    }
}
