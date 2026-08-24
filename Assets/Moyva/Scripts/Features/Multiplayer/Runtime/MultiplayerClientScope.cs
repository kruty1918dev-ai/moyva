using System;
using System.Text;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Shared.Common;
using Unity.Services.Authentication;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    public static class MultiplayerClientScope
    {
        private static IClientInstanceScope Scope => ClientInstanceScope.Default;

        public static string ScopeId => Scope.ScopeId;
        public static bool IsDefault => Scope.IsDefault;
        public static string BuildScopedFileName(string fileName) => Scope.BuildScopedFileName(fileName);
        public static string CreateDefaultPlayerName() => Scope.CreateDefaultPlayerName();

        public static void ApplyAuthenticationProfileIfNeeded(IMultiplayerLogger logger = null)
        {
            if (Scope.IsDefault)
                return;

            try
            {
                var authentication = AuthenticationService.Instance;
                if (authentication == null || authentication.IsSignedIn)
                    return;

                authentication.SwitchProfile(BuildAuthenticationProfileName(Scope.ScopeId));
            }
            catch (Exception exception)
            {
                logger?.Warn(
                    $"[MultiplayerClientScope] Failed to switch UGS auth profile " +
                    $"for scope '{Scope.ScopeId}': {exception.Message}");
            }
        }

        private static string BuildAuthenticationProfileName(string scopeId)
        {
            string token = SanitizeToken(scopeId);
            string profileName = string.IsNullOrWhiteSpace(token)
                ? "moyva_player"
                : $"moyva_{token}";
            return profileName.Length <= 30 ? profileName : profileName[..30];
        }

        private static string SanitizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            foreach (char character in value.Trim())
            {
                if (char.IsLetterOrDigit(character) || character is '_' or '-')
                    builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
