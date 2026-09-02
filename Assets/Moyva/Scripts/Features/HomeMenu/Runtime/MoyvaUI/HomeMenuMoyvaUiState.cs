using System;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal enum HomeMenuSettingsSection
    {
        General,
        Audio,
        Graphics
    }

    internal enum HomeMenuPlayFlow
    {
        Solo,
        Multiplayer
    }

    internal sealed class HomeMenuMoyvaUiState
    {
        private bool _dirty = true;
        private int _interactionDepth;

        public event Action Changed;

        public string CurrentRoute { get; private set; } = string.Empty;
        public HomeMenuSettingsSection SettingsSection { get; private set; } = HomeMenuSettingsSection.General;
        public HomeMenuPlayFlow PlayFlow { get; private set; } = HomeMenuPlayFlow.Solo;
        public bool IsInteractionActive => _interactionDepth > 0;
        public bool IsMounted { get; set; }
        public bool IsFallback { get; set; }

        public void Open(string route)
        {
            route = route?.Trim() ?? string.Empty;
            if (string.Equals(CurrentRoute, route, StringComparison.Ordinal))
                return;

            _interactionDepth = 0;
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

        public void SetSettingsSection(HomeMenuSettingsSection section)
        {
            if (SettingsSection == section)
                return;

            SettingsSection = section;
            MarkDirty();
        }

        public void SetPlayFlow(HomeMenuPlayFlow flow)
        {
            if (PlayFlow == flow)
                return;

            PlayFlow = flow;
            MarkDirty();
        }

        public void BeginInteraction()
        {
            _interactionDepth++;
        }

        public void EndInteraction()
        {
            if (_interactionDepth <= 0)
                return;

            _interactionDepth--;
            if (_interactionDepth == 0 && _dirty)
                Changed?.Invoke();
        }

        public void MarkDirty()
        {
            if (_dirty)
                return;

            _dirty = true;
            if (!IsInteractionActive)
                Changed?.Invoke();
        }

        public bool ConsumeDirty()
        {
            if (!_dirty || IsInteractionActive)
                return false;

            _dirty = false;
            return true;
        }
    }
}
