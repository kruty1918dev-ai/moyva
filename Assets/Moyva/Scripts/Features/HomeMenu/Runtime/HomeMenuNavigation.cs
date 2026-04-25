using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuNavigation : INavigation
    {
        private readonly Stack<string> _menuStack = new Stack<string>();
        private readonly Dictionary<string, INavigationPanel> _panelsByName = new Dictionary<string, INavigationPanel>(StringComparer.Ordinal);
        private readonly Stack<string> _closedStack = new Stack<string>();

        public string CurrentMenu => _menuStack.Count > 0 ? _menuStack.Peek() : string.Empty;

        public HomeMenuNavigation(INavigationPanel[] panels)
        {
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
            // Close currently opened panel (if any) and push it to closed history
            CloseLast();

            // When explicitly opening a named menu, remove it from closed history to avoid duplicates
            RemoveFromClosedHistory(menuName);

            _menuStack.Push(menuName);
            panel.Open();
            LogInfo($"Opened menu '{menuName}'.");
        }

        public void Close(string menuName)
        {
            if (_menuStack.Count == 0)
                return;

            if (_menuStack.Peek() != menuName)
                return;

            _menuStack.Pop();
            if (_panelsByName.TryGetValue(menuName, out var panel))
                panel.Close();
            else
                LogWarning($"Panel '{menuName}' not found when closing.");

            _closedStack.Push(menuName);

            LogInfo($"Closed menu '{menuName}'.");
        }

        public async Task CloseIf(string menuName, Func<Task<bool>> condition)
        {
            if (_menuStack.Count == 0 || _menuStack.Peek() != menuName)
                return;

            if (await condition().ConfigureAwait(false))
            {
                _menuStack.Pop();
                if (_panelsByName.TryGetValue(menuName, out var panel))
                    panel.Close();
                else
                    LogWarning($"Panel '{menuName}' not found when conditional closing.");

                _closedStack.Push(menuName);

                LogInfo($"Conditionally closed menu '{menuName}'.");
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

            var menuName = _menuStack.Pop();
            if (_panelsByName.TryGetValue(menuName, out var panel))
                panel.Close();
            else
                LogWarning($"Panel '{menuName}' not found when closing last.");

            _closedStack.Push(menuName);

            LogInfo($"Closed last menu '{menuName}'.");
        }

        public void OpenLast()
        {
            if (_closedStack.Count == 0)
            {
                LogWarning("No closed menus to reopen.");
                return;
            }

            // Try to reopen the most-recently closed panel that still exists.
            while (_closedStack.Count > 0)
            {
                var menuName = _closedStack.Pop();
                if (!_panelsByName.TryGetValue(menuName, out var panel))
                {
                    LogWarning($"Panel '{menuName}' not found when reopening last; skipping.");
                    continue;
                }

                // Close currently opened panel (if any) and push it to closed history
                if (_menuStack.Count > 0)
                {
                    var current = _menuStack.Pop();
                    if (_panelsByName.TryGetValue(current, out var currentPanel))
                        currentPanel.Close();
                    else
                        LogWarning($"Panel '{current}' not found when closing current.");

                    _closedStack.Push(current);
                    LogInfo($"Closed current menu '{current}' before reopening.");
                }

                // Open the target panel
                _menuStack.Push(menuName);
                panel.Open();
                LogInfo($"Reopened last closed menu '{menuName}'.");
                return;
            }

            LogWarning("No valid closed menus to reopen.");
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
    }
}