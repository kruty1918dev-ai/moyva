using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    internal sealed class MultiplayerTransportPump : IDisposable
    {
        private const int PumpDelayMilliseconds = 16;

        private readonly IMultiplayerLogger _logger;
        private readonly string _transportName;
        private CancellationTokenSource _cancellation;
        private Task _task;

        public MultiplayerTransportPump(
            IMultiplayerLogger logger,
            string transportName)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transportName = string.IsNullOrWhiteSpace(transportName)
                ? "Transport"
                : transportName.Trim();
        }

        public void Start(
            CancellationToken externalToken,
            Func<bool> shouldContinue,
            Action tick)
        {
            if (shouldContinue == null)
                throw new ArgumentNullException(nameof(shouldContinue));
            if (tick == null)
                throw new ArgumentNullException(nameof(tick));

            DisposeCurrent();
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _task = RunAsync(shouldContinue, tick, _cancellation.Token);
        }

        public async Task StopAsync()
        {
            CancellationTokenSource cancellation = _cancellation;
            Task task = _task;
            _cancellation = null;
            _task = null;

            if (cancellation == null)
                return;

            cancellation.Cancel();
            try
            {
                if (task != null)
                    await task;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cancellation.Dispose();
            }
        }

        public void Dispose()
            => DisposeCurrent();

        private async Task RunAsync(
            Func<bool> shouldContinue,
            Action tick,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && shouldContinue())
            {
                try
                {
                    tick();
                }
                catch (Exception exception)
                {
                    _logger.Warn($"[{_transportName}] Pump error: {exception.Message}");
                }

                try
                {
                    await Task.Delay(PumpDelayMilliseconds, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private void DisposeCurrent()
        {
            try
            {
                _cancellation?.Cancel();
            }
            catch
            {
            }

            _cancellation?.Dispose();
            _cancellation = null;
            _task = null;
        }
    }
}
