using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public interface IBotDiagnosticsBuffer
    {
        int Count { get; }
        int Capacity { get; }

        void Add(BotDiagnosticEvent diagnosticEvent);
        IReadOnlyList<BotDiagnosticEvent> Capture();
        IReadOnlyList<BotDiagnosticEvent> CaptureSince(long sequenceExclusive);
        void Clear();
    }
}
