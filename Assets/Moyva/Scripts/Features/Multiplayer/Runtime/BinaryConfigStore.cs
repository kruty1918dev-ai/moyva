using System;
using System.IO;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Stores and retrieves MultiplayerConfig from a local binary file.
    /// Runtime-friendly; does not depend on UnityEditor.
    /// Schema v1 files are read without provider-specific settings (defaults applied).
    /// Schema v2 adds RelayProviderSettings, WebSocketProviderSettings, and FallbackProviderType.
    /// Schema v3 adds reconnect local-time tolerance.
    /// Schema v4 adds risky feature toggles.
    /// Schema v5 adds graceful reconnect window.
    /// </summary>
    public sealed class BinaryConfigStore : IConfigStore
    {
        private readonly string _filePath;

        public BinaryConfigStore(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(Application.persistentDataPath, MultiplayerClientScope.BuildScopedFileName("multiplayer_config.dat"));
        }

        public bool Exists() => File.Exists(_filePath);

        public MultiplayerConfig Load()
        {
            if (!Exists())
                return MultiplayerConfigLifecycle.ValidateAndFreeze(MultiplayerConfig.Default());

            try
            {
                using var fs = File.OpenRead(_filePath);
                using var br = new BinaryReader(fs);
                var raw = ReadConfig(br);
                var migrated = MultiplayerConfigMigrationPipeline.MigrateToLatest(raw);
                return MultiplayerConfigLifecycle.ValidateAndFreeze(migrated);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Multiplayer] Failed to load config: {e.Message}. Using defaults.");
                return MultiplayerConfigLifecycle.ValidateAndFreeze(MultiplayerConfig.Default());
            }
        }

        public void Save(MultiplayerConfig config)
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var fs = File.Create(_filePath);
                using var bw = new BinaryWriter(fs);
                WriteConfig(bw, config);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Multiplayer] Failed to save config: {e.Message}");
            }
        }

        internal static void WriteConfig(BinaryWriter bw, MultiplayerConfig config)
        {
            // v1 fields
            bw.Write(config.SchemaVersion);
            bw.Write((int)config.ProviderType);
            bw.Write(config.StrictParticipantLock);
            bw.Write(config.EnforceConfigConsistency);
            bw.Write(config.MatchmakingEnabled);

            var rules = config.DefaultSessionRules;
            bw.Write((int)rules.Mode);
            bw.Write(rules.MaxParticipants);
            bw.Write(rules.MaxHumans);
            bw.Write(rules.MaxBots);
            bw.Write(rules.AllowBotsFallbackOnLeave);
            bw.Write(rules.AllowMatchSaveForAnalysis);
            bw.Write(rules.StrictParticipantLock);

            // v2 fields — provider-specific settings
            bw.Write((int)config.FallbackProviderType);

            var relay = config.RelaySettings;
            bw.Write(relay.ProjectId);
            bw.Write(relay.Environment);
            bw.Write(relay.Region);
            bw.Write(relay.MaxConnections);

            var ws = config.WebSocketSettings;
            bw.Write(ws.ServerUrl);
            bw.Write(ws.Port);
            bw.Write(ws.AuthToken);
            bw.Write(ws.ReconnectAttempts);
            bw.Write(ws.ReconnectDelaySeconds);

            if (config.SchemaVersion >= 3)
                bw.Write(config.ReconnectLocalTimeToleranceSeconds);

            if (config.SchemaVersion >= 4)
            {
                bw.Write(config.EnableRelayProvider);
                bw.Write(config.EnableHostMigration);
            }

            if (config.SchemaVersion >= 5)
                bw.Write(config.GracefulReconnectWindowSeconds);
        }

        internal static MultiplayerConfig ReadConfig(BinaryReader br)
        {
            int schemaVersion = br.ReadInt32();
            var providerType = (NetworkProviderType)br.ReadInt32();
            bool strictLock = br.ReadBoolean();
            bool enforceConsistency = br.ReadBoolean();
            bool matchmaking = br.ReadBoolean();

            var mode = (SessionMode)br.ReadInt32();
            int maxParticipants = br.ReadInt32();
            int maxHumans = br.ReadInt32();
            int maxBots = br.ReadInt32();
            bool botsFallback = br.ReadBoolean();
            bool matchSave = br.ReadBoolean();
            bool rulesStrictLock = br.ReadBoolean();

            var rules = new SessionRules(mode, maxParticipants, maxHumans, maxBots, botsFallback, matchSave, rulesStrictLock);

            // v2 fields — use defaults for v1 configs
            var fallbackProviderType = NetworkProviderType.Offline;
            var relaySettings = RelayProviderSettings.Default();
            var webSocketSettings = WebSocketProviderSettings.Default();

            if (schemaVersion >= 2)
            {
                fallbackProviderType = (NetworkProviderType)br.ReadInt32();

                string relayProjectId = br.ReadString();
                string relayEnvironment = br.ReadString();
                string relayRegion = br.ReadString();
                int relayMaxConnections = br.ReadInt32();
                relaySettings = new RelayProviderSettings(relayProjectId, relayEnvironment, relayRegion, relayMaxConnections);

                string wsUrl = br.ReadString();
                int wsPort = br.ReadInt32();
                string wsAuthToken = br.ReadString();
                int wsReconnectAttempts = br.ReadInt32();
                float wsReconnectDelay = br.ReadSingle();
                webSocketSettings = new WebSocketProviderSettings(wsUrl, wsPort, wsAuthToken, wsReconnectAttempts, wsReconnectDelay);
            }

            float reconnectTolerance = schemaVersion >= 3 ? br.ReadSingle() : 120f;
            bool enableRelayProvider = schemaVersion >= 4 ? br.ReadBoolean() : true;
            bool enableHostMigration = schemaVersion >= 4 ? br.ReadBoolean() : true;
            float gracefulReconnectWindow = schemaVersion >= 5 ? br.ReadSingle() : 8f;

            return new MultiplayerConfig(
                schemaVersion,
                providerType,
                rules,
                strictLock,
                enforceConsistency,
                matchmaking,
                relaySettings,
                webSocketSettings,
                fallbackProviderType,
                reconnectTolerance,
                gracefulReconnectWindow,
                enableRelayProvider,
                enableHostMigration);
        }
    }
}
