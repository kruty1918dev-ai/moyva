using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionLocalSpawnResolver
    {
        bool TryGetLocalSpawnPosition(out Vector2Int position);
        Vector2Int ResolveLocalRevealCenter(int width, int height);
    }

    internal sealed class StartingPositionLocalSpawnResolver
        : IStartingPositionLocalSpawnResolver
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private readonly ISessionManager _sessionManager;
        private readonly IStartingPositionState _startingPositionState;

        public StartingPositionLocalSpawnResolver(
            ISessionManager sessionManager,
            IStartingPositionState startingPositionState)
        {
            _sessionManager = sessionManager;
            _startingPositionState = startingPositionState;
        }

        public bool TryGetLocalSpawnPosition(out Vector2Int position)
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;
            Debug.Log($"{DirectDiagTag} LocalSpawnResolver.ENTER stateSet={_startingPositionState.IsSet}, assignments={_startingPositionState.SpawnAssignments.Count}, localPlayerId={localPlayerId}.");
            if (!string.IsNullOrEmpty(localPlayerId) &&
                _startingPositionState.PlayerStartPositions.TryGetValue(localPlayerId, out position))
            {
                Debug.Log($"{DirectDiagTag} LocalSpawnResolver.RESULT center={position}, source=player-start-dictionary, found=true.");
                return true;
            }

            var assignments = _startingPositionState.SpawnAssignments;
            for (int index = 0; index < assignments.Count; index++)
            {
                if (!assignments[index].IsBot)
                {
                    position = assignments[index].Position;
                    Debug.Log($"{DirectDiagTag} LocalSpawnResolver.RESULT center={position}, source=first-non-bot-assignment, found=true.");
                    return true;
                }
            }

            position = default;
            Debug.Log($"{DirectDiagTag} LocalSpawnResolver.RESULT center={position}, source=none, found=false.");
            return false;
        }

        public Vector2Int ResolveLocalRevealCenter(int width, int height)
        {
            if (TryGetLocalSpawnPosition(out Vector2Int localSpawn))
            {
                Vector2Int center = StartingPositionMapUtility.ClampToMap(localSpawn, width, height);
                Debug.Log($"{DirectDiagTag} LocalSpawnResolver.RESULT center={center}, source=local-spawn, found=true.");
                return center;
            }

            if (_startingPositionState.IsSet)
            {
                Vector2Int center = StartingPositionMapUtility.ClampToMap(_startingPositionState.StartPosition, width, height);
                Debug.Log($"{DirectDiagTag} LocalSpawnResolver.RESULT center={center}, source=start-state, found=true.");
                return center;
            }

            Vector2Int fallbackCenter = new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));
            Debug.Log($"{DirectDiagTag} LocalSpawnResolver.RESULT center={fallbackCenter}, source=map-center-fallback, found=false.");
            return fallbackCenter;
        }
    }
}
