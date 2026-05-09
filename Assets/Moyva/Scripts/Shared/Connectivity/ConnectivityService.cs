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
        private const string Prefix = "[ConnectivityService]";
        private const float DefaultPollingSeconds = 2f;
        private const int DefaultAttempts = 2;
        private const int DefaultTimeoutSeconds = 3;

        public bool IsOnline { get; private set; }
        public event Action<bool> StatusChanged;

        private CancellationTokenSource _cts;

        public void Initialize()
        {
            Debug.Log($"{Prefix} Initialize called. Starting monitor loop.");
            _cts = new CancellationTokenSource();
            _ = MonitorLoopAsync(_cts.Token);
        }

        public void Dispose()
        {
            Debug.Log($"{Prefix} Dispose called. Cancelling monitor loop.");
            try { _cts?.Cancel(); } catch { }
            _cts = null;
        }

        private async Task MonitorLoopAsync(CancellationToken ct)
        {
            Debug.Log($"{Prefix} MonitorLoopAsync started.");
            // initial quick check
            await CheckAndNotifyAsync();
            Debug.Log($"{Prefix} Initial check complete. IsOnline={IsOnline}");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Debug.Log($"{Prefix} Waiting {DefaultPollingSeconds}s before next connectivity check.");
                    await Task.Delay(TimeSpan.FromSeconds(DefaultPollingSeconds), ct);
                }
                catch (TaskCanceledException)
                {
                    Debug.Log($"{Prefix} Monitor loop cancelled.");
                    break;
                }

                await CheckAndNotifyAsync();
            }
        }

        private async Task CheckAndNotifyAsync()
        {
            Debug.Log($"{Prefix} CheckAndNotifyAsync start.");
            bool current = false;
            try
            {
                current = await InternetChecker.HasInternetAsync(DefaultAttempts, DefaultTimeoutSeconds);
                Debug.Log($"{Prefix} Connectivity probe result: {current}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Prefix} Connectivity probe failed: {ex.Message}");
                current = false;
            }

            if (current != IsOnline)
            {
                Debug.Log($"{Prefix} Status changed from {IsOnline} to {current}. Invoking StatusChanged.");
                IsOnline = current;
                try { StatusChanged?.Invoke(IsOnline); } catch (Exception ex) { Debug.LogError($"{Prefix} StatusChanged handler threw: {ex.Message}"); }
            }
        }

        public async Task<bool> WaitForOnlineAsync(TimeSpan timeout)
        {
            Debug.Log($"{Prefix} WaitForOnlineAsync start. timeout={timeout}");
            if (IsOnline)
            {
                Debug.Log($"{Prefix} Already online — returning true.");
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
                Debug.Log($"{Prefix} Performing quick direct probe before waiting on event.");
                // do a direct quick check first
                try
                {
                    if (await InternetChecker.HasInternetAsync(DefaultAttempts, DefaultTimeoutSeconds))
                    {
                        Debug.Log($"{Prefix} Quick direct probe succeeded — returning true.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{Prefix} Quick probe failed: {ex.Message}");
                }

                Debug.Log($"{Prefix} Waiting for StatusChanged event or timeout.");
                var delay = Task.Delay(timeout);
                var completed = await Task.WhenAny(tcs.Task, delay);
                var result = completed == tcs.Task && tcs.Task.Result;
                Debug.Log($"{Prefix} WaitForOnlineAsync completed. result={result}");
                return result;
            }
            finally
            {
                StatusChanged -= Handler;
                Debug.Log($"{Prefix} Handler removed and exiting WaitForOnlineAsync.");
            }
        }
    }
}
