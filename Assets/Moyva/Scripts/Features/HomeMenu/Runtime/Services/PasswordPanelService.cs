using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Сервіс модального запиту пароля. Підтримує лише один активний запит водночас:
    /// якщо новий запит надходить під час показаного — попередній скасовується.
    /// </summary>
    internal sealed class PasswordPanelService : IPasswordPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private IPasswordPanelViewController _viewController;

        private readonly object _lock = new object();
        private TaskCompletionSource<PasswordPromptResult> _pending;
        private CancellationTokenRegistration _ctr;
        private bool _subscribed;

        public void Initialize()
        {
            if (_viewController == null)
            {
                Debug.LogWarning("[PasswordPanelService] IPasswordPanelViewController не підключено — приватні кімнати недоступні.");
                return;
            }

            if (!_subscribed)
            {
                _viewController.OnConfirmed += OnConfirmed;
                _viewController.OnCancelled += OnCancelled;
                _subscribed = true;
            }
        }

        public void Dispose()
        {
            if (_subscribed && _viewController != null)
            {
                _viewController.OnConfirmed -= OnConfirmed;
                _viewController.OnCancelled -= OnCancelled;
                _subscribed = false;
            }

            CompletePending(PasswordPromptResult.Cancelled);
        }

        public Task<PasswordPromptResult> RequestPasswordAsync(string roomDisplayName, string errorText = null, CancellationToken ct = default)
        {
            if (_viewController == null)
                return Task.FromResult(PasswordPromptResult.Cancelled);

            // Скасувати попередній (якщо був).
            CompletePending(PasswordPromptResult.Cancelled);

            var tcs = new TaskCompletionSource<PasswordPromptResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_lock) { _pending = tcs; }

            _ctr = ct.Register(() => CompletePending(PasswordPromptResult.Cancelled));

            MainThreadDispatcher.Enqueue(() => _viewController.Show(roomDisplayName, errorText));
            return tcs.Task;
        }

        public void Cancel()
        {
            CompletePending(PasswordPromptResult.Cancelled);
        }

        private void OnConfirmed(string password)
        {
            CompletePending(PasswordPromptResult.Confirm(password));
        }

        private void OnCancelled()
        {
            CompletePending(PasswordPromptResult.Cancelled);
        }

        private void CompletePending(PasswordPromptResult result)
        {
            TaskCompletionSource<PasswordPromptResult> tcs;
            lock (_lock)
            {
                tcs = _pending;
                _pending = null;
            }

            try { _ctr.Dispose(); } catch { }
            _ctr = default;

            if (_viewController != null)
                MainThreadDispatcher.Enqueue(_viewController.Hide);

            tcs?.TrySetResult(result);
        }
    }
}
