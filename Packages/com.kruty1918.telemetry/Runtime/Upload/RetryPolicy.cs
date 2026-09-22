using System;

namespace Kruty1918.Telemetry.Upload
{
    /// <summary>
    /// Exponential backoff with full jitter (AWS-style), classified retries.
    /// Deterministic under an injected <see cref="Random"/> for tests.
    /// </summary>
    public sealed class RetryPolicy
    {
        public TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
        public TimeSpan MaxDelay = TimeSpan.FromMinutes(15);
        public int MaxAttempts = 8;
        private readonly Random _random;

        public RetryPolicy(Random random = null) { _random = random ?? new Random(); }

        /// <summary>Delay before the NEXT attempt; attempt is 1-based count of failures so far.</summary>
        public TimeSpan DelayFor(int attempt, double serverRetryAfterSeconds = -1)
        {
            if (serverRetryAfterSeconds > 0)
                return TimeSpan.FromSeconds(serverRetryAfterSeconds);
            double cap = Math.Min(MaxDelay.TotalMilliseconds,
                InitialDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            return TimeSpan.FromMilliseconds(_random.NextDouble() * cap);
        }

        /// <summary>After this many failures the batch is permanently failed → quarantine.</summary>
        public bool Exhausted(int attempts) => attempts >= MaxAttempts;

        /// <summary>HTTP status classification.</summary>
        public static bool IsRetryableStatus(int http)
            => http == 0 || http == 408 || http == 425 || http == 429 || http >= 500;

        public static bool IsPermanentStatus(int http)
            => http >= 400 && http < 500 && !IsRetryableStatus(http);
    }
}
