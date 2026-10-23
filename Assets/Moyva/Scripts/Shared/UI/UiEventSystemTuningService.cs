using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Kruty1918.Moyva.Shared.UI
{
    /// <summary>
    /// Keeps the active EventSystem pointer thresholds proportional to the
    /// display DPI. Serialized scenes carry the desktop-calibrated drag
    /// threshold (a few pixels), which turns ordinary taps into drags on
    /// high-density phone screens — presses never reach onClick. The tuning
    /// follows EventSystem.current across scene loads.
    /// </summary>
    internal sealed class UiEventSystemTuningService : ITickable
    {
        internal const int MinDragThresholdPixels = 10;
        // ~2.5 mm of finger travel before a press becomes a drag.
        internal const float DragThresholdDpiFactor = 0.1f;
        internal const float FallbackDpi = 160f;

        private EventSystem _eventSystem;

        internal static int ResolveDragThresholdPixels(float dpi)
        {
            if (float.IsNaN(dpi) || float.IsInfinity(dpi) || dpi <= 0f)
                dpi = FallbackDpi;
            return Mathf.Max(MinDragThresholdPixels, Mathf.RoundToInt(dpi * DragThresholdDpiFactor));
        }

        public void Tick()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                _eventSystem = null;
                return;
            }

            int threshold = ResolveDragThresholdPixels(Screen.dpi);
            if (!ReferenceEquals(eventSystem, _eventSystem)
                || eventSystem.pixelDragThreshold != threshold)
            {
                eventSystem.pixelDragThreshold = threshold;
                _eventSystem = eventSystem;
            }
        }
    }
}
