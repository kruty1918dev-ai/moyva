using System;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Multiplayer.Core;
using Unity.Services.Authentication;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    public static class MultiplayerClientScope
    {
        private const string DefaultScopeId = "default";
        private const string MppmPrefix = "mppm";
        private static string _scopeId;

        public static string ScopeId => _scopeId ?? (_scopeId = ResolveScopeId());
        public static bool IsDefault => string.Equals(ScopeId, DefaultScopeId, StringComparison.Ordinal);

        public static string BuildScopedFileName(string fileName)
        {
            var safeFileName = string.IsNullOrWhiteSpace(fileName) ? "settings.dat" : Path.GetFileName(fileName);
            if (IsDefault)
                return safeFileName;

            var baseName = Path.GetFileNameWithoutExtension(safeFileName);
            var extension = Path.GetExtension(safeFileName);
            return $"{baseName}_{SanitizeToken(ScopeId)}{extension}";
        }

        public static string CreateDefaultPlayerName()
        {
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant();
            if (IsDefault)
                return $"Player-{randomSuffix}";

            return $"Player-{BuildShortScopeLabel(ScopeId)}-{randomSuffix}";
        }

        public static void ApplyAuthenticationProfileIfNeeded(IMultiplayerLogger logger = null)
        {
            if (IsDefault)
                return;

            try
            {
                var authentication = AuthenticationService.Instance;
                if (authentication == null || authentication.IsSignedIn)
                    return;

                authentication.SwitchProfile(BuildAuthenticationProfileName(ScopeId));
            }
            catch (Exception e)
            {
                logger?.Warn($"[MultiplayerClientScope] Failed to switch UGS auth profile for scope '{ScopeId}': {e.Message}");
            }
        }

        private static string ResolveScopeId()
        {
            var scope = TryResolveMppmScopeId(Application.dataPath);
            if (!string.IsNullOrWhiteSpace(scope))
                return scope;

            scope = TryResolveMppmScopeId(Environment.CurrentDirectory);
            if (!string.IsNullOrWhiteSpace(scope))
                return scope;

            var args = Environment.GetCommandLineArgs();
            if (args != null)
            {
                foreach (var arg in args)
                {
                    scope = TryResolveMppmScopeId(arg);
                    if (!string.IsNullOrWhiteSpace(scope))
                        return scope;
                }
            }

            return DefaultScopeId;
        }

        private static string TryResolveMppmScopeId(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            var normalized = path.Replace('\\', '/');
            var parts = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (string.Equals(part, "VP", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length)
                {
                    var candidate = parts[i + 1];
                    if (candidate.StartsWith(MppmPrefix, StringComparison.OrdinalIgnoreCase))
                        return SanitizeToken(candidate);
                }
            }

            return string.Empty;
        }

        private static string BuildAuthenticationProfileName(string scopeId)
        {
            var token = SanitizeToken(scopeId);
            var profileName = string.IsNullOrWhiteSpace(token) ? "moyva_player" : $"moyva_{token}";
            return profileName.Length <= 30 ? profileName : profileName.Substring(0, 30);
        }

        private static string BuildShortScopeLabel(string scopeId)
        {
            var token = SanitizeToken(scopeId);
            if (token.StartsWith(MppmPrefix, StringComparison.OrdinalIgnoreCase) && token.Length > MppmPrefix.Length)
                token = token.Substring(MppmPrefix.Length);

            if (token.Length > 4)
                token = token.Substring(0, 4);

            return string.IsNullOrWhiteSpace(token) ? "MP" : token.ToUpperInvariant();
        }

        private static string SanitizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            foreach (var character in value.Trim())
            {
                if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
                    builder.Append(character);
            }

            return builder.ToString();
        }
    }
}