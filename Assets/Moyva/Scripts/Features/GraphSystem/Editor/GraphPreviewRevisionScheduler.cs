namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Детермінований revision/debounce state machine для Live Preview.
    /// Не залежить від UI й тестується без відкриття EditorWindow.
    /// </summary>
    internal sealed class GraphPreviewRevisionScheduler
    {
        internal const double DebounceSeconds = 0.2d;

        internal long RequestedRevision { get; private set; }
        internal long RunningRevision { get; private set; }
        internal long AppliedRevision { get; private set; } = -1;
        internal double NextRunAt { get; private set; }
        internal bool IsRunning { get; private set; }

        internal long Request(double now, bool schedulingEnabled)
        {
            RequestedRevision++;
            NextRunAt = schedulingEnabled
                ? now + DebounceSeconds
                : 0d;
            return RequestedRevision;
        }

        internal bool TryBegin(
            double now,
            bool schedulingEnabled,
            bool force,
            out long revision)
        {
            revision = RequestedRevision;
            if (IsRunning)
                return false;
            if (!force
                && (!schedulingEnabled
                    || NextRunAt <= 0d
                    || now < NextRunAt))
            {
                return false;
            }

            IsRunning = true;
            RunningRevision = RequestedRevision;
            revision = RunningRevision;
            NextRunAt = 0d;
            return true;
        }

        internal bool IsCurrent(long revision)
        {
            return IsRunning
                   && RunningRevision == revision
                   && RequestedRevision == revision;
        }

        internal void Complete(
            long revision,
            bool appliedSuccessfully,
            double now,
            bool schedulingEnabled)
        {
            if (!IsRunning || revision != RunningRevision)
                return;

            if (appliedSuccessfully && RequestedRevision == revision)
                AppliedRevision = revision;

            IsRunning = false;
            if (RequestedRevision > revision && schedulingEnabled)
                NextRunAt = now + DebounceSeconds;
        }

        internal void CancelPendingSchedule()
        {
            NextRunAt = 0d;
        }

        internal void InvalidateAppliedRevision()
        {
            AppliedRevision = -1;
        }
    }
}
