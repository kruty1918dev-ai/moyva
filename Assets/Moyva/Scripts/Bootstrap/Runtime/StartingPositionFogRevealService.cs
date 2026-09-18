using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionFogRevealService
    {
        void EnsureStartRevealVisible(int width, int height, Vector2Int revealCenter);
        void RegisterStartupCoreVisibility(int width, int height, Vector2Int revealCenter);
        void RevealStartingAreas(int width, int height, Vector2Int center);
    }

    internal sealed partial class StartingPositionFogRevealService
        : IStartingPositionFogRevealService
    {
        private const string StartupChainTag = "[MoyvaStartupChain]";
        private const string StartDiagTag = "[MoyvaFogStartDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly IFogOfWarService _fogOfWarService;
        private readonly IFogVisualUpdater _fogVisualUpdater;
        private readonly StartingPositionInitializerSettings _settings;
        private readonly string _startVisionAnchorId;
        private readonly string _startRevealAnchorId;
        private readonly string _debugTag;

        private bool _startAnchorRegistered;
        private int _registeredStartAnchorCount;

        public StartingPositionFogRevealService(
            IFogOfWarService fogOfWarService,
            IFogVisualUpdater fogVisualUpdater,
            StartingPositionInitializerSettings settings,
            string startVisionAnchorId,
            string startRevealAnchorId,
            string debugTag)
        {
            _fogOfWarService = fogOfWarService;
            _fogVisualUpdater = fogVisualUpdater;
            _settings = settings;
            _startVisionAnchorId = startVisionAnchorId;
            _startRevealAnchorId = startRevealAnchorId;
            _debugTag = debugTag;
        }

        public void RegisterStartupCoreVisibility(int width, int height, Vector2Int revealCenter)
        {
            if (!_settings.keepCoreFullyVisible)
            {
                return;
            }

            if (_startAnchorRegistered)
                UnregisterStartVisionAnchors();

            int visibleRange = _settings.coreVisibleRadiusOverride > 0
                ? _settings.coreVisibleRadiusOverride
                : _settings.ResolveCoreVisibleRadius(width, height);
            if (visibleRange <= 0)
            {
                return;
            }

            string anchorId = ResolveStartVisionAnchorId(0);
            var shape = _settings.ResolveRevealShape();
            _fogOfWarService.RegisterFixedVisionArea(ResolveStartVisionAnchorId(0), revealCenter, visibleRange, _settings.ResolveRevealShape());
            _startAnchorRegistered = true;
            _registeredStartAnchorCount = 1;

            bool isVisibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(revealCenter);
            bool isExploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(revealCenter);
        }

        public string ResolveStartVisionAnchorId(int index)
        {
            return index <= 0 ? _startVisionAnchorId : $"{_startVisionAnchorId}-{index}";
        }

        public void UnregisterStartVisionAnchors()
        {
            int count = Mathf.Max(1, _registeredStartAnchorCount);
            for (int index = 0; index < count; index++)
                _fogOfWarService.UnregisterUnit(ResolveStartVisionAnchorId(index));

            _registeredStartAnchorCount = 0;
        }

        private FogStateType ResolveState(Vector2Int position)
        {
            return _fogOfWarService != null
                ? _fogOfWarService.GetFogState(position)
                : FogStateType.Unexplored;
        }

        private string FormatStateSamples(Vector2Int center)
        {
            if (_fogOfWarService == null)
                return "fog-service=null";

            return $"C={ResolveState(center)}, E={ResolveState(center + Vector2Int.right)}, W={ResolveState(center + Vector2Int.left)}, N={ResolveState(center + Vector2Int.up)}, S={ResolveState(center + Vector2Int.down)}";
        }
    }
}
