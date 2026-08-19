using System;

namespace Kruty1918.Moyva.UIActions.API
{
    [Obsolete("Use UiActionReason.")]
    public enum UiActionReasonCode
    {
        None = (int)UiActionReason.None,
        WrongContext = (int)UiActionReason.WrongContext,
        ActionUnavailable = (int)UiActionReason.ActionUnavailable,
        BuildingNotOperational = (int)UiActionReason.BuildingNotOperational,
        InsufficientResources = (int)UiActionReason.InsufficientResources,
        InvalidPlacement = (int)UiActionReason.InvalidPlacement,
        NoSelection = (int)UiActionReason.NoSelection,
        ModalBlocked = (int)UiActionReason.ModalBlocked,
        AlreadyOpen = (int)UiActionReason.AlreadyOpen,
        AlreadyClosed = (int)UiActionReason.AlreadyClosed,
        TextEditing = (int)UiActionReason.TextEditing,
        Conflict = (int)UiActionReason.Conflict,
    }
}
