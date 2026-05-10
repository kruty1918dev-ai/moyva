using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuNavigation : INavigation
    {
        private readonly Stack<string> _menuStack = new Stack<string>();
        private readonly Dictionary<string, INavigationPanel> _panelsByName = new Dictionary<string, INavigationPanel>(StringComparer.Ordinal);
        private readonly Stack<string> _closedStack = new Stack<string>();

        private readonly IConfirmationService _confirmationService;
        private readonly HashSet<string> _confirmOnBackNames;
        private bool _suppressConfirmationNextClose;

        public event Action<NavigationChangeEventArgs> OnMenuChanged;

        public string CurrentMenu => _menuStack.Count > 0 ? _menuStack.Peek() : string.Empty;

        public HomeMenuNavigation(
            INavigationPanel[] panels,
            [InjectOptional] IConfirmationService confirmationService = null,
            [InjectOptional] string[] confirmOnBackMenuNames = null)
        {
            _confirmationService = confirmationService;
            _confirmOnBackNames = new HashSet<string>(confirmOnBackMenuNames ?? Array.Empty<string>(), StringComparer.Ordinal);

            if (panels == null) return;

            foreach (var p in panels)
            {
                if (p == null) continue;

                var name = p.MenuName ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    if (p is MonoBehaviour mb)
                        name = mb.gameObject.name;
                }

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                name = name.Trim();

                if (!_panelsByName.ContainsKey(name))
                    _panelsByName.Add(name, p);
                else
                    LogWarning($"Duplicate panel name '{name}' detected; ignoring duplicate.");
            }
        }

        public void Open(string menuName)
        {
            if (string.IsNullOrWhiteSpace(menuName))
                return;

            menuName = menuName.Trim();

            if (!_panelsByName.TryGetValue(menuName, out var panel))
            {
                LogWarning($"Panel '{menuName}' not found.");
                return;
            }
            // If requested menu is already open, do nothing.
            if (_menuStack.Count > 0 && _menuStack.Peek() == menuName)
            {
                LogInfo($"Menu '{menuName}' already open.");
                return;
            }
            UnityEngine.Debug.Log($"[HomeMenuNavigation] Open('{menuName}') from: {new System.Diagnostics.StackTrace(1, true)}");
            var previous = CurrentMenu;

            // Close currently opened panel (if any) and push it to closed history.
            // When this Open call triggers a close, suppress confirmation for that close
            if (_menuStack.Count > 0)
            {
                _suppressConfirmationNextClose = true;
                CloseLast();
            }

            // When explicitly opening a named menu, remove it from closed history to avoid duplicates
            RemoveFromClosedHistory(menuName);

            _menuStack.Push(menuName);
            panel.Open();
            LogInfo($"Opened menu '{menuName}'.");

            RaiseMenuChanged(previous, CurrentMenu, true);
        }

        public void Close(string menuName)
        {
            if (_menuStack.Count == 0)
                return;

            if (_menuStack.Peek() != menuName)
                return;

            if (_suppressConfirmationNextClose)
            {
                _suppressConfirmationNextClose = false;
                DoClose(menuName);
                return;
            }

            if (_confirmationService != null && _confirmOnBackNames.Contains(menuName))
            {
                _confirmationService.Show(new ConfirmationRequest
                {
                    LabelText = "Підтвердження",
                    MessageText = "Ви впевнені, що хочете повернутися назад?",
                    OnConfirm = () => DoClose(menuName),
                    OnCancel = () => LogInfo($"Close of '{menuName}' was cancelled by user.")
                });

                return;
            }

            DoClose(menuName);
        }

        public async Task CloseIf(string menuName, Func<Task<bool>> condition)
        {
            if (_menuStack.Count == 0 || _menuStack.Peek() != menuName)
                return;

            if (await condition().ConfigureAwait(false))
            {
                var previous = CurrentMenu;
                _menuStack.Pop();
                if (_panelsByName.TryGetValue(menuName, out var panel))
                    panel.Close();
                else
                    LogWarning($"Panel '{menuName}' not found when conditional closing.");

                _closedStack.Push(menuName);

                LogInfo($"Conditionally closed menu '{menuName}'.");

                RaiseMenuChanged(previous, CurrentMenu, _menuStack.Count > 0 && _menuStack.Peek() == CurrentMenu);
            }
        }

        public async Task OpenIfAsync(string menuName, Func<Task<bool>> condition)
        {
            if (await condition().ConfigureAwait(false))
            {
                Open(menuName);
            }
        }

        public void CloseLast()
        {
            if (_menuStack.Count == 0)
                return;

            var menuName = _menuStack.Peek();

            if (_suppressConfirmationNextClose)
            {
                _suppressConfirmationNextClose = false;
                DoClose(menuName);
                return;
            }

            if (_confirmationService != null && _confirmOnBackNames.Contains(menuName))
            {
                _confirmationService.Show(new ConfirmationRequest
                {
                    LabelText = "Підтвердження",
                    MessageText = "Ви впевнені, що хочете повернутися назад?",
                    OnConfirm = () => DoClose(menuName),
                    OnCancel = () => LogInfo($"CloseLast of '{menuName}' was cancelled by user.")
                });

                return;
            }

            DoClose(menuName);
        }

        public void OpenLast()
        {
            if (_closedStack.Count == 0)
            {
                LogWarning("No closed menus to reopen.");
                return;
            }
            UnityEngine.Debug.Log($"[HomeMenuNavigation] OpenLast() from: {new System.Diagnostics.StackTrace(1, true)}");
            // Try to reopen the most-recently closed panel that still exists.
            while (_closedStack.Count > 0)
            {
                var menuName = _closedStack.Pop();
                if (!_panelsByName.TryGetValue(menuName, out var panel))
                {
                    LogWarning($"Panel '{menuName}' not found when reopening last; skipping.");
                    continue;
                }

                var previous = CurrentMenu;

                // Close the current panel without making it the next back target.
                if (_menuStack.Count > 0)
                {
                    var current = _menuStack.Pop();
                    if (_panelsByName.TryGetValue(current, out var currentPanel))
                        currentPanel.Close();
                    else
                        LogWarning($"Panel '{current}' not found when closing current.");

                    LogInfo($"Closed current menu '{current}' before reopening.");
                }

                // Open the target panel
                _menuStack.Push(menuName);
                panel.Open();
                LogInfo($"Reopened last closed menu '{menuName}'.");

                RaiseMenuChanged(previous, CurrentMenu, true);
                return;
            }

            LogWarning("No valid closed menus to reopen.");
        }

        private void DoClose(string menuName)
        {
            var previous = CurrentMenu;

            // actual pop/close
            if (_menuStack.Count > 0 && _menuStack.Peek() == menuName)
                _menuStack.Pop();
            else
                LogWarning($"Attempted to close '{menuName}' but it was not on top of the stack.");

            if (_panelsByName.TryGetValue(menuName, out var panel))
                panel.Close();
            else
                LogWarning($"Panel '{menuName}' not found when closing.");

            _closedStack.Push(menuName);

            LogInfo($"Closed menu '{menuName}'.");

            RaiseMenuChanged(previous, CurrentMenu, _menuStack.Count > 0 && _menuStack.Peek() == CurrentMenu);
        }

        private void RaiseMenuChanged(string previous, string current, bool currentIsOpen)
        {
            try
            {
                OnMenuChanged?.Invoke(new NavigationChangeEventArgs
                {
                    PreviousMenu = previous,
                    CurrentMenu = current,
                    CurrentIsOpen = currentIsOpen
                });
            }
            catch (Exception ex)
            {
                LogError($"OnMenuChanged handler threw: {ex.Message}");
            }
        }

        private void RemoveFromClosedHistory(string menuName)
        {
            if (_closedStack.Count == 0) return;

            var temp = new Stack<string>();
            while (_closedStack.Count > 0)
            {
                var m = _closedStack.Pop();
                if (m != menuName)
                    temp.Push(m);
            }

            while (temp.Count > 0)
                _closedStack.Push(temp.Pop());
        }


        // --- Logging helpers ---
        private void LogInfo(string message)
        {
            Debug.Log($"[HomeMenuNavigation] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[HomeMenuNavigation] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[HomeMenuNavigation] {message}");
        }

        public void CloseForce(string menuName)
        {
            if (_menuStack.Count == 0) return;
            if (_menuStack.Peek() != menuName) return;
            DoClose(menuName);
        }

        public void OpenForce(string menuName)
        {
            if (string.IsNullOrWhiteSpace(menuName)) return;
            menuName = menuName.Trim();

            if (!_panelsByName.TryGetValue(menuName, out var panel))
            {
                LogWarning($"Panel '{menuName}' not found.");
                return;
            }

            UnityEngine.Debug.Log($"[HomeMenuNavigation] OpenForce('{menuName}') from: {new System.Diagnostics.StackTrace(1, true)}");
            var previous = CurrentMenu;

            if (_menuStack.Count > 0)
                CloseForce(_menuStack.Peek());

            RemoveFromClosedHistory(menuName);
            _menuStack.Push(menuName);
            panel.Open();

            RaiseMenuChanged(previous, CurrentMenu, true);
        }

        public void OpenLastForce()
        {
            if (_closedStack.Count == 0) return;
            UnityEngine.Debug.Log($"[HomeMenuNavigation] OpenLastForce() from: {new System.Diagnostics.StackTrace(1, true)}");

            while (_closedStack.Count > 0)
            {
                var menuName = _closedStack.Pop();
                if (!_panelsByName.TryGetValue(menuName, out var panel)) continue;

                var previous = CurrentMenu;

                if (_menuStack.Count > 0)
                {
                    var current = _menuStack.Pop();
                    if (_panelsByName.TryGetValue(current, out var currentPanel))
                        currentPanel.Close();
                }

                _menuStack.Push(menuName);
                panel.Open();
                RaiseMenuChanged(previous, CurrentMenu, true);
                return;
            }
        }

        public void CloseLastForce()
        {
            if (_menuStack.Count == 0) return;
            var menuName = _menuStack.Peek();
            DoClose(menuName);
        }
    }
}