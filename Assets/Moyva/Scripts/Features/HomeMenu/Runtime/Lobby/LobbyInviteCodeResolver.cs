using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class LobbyInviteCodeResolver
    {
        public static LobbyInviteCodePresentation Resolve(LobbyRoom lobby, NetworkProviderType provider)
        {
            if (lobby == null)
                return new LobbyInviteCodePresentation(GetLabel(provider), string.Empty);

            var code = provider == NetworkProviderType.Lan
                ? FirstNonEmpty(lobby.RelayJoinCode, lobby.LobbyCode, lobby.LobbyId)
                : FirstNonEmpty(lobby.LobbyCode, lobby.LobbyId, lobby.RelayJoinCode);

            return new LobbyInviteCodePresentation(GetLabel(provider), code);
        }

        private static string GetLabel(NetworkProviderType provider)
        {
            return provider == NetworkProviderType.Lan
                ? "LAN Join Code"
                : "Invite Code";
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return string.Empty;

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }
    }
}
