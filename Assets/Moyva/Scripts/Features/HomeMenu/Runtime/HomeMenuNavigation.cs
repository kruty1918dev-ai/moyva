using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuNavigation : INavigation
    {
        private readonly Stack<string> _menuStack = new Stack<string>();

        public string CurrentMenu => _menuStack.Count > 0 ? _menuStack.Peek() : string.Empty;

        public void Open(string menuName)
        {
            if (string.IsNullOrWhiteSpace(menuName))
                return;

            _menuStack.Push(menuName);
            Debug.Log($"[HomeMenuNavigation] Opened menu '{menuName}'.");
        }

        public void Close(string menuName)
        {
            if (_menuStack.Count == 0)
                return;

            if (_menuStack.Peek() != menuName)
                return;

            _menuStack.Pop();
            Debug.Log($"[HomeMenuNavigation] Closed menu '{menuName}'.");
        }

        public async Task CloseIf(string menuName, Func<Task<bool>> condition)
        {
            if (_menuStack.Count == 0 || _menuStack.Peek() != menuName)
                return;

            if (await condition().ConfigureAwait(false))
            {
                _menuStack.Pop();
                Debug.Log($"[HomeMenuNavigation] Conditionally closed menu '{menuName}'.");
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
            Debug.Log($"[HomeMenuNavigation] Closed last menu '{menuName}'.");
        }
    }
}