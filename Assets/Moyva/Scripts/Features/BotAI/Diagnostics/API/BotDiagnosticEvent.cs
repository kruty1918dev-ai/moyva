using System;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public readonly struct BotDiagnosticEvent
    {
        public BotDiagnosticEvent(
            long sequence,
            double realtime,
            string utcTimestamp,
            BotDiagnosticLevel level,
            BotDiagnosticCategory category,
            string code,
            string ownerId,
            long globalTurn,
            string phase,
            string message,
            string reason,
            string details)
        {
            Sequence = sequence;
            Realtime = realtime;
            UtcTimestamp = utcTimestamp ?? string.Empty;
            Level = level;
            Category = category;
            Code = Normalize(code);
            OwnerId = Normalize(ownerId);
            GlobalTurn = globalTurn;
            Phase = Normalize(phase);
            Message = Normalize(message);
            Reason = Normalize(reason);
            Details = Normalize(details);
        }

        public long Sequence { get; }
        public double Realtime { get; }
        public string UtcTimestamp { get; }
        public BotDiagnosticLevel Level { get; }
        public BotDiagnosticCategory Category { get; }
        public string Code { get; }
        public string OwnerId { get; }
        public long GlobalTurn { get; }
        public string Phase { get; }
        public string Message { get; }
        public string Reason { get; }
        public string Details { get; }

        public override string ToString()
            => $"{BotDiagnosticsConstants.Prefix}[{Level}][{Category}][{Code}] {Message}";

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
