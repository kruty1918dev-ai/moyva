using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>
    /// Optional parameters for a camera focus transition. A zero-initialized
    /// request focuses the point while keeping the current zoom.
    /// </summary>
    public struct CameraFocusRequest
    {
        /// <summary>Optional zoom target (orthographic size or field of view).</summary>
        public float? TargetZoom;

        /// <summary>Extra bounds padding multiplier; 0 uses the configured default.</summary>
        public float Padding;

        /// <summary>Explicit transition duration override in seconds.</summary>
        public float? DurationOverride;

        /// <summary>Force the transition to keep the current zoom level.</summary>
        public bool KeepCurrentZoom;

        /// <summary>
        /// Allow FocusBounds to zoom in so small objects are framed closer.
        /// Default is conservative: bounds focus only zooms out when needed.
        /// </summary>
        public bool AllowZoomIn;
    }

    /// <summary>
    /// Interruptible camera focus transitions. Any player navigation input
    /// (pan, drag, orbit, wheel, pinch) cancels the active transition
    /// immediately; the player never waits for the camera.
    /// </summary>
    public interface ICameraFocusService
    {
        bool IsFocusActive { get; }

        /// <summary>Moves the view so the world point sits at screen center.</summary>
        void FocusWorldPoint(Vector3 worldPoint, CameraFocusRequest request = default);

        /// <summary>Moves and zooms the view so the bounds fit on screen.</summary>
        void FocusBounds(Bounds worldBounds, CameraFocusRequest request = default);

        /// <summary>
        /// Focuses a scene object. Uses renderer bounds when available so
        /// buildings and settlements frame correctly; falls back to the
        /// transform position.
        /// </summary>
        void FocusObject(GameObject target, CameraFocusRequest request = default);

        /// <summary>Stops the active transition and leaves the camera where it is.</summary>
        void CancelFocus();
    }
}
