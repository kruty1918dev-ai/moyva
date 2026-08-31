using System;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class HomeMenuMoyvaUiState
    {
        private bool _dirty = true;

        public event Action Changed;

        public string CurrentRoute { get; private set; } = string.Empty;
        public bool IsMounted { get; set; }
        public bool IsFallback { get; set; }

        public void Open(string route)
        {
            route = route?.Trim() ?? string.Empty;
            if (string.Equals(CurrentRoute, route, StringComparison.Ordinal))
                return;

            CurrentRoute = route;
            MarkDirty();
        }

        public void Close(string route)
        {
            if (!string.IsNullOrWhiteSpace(route) &&
                !string.Equals(CurrentRoute, route.Trim(), StringComparison.Ordinal))
            {
                return;
            }

            Open(string.Empty);
        }

        public void MarkDirty()
        {
            _dirty = true;
            Changed?.Invoke();
        }

        public bool ConsumeDirty()
        {
            if (!_dirty)
                return false;

            _dirty = false;
            return true;
        }
    }
}
