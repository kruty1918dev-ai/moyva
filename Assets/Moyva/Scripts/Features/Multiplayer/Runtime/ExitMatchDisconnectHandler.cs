using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed class ExitMatchDisconnectHandler
        : IExitMatchDisconnectHandler
    {
        private readonly ISessionManager _sessionManager;

        public ExitMatchDisconnectHandler(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public Task DisconnectBeforeExitAsync(
            CancellationToken cancellationToken)
        {
            if (_sessionManager?.Participants == null
                || _sessionManager.Participants.Count == 0)
            {
                return Task.CompletedTask;
            }

            return _sessionManager.LeaveSessionAsync(cancellationToken);
        }
    }
}
