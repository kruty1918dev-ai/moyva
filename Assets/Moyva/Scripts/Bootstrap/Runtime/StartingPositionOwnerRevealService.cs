using System;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Replicates the local start reveal into every participant's per-owner
    /// fog grid. The shared local grid is the local player's view only, so a
    /// remote owner's start area must be recorded in
    /// <see cref="IFogOwnerVisionSourceRegistry"/> for owner-scoped validation
    /// (authoritative placement checks on the host) to see it.
    /// </summary>
    internal interface IStartingPositionOwnerRevealService
    {
        void RevealOwnerStartAreas(int width, int height);
    }

    internal sealed class StartingPositionOwnerRevealService
        : IStartingPositionOwnerRevealService
    {
        private const string OwnerRevealAreaPrefix = "start-reveal:";
        private const string OwnerVisionAnchorPrefix = "start-vision:";

        private readonly IFogOfWarService _fogOfWarService;
        private readonly IFogOwnerVisionSourceRegistry _ownerVisionSources;
        private readonly IStartingPositionState _startingPositionState;
        private readonly StartingPositionInitializerSettings _settings;

        public StartingPositionOwnerRevealService(
            IFogOfWarService fogOfWarService,
            IStartingPositionState startingPositionState,
            StartingPositionInitializerSettings settings)
        {
            _fogOfWarService = fogOfWarService;
            _ownerVisionSources = fogOfWarService as IFogOwnerVisionSourceRegistry;
            _startingPositionState = startingPositionState;
            _settings = settings;
        }

        public void RevealOwnerStartAreas(int width, int height)
        {
            if (_fogOfWarService == null
                || _ownerVisionSources == null
                || _settings == null
                || _startingPositionState == null)
                return;

            var assignments = _startingPositionState.SpawnAssignments;
            if (assignments == null || assignments.Count == 0)
                return;

            int revealRadius = _settings.ResolveRevealedRadius(width, height);
            int coreRadius = _settings.keepCoreFullyVisible
                ? Mathf.Max(1, _settings.ResolveCoreVisibleRadius(width, height))
                : 0;
            FogRevealShape shape = _settings.ResolveRevealShape();

            for (int index = 0; index < assignments.Count; index++)
            {
                SpawnPositionAssignment assignment = assignments[index];
                string ownerId = assignment.ParticipantId;
                if (string.IsNullOrWhiteSpace(ownerId))
                    continue;

                ownerId = ownerId.Trim();
                // The local player's start area is already revealed into the
                // shared local grid by the regular reveal pipeline.
                if (_fogOfWarService.IsLocalPerspectiveOwner(ownerId))
                    continue;

                Vector2Int center = StartingPositionMapUtility.ClampToMap(
                    assignment.Position, width, height);
                _ownerVisionSources.RevealArea(
                    ownerId,
                    center,
                    revealRadius,
                    shape,
                    keepVisible: true,
                    visibleAreaId: OwnerRevealAreaPrefix + ownerId);

                if (coreRadius > 0)
                {
                    _ownerVisionSources.RegisterFixedVisionArea(
                        ownerId,
                        OwnerVisionAnchorPrefix + ownerId,
                        center,
                        coreRadius,
                        shape);
                }
            }
        }
    }
}
