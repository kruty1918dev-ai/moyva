using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    public interface ICameraZoom
    {
        /// <summary>ручного керування запитаного.</summary>
        event System.Action ManualControlRequested;

        void ZoomCamera(float zoomAmount);
        void ZoomCamera(float zoomAmount, Vector2 screenFocalPoint);
        void ZoomCameraByScale(float scaleFactor, bool immediate);
        void ZoomCameraByScale(float scaleFactor, bool immediate, Vector2 screenFocalPoint);
        void ForceZoomCamera(float zoomLevel);
    }
}
