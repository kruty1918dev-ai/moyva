using Kruty1918.Moyva.HomeMenu.UI;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class HomeMenuMoyvaUiNavigationPanel : INavigationPanel
    {
        private readonly HomeMenuMoyvaUiState _state;

        public HomeMenuMoyvaUiNavigationPanel(string menuName, HomeMenuMoyvaUiState state)
        {
            MenuName = menuName?.Trim() ?? string.Empty;
            _state = state;
        }

        public string MenuName { get; }

        public void Open() => _state?.Open(MenuName);

        public void Close() => _state?.Close(MenuName);
    }
}
