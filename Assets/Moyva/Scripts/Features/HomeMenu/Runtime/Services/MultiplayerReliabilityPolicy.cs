using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal static class MultiplayerReliabilityPolicy
    {
        private static readonly Dictionary<NetworkProviderType, TimeSpan> JoinTimeoutByProvider = new Dictionary<NetworkProviderType, TimeSpan>
        {
            { NetworkProviderType.Relay, TimeSpan.FromSeconds(15) },
            { NetworkProviderType.Lan, TimeSpan.FromSeconds(8) },
            { NetworkProviderType.Offline, TimeSpan.FromSeconds(5) },
        };

        public static TimeSpan GetJoinTimeout(NetworkProviderType providerType)
        {
            return JoinTimeoutByProvider.TryGetValue(providerType, out var timeout)
                ? timeout
                : TimeSpan.FromSeconds(12);
        }

        public static async Task<T> RetryWithBackoffAndJitterAsync<T>(
            Func<CancellationToken, Task<T>> action,
            Func<T, bool> isSuccess,
            int maxAttempts,
            TimeSpan baseDelay,
            string operationName,
            CancellationToken ct)
        {
            Exception lastException = null;
            var random = new System.Random();

            for (int attempt = 1; attempt <= Math.Max(1, maxAttempts); attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var result = await action(ct);
                    if (isSuccess == null || isSuccess(result))
                        return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (attempt == maxAttempts)
                    break;

                var pow = Math.Pow(2, attempt - 1);
                var delayMs = baseDelay.TotalMilliseconds * pow;
                var jitter = random.NextDouble() * 0.25d * delayMs;
                var totalDelay = TimeSpan.FromMilliseconds(delayMs + jitter);
                Debug.LogWarning($"[MultiplayerReliability] {operationName} attempt {attempt}/{maxAttempts} failed, retrying in {totalDelay.TotalMilliseconds:0} ms");
                await Task.Delay(totalDelay, ct);
            }

            if (lastException != null)
                throw lastException;

            return default;
        }
    }
}
