namespace Kruty1918.Moyva.Camera.API
{
    public interface ICameraZoom
    {
        void ZoomCamera(float zoomAmount);
        void ZoomCameraByScale(float scaleFactor, bool immediate);
        void ForceZoomCamera(float zoomLevel);
    }
}
