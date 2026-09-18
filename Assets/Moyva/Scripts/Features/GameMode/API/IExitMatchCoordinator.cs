using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.GameMode.API
{
    public readonly struct ExitMatchResult
    {
        private ExitMatchResult(bool succeeded, bool cancelled, string error)
        {
            Succeeded = succeeded;
            Cancelled = cancelled;
            Error = error;
        }

        public bool Succeeded { get; }
        public bool Cancelled { get; }
        public string Error { get; }

        public static ExitMatchResult Success()
            => new ExitMatchResult(true, false, null);

        public static ExitMatchResult Failure(string error)
            => new ExitMatchResult(false, false, error);

        public static ExitMatchResult Cancellation()
            => new ExitMatchResult(false, true, null);
    }

    /// <summary>
    /// Idempotent gameplay exit flow: save or disconnect, restore pause state,
    /// then transition to HomeMenu.
    /// </summary>
    public interface IExitMatchCoordinator
    {
        bool IsExiting { get; }
        Task<ExitMatchResult> ExitToMenuAsync(
            CancellationToken cancellationToken = default);
    }

    /// <summary>Optional local persistence adapter supplied by SaveSystem.</summary>
    public interface IExitMatchSaveHandler
    {
        Task SaveBeforeExitAsync(CancellationToken cancellationToken);
    }

    /// <summary>Optional network leave adapter supplied by Multiplayer.</summary>
    public interface IExitMatchDisconnectHandler
    {
        Task DisconnectBeforeExitAsync(CancellationToken cancellationToken);
    }

    /// <summary>Scene transition seam kept injectable for failure tests.</summary>
    public interface IExitMatchSceneLoader
    {
        Task LoadHomeMenuAsync(CancellationToken cancellationToken);
    }
}
