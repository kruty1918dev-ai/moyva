using UnityEditor;

namespace Kruty1918.Moyva.Editor.Shared
{
    public sealed class EditorLivePreviewThrottle
    {
        private double _nextRepaintAt;
        private double _nextCostlyTickAt;

        public double RepaintIntervalSeconds { get; }
        public double CostlyTickIntervalSeconds { get; }

        public EditorLivePreviewThrottle(double repaintFps = 30.0, double costlyTickHz = 12.0)
        {
            RepaintIntervalSeconds = 1.0 / System.Math.Max(1.0, repaintFps);
            CostlyTickIntervalSeconds = 1.0 / System.Math.Max(1.0, costlyTickHz);
        }

        public bool TryRepaint(EditorWindow window, bool force = false)
        {
            if (window == null)
                return false;

            double now = EditorApplication.timeSinceStartup;
            if (!force && now < _nextRepaintAt)
                return false;

            _nextRepaintAt = now + RepaintIntervalSeconds;
            window.Repaint();
            return true;
        }

        public bool ShouldRunCostlyTick(bool force = false)
        {
            double now = EditorApplication.timeSinceStartup;
            if (!force && now < _nextCostlyTickAt)
                return false;

            _nextCostlyTickAt = now + CostlyTickIntervalSeconds;
            return true;
        }

        public void Reset()
        {
            _nextRepaintAt = 0;
            _nextCostlyTickAt = 0;
        }
    }
}
