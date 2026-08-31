using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Shared.UI
{
    /// <summary>
    /// Canonical logical coordinate space for screen-space Moyva UI.
    /// Feature-owned views keep their own layout, while every Canvas uses
    /// the same scale calculation and sprite pixel density.
    /// </summary>
    internal static class UiCanvasScalePolicy
    {
        internal static readonly Vector2 ReferenceResolution = new(1280f, 720f);
        internal const float MatchWidthOrHeight = 0.5f;
        internal const float ReferencePixelsPerUnit = 100f;

        internal static void Apply(Canvas canvas, CanvasScaler scaler)
        {
            if (scaler == null)
                return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = ReferencePixelsPerUnit;

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                canvas.pixelPerfect = true;
        }
    }
}
