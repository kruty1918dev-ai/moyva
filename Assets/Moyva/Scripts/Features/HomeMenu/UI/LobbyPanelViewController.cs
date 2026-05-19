using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер UI-панелі лобі: invite-code, список користувачів, старт гри та refresh.
    /// </summary>
    public class LobbyPanelViewController : MonoBehaviour, ILobbyPanelViewController, IInitializable
    {
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _inviteCodeText;
        [SerializeField] private Transform _userListContainer;
        [SerializeField] private LobbyPlayerInfoView _userListItemPrefab;
        [SerializeField] private Button _coppyInviteCodeButton;
        [SerializeField] private Button _refreshUserListButton;

        private string _currentInviteCode = string.Empty;
        private bool _bound;

        public Button StartGameButton => _startGameButton;
        public Button BackButton => _backButton;

        public void SetLobbyInvateCode(string code)
        {
            if (_inviteCodeText != null)
                _inviteCodeText.text = $"Invite Code: {code}";
            _currentInviteCode = code;
        }

        public void ClearLobbyInvateCode()
        {
            if (_inviteCodeText != null)
                _inviteCodeText.text = "Invite Code: N/A";
            _currentInviteCode = string.Empty;
        }

        public void AddNewUser(LobbyUserInfo userInfo)
        {
            if (_userListContainer == null || _userListItemPrefab == null) return;

            // Інстанціюємо запис гравця та позначимо його GameObject іменем, щоб
            // можна було знаходити і видаляти по UserId.
            var item = Instantiate(_userListItemPrefab, _userListContainer);
            if (item != null)
            {
                item.gameObject.name = $"LobbyUser_{userInfo.UserId}";
                item.SetPlayerInfo(userInfo);
            }
        }

        public void RefreshUserList()
        {
            if (_userListContainer == null) return;

            var items = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < _userListContainer.childCount; i++)
            {
                items.Add(_userListContainer.GetChild(i));
            }

            if (items.Count <= 1) return;

            System.Func<Transform, int> parseId = t =>
            {
                if (t == null) return int.MaxValue;
                var name = t.gameObject.name;
                const string prefix = "LobbyUser_";
                if (!string.IsNullOrEmpty(name) && name.StartsWith(prefix))
                {
                    if (int.TryParse(name.Substring(prefix.Length), out var id))
                        return id;
                }
                return int.MaxValue;
            };

            items.Sort((a, b) => parseId(a).CompareTo(parseId(b)));

            for (int i = 0; i < items.Count; i++)
            {
                items[i].SetSiblingIndex(i);
            }
        }

        public void RemoveUser(int userId)
        {
            if (_userListContainer == null) return;

            string targetName = $"LobbyUser_{userId}";
            for (int i = 0; i < _userListContainer.childCount; i++)
            {
                var child = _userListContainer.GetChild(i);
                if (child == null) continue;
                if (child.gameObject.name == targetName)
                {
                    Destroy(child.gameObject);
                    return;
                }
            }

            Debug.LogWarning($"[LobbyPanelViewController] RemoveUser: userId={userId} not found in UI list.");
        }

        public void ClearUsers()
        {
            if (_userListContainer == null) return;

            foreach (Transform child in _userListContainer)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        public void Initialize()
        {
            if (_bound)
                return;

            // 1: Скидаємо UI у базовий стартовий стан.
            // Початковий стан UI: очистити список та встановити дефолтний текст
            ClearUsers();
            ClearLobbyInvateCode();

            // Стан кнопки старту за замовчуванням — недоступний
            if (_startGameButton != null)
                _startGameButton.interactable = false;

            // 2: Прив'язуємо обробники кнопок копіювання і оновлення.
            // Підключаємо слухачі кнопок
            if (_coppyInviteCodeButton != null)
            {
                _coppyInviteCodeButton.onClick.RemoveListener(OnCopyInviteClicked);
                _coppyInviteCodeButton.onClick.AddListener(OnCopyInviteClicked);
            }

            if (_refreshUserListButton != null)
            {
                _refreshUserListButton.onClick.RemoveListener(RefreshUserList);
                _refreshUserListButton.onClick.AddListener(RefreshUserList);
            }

            _bound = true;
        }

        private void OnCopyInviteClicked()
        {
            if (string.IsNullOrEmpty(_currentInviteCode))
            {
                Debug.LogWarning("[LobbyPanelViewController] No invite code to copy.");
                return;
            }

            GUIUtility.systemCopyBuffer = _currentInviteCode;
            Debug.Log($"[LobbyPanelViewController] Copied invite code: {_currentInviteCode}");
        }

        private void OnDestroy()
        {
            if (_coppyInviteCodeButton != null)
                _coppyInviteCodeButton.onClick.RemoveListener(OnCopyInviteClicked);
            if (_refreshUserListButton != null)
                _refreshUserListButton.onClick.RemoveListener(RefreshUserList);
            _bound = false;
        }
    }
}