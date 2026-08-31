using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
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
        private readonly Dictionary<int, LobbyPlayerInfoView> _usersById = new Dictionary<int, LobbyPlayerInfoView>();
        private readonly Stack<LobbyPlayerInfoView> _pool = new Stack<LobbyPlayerInfoView>();
        private bool _bound;

        public Button StartGameButton => _startGameButton;
        public Button BackButton => _backButton;

        public void SetInviteCode(LobbyInviteCodePresentation presentation)
        {
            if (_inviteCodeText != null)
                _inviteCodeText.text = presentation.DisplayText;
            _currentInviteCode = presentation.Code;
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

            if (_usersById.TryGetValue(userInfo.UserId, out var existing) && existing != null)
            {
                existing.SetPlayerInfo(userInfo);
                return;
            }

            var item = GetOrCreateUserItem();
            if (item != null)
            {
                item.gameObject.name = $"LobbyUser_{userInfo.UserId}";
                item.transform.SetParent(_userListContainer, false);
                item.gameObject.SetActive(true);
                item.SetPlayerInfo(userInfo);
                _usersById[userInfo.UserId] = item;
            }
        }

        public void RefreshUserList()
        {
            if (_userListContainer == null) return;

            var items = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < _userListContainer.childCount; i++)
            {
                var child = _userListContainer.GetChild(i);
                if (child != null && child.gameObject.activeSelf && child != _userListItemPrefab.transform)
                    items.Add(child);
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

            if (_usersById.TryGetValue(userId, out var item))
            {
                ReleaseUserItem(item);
                _usersById.Remove(userId);
            }
        }

        public void ClearUsers()
        {
            foreach (var pair in _usersById)
                ReleaseUserItem(pair.Value);

            _usersById.Clear();
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
                return;
            }

            GUIUtility.systemCopyBuffer = _currentInviteCode;
        }

        private void OnDestroy()
        {
            if (_coppyInviteCodeButton != null)
                _coppyInviteCodeButton.onClick.RemoveListener(OnCopyInviteClicked);
            if (_refreshUserListButton != null)
                _refreshUserListButton.onClick.RemoveListener(RefreshUserList);
            _bound = false;
        }

        private LobbyPlayerInfoView GetOrCreateUserItem()
        {
            while (_pool.Count > 0)
            {
                var pooled = _pool.Pop();
                if (pooled != null)
                    return pooled;
            }

            return Instantiate(_userListItemPrefab, _userListContainer);
        }

        private void ReleaseUserItem(LobbyPlayerInfoView item)
        {
            if (item == null || item == _userListItemPrefab)
                return;

            item.gameObject.SetActive(false);
            _pool.Push(item);
        }
    }
}
