using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>Who is currently driving the navigation pose.</summary>
    public enum CameraNavigationMode
    {
        Idle,
        Player,
        FocusTransition,
    }

    /// <summary>
    /// Canonical read-only view of the camera's current state for presentation
    /// consumers (audio, shaders, clouds, VFX, HUD). Updated once per frame.
    /// Gameplay systems must read camera state through this interface instead
    /// of reaching for Camera.main.
    /// </summary>
    public interface ICameraViewState
    {
        /// <summary>Current zoom: orthographic size or vertical field of view.</summary>
        float CurrentZoom { get; }

        /// <summary>0 = closest zoom, 1 = farthest zoom over the configured range.</summary>
        float NormalizedZoom { get; }

        /// <summary>Smoothed NormalizedZoom, frame-rate independent.</summary>
        float SmoothedNormalizedZoom { get; }

        bool IsPerspective { get; }

        /// <summary>World-space point at the screen center on the navigation plane.</summary>
        Vector3 FocusWorldPoint { get; }

        /// <summary>Camera yaw around the navigation plane normal, in degrees.</summary>
        float OrbitYawDegrees { get; }

        CameraNavigationMode NavigationMode { get; }
        bool IsFocusTransitionActive { get; }
        bool IsPlayerNavigating { get; }
        bool IsImpulseActive { get; }
    }
}
