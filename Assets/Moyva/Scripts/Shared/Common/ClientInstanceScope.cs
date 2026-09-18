using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.Common
{
    public interface IClientInstanceScope
    {
        string ScopeId { get; }
        bool IsDefault { get; }
        string BuildScopedFileName(string fileName);
        string CreateDefaultPlayerName();
    }

    public sealed class ClientInstanceScope : IClientInstanceScope
    {
        private const string DefaultScopeId = "default";
        private const string MppmPrefix = "mppm";
        private static readonly Lazy<IClientInstanceScope> SharedInstance =
            new(() => new ClientInstanceScope());

        private readonly string _scopeId;

        public ClientInstanceScope()
        {
            _scopeId = ResolveScopeId();
        }

        public static IClientInstanceScope Default => SharedInstance.Value;
        public string ScopeId => _scopeId;
        public bool IsDefault => string.Equals(_scopeId, DefaultScopeId, StringComparison.Ordinal);

        public string BuildScopedFileName(string fileName)
        {
            string safeFileName = string.IsNullOrWhiteSpace(fileName)
                ? "settings.dat"
                : Path.GetFileName(fileName);
            if (IsDefault)
                return safeFileName;

            string baseName = Path.GetFileNameWithoutExtension(safeFileName);
            string extension = Path.GetExtension(safeFileName);
            return $"{baseName}_{SanitizeToken(_scopeId)}{extension}";
        }

        public string CreateDefaultPlayerName()
        {
            string suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return IsDefault
                ? $"Player-{suffix}"
                : $"Player-{BuildShortScopeLabel(_scopeId)}-{suffix}";
        }

        private static string ResolveScopeId()
        {
            string scope = TryResolveMppmScopeId(Application.dataPath);
            if (!string.IsNullOrWhiteSpace(scope))
                return scope;

            scope = TryResolveMppmScopeId(Environment.CurrentDirectory);
            if (!string.IsNullOrWhiteSpace(scope))
                return scope;

            string[] args = Environment.GetCommandLineArgs();
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    scope = TryResolveMppmScopeId(args[i]);
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

            string[] parts = path.Replace('\\', '/').Split(
                new[] { '/' },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i + 1 < parts.Length; i++)
            {
                if (!string.Equals(parts[i], "VP", StringComparison.OrdinalIgnoreCase))
                    continue;

                string candidate = parts[i + 1];
                if (candidate.StartsWith(MppmPrefix, StringComparison.OrdinalIgnoreCase))
                    return SanitizeToken(candidate);
            }

            return string.Empty;
        }

        private static string BuildShortScopeLabel(string scopeId)
        {
            string token = SanitizeToken(scopeId);
            if (token.StartsWith(MppmPrefix, StringComparison.OrdinalIgnoreCase)
                && token.Length > MppmPrefix.Length)
            {
                token = token[MppmPrefix.Length..];
            }

            if (token.Length > 4)
                token = token[..4];

            return string.IsNullOrWhiteSpace(token) ? "MP" : token.ToUpperInvariant();
        }

        internal static string SanitizeToken(string value)
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
