using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface INavigation
    {
        void Close(string menuName);
        Task CloseIf(string menuName, Func<Task<bool>> condition);
        void Open(string menuName);
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

    public class OverlayLoaderResult : Task
    {
        public static OverlayLoaderResult Current { get; private set; }
        public static event Action<OverlayLoaderResult> CurrentChanged;

        public bool IsLoading { get; private set; }
        public float Progress { get; private set; }

        public Action OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }

        public OverlayLoaderResult(Action action) : base(action)
        {
        }

        public OverlayLoaderResult(Action action, CancellationToken cancellationToken) : base(action, cancellationToken)
        {
        }

        public OverlayLoaderResult(Action action, TaskCreationOptions creationOptions) : base(action, creationOptions)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state) : base(action, state)
        {
        }

        public OverlayLoaderResult(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action, cancellationToken, creationOptions)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state, CancellationToken cancellationToken) : base(action, state, cancellationToken)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action, state, creationOptions)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action, state, cancellationToken, creationOptions)
        {
        }

        public static OverlayLoaderResult Start(Action action, Action onSuccess = null, Action<Exception> onError = null)
        {
            var result = new OverlayLoaderResult(action)
            {
                OnSuccess = onSuccess,
                OnError = onError
            };

            // Do not broadcast CurrentChanged here — the result has not yet
            // been set to a loading state. Broadcast only when SetLoading is called.
            result.Start();
            result.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    result.OnError?.Invoke(t.Exception.InnerException ?? t.Exception);
                else
                    result.OnSuccess?.Invoke();
            });

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