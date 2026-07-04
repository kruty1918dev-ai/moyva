using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Creates editor-only fallback services so the controller stays a thin scene host.
    /// </summary>
    internal static class FogVolumeControllerEditorFallbackFactory
    {
        internal static IFogVolumePreviewBuilder CreatePreviewBuilder()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new FogVolumePreviewBuilder();
#endif
            return null;
        }

        internal static IFogVolumeSceneContextBuilder CreateSceneContextBuilder()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new FogVolumeSceneContextBuilder();
#endif
            return null;
        }

        internal static IFogVolumeOutputCleaner CreateOutputCleaner()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new FogVolumeOutputCleaner();
#endif
            return null;
        }

        internal static IFogVolumeValidationService CreateValidationService()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new FogVolumeValidationService();
#endif
            return null;
        }
    }
}
