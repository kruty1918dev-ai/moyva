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
            if (wasVisible)
            {
                return;
            }

            int radius = _settings.ResolveRevealedRadius(width, height);
            var shape = _settings.ResolveRevealShape();
            _fogOfWarService.RevealArea(clampedCenter, radius, shape, keepVisible: true, visibleAreaId: _startRevealAnchorId);

            if (_settings.keepCoreFullyVisible)
                RegisterStartupCoreVisibility(width, height, clampedCenter);

            bool isVisibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(clampedCenter);
            bool isExploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(clampedCenter);
        }

        public void RevealStartingAreas(int width, int height, Vector2Int center)
        {
            int radius = _settings.ResolveRevealedRadius(width, height);
            var shape = _settings.ResolveRevealShape();
            bool visibleBefore = _fogOfWarService != null && _fogOfWarService.IsVisible(center);
            bool exploredBefore = _fogOfWarService != null && _fogOfWarService.IsExplored(center);
            _fogOfWarService.RevealArea(center, radius, shape, keepVisible: true, visibleAreaId: _startRevealAnchorId);
            _fogVisualUpdater?.PreviewRevealArea(center, radius, shape, keepVisible: true);

            bool visibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(center);
            bool exploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(center);
        }
    }
}
