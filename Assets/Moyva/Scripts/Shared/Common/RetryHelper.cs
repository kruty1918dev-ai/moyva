using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Shared.Common
{
    public static class RetryHelper
    {
        public static async Task<T> PollUntilAsync<T>(
            Func<Task<T>> attempt,
            Func<T, bool> isSuccess,
            TimeSpan timeout,
            TimeSpan interval,
            CancellationToken ct = default)
        {
            Guard.NotNull(attempt, nameof(attempt));
            Guard.NotNull(isSuccess, nameof(isSuccess));

            var deadline = MoyvaClock.UtcNow.Add(timeout);
            while (MoyvaClock.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();

                var result = await attempt();
                if (isSuccess(result))
                    return result;

                await Task.Delay(interval, ct);
            }

            return default;
        }
    }
}
