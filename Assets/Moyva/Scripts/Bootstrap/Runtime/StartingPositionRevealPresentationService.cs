using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionRevealPresentationService
    {
        Vector2Int ResolveRevealCenter(int width, int height);
        void ApplyReveal(int width, int height, Vector2Int revealCenter);
        void TeleportMainCamera(Vector2Int startPos, WorldGeneratedDataSignal signal);
    }

    internal sealed class StartingPositionRevealPresentationService
        : IStartingPositionRevealPresentationService
    {
        private readonly IStartingPositionLocalSpawnResolver _localSpawnResolver;
        private readonly IStartingPositionFogRevealService _fogRevealService;
        private readonly IStartingPositionCameraService _cameraService;

        public StartingPositionRevealPresentationService(
            IStartingPositionLocalSpawnResolver localSpawnResolver,
            IStartingPositionFogRevealService fogRevealService,
            IStartingPositionCameraService cameraService)
        {
            _localSpawnResolver = localSpawnResolver;
            _fogRevealService = fogRevealService;
            _cameraService = cameraService;
        }

        public Vector2Int ResolveRevealCenter(int width, int height)
        {
            return _localSpawnResolver.ResolveLocalRevealCenter(width, height);
        }

        public void ApplyReveal(int width, int height, Vector2Int revealCenter)
        {
            _fogRevealService.RevealStartingAreas(width, height, revealCenter);
            _fogRevealService.RegisterStartupCoreVisibility(width, height, revealCenter);
            _fogRevealService.EnsureStartRevealVisible(width, height, revealCenter);
        }

        public void TeleportMainCamera(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            _cameraService.TeleportMainCamera(startPos, signal);
        }
    }
}
