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

    internal sealed class StartingPositionPolicy
        : IStartingPositionPolicy
    {
        private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly StartingPositionInitializerSettings _settings;
        private readonly ISessionManager _sessionManager;
        private readonly IStartingPositionState _startingPositionState;

        public StartingPositionPolicy(
            StartingPositionInitializerSettings settings,
            ISessionManager sessionManager,
            IStartingPositionState startingPositionState)
        {
            _settings = settings;
            _sessionManager = sessionManager;
            _startingPositionState = startingPositionState;
        }

        public bool HasSessionManager => _sessionManager != null;

        public IReadOnlyList<Participant> Participants => _sessionManager?.Participants;

        public int ResolveStartPositionCount()
        {
            int participantCount = _sessionManager?.Participants?.Count ?? 1;
            if (participantCount > 1 || IsMultiplayerHost())
                return Mathf.Max(participantCount, _settings.multiplayerStartSlots);

            if (GameLaunchContext.HasWorldSettings && GameLaunchContext.MaxPlayers > 1)
                return Mathf.Max(GameLaunchContext.MaxPlayers, _settings.multiplayerStartSlots);

            return 1;
        }

        public string ResolveLocalPlayerId()
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;
            if (!string.IsNullOrEmpty(localPlayerId))
                return localPlayerId;

            return "local-player";
        }

        public bool CanRunStartLogic()
        {
            int participantCount = _sessionManager?.Participants?.Count ?? 0;
            bool hasSession = _sessionManager != null;
            bool isHost = _sessionManager != null && _sessionManager.IsLocalPlayerHost;
            string localPlayerId = _sessionManager?.LocalPlayerId ?? string.Empty;
            bool isMultiplayerContext = IsMultiplayerLaunchContext();
            bool result;

            if (_startingPositionState.IsSet)
            {
                result = true;
                Debug.Log($"{DirectDiagTag} Policy.CanRunStartLogic mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, hasSession={hasSession}, participants={participantCount}, result={result}, reason=start-state-set.");
                LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "start-state-set");
                return result;
            }

            if (isHost)
            {
                result = true;
                Debug.Log($"{DirectDiagTag} Policy.CanRunStartLogic mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, hasSession={hasSession}, participants={participantCount}, result={result}, reason=local-host.");
                LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "local-host");
                return result;
            }

            if (_sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
            {
                result = !isMultiplayerContext;
                Debug.Log($"{DirectDiagTag} Policy.CanRunStartLogic mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, hasSession={hasSession}, participants={participantCount}, result={result}, reason=no-session-or-participants.");
                LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "no-session-or-participants");
                return result;
            }

            result = false;
            Debug.Log($"{DirectDiagTag} Policy.CanRunStartLogic mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, hasSession={hasSession}, participants={participantCount}, result={result}, reason=remote-participant-context.");
            LogPolicyDecision(nameof(CanRunStartLogic), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "remote-participant-context");
            return result;
        }

        public bool IsMultiplayerHost()
        {
            return _sessionManager != null && _sessionManager.IsLocalPlayerHost;
        }

        public bool ShouldComputeHostStartPositions()
        {
            int participantCount = _sessionManager?.Participants?.Count ?? 0;
            bool hasSession = _sessionManager != null;
            bool isHost = _sessionManager != null && _sessionManager.IsLocalPlayerHost;
            string localPlayerId = _sessionManager?.LocalPlayerId ?? string.Empty;
            bool isMultiplayerContext = IsMultiplayerLaunchContext();
            bool result;

            if (_sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
            {
                result = !isMultiplayerContext;
                Debug.Log($"{DirectDiagTag} Policy.ShouldComputeHostStartPositions mode={GameLaunchContext.Mode}, isMultiplayer={isMultiplayerContext}, hasSession={hasSession}, participants={participantCount}, isLocalHost={isHost}, localPlayerId={(string.IsNullOrEmpty(localPlayerId) ? "<empty>" : localPlayerId)}, result={result}, reason=no-session-or-participants.");
                LogPolicyDecision(nameof(ShouldComputeHostStartPositions), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "no-session-or-participants");
                return result;
            }

            result = _sessionManager.IsLocalPlayerHost;
            Debug.Log($"{DirectDiagTag} Policy.ShouldComputeHostStartPositions mode={GameLaunchContext.Mode}, isMultiplayer={isMultiplayerContext}, hasSession={hasSession}, participants={participantCount}, isLocalHost={isHost}, localPlayerId={(string.IsNullOrEmpty(localPlayerId) ? "<empty>" : localPlayerId)}, result={result}, reason=participants-present.");
            LogPolicyDecision(nameof(ShouldComputeHostStartPositions), participantCount, hasSession, isHost, localPlayerId, isMultiplayerContext, result, "participants-present");
            return result;
        }

        public bool IsMultiplayerLaunchContext()
        {
            bool result = IsMultiplayerLaunchContextStatic();
            int participantCount = _sessionManager?.Participants?.Count ?? 0;
            bool hasSession = _sessionManager != null;
            bool isHost = _sessionManager != null && _sessionManager.IsLocalPlayerHost;
            string localPlayerId = _sessionManager?.LocalPlayerId ?? string.Empty;
            bool maxPlayersSuggestsMultiplayer = GameLaunchContext.MaxPlayers > 1;
            bool realMultiplayerMode = GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;

            Debug.Log(
                $"{PolicyDiagTag} Policy.{nameof(IsMultiplayerLaunchContext)} mode={GameLaunchContext.Mode}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                $"maxPlayersSuggestsMultiplayer={maxPlayersSuggestsMultiplayer}, realMultiplayerMode={realMultiplayerMode}, hasSession={hasSession}, " +
                $"participants={participantCount}, isHost={isHost}, localPlayerId={(string.IsNullOrEmpty(localPlayerId) ? "<empty>" : localPlayerId)}, result={result}.");
            Debug.Log($"{DirectDiagTag} Policy.IsMultiplayerLaunchContext mode={GameLaunchContext.Mode}, maxPlayers={GameLaunchContext.MaxPlayers}, maxPlayersSuggestsMultiplayer={maxPlayersSuggestsMultiplayer}, realMultiplayerMode={realMultiplayerMode}, result={result}.");

            return result;
        }

        public static bool IsMultiplayerLaunchContextStatic()
        {
            return GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;
        }

        private static void LogPolicyDecision(
            string methodName,
            int participantCount,
            bool hasSession,
            bool isHost,
            string localPlayerId,
            bool isMultiplayerContext,
            bool result,
            string reason)
        {
            bool maxPlayersSuggestsMultiplayer = GameLaunchContext.MaxPlayers > 1;
            bool realMultiplayerMode = GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;

            Debug.Log(
                $"{PolicyDiagTag} Policy.{methodName} mode={GameLaunchContext.Mode}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                $"maxPlayersSuggestsMultiplayer={maxPlayersSuggestsMultiplayer}, realMultiplayerMode={realMultiplayerMode}, " +
                $"isMultiplayerContext={isMultiplayerContext}, hasSession={hasSession}, participants={participantCount}, " +
                $"isHost={isHost}, localPlayerId={(string.IsNullOrEmpty(localPlayerId) ? "<empty>" : localPlayerId)}, result={result}, reason={reason}.");
        }
    }
}
