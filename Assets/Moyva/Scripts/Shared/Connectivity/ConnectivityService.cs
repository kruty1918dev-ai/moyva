using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Shared.Connectivity
{
    internal class ConnectivityService : IConnectivityService, IInitializable, IDisposable
    {
        private const float DefaultPollingSeconds = 2f;
        private const int DefaultAttempts = 2;
        private const int DefaultTimeoutSeconds = 3;

        public bool IsOnline { get; private set; }
        public event Action<bool> StatusChanged;

        private CancellationTokenSource _cts;

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            _ = MonitorLoopAsync(_cts.Token);
        }

        public void Dispose()
        {
            try { _cts?.Cancel(); } catch { }
            _cts = null;
        }

        private async Task MonitorLoopAsync(CancellationToken ct)
        {
            // initial quick check
            await CheckAndNotifyAsync();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(DefaultPollingSeconds), ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await CheckAndNotifyAsync();
            }
        }

        private async Task CheckAndNotifyAsync()
        {
            bool current = false;
            try
            {
                current = await InternetChecker.HasInternetAsync(DefaultAttempts, DefaultTimeoutSeconds);
            }
            catch { current = false; }

            if (current != IsOnline)
            {
                IsOnline = current;
                try { StatusChanged?.Invoke(IsOnline); } catch { }
            }
        }

        public async Task<bool> WaitForOnlineAsync(TimeSpan timeout)
        {
            if (IsOnline)
                return true;

            var tcs = new TaskCompletionSource<bool>();

            void Handler(bool online)
            {
                if (online) tcs.TrySetResult(true);
            }

            StatusChanged += Handler;
            try
            {
                // do a direct quick check first
                try
                {
                    if (await InternetChecker.HasInternetAsync(DefaultAttempts, DefaultTimeoutSeconds))
                        return true;
                }
                catch { }

                var delay = Task.Delay(timeout);
                var completed = await Task.WhenAny(tcs.Task, delay);
                return completed == tcs.Task && tcs.Task.Result;
            }
            finally
            {
                StatusChanged -= Handler;
            }
        }
    }
}
