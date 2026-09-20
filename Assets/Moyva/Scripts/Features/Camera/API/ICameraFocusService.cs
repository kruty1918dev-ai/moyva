using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>CameraFocusRequest — struct: камери фокус запиту.</summary>
    public struct CameraFocusRequest
    {
        /// <summary>цілі зум — float?.</summary>
        public float? TargetZoom;

        /// <summary>Відступ навколо цілі при фокусуванні.</summary>
        public float Padding;

        /// <summary>тривалості перевизначення — float?.</summary>
        public float? DurationOverride;

        /// <summary>Чи поточного зум — KeepCurrentZoom.</summary>
        public bool KeepCurrentZoom;

        /// <summary>Чи зум у — AllowZoomIn.</summary>
        public bool AllowZoomIn;
    }

    /// <summary>ICameraFocusService — interface: I камери фокус сервісу.</summary>
    public interface ICameraFocusService
    {
        /// <summary>Чи фокус активної — IsFocusActive.</summary>
        bool IsFocusActive { get; }

        /// <summary>Фокусує світу точки.</summary>
        void FocusWorldPoint(Vector3 worldPoint, CameraFocusRequest request = default);

        /// <summary>Фокусує межі.</summary>
        void FocusBounds(Bounds worldBounds, CameraFocusRequest request = default);

        /// <summary>Фокусує обʼєкта.</summary>
        void FocusObject(GameObject target, CameraFocusRequest request = default);

        /// <summary>Скасовує фокус.</summary>
        void CancelFocus();
    }
}
