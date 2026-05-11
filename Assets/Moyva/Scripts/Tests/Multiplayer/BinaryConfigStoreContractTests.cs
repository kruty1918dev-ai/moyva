using System.IO;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    /// <summary>
    /// Контрактні тести серіалізації (#85): перевіряють backward-compatible round-trip
    /// для BinaryConfigStore від схеми v1 до v5.
    /// Категорія: ContractSerialization (запускається в CI quality gate).
    /// </summary>
    [TestFixture]
    [Category("ContractSerialization")]
    public sealed class BinaryConfigStoreContractTests
    {
        // ---- Helpers -------------------------------------------------------

        private static byte[] WriteToBytes(MultiplayerConfig config)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            BinaryConfigStore.WriteConfig(bw, config);
            return ms.ToArray();
        }

        private static MultiplayerConfig ReadFromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);
            return BinaryConfigStore.ReadConfig(br);
        }

        private static MultiplayerConfig MakeV5Config() => new MultiplayerConfig(
            schemaVersion: 5,
            providerType: NetworkProviderType.Relay,
            defaultSessionRules: new SessionRules(SessionMode.MultiplayerHumans, 4, 4, 0, false, false, false),
            strictParticipantLock: true,
            enforceConfigConsistency: true,
            matchmakingEnabled: false,
            relaySettings: RelayProviderSettings.Default(),
            webSocketSettings: WebSocketProviderSettings.Default(),
            fallbackProviderType: NetworkProviderType.Offline,
            reconnectLocalTimeToleranceSeconds: 180f,
            gracefulReconnectWindowSeconds: 12f,
            enableRelayProvider: true,
            enableHostMigration: true);

        // ---- Round-trip tests ---------------------------------------------

        [Test]
        public void RoundTrip_V5Config_PreservesAllFields()
        {
            var original = MakeV5Config();
            var bytes = WriteToBytes(original);
            var restored = ReadFromBytes(bytes);

            Assert.AreEqual(original.SchemaVersion, restored.SchemaVersion, "SchemaVersion");
            Assert.AreEqual(original.ProviderType, restored.ProviderType, "ProviderType");
            Assert.AreEqual(original.StrictParticipantLock, restored.StrictParticipantLock, "StrictParticipantLock");
            Assert.AreEqual(original.EnforceConfigConsistency, restored.EnforceConfigConsistency, "EnforceConfigConsistency");
            Assert.AreEqual(original.GracefulReconnectWindowSeconds, restored.GracefulReconnectWindowSeconds, 0.001f, "GracefulReconnectWindowSeconds");
            Assert.AreEqual(original.ReconnectLocalTimeToleranceSeconds, restored.ReconnectLocalTimeToleranceSeconds, 0.001f, "ReconnectLocalTimeToleranceSeconds");
            Assert.AreEqual(original.EnableRelayProvider, restored.EnableRelayProvider, "EnableRelayProvider");
            Assert.AreEqual(original.EnableHostMigration, restored.EnableHostMigration, "EnableHostMigration");
        }

        [Test]
        public void RoundTrip_V5Config_SessionRulesPreserved()
        {
            var original = MakeV5Config();
            var restored = ReadFromBytes(WriteToBytes(original));

            var or = original.DefaultSessionRules;
            var rr = restored.DefaultSessionRules;

            Assert.AreEqual(or.Mode, rr.Mode, "SessionRules.Mode");
            Assert.AreEqual(or.MaxParticipants, rr.MaxParticipants, "MaxParticipants");
            Assert.AreEqual(or.MaxHumans, rr.MaxHumans, "MaxHumans");
            Assert.AreEqual(or.MaxBots, rr.MaxBots, "MaxBots");
            Assert.AreEqual(or.AllowBotsFallbackOnLeave, rr.AllowBotsFallbackOnLeave, "AllowBotsFallbackOnLeave");
        }

        [Test]
        public void BackwardCompat_V1Bytes_LoadedWithDefaults()
        {
            // Simulates a config written by an old v1 client (only v1 fields present).
            var v1Config = new MultiplayerConfig(
                schemaVersion: 1,
                providerType: NetworkProviderType.Lan,
                defaultSessionRules: new SessionRules(SessionMode.PeacefulSolo, 2, 2, 0, false, false, false),
                strictParticipantLock: false,
                enforceConfigConsistency: false,
                matchmakingEnabled: false,
                relaySettings: RelayProviderSettings.Default(),
                webSocketSettings: WebSocketProviderSettings.Default(),
                fallbackProviderType: NetworkProviderType.Offline,
                reconnectLocalTimeToleranceSeconds: 120f,
                gracefulReconnectWindowSeconds: 8f,
                enableRelayProvider: true,
                enableHostMigration: true);

            var bytes = WriteToBytes(v1Config);

            // Read back: schema 1 → defaults are applied for v2+ fields
            var restored = ReadFromBytes(bytes);

            Assert.AreEqual(1, restored.SchemaVersion, "SchemaVersion must be 1 when stored as v1");
            Assert.AreEqual(NetworkProviderType.Lan, restored.ProviderType, "ProviderType");
            Assert.AreEqual(8f, restored.GracefulReconnectWindowSeconds, 0.001f, "GracefulReconnectWindow must fall back to 8f for v1");
            Assert.AreEqual(120f, restored.ReconnectLocalTimeToleranceSeconds, 0.001f, "ReconnectTolerance must fall back to 120f for v1");
        }

        [Test]
        public void BackwardCompat_V4Bytes_MissingGracefulWindow_DefaultApplied()
        {
            // Schema 4 does not write GracefulReconnectWindowSeconds; reader must apply 8f default.
            var v4Config = new MultiplayerConfig(
                schemaVersion: 4,
                providerType: NetworkProviderType.Relay,
                defaultSessionRules: new SessionRules(SessionMode.MultiplayerHumans, 4, 4, 0, false, false, false),
                strictParticipantLock: false,
                enforceConfigConsistency: false,
                matchmakingEnabled: true,
                relaySettings: RelayProviderSettings.Default(),
                webSocketSettings: WebSocketProviderSettings.Default(),
                fallbackProviderType: NetworkProviderType.Offline,
                reconnectLocalTimeToleranceSeconds: 60f,
                gracefulReconnectWindowSeconds: 8f,
                enableRelayProvider: true,
                enableHostMigration: false);

            var bytes = WriteToBytes(v4Config);
            var restored = ReadFromBytes(bytes);

            // v4 writes enableHostMigration but NOT gracefulReconnectWindow
            Assert.AreEqual(4, restored.SchemaVersion, "SchemaVersion");
            Assert.AreEqual(8f, restored.GracefulReconnectWindowSeconds, 0.001f,
                "GracefulReconnectWindow must be the compile-time default 8f for v4 data");
            Assert.IsFalse(restored.EnableHostMigration, "EnableHostMigration must be preserved as false from v4");
        }

        [Test]
        public void MigrationPipeline_V1_MigratesTo_V5()
        {
            // A minimal v1 config should migrate all the way to v5 via the pipeline.
            var v1 = MultiplayerConfig.Default();
            var migrated = MultiplayerConfigMigrationPipeline.MigrateToLatest(v1);

            Assert.AreEqual(5, migrated.SchemaVersion, "Migration must produce v5");
            Assert.IsTrue(migrated.GracefulReconnectWindowSeconds > 0f, "GracefulReconnectWindow must be > 0 after migration");
        }

        [Test]
        public void FileRoundTrip_SaveAndLoad_ProducesIdenticalConfig()
        {
            var tmpFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"moyva_cfg_test_{System.Guid.NewGuid():N}.dat");
            try
            {
                var store = new BinaryConfigStore(tmpFile);
                var original = MakeV5Config();
                store.Save(original);

                Assert.IsTrue(store.Exists(), "File must exist after Save()");
                var loaded = store.Load();

                Assert.AreEqual(original.GracefulReconnectWindowSeconds, loaded.GracefulReconnectWindowSeconds, 0.001f);
                Assert.AreEqual(original.ProviderType, loaded.ProviderType);
                Assert.AreEqual(original.DefaultSessionRules.MaxParticipants, loaded.DefaultSessionRules.MaxParticipants);
            }
            finally
            {
                if (System.IO.File.Exists(tmpFile))
                    System.IO.File.Delete(tmpFile);
            }
        }
    }
}
