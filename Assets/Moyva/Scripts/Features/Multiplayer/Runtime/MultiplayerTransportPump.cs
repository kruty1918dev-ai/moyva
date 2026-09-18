using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Drives <c>NetworkDriver.ScheduleUpdate</c> on Unity's main thread.
    /// Unity Transport allocates send buffers with <see cref="Unity.Collections.Allocator.Temp"/>,
    /// which is only legal on the main thread or inside jobs — a managed
    /// thread-pool pump throws on every send. Ticking via the main-thread
    /// synchronization context keeps all driver access legal and still gives
    /// the pump air between generation stages (BuildWorldAsync yields).
    /// In environments without a captured Unity context the pump falls back
    /// to a thread-pool loop (editors/tests always capture one).
    /// </summary>
    internal sealed class MultiplayerTransportPump : IDisposable
    {
        private const int PumpDelayMilliseconds = 16;
        private const int StopFallbackTimeoutMilliseconds = 500;

        private CancellationTokenSource _cancellation;
        private Task _task;
        private Func<bool> _shouldContinue;
        private Action _tick;
        private volatile bool _running;
        private int _epoch;

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
            _shouldContinue = shouldContinue;
            _tick = tick;
            _running = true;
            int epoch = ++_epoch;

            if (MultiplayerThreadContext.CanPost)
            {
                var stopped = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _task = stopped.Task;
                MultiplayerThreadContext.Post(() => TickOnce(epoch, stopped));
            }
            else
            {
                _task = Task.Run(() => RunAsync(cancellation.Token));
            }
        }

        public async Task StopAsync()
        {
            CancellationTokenSource cancellation = Detach();

            if (cancellation == null)
                return;

            cancellation.Cancel();
            try
            {
                if (_task != null)
                {
                    // The main-thread loop completes when the queued tick runs —
                    // bound the wait so a stalled context cannot hang shutdown.
                    await Task.WhenAny(
                        _task,
                        Task.Delay(StopFallbackTimeoutMilliseconds));
                }
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
        /// possible. On the main thread the loop ends when the next queued
        /// tick sees the epoch bump, so there is nothing to wait for here.
        /// </summary>
        public void Stop(int timeoutMilliseconds = 2000)
        {
            CancellationTokenSource cancellation = Detach();

            if (cancellation == null)
                return;

            cancellation.Cancel();
            try
            {
                // Awaiting on the main thread would deadlock: loop completion
                // is delivered through the main-thread context itself.
                if (_task != null && !MultiplayerThreadContext.IsMainThread)
                    _task.Wait(timeoutMilliseconds);
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

        private CancellationTokenSource Detach()
        {
            CancellationTokenSource cancellation = _cancellation;
            _cancellation = null;
            _task = null;
            _running = false;
            _epoch++;
            return cancellation;
        }

        // Runs on Unity's main thread via MultiplayerThreadContext. Each call
        // performs one pump iteration and re-posts itself, giving one tick per
        // frame (~16ms at 60 fps) — the cadence the previous delay loop used.
        private void TickOnce(int epoch, TaskCompletionSource<bool> stopped)
        {
            CancellationTokenSource cancellation = _cancellation;
            if (epoch != _epoch
                || !_running
                || cancellation == null
                || cancellation.IsCancellationRequested)
            {
                stopped.TrySetResult(true);
                return;
            }

            if (!ContinueTick())
            {
                _running = false;
                stopped.TrySetResult(true);
                return;
            }

            // Deferred repost, never inline: an inline invoke on the main
            // thread would recurse TickOnce -> Post -> TickOnce forever.
            int capturedEpoch = epoch;
            MultiplayerThreadContext.PostDeferred(() => TickOnce(capturedEpoch, stopped));
        }

        // One pump iteration shared by both execution modes.
        // Returns false when the loop should terminate.
        private bool ContinueTick()
        {
            bool keepGoing;
            try
            {
                keepGoing = _shouldContinue();
            }
            catch (Exception)
            {
                // A dead native driver makes IsCreated throw; treat it as
                // a stop signal instead of faulting the task.
                return false;
            }
            if (!keepGoing)
                return false;

            try
            {
                _tick();
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
                    return false;
                }
                Debug.LogError($"Multiplayer transport update failed: {exception.Message}");
                return false;
            }

            return true;
        }

        // Thread-pool fallback for environments without a captured Unity
        // synchronization context (plain .NET hosts). Never used inside the
        // editor or a player, where the main-thread context always exists.
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!ContinueTick())
                    return;

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
            _running = false;
            _epoch++;
        }
    }
}
