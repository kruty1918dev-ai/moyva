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
            string escapeActionId = null,
            bool blocksLowerHotkeys = false,
            IEnumerable<string> allowedHotkeyActionIds = null)
        {
            ContextId = string.IsNullOrWhiteSpace(contextId) ? "Unknown" : contextId;
            Layer = layer;
            Priority = priority;
            IsActive = isActive ?? (() => true);
            EscapeActionId = escapeActionId;
            BlocksLowerHotkeys = blocksLowerHotkeys;
            AllowedHotkeyActionIds = allowedHotkeyActionIds != null
                ? new HashSet<string>(allowedHotkeyActionIds)
                : null;
        }

        public string ContextId { get; }
        public UiContextLayer Layer { get; }
        public int Priority { get; }
        public Func<bool> IsActive { get; }
        public string EscapeActionId { get; }
        public bool BlocksLowerHotkeys { get; }
        public IReadOnlyCollection<string> AllowedHotkeyActionIds { get; }
    }
}
