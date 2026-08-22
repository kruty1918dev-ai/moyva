using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class StartingPositionFogRevealService
    {
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
    }
}
