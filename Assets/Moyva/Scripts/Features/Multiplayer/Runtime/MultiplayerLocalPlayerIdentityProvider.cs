using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed class MultiplayerLocalPlayerIdentityProvider : ILocalPlayerIdentityProvider
    {
        private readonly ISessionManager _sessionManager;

        public MultiplayerLocalPlayerIdentityProvider(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public string LocalPlayerId => _sessionManager?.LocalPlayerId ?? string.Empty;
    }
}