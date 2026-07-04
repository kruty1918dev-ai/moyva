using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogRendererCullingSettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            settings.RendererCullingMaxRenderersPerFrame = Mathf.Max(1, settings.RendererCullingMaxRenderersPerFrame);
            settings.RendererCullingDiscoveryInterval = Mathf.Max(0.05f, settings.RendererCullingDiscoveryInterval);
            settings.RendererCullingBoundsPaddingCells = Mathf.Max(0f, settings.RendererCullingBoundsPaddingCells);
            settings.UnexploredAlpha = Mathf.Clamp01(settings.UnexploredAlpha);
            settings.ExploredAlpha = Mathf.Clamp01(settings.ExploredAlpha);
        }
    }
}
