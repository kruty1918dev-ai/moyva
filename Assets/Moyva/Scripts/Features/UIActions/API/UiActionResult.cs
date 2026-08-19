namespace Kruty1918.Moyva.UIActions.API
{
    public readonly struct UiActionResult
    {
        public UiActionResult(
            UiActionResultState state,
            UiActionReasonCode reason = UiActionReasonCode.None,
            string details = null)
        {
            State = state;
            Reason = reason;
            Details = details;
        }

        public UiActionResultState State { get; }
        public UiActionReasonCode Reason { get; }
        public string Details { get; }
        public bool Consumed => State == UiActionResultState.Performed
            || State == UiActionResultState.Cancelled;

        public static UiActionResult Performed()
            => new(UiActionResultState.Performed);

        public static UiActionResult Rejected(
            UiActionReasonCode reason,
            string details = null)
            => new(UiActionResultState.Rejected, reason, details);

        public static UiActionResult Ignored(
            UiActionReasonCode reason = UiActionReasonCode.None,
            string details = null)
            => new(UiActionResultState.Ignored, reason, details);

        public static UiActionResult Cancelled(
            UiActionReasonCode reason = UiActionReasonCode.None,
            string details = null)
            => new(UiActionResultState.Cancelled, reason, details);
    }
}
