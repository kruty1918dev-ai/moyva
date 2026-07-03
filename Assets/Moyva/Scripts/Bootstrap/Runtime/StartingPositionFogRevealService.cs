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

    internal sealed class StartingPositionFogRevealService
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

        public void EnsureStartRevealVisible(int width, int height, Vector2Int revealCenter)
        {
            Vector2Int clampedCenter = StartingPositionMapUtility.ClampToMap(revealCenter, width, height);
            bool wasVisible = _fogOfWarService != null && _fogOfWarService.IsVisible(clampedCenter);
            bool wasExplored = _fogOfWarService != null && _fogOfWarService.IsExplored(clampedCenter);
            Debug.Log($"{DirectDiagTag} FogReveal.EnsureVisible ENTER center={clampedCenter}, centerVisible={wasVisible}, centerExplored={wasExplored}.");
            Debug.Log($"{StartupChainTag} Fog.EnsureVisible ENTER center={clampedCenter}, map={width}x{height}, centerState={ResolveState(clampedCenter)}, samples={FormatStateSamples(clampedCenter)}.");
            Debug.Log($"{StartDiagTag} EnsureStartRevealVisible begin center={clampedCenter}, map={width}x{height}, visibleBefore={wasVisible}, exploredBefore={wasExplored}, hasFogService={_fogOfWarService != null}.");
            if (wasVisible)
            {
                Debug.Log($"{DirectDiagTag} FogReveal.EnsureVisible ACTION extraReveal=false, reason=already-visible.");
                Debug.Log($"{DirectDiagTag} FogReveal.EnsureVisible EXIT centerVisible=true, centerExplored={wasExplored || wasVisible}.");
                Debug.Log($"{StartupChainTag} Fog.EnsureVisible EXIT action=skip reason=already-visible center={clampedCenter}, centerState={ResolveState(clampedCenter)}, samples={FormatStateSamples(clampedCenter)}.");
                Debug.Log($"{_debugTag} Bootstrap.EnsureStartRevealVisible ok center={clampedCenter}, state=Visible.");
                Debug.Log($"{StartDiagTag} EnsureStartRevealVisible result center={clampedCenter}, extraReveal=false, visibleAfter=true, exploredAfter={wasExplored || wasVisible}.");
                return;
            }

            int radius = _settings.ResolveRevealedRadius(width, height);
            var shape = _settings.ResolveRevealShape();
            Debug.Log($"{DirectDiagTag} FogReveal.EnsureVisible ACTION extraReveal=true, reason=center-not-visible.");
            Debug.LogWarning($"{StartDiagTag} EnsureStartRevealVisible repair-request center={clampedCenter}, radius={radius}, shape={shape}, visibleBefore={wasVisible}, exploredBefore={wasExplored}.");
            Debug.LogWarning(
                $"{_debugTag} Bootstrap.EnsureStartRevealVisible repair center={clampedCenter}, radius={radius}, shape={shape}. " +
                "Start reveal did not become visible after the primary bootstrap pass, so the area is being forced visible again.");
            _fogOfWarService.RevealArea(clampedCenter, radius, shape, keepVisible: true, visibleAreaId: _startRevealAnchorId);

            if (_settings.keepCoreFullyVisible)
                RegisterStartupCoreVisibility(width, height, clampedCenter);

            bool isVisibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(clampedCenter);
            bool isExploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(clampedCenter);
            Debug.Log($"{DirectDiagTag} FogReveal.EnsureVisible EXIT centerVisible={isVisibleAfter}, centerExplored={isExploredAfter}.");
            Debug.Log($"{StartupChainTag} Fog.EnsureVisible EXIT action=extra-reveal center={clampedCenter}, centerState={ResolveState(clampedCenter)}, samples={FormatStateSamples(clampedCenter)}.");
            Debug.Log($"{StartDiagTag} EnsureStartRevealVisible result center={clampedCenter}, extraReveal=true, visibleAfter={isVisibleAfter}, exploredAfter={isExploredAfter}.");
        }

        public void RegisterStartupCoreVisibility(int width, int height, Vector2Int revealCenter)
        {
            if (!_settings.keepCoreFullyVisible)
            {
                Debug.Log($"{StartDiagTag} RegisterStartupCoreVisibility skipped center={revealCenter}, reason=keepCoreFullyVisible-disabled.");
                return;
            }

            if (_startAnchorRegistered)
                UnregisterStartVisionAnchors();

            int visibleRange = _settings.coreVisibleRadiusOverride > 0
                ? _settings.coreVisibleRadiusOverride
                : _settings.ResolveCoreVisibleRadius(width, height);
            if (visibleRange <= 0)
            {
                Debug.LogWarning($"{StartDiagTag} RegisterStartupCoreVisibility skipped center={revealCenter}, reason=radius<=0, radius={visibleRange}.");
                return;
            }

            string anchorId = ResolveStartVisionAnchorId(0);
            var shape = _settings.ResolveRevealShape();
            Debug.Log($"{DirectDiagTag} FogReveal.RegisterCore ENTER center={revealCenter}, radius={visibleRange}, shape={shape}, anchorId={anchorId}, hasFogService={_fogOfWarService != null}.");
            Debug.Log($"{StartupChainTag} Fog.RegisterCore ENTER center={revealCenter}, radius={visibleRange}, shape={shape}, anchorId={anchorId}, beforeState={ResolveState(revealCenter)}, samples={FormatStateSamples(revealCenter)}.");
            _fogOfWarService.RegisterFixedVisionArea(ResolveStartVisionAnchorId(0), revealCenter, visibleRange, _settings.ResolveRevealShape());
            _startAnchorRegistered = true;
            _registeredStartAnchorCount = 1;

            bool isVisibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(revealCenter);
            bool isExploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(revealCenter);
            Debug.Log($"{DirectDiagTag} FogReveal.RegisterCore AFTER centerVisible={isVisibleAfter}, centerExplored={isExploredAfter}.");
            Debug.Log($"{StartupChainTag} Fog.RegisterCore EXIT center={revealCenter}, afterState={ResolveState(revealCenter)}, samples={FormatStateSamples(revealCenter)}.");
            Debug.Log($"{StartDiagTag} RegisterStartupCoreVisibility center={revealCenter}, radius={visibleRange}, anchorId={anchorId}, shape={shape}, keepVisible=true, visibleAfter={isVisibleAfter}, exploredAfter={isExploredAfter}.");
        }

        public void RevealStartingAreas(int width, int height, Vector2Int center)
        {
            int radius = _settings.ResolveRevealedRadius(width, height);
            var shape = _settings.ResolveRevealShape();
            bool visibleBefore = _fogOfWarService != null && _fogOfWarService.IsVisible(center);
            bool exploredBefore = _fogOfWarService != null && _fogOfWarService.IsExplored(center);

            Debug.Log($"{DirectDiagTag} FogReveal.RevealStartingAreas ENTER center={center}, map={width}x{height}, radius={radius}, shape={shape}, keepVisible=true, hasFogService={_fogOfWarService != null}.");
            Debug.Log($"{DirectDiagTag} FogReveal.Before centerVisible={visibleBefore}, centerExplored={exploredBefore}.");
            Debug.Log($"{StartupChainTag} Fog.RevealStartingAreas ENTER center={center}, map={width}x{height}, radius={radius}, shape={shape}, anchorId={_startRevealAnchorId}, beforeState={ResolveState(center)}, samples={FormatStateSamples(center)}.");
            Debug.Log($"{StartDiagTag} RevealStartingAreas request center={center}, map={width}x{height}, radius={radius}, shape={shape}, keepVisible=true, hasFogService={_fogOfWarService != null}, hasVisualUpdater={_fogVisualUpdater != null}, centerVisibleBefore={visibleBefore}, centerExploredBefore={exploredBefore}.");
            Debug.Log($"{_debugTag} Bootstrap.RevealStartingAreas center={center}, radius={radius}, shape={shape}, map={width}x{height}, scaled={_settings.useMapSizeScaledFog}, keepCore={_settings.keepCoreFullyVisible}.");
            Debug.Log($"{DirectDiagTag} FogReveal.CALL FogOfWarService.RevealArea center={center}, radius={radius}.");
            _fogOfWarService.RevealArea(center, radius, shape, keepVisible: true, visibleAreaId: _startRevealAnchorId);
            _fogVisualUpdater?.PreviewRevealArea(center, radius, shape, keepVisible: true);

            bool visibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(center);
            bool exploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(center);
            Debug.Log($"{DirectDiagTag} FogReveal.After centerVisible={visibleAfter}, centerExplored={exploredAfter}.");
            Debug.Log($"{StartupChainTag} Fog.RevealStartingAreas EXIT center={center}, afterState={ResolveState(center)}, samples={FormatStateSamples(center)}.");
            Debug.Log($"{StartDiagTag} RevealStartingAreas result center={center}, centerVisibleBefore={visibleBefore}, centerExploredBefore={exploredBefore}, centerVisibleAfter={visibleAfter}, centerExploredAfter={exploredAfter}.");
            if (!visibleAfter && !exploredAfter)
                Debug.LogWarning($"{StartDiagTag} RevealStartingAreas center did not become visible or explored after RevealArea center={center}, radius={radius}, shape={shape}, map={width}x{height}.");
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
