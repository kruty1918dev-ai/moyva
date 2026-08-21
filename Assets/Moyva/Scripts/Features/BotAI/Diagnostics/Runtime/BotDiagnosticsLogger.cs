using System;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsLogger : IBotDiagnosticsLogger
    {
        private readonly BotDiagnosticsSettings _settings;
        private readonly BotDiagnosticsContextProvider _context;
        private readonly IBotDiagnosticsBuffer _buffer;
        private readonly BotDiagnosticsConsoleSink _console;
        private long _sequence;

        public BotDiagnosticsLogger(
            BotDiagnosticsSettings settings,
            BotDiagnosticsContextProvider context,
            IBotDiagnosticsBuffer buffer,
            BotDiagnosticsConsoleSink console)
        {
            _settings = settings ?? new BotDiagnosticsSettings();
            _context = context;
            _buffer = buffer;
            _console = console;
        }

        public string Prefix => BotDiagnosticsConstants.Prefix;

        public void Trace(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null)
        {
            if (!_settings.IncludeTrace)
                return;
            Write(BotDiagnosticLevel.Trace, category, code, message, reason, details, ownerId);
        }

        public void Info(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null)
            => Write(BotDiagnosticLevel.Info, category, code, message, reason, details, ownerId);

        public void Warning(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null)
            => Write(BotDiagnosticLevel.Warning, category, code, message, reason, details, ownerId);

        public void Error(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null)
            => Write(BotDiagnosticLevel.Error, category, code, message, reason, details, ownerId);

        public void Critical(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null)
            => Write(BotDiagnosticLevel.Critical, category, code, message, reason, details, ownerId);

        public void Exception(
            BotDiagnosticCategory category,
            string code,
            Exception exception,
            string message = null,
            string ownerId = null)
        {
            if (exception == null)
            {
                Error(category, code, message ?? "Unknown bot exception.", "exception=null", ownerId: ownerId);
                return;
            }

            string details = _settings.IncludeStackTraceForExceptions
                ? exception.ToString()
                : $"{exception.GetType().Name}: {exception.Message}";

            Write(
                BotDiagnosticLevel.Error,
                category,
                code,
                message ?? exception.Message,
                exception.GetType().FullName,
                details,
                ownerId);
        }

        private void Write(
            BotDiagnosticLevel level,
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason,
            string details,
            string ownerId)
        {
            if (!_settings.Enabled)
                return;

            var diagnosticEvent = new BotDiagnosticEvent(
                ++_sequence,
                Time.realtimeSinceStartupAsDouble,
                DateTime.UtcNow.ToString("O"),
                level,
                category,
                code,
                string.IsNullOrWhiteSpace(ownerId) ? _context?.OwnerId : ownerId,
                _context?.GlobalTurn ?? 0,
                _context?.Phase ?? "Unavailable",
                message,
                reason,
                details);

            _buffer?.Add(diagnosticEvent);
            _console?.Write(diagnosticEvent);
        }
    }
}
