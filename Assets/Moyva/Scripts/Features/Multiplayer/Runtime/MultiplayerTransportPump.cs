using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    internal sealed class MultiplayerTransportPump : IDisposable
    {
        private const int PumpDelayMilliseconds = 16;

        private CancellationTokenSource _cancellation;
        private Task _task;

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
            var cancellation =
                CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _cancellation = cancellation;
            // Force the loop onto the thread pool so the transport keeps
            // ticking while the main thread is busy (world generation).
            _task = Task.Run(
                () => RunAsync(shouldContinue, tick, cancellation.Token));
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

        /// <summary>
        /// Synchronous bounded stop for dispose paths where awaiting is not
        /// possible. The pump tick is short (a driver update + flush), so a
        /// generous timeout still returns quickly in practice.
        /// </summary>
        public void Stop(int timeoutMilliseconds = 2000)
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
                task?.Wait(timeoutMilliseconds);
            }
            catch
            {
            }
            finally
            {
                cancellation.Dispose();
            }
        }

        public void Dispose()
            => Stop();

        private async Task RunAsync(
            Func<bool> shouldContinue,
            Action tick,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                bool keepGoing;
                try
                {
                    keepGoing = shouldContinue();
                }
                catch (Exception)
                {
                    // A dead native driver makes IsCreated throw; treat it as
                    // a stop signal instead of faulting the task.
                    return;
                }
                if (!keepGoing)
                    return;

                try
                {
                    tick();
                }
                catch (Exception exception)
                {
                    // Teardown race: the native driver can be deallocated between the
                    // IsCreated check above and this tick. Shutdown, not a failure —
                    // a LogError here fails tests and alarms players on quit.
                    if (exception is ObjectDisposedException ||
                        (exception.Message ?? string.Empty).Contains("deallocated"))
                    {
                        Debug.LogWarning(
                            $"[TransportPump] Stopping after driver disposal: {exception.Message}");
                        return;
                    }
                    Debug.LogError($"Multiplayer transport update failed: {exception.Message}");
                    return;
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
