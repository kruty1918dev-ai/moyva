using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Shared.Diagnostics
{
    public enum RuntimeLogLevel
    {
        Trace = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
    }

    public static class StructuredLogFormatter
    {
        public static string Format(
            RuntimeLogLevel level,
            string feature,
            string message,
            string traceId = "",
            string playerId = "",
            string sessionId = "",
            string errorCode = "")
        {
            string safeMessage = (message ?? string.Empty).Replace("\n", " ").Replace("\r", " ").Replace("\"", "'");
            return $"feature={feature ?? string.Empty} level={level} traceId={traceId ?? string.Empty} playerId={playerId ?? string.Empty} sessionId={sessionId ?? string.Empty} errorCode={errorCode ?? string.Empty} msg=\"{safeMessage}\"";
        }
    }

    public sealed class RuntimeLogSwitches
    {
        private readonly Dictionary<string, bool> _moduleFlags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public RuntimeLogSwitches()
        {
            _moduleFlags["multiplayer"] = Read("moyva.logs.multiplayer", true);
            _moduleFlags["multiplayer.session"] = Read("moyva.logs.multiplayer.session", true);
            _moduleFlags["multiplayer.network"] = Read("moyva.logs.multiplayer.network", true);
            _moduleFlags["multiplayer.lobby"] = Read("moyva.logs.multiplayer.lobby", true);
            _moduleFlags["multiplayer.trace"] = Read("moyva.logs.multiplayer.trace", false);
        }

        public bool IsEnabled(string module, bool fallback = true)
        {
            if (string.IsNullOrWhiteSpace(module))
                return fallback;

            if (_moduleFlags.TryGetValue(module, out bool value))
                return value;

            return fallback;
        }

        private static bool Read(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }
    }

    internal sealed class AsyncGlobalErrorHandlerService : IInitializable, IDisposable
    {
        public void Initialize()
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public void Dispose()
        {
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError($"[AsyncErrorHandler] UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.LogError($"[AsyncErrorHandler] UnhandledException: {e.ExceptionObject}");
        }
    }

    public class MoyvaDomainException : Exception
    {
        public string ErrorCode { get; }

        public MoyvaDomainException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode ?? string.Empty;
        }
    }

    public sealed class MultiplayerCompatibilityException : MoyvaDomainException
    {
        public MultiplayerCompatibilityException(string message)
            : base("MP-COMPAT", message)
        {
        }
    }

    public sealed class PerformanceBudgetExceededException : MoyvaDomainException
    {
        public PerformanceBudgetExceededException(string message)
            : base("PERF-BUDGET", message)
        {
        }
    }
}
