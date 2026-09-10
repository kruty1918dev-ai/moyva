// UgsLobbyService — Unity Gaming Services Lobby backend.
//
// SETUP:
//   1. Install package com.unity.services.multiplayer via Package Manager (contains Lobbies).
//   2. Enable Lobby + Authentication + Relay in the Unity Dashboard.
//
// Note: if the Lobbies package is not installed the service may behave as a no-op.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Real UGS Lobby implementation. Creates/joins lobbies, stores the Relay join code
    /// in lobby data, runs a heartbeat loop for the host and a poll loop for all peers.
    /// </summary>
    public sealed partial class UgsLobbyService : ILobbyService, ILobbyLocalIdentity, ILobbyHostMigrationService, IDisposable
    {
        private const string RelayCodeDataKey = "relayJoinCode";
        private const string ProjectDataKey = "moyvaProject";
        private const string ProjectDataValue = "moyva";
        private const string ProviderDataKey = "moyvaProvider";
        private const string ProviderDataValue = "relay";
        private const string PasswordHashDataKey = "moyvaPasswordHash";
        private const string StateDataKey = "moyvaState";
        private const string WorldSettingsDataKey = "moyvaWorldSettings";
        private const string ReconnectRecordsDataKey = "moyvaReconnectRecords";
        private const string ConfigFingerprintDataKey = "moyvaConfigFingerprint";
        private const string LocalTimeTicksDataKey = "localTimeTicks";
        private const float HeartbeatSeconds = 15f;
        private const float PollSeconds = 5f;
        private const float PollBackoffSeconds = 20f;
        private const float JoinRequestTimeoutSeconds = 20f;

        private readonly SemaphoreSlim _operationLock = new SemaphoreSlim(1, 1);
        private LobbyRoom _current;
        private LobbyState _state = LobbyState.Closed;
        private bool _isHost;
        private CancellationTokenSource _loopCts;


        public LobbyRoom Current => _current;
        public string LocalPlayerId => _current == null ? string.Empty : AuthenticationService.Instance.PlayerId;
        public LobbyState State => _state;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;
        public event Action<LobbyState> StateChanged;

        private Lobby _lobby;

    }
}
