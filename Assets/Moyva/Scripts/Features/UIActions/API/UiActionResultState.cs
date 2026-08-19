using System;

namespace Kruty1918.Moyva.UIActions.API
{
    [Obsolete("Use UiActionStatus.")]
    public enum UiActionResultState
    {
        Performed = (int)UiActionStatus.Performed,
        Rejected = (int)UiActionStatus.Rejected,
        Cancelled = (int)UiActionStatus.Cancelled,
        Ignored = (int)UiActionStatus.Ignored,
    }
}
