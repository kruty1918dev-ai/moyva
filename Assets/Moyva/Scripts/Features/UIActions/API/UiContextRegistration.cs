using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.UIActions.API
{
    public sealed class UiContextRegistration
    {
        public UiContextRegistration(
            string contextId,
            UiContextLayer layer,
            int priority,
            Func<bool> isActive,
            UiActionId escapeActionId = default,
            bool blocksLowerHotkeys = false,
            IEnumerable<UiActionId> allowedHotkeyActionIds = null)
        {
            ContextId = string.IsNullOrWhiteSpace(contextId) ? "Unknown" : contextId;
            Layer = layer;
            Priority = priority;
            IsActive = isActive ?? (() => true);
            EscapeActionId = escapeActionId;
            BlocksLowerHotkeys = blocksLowerHotkeys;
            AllowedHotkeyActionIds = allowedHotkeyActionIds != null
                ? new HashSet<UiActionId>(allowedHotkeyActionIds)
                : null;
        }

        public string ContextId { get; }
        public UiContextLayer Layer { get; }
        public int Priority { get; }
        public Func<bool> IsActive { get; }
        public UiActionId EscapeActionId { get; }
        public bool BlocksLowerHotkeys { get; }
        public IReadOnlyCollection<UiActionId> AllowedHotkeyActionIds { get; }
    }
}
