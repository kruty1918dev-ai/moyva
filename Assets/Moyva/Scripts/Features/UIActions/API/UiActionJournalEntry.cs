namespace Kruty1918.Moyva.UIActions.API
{
    public readonly struct UiActionJournalEntry
    {
        public UiActionJournalEntry(
            double timestampSeconds,
            int frame,
            string actionId,
            UiActionSource source,
            string activeContext,
            string targetId,
            UiActionResultState result,
            UiActionReasonCode reason,
            string details)
        {
            TimestampSeconds = timestampSeconds;
            Frame = frame;
            ActionId = actionId;
            Source = source;
            ActiveContext = activeContext;
            TargetId = targetId;
            Result = result;
            Reason = reason;
            Details = details;
        }

        public double TimestampSeconds { get; }
        public int Frame { get; }
        public string ActionId { get; }
        public UiActionSource Source { get; }
        public string ActiveContext { get; }
        public string TargetId { get; }
        public UiActionResultState Result { get; }
        public UiActionReasonCode Reason { get; }
        public string Details { get; }
    }
}
