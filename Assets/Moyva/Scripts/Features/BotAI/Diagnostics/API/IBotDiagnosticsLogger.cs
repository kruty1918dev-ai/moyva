using System;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public interface IBotDiagnosticsLogger
    {
        string Prefix { get; }

        void Trace(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null);

        void Info(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null);

        void Warning(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null);

        void Error(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null);

        void Critical(
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason = null,
            string details = null,
            string ownerId = null);

        void Exception(
            BotDiagnosticCategory category,
            string code,
            Exception exception,
            string message = null,
            string ownerId = null);
    }
}
