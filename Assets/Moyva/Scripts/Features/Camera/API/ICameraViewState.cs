using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>CameraNavigationMode — enum: камери Navigation режим.</summary>
    public enum CameraNavigationMode
    {
        /// <summary>Варіант Idle.</summary>
        Idle,
        /// <summary>Варіант Player.</summary>
        Player,
        /// <summary>Варіант FocusTransition.</summary>
        FocusTransition,
    }

    /// <summary>ICameraViewState — interface: I камери виду стану.</summary>
    public interface ICameraViewState
    {
        /// <summary>поточного зум.</summary>
        float CurrentZoom { get; }

        /// <summary>Normalized зум.</summary>
        float NormalizedZoom { get; }

        /// <summary>Smoothed Normalized зум.</summary>
        float SmoothedNormalizedZoom { get; }

        /// <summary>Чи перспективи — IsPerspective.</summary>
        bool IsPerspective { get; }

        /// <summary>фокус світу точки.</summary>
        Vector3 FocusWorldPoint { get; }

        /// <summary>орбіти рискання у градусах.</summary>
        float OrbitYawDegrees { get; }

        /// <summary>Navigation режим.</summary>
        CameraNavigationMode NavigationMode { get; }
        /// <summary>Чи фокус переходу активної — IsFocusTransitionActive.</summary>
        bool IsFocusTransitionActive { get; }
        /// <summary>Чи гравця Navigating — IsPlayerNavigating.</summary>
        bool IsPlayerNavigating { get; }
        /// <summary>Чи імпульсу активної — IsImpulseActive.</summary>
        bool IsImpulseActive { get; }
    }
}
