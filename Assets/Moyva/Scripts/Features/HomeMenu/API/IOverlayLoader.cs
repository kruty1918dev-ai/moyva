namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface IOverlayLoader
    {
        OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%");
        void UpdateOverlay(float value, float maxValue = 100, string sufix = "%");
        // If forceImmediate == true, the implementation should hide the overlay immediately
        // (skip delayed animation) to guarantee the panel is closed when initialization completes.
        void StopOverlay(bool forceImmediate = false);
    }
}