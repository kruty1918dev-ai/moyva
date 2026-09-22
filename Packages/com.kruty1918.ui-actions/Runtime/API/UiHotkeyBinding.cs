using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Kruty1918.Moyva.UIActions.API
{
    public enum UiHotkeyTriggerMode
    {
        TriggeredOnce = 0,
        Held = 1,
        Repeatable = 2,
    }

    public readonly struct UiHotkeyBinding
    {
        public UiHotkeyBinding(
            UiActionId actionId,
            Key primaryKey,
            Key secondaryKey = Key.None,
            bool ctrl = false,
            bool shift = false,
            bool alt = false,
            UiHotkeyTriggerMode triggerMode = UiHotkeyTriggerMode.TriggeredOnce,
            IReadOnlyCollection<string> allowedContexts = null)
        {
            ActionId = actionId;
            PrimaryKey = primaryKey;
            SecondaryKey = secondaryKey;
            Ctrl = ctrl;
            Shift = shift;
            Alt = alt;
            TriggerMode = triggerMode;
            AllowedContexts = allowedContexts;
        }

        public UiActionId ActionId { get; }
        public Key PrimaryKey { get; }
        public Key SecondaryKey { get; }
        public bool Ctrl { get; }
        public bool Shift { get; }
        public bool Alt { get; }
        public UiHotkeyTriggerMode TriggerMode { get; }
        public IReadOnlyCollection<string> AllowedContexts { get; }

        public bool HasBinding => PrimaryKey != Key.None || SecondaryKey != Key.None;

        public static IReadOnlyList<UiHotkeyBinding> CreateDefaults() => new[]
        {
            new UiHotkeyBinding(UiActionIds.Construction.Toggle, Key.B, allowedContexts: new[] { "Gameplay", "ConstructionMode" }),
            new UiHotkeyBinding(UiActionIds.Construction.ConfirmPlacement, Key.Enter, Key.NumpadEnter, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Construction.RotatePlacement, Key.R, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Construction.UndoPlacement, Key.Z, ctrl: true, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Construction.RedoPlacement, Key.Y, ctrl: true, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Deployment.Confirm, Key.Enter, Key.NumpadEnter, allowedContexts: new[] { "DeploymentMode" })
        };
    }
}
