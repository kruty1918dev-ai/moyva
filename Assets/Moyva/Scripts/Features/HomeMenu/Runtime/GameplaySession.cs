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
        private static readonly IReadOnlyList<GameplayPlayer> EmptyPlayers = Array.AsReadOnly(Array.Empty<GameplayPlayer>());

        private readonly object _lock = new object();
        private readonly List<GameplayPlayer> _players = new List<GameplayPlayer>();
        private IReadOnlyList<GameplayPlayer> _playersSnapshot = EmptyPlayers;

        public bool IsHost { get; private set; }
        public NetworkProviderType Mode { get; private set; } = NetworkProviderType.Offline;
        public WorldSettingsDto WorldSettings { get; private set; }
        public IReadOnlyList<GameplayPlayer> Players { get { lock (_lock) return _playersSnapshot; } }
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

                _playersSnapshot = _players.Count > 0
                    ? Array.AsReadOnly(_players.ToArray())
                    : EmptyPlayers;
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
                _playersSnapshot = EmptyPlayers;
                IsHost = false;
                Mode = NetworkProviderType.Offline;
                WorldSettings = default;
            }
        }
    }
}
