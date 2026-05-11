using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Runtime-safe logger with structured formatting and per-module runtime switches (via PlayerPrefs).
    /// Log level criteria:
    /// Trace = high-frequency diagnostics, Info = expected flow, Warn = recoverable anomalies, Error = failed operation.
    /// </summary>
    public sealed class UnityMultiplayerLogger : IMultiplayerLogger
    {
        private const string Feature = "multiplayer";
        private const float TraceRateLimitSeconds = 0.5f;

        // Runtime log switches read once from PlayerPrefs at construction time.
        private readonly bool _mpEnabled;
        private readonly bool _sessionEnabled;
        private readonly bool _networkEnabled;
        private readonly bool _lobbyEnabled;
        private readonly bool _traceEnabled;
        private readonly Dictionary<int, float> _lastTraceTimeByMessage = new Dictionary<int, float>();

        public UnityMultiplayerLogger()
        {
            _mpEnabled      = ReadSwitch("moyva.logs.multiplayer", true);
            _sessionEnabled = ReadSwitch("moyva.logs.multiplayer.session", true);
            _networkEnabled = ReadSwitch("moyva.logs.multiplayer.network", true);
            _lobbyEnabled   = ReadSwitch("moyva.logs.multiplayer.lobby", true);
            _traceEnabled   = ReadSwitch("moyva.logs.multiplayer.trace", false);
        }

        public void Info(string message)
        {
            if (!IsEnabled(message, isTrace: false)) return;
            Debug.Log(Format("INFO", message));
        }

        public void Warn(string message)
        {
            if (!IsEnabled(message, isTrace: false)) return;
            Debug.LogWarning(Format("WARN", message));
        }

        public void Error(string message)
        {
            if (!IsEnabled(message, isTrace: false)) return;
            Debug.LogError(Format("ERROR", message));
        }

        public void Trace(string message)
        {
            if (!_mpEnabled || !_traceEnabled) return;

            int hash = (message ?? string.Empty).GetHashCode();
            float now = Time.realtimeSinceStartup;
            if (_lastTraceTimeByMessage.TryGetValue(hash, out var last) && now - last < TraceRateLimitSeconds)
                return;

            _lastTraceTimeByMessage[hash] = now;
            Debug.Log(Format("TRACE", message));
        }

        // ---- Helpers -------------------------------------------------------

        private bool IsEnabled(string message, bool isTrace)
        {
            if (!_mpEnabled) return false;
            if (isTrace && !_traceEnabled) return false;

            string text = message ?? string.Empty;
            if (text.IndexOf("[Session", StringComparison.OrdinalIgnoreCase) >= 0)
                return _sessionEnabled;
            if (text.IndexOf("[WebSocket", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("[Net", StringComparison.OrdinalIgnoreCase) >= 0)
                return _networkEnabled;
            if (text.IndexOf("[Lobby", StringComparison.OrdinalIgnoreCase) >= 0)
                return _lobbyEnabled;

            return true;
        }

        private static string Format(string level, string message)
        {
            string safe = (message ?? string.Empty).Replace("\n", " ").Replace("\r", " ").Replace("\"", "'");
            return $"feature={Feature} level={level} msg=\"{safe}\"";
        }

        private static bool ReadSwitch(string key, bool defaultValue)
        {
            try { return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0; }
            catch { return defaultValue; }
        }
    }
}
