using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Сервіс відображення інформаційних повідомлень (OK-only).
    /// Підтримує чергу: якщо панель уже показується — наступне Show ставиться у чергу
    /// та виводиться після підтвердження попереднього.
    /// </summary>
    internal sealed class InfoPanelService : IInfoPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private IInfoPanelViewController _viewController;

        private readonly Queue<InfoMessage> _queue = new Queue<InfoMessage>();
        private readonly object _lock = new object();
        private InfoMessage? _current;
        private bool _subscribed;

        public bool IsShown => _viewController?.IsVisible ?? false;

        public void Initialize()
        {
            if (_viewController == null)
            {
                Debug.LogWarning("[InfoPanelService] IInfoPanelViewController не підключено — повідомлення будуть лише логуватись.");
                return;
            }

            if (!_subscribed)
            {
                _viewController.OnAcknowledged += OnAcknowledged;
                _subscribed = true;
            }
        }

        public void Dispose()
        {
            if (_subscribed && _viewController != null)
            {
                _viewController.OnAcknowledged -= OnAcknowledged;
                _subscribed = false;
            }
        }

        public void Show(InfoMessage message)
        {
            // Без View — повідомляємо у консоль, аби нічого не загубити.
            if (_viewController == null)
            {
                Debug.Log($"[InfoPanel] {message.Title}: {message.Message}");
                try { message.OnAcknowledged?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
                return;
            }

            lock (_lock)
            {
                if (_current.HasValue)
                {
                    _queue.Enqueue(message);
                    return;
                }

                _current = message;
            }

            MainThreadDispatcher.Enqueue(() => _viewController.Show(message));
        }

        public void ForceHide()
        {
            lock (_lock)
            {
                _queue.Clear();
                _current = null;
            }

            if (_viewController != null)
                MainThreadDispatcher.Enqueue(_viewController.Hide);
        }

        private void OnAcknowledged()
        {
            InfoMessage? finished;
            InfoMessage? next = null;

            lock (_lock)
            {
                finished = _current;
                _current = null;

                if (_queue.Count > 0)
                {
                    next = _queue.Dequeue();
                    _current = next;
                }
            }

            // Виконуємо колбек ПІСЛЯ зняття стану, щоб уникнути reentrancy.
            try { finished?.OnAcknowledged?.Invoke(); }
            catch (Exception e) { Debug.LogException(e); }

            if (next.HasValue && _viewController != null)
                MainThreadDispatcher.Enqueue(() => _viewController.Show(next.Value));
            else if (_viewController != null)
                MainThreadDispatcher.Enqueue(_viewController.Hide);
        }
    }
}
