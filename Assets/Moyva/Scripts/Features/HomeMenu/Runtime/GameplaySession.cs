using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Реалізація <see cref="IGameplaySession"/>: тримає налаштування світу й учасників між сценами.
    /// Біндиться як AsSingle у HomeMenuInstaller (бажано переносити у ProjectContext для крос-сценового життя).
    /// </summary>
    internal sealed class GameplaySession : IGameplaySession
    {
        private readonly object _lock = new object();
        private List<GameplayPlayer> _players = new List<GameplayPlayer>();

        public bool IsHost { get; private set; }
        public NetworkProviderType Mode { get; private set; } = NetworkProviderType.Offline;
        public WorldSettingsDto WorldSettings { get; private set; }
        public IReadOnlyList<GameplayPlayer> Players { get { lock (_lock) return _players.ToArray(); } }
        public GameplayPlayer LocalPlayer { get; private set; }
        public GameplayPlayer Host { get; private set; }

        public void Apply(NetworkProviderType mode, WorldSettingsDto worldSettings, IReadOnlyList<GameplayPlayer> players, string localPlayerId)
        {
            lock (_lock)
            {
                Mode = mode;
                WorldSettings = worldSettings;
                _players.Clear();
                if (players != null)
                {
                    foreach (var p in players)
                    {
                        var isLocal = !string.IsNullOrEmpty(localPlayerId) && string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal);
                        _players.Add(new GameplayPlayer(p.PlayerId, p.DisplayName, p.IsHost, isLocal));
                    }
                }

                LocalPlayer = default;
                Host = default;
                foreach (var p in _players)
                {
                    if (string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal))
                        LocalPlayer = p;
                    if (p.IsHost)
                        Host = p;
                }
                IsHost = LocalPlayer.IsHost;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _players.Clear();
                LocalPlayer = default;
                Host = default;
                IsHost = false;
                Mode = NetworkProviderType.Offline;
                WorldSettings = default;
            }
        }
    }
}
