using System.Collections.Generic;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.UIActions.API;
using UnityEngine.InputSystem;

namespace Kruty1918.Moyva.UIActions.Runtime
{
    public static class MoyvaUiActionCatalog
    {
        public static IReadOnlyList<UiHotkeyBinding> CreateDefaultHotkeys() => new[]
        {
            new UiHotkeyBinding(UiActionIds.Construction.Toggle, Key.B, allowedContexts: new[] { "Gameplay", "ConstructionMode" }),
            new UiHotkeyBinding(UiActionIds.Construction.ConfirmPlacement, Key.Enter, Key.NumpadEnter, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Construction.RotatePlacement, Key.R, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Construction.UndoPlacement, Key.Z, ctrl: true, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Construction.RedoPlacement, Key.Y, ctrl: true, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }),
            new UiHotkeyBinding(UiActionIds.Deployment.Confirm, Key.Enter, Key.NumpadEnter, allowedContexts: new[] { "DeploymentMode" })
        };

        public static UiEscapeRoutingOptions CreateEscapeRoutingOptions() => new()
        {
            EscapeAction = UiActionIds.Diagnostics.InputEscape,
            TextUnfocusAction = UiActionIds.Diagnostics.TextUnfocus,
            TextEditingContextId = "TextEditing"
        };
    }
}
