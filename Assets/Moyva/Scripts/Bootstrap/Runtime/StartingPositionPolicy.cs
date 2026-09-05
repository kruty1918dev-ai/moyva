using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionPolicy
    {
        bool HasSessionManager { get; }
        IReadOnlyList<Participant> Participants { get; }
        int ResolveStartPositionCount();
        string ResolveLocalPlayerId();
        bool CanRunStartLogic();
        bool ShouldComputeHostStartPositions();
        bool IsMultiplayerLaunchContext();
    }

    internal sealed partial class StartingPositionPolicy
        : IStartingPositionPolicy
    {
        private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly StartingPositionInitializerSettings _settings;
        private readonly ISessionManager _sessionManager;
        private readonly IStartingPositionState _startingPositionState;
        private readonly ILocalGameplayRoleResolver _roleResolver;

        public StartingPositionPolicy(
            StartingPositionInitializerSettings settings,
            ISessionManager sessionManager,
            IStartingPositionState startingPositionState,
            ILocalGameplayRoleResolver roleResolver = null)
        {
            _settings = settings;
            _sessionManager = sessionManager;
            _startingPositionState = startingPositionState;
            _roleResolver = roleResolver;
        }

        public bool HasSessionManager => _sessionManager != null;

        public IReadOnlyList<Participant> Participants => _sessionManager?.Participants;

        public int ResolveStartPositionCount()
        {
            int participantCount = _sessionManager?.Participants?.Count ?? 1;
            return GameplayLaunchTopology.ResolveStartPositionCount(
                GameLaunchContext.Mode,
                GameLaunchContext.MaxPlayers,
                GameLaunchContext.HasWorldSettings,
                participantCount,
                IsMultiplayerHost(),
                _settings.multiplayerStartSlots);
        }

        public string ResolveLocalPlayerId()
        {
            return GameplayLaunchTopology.ResolveLocalPlayerId(
                GameLaunchContext.Mode,
                ResolveSessionOrLaunchLocalPlayerId());
        }

        public bool CanRunStartLogic()
        {
            if (GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest)
                return true;
            int participantCount = _sessionManager?.Participants?.Count ?? 0;
            bool hasSession = _sessionManager != null;
            bool isHost = IsMultiplayerHost();
            string localPlayerId = ResolveSessionOrLaunchLocalPlayerId();
            bool isMultiplayerContext = IsMultiplayerLaunchContext();
            bool result;

            if (_startingPositionState.IsSet)
            {
                result = true;
                LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "start-state-set");
                return result;
            }

            if (isHost)
            {
                result = true;
                LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "local-host");
                return result;
            }

            if (_sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
            {
                result = !isMultiplayerContext;
                LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "no-session-or-participants");
                return result;
            }

            result = false;
            LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "remote-participant-context");
            return result;
        }

        public bool ShouldComputeHostStartPositions()
        {
            if (GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest)
                return true;
            int participantCount = _sessionManager?.Participants?.Count ?? 0;
            bool hasSession = _sessionManager != null;
            bool isHost = IsMultiplayerHost();
            string localPlayerId = ResolveSessionOrLaunchLocalPlayerId();
            bool isMultiplayerContext = IsMultiplayerLaunchContext();
            bool result;

            if (_sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
            {
                result = !isMultiplayerContext || isHost;
                LogPolicyDecision(nameof(ShouldComputeHostStartPositions), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "no-session-or-participants");
                return result;
            }

            result = isHost;
            LogPolicyDecision(nameof(ShouldComputeHostStartPositions), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "participants-present");
            return result;
        }

        private bool IsMultiplayerHost()
        {
            if (_roleResolver != null)
                return _roleResolver.Resolve().Role == LocalGameplayRole.Host;

            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost)
                return true;

            if (GameLaunchContext.HasLocalPlayerRole)
            {
                bool hasSessionLocalPlayer = !string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId);
                if (hasSessionLocalPlayer)
                    return string.Equals(
                        _sessionManager.LocalPlayerId.Trim(),
                        GameLaunchContext.LocalPlayerId?.Trim(),
                        System.StringComparison.Ordinal) &&
                        GameLaunchContext.IsLocalPlayerHost;

                return GameLaunchContext.IsLocalPlayerHost;
            }

            return false;
        }

        private string ResolveSessionOrLaunchLocalPlayerId()
        {
            if (!string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId))
                return _sessionManager.LocalPlayerId;

            if (GameLaunchContext.HasLocalPlayerRole &&
                !string.IsNullOrWhiteSpace(GameLaunchContext.LocalPlayerId))
            {
                return GameLaunchContext.LocalPlayerId;
            }

            return string.Empty;
        }

    }
}
