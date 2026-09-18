using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Shared.Connectivity
{
    internal class ConnectivityService : IConnectivityService, IInitializable, IDisposable
    {
        private const string Prefix = "[ConnectivityService]";
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
            catch (Exception ex)
            {
                Debug.LogError($"{Prefix} Connectivity probe failed: {ex.Message}");
                current = false;
            }

            if (current != IsOnline)
            {
                IsOnline = current;
                try { StatusChanged?.Invoke(IsOnline); } catch (Exception ex) { Debug.LogError($"{Prefix} StatusChanged handler threw: {ex.Message}"); }
            }
        }

        public async Task<bool> WaitForOnlineAsync(TimeSpan timeout)
        {
            if (IsOnline)
            {
                return true;
            }

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
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{Prefix} Quick probe failed: {ex.Message}");
                }
                var delay = Task.Delay(timeout);
                var completed = await Task.WhenAny(tcs.Task, delay);
                var result = completed == tcs.Task && tcs.Task.Result;
                return result;
            }
            finally
            {
                StatusChanged -= Handler;
            }
        }
    }
}
