using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// LAN room discovery and lobby snapshots exchanged over UDP.
    /// </summary>
    public sealed partial class LanLobbyService : ILobbyService, ILobbyLocalIdentity, ILobbyHostMigrationService, IDisposable
    {
        public const int DefaultPort = 54545;
        private const int DiscoveryPort = 54544;
        private const string PayloadProtocol = "MOYVA_LAN_LOBBY_V1";
        private const string DiscoveryQuery = "QUERY";
        private const int BroadcastIntervalMs = 1000;
        private const int QueryTimeoutMs = 2500;
        private const int QueryRetryIntervalMs = 500;
        private const int DiscoveredRoomTtlMs = 8000;
        private const int ShortLobbyCodeLength = 6;

        private readonly UdpClient _udp;
        private readonly IPEndPoint _broadcastEndPoint;
        private readonly IPEndPoint _loopbackEndPoint;
        private readonly object _stateLock = new object();
        private readonly object _discoveryLock = new object();
        private readonly Dictionary<string, DiscoveredRoomEntry> _discoveredRooms = new Dictionary<string, DiscoveredRoomEntry>(StringComparer.OrdinalIgnoreCase);

        private CancellationTokenSource _cts;
        private UdpClient _listenUdp;
        private LobbyRoom _current;
        private string _currentPasswordHash = string.Empty;
        private LobbyState _state = LobbyState.Closed;
        private string _lastDiscoveryError;
        private volatile bool _disposed;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<LobbyState> StateChanged;
#pragma warning disable CS0067
        public event Action<string> KickedFromLobby;
#pragma warning restore CS0067

        public LobbyRoom Current => _current;
        public string LocalPlayerId => BuildLocalHostId();
        public LobbyState State => _state;

        public LanLobbyService()
        {
            _udp = new UdpClient();
            _udp.EnableBroadcast = true;
            _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            _loopbackEndPoint = new IPEndPoint(IPAddress.Loopback, DiscoveryPort);
        }

        private sealed class DiscoveredRoomEntry
        {
            public LobbyRoom Room { get; }
            public DateTime LastSeenUtc { get; }

            public DiscoveredRoomEntry(LobbyRoom room, DateTime lastSeenUtc)
            {
                Room = room;
                LastSeenUtc = lastSeenUtc;
            }
        }

    }
}
