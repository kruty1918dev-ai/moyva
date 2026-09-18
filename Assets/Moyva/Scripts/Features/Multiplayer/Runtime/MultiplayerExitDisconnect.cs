using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Гарантує, що клієнт залишає мережеву сесію (lobby + transport) при виході з гри
    /// або примусовому закритті application. Підписується на <see cref="Application.quitting"/>
    /// та <see cref="Application.wantsToQuit"/>, виконуючи best-effort leave з коротким timeout.
    /// </summary>
    internal sealed class MultiplayerExitDisconnect : IInitializable, IDisposable
    {
        private const string Prefix = "[MultiplayerExitDisconnect]";
        private const int LeaveTimeoutMs = 1500;

        [Inject(Optional = true)] private ISessionManager _sessionManager = null;
        [Inject(Optional = true)] private ILobbyService _lobbyService = null;

        private bool _quitting;

        public void Initialize()
        {
            Application.quitting += OnApplicationQuitting;
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        public void Dispose()
        {
            Application.quitting -= OnApplicationQuitting;
            Application.wantsToQuit -= OnApplicationWantsToQuit;
        }

        private bool OnApplicationWantsToQuit()
        {
            // wantsToQuit працює лише на standalone/editor. На мобільних спирайтеся на quitting.
            if (_quitting)
                return true;

            try
            {
                TryLeaveSessionSync();
            }
            catch (Exception)
            {
            }
            return true;
        }

        private void OnApplicationQuitting()
        {
            if (_quitting)
                return;
            _quitting = true;

            try
            {
                TryLeaveSessionSync();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Виконує best-effort leave з обмеженим тайм-аутом, щоб не блокувати shutdown.
        /// </summary>
        private void TryLeaveSessionSync()
        {
            using var cts = new CancellationTokenSource(LeaveTimeoutMs);
            try
            {
                Task task = null;
                if (_sessionManager != null)
                    task = _sessionManager.LeaveSessionAsync(cts.Token);
                else if (_lobbyService != null)
                    task = _lobbyService.LeaveAsync(cts.Token);

                if (task == null)
                    return;

                // Чекаємо коротко; не блокуємо shutdown більше ніж на LeaveTimeoutMs.
                task.Wait(LeaveTimeoutMs);
            }
            catch (AggregateException)
            {
            }
        }
    }
}
