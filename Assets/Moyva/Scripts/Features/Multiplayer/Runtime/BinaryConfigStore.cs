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
    /// </summary>
    public sealed class BinaryConfigStore : IConfigStore
    {
        private readonly string _filePath;

        public BinaryConfigStore(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(Application.persistentDataPath, "multiplayer_config.dat");
        }

        public bool Exists() => File.Exists(_filePath);

        public MultiplayerConfig Load()
        {
            if (!Exists())
                return MultiplayerConfig.Default();

            try
            {
                using var fs = File.OpenRead(_filePath);
                using var br = new BinaryReader(fs);
                return ReadConfig(br);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Multiplayer] Failed to load config: {e.Message}. Using defaults.");
                return MultiplayerConfig.Default();
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
            return new MultiplayerConfig(schemaVersion, providerType, rules, strictLock, enforceConsistency, matchmaking);
        }
    }
}
