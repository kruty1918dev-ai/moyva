using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.Runtime;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface INavigation
    {
        void Close(string menuName);
        Task CloseIf(string menuName, Func<Task<bool>> condition);
        void Open(string menuName);
        void OpenLast();
        Task OpenIfAsync(string menuName, Func<Task<bool>> condition);

        void CloseLast();

        string CurrentMenu { get; }
    }

    public interface IOverlayLoader
    {
        OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%");
        void UpdateOverlay(float value, float maxValue = 100, string sufix = "%");
        // If forceImmediate == true, the implementation should hide the overlay immediately
        // (skip delayed animation) to guarantee the panel is closed when initialization completes.
        void StopOverlay(bool forceImmediate = false);
    }

    public interface IHomeMultiplayerService
    {
        Task JoinToLobbyAsync(string lobbyId, CancellationToken ct = default);
        Task CreateLobbyAsync(CancellationToken ct = default);
        Task LeaveLobbyAsync(CancellationToken ct = default);
        Task StartGameAsync(CancellationToken ct = default);
        Task<LobbyModelUIWrapper[]> GetAvailableLobbiesAsync(CancellationToken ct = default);
    }

    public interface IApplicationQuitHandler
    {
        Task QuitApplicationIfAsync(Func<Task<bool>> match, CancellationToken ct = default);
    }

    public interface IHomeMenuGameStarter
    {
        Task StartGameAsync(CancellationToken ct = default);
    }

    public struct LobbyModelUIWrapper
    {
        public string LobbyDisplayName;
        public string LobbyId { get; }
        public string HostNickname { get; }
        public PlayerUIWrapper[] CurrentPlayers { get; }
        public int MaxPlayers { get; }
        public string HashPassword;
        public bool HasPassword => !string.IsNullOrWhiteSpace(HashPassword);
        public bool IsFull => CurrentPlayers.Length >= MaxPlayers;

        public LobbyModelUIWrapper(
            string lobbyId,
            string hostNickname,
            PlayerUIWrapper[] currentPlayers,
            int maxPlayers,
            string hashPassword,
            string lobbyDisplayName)
        {
            LobbyId = lobbyId;
            HostNickname = hostNickname;
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
            HashPassword = hashPassword;
            LobbyDisplayName = lobbyDisplayName ?? $"{hostNickname}'s Lobby";
        }
    }

    public struct PlayerUIWrapper
    {
        public string PlayerId { get; }
        public string Nickname { get; }

        public PlayerUIWrapper(string playerId, string nickname)
        {
            PlayerId = playerId;
            Nickname = nickname;
        }
    }

    public class OverlayLoaderResult
    {
        public static OverlayLoaderResult Current { get; private set; }
        public static event Action<OverlayLoaderResult> CurrentChanged;

        public bool IsLoading { get; private set; }
        public float Progress { get; private set; }

        public Action OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }

        private Task _task;

        private OverlayLoaderResult()
        {
        }

        /// <summary>
        /// Start an async overlay task. The returned <see cref="OverlayLoaderResult"/> does not block threads;
        /// it tracks the provided async action and will invoke OnSuccess/OnError when it completes.
        /// </summary>
        public static OverlayLoaderResult Start(Func<Task> asyncAction, Action onSuccess = null, Action<Exception> onError = null)
        {
            var result = new OverlayLoaderResult
            {
                OnSuccess = onSuccess,
                OnError = onError
            };

            // Run the provided async action on the thread-pool and observe completion.
            result._task = Task.Run(async () =>
            {
                await asyncAction().ConfigureAwait(false);
            });

            result._task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    MainThreadDispatcher.Enqueue(() => result.OnError?.Invoke(t.Exception.InnerException ?? t.Exception));
                else
                    MainThreadDispatcher.Enqueue(() => result.OnSuccess?.Invoke());
            }, TaskScheduler.Default);

            return result;
        }

        internal void SetLoading(bool isLoading, float progress)
        {
            IsLoading = isLoading;
            Progress = progress;
            Current = this;
            CurrentChanged?.Invoke(this);
        }
    }

    public interface IConfirmationService
    {
        void Show(ConfirmationRequest request);
        void ForeceHide();
        bool TryGetReqest(out ConfirmationRequest? request);
    }
}