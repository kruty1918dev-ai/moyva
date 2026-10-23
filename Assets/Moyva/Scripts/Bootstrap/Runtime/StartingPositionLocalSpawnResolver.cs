using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionLocalSpawnResolver
    {
        bool TryGetLocalSpawnPosition(out Vector2Int position);
        Vector2Int ResolveLocalRevealCenter(int width, int height);
    }

    internal sealed class StartingPositionLocalSpawnResolver
        : IStartingPositionLocalSpawnResolver, ICameraStartupFocusResolver
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
            string localPlayerId = !string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId)
                ? _sessionManager.LocalPlayerId
                : (GameLaunchContext.HasLocalPlayerRole ? GameLaunchContext.LocalPlayerId : string.Empty);
            if (!string.IsNullOrEmpty(localPlayerId) &&
                _startingPositionState.PlayerStartPositions.TryGetValue(localPlayerId, out position))
            {
                return true;
            }

            var assignments = _startingPositionState.SpawnAssignments;
            if (assignments.Count > 0)
            {
                position = assignments[0].Position;
                return true;
            }

            position = default;
            return false;
        }

        // Camera seam: the startup frame targets the local player's spawn, so a
        // multiplayer client frames its own assignment instead of the host's.
        public bool TryResolveStartupFocusGridPosition(out Vector2Int position)
            => TryGetLocalSpawnPosition(out position);

        public Vector2Int ResolveLocalRevealCenter(int width, int height)
        {
            if (TryGetLocalSpawnPosition(out Vector2Int localSpawn))
            {
                Vector2Int center = StartingPositionMapUtility.ClampToMap(localSpawn, width, height);
                return center;
            }

            if (_startingPositionState.IsSet)
            {
                Vector2Int center = StartingPositionMapUtility.ClampToMap(_startingPositionState.StartPosition, width, height);
                return center;
            }

            Vector2Int fallbackCenter = StartingPositionMapUtility.PickRuntimeRandomPoint(
                width,
                height,
                minMarginFromBorder: 0,
                relativeMarginFactor: 0f,
                out int seed);
            return fallbackCenter;
        }
    }
}
