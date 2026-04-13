using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.UI
{
    /// <summary>
    /// UI-панель лобі мультиплеєру.
    /// Відображається коли активний режим гри — <see cref="GameModeType.Lobby"/>.
    /// Реєструється через Zenject як <see cref="IGameModePanel"/>.
    /// </summary>
    public sealed class LobbyPanel : MonoBehaviour, IGameModePanel
    {
        [SerializeField] private GameObject _content;

        public GameModeType TargetMode => GameModeType.Lobby;

        public void Show()
        {
            gameObject.SetActive(true);
            if (_content != null)
                _content.SetActive(true);
        }

        public void Hide()
        {
            if (_content != null)
                _content.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
