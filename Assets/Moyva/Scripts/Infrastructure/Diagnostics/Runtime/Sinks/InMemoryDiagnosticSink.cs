using System.Collections.Generic;
using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime.Sinks
{
    internal sealed class InMemoryDiagnosticSink : IDiagnosticSink
    {
        private readonly List<DiagnosticFlowAnalysis> _analyses = new List<DiagnosticFlowAnalysis>();
        private readonly List<string> _messages = new List<string>();

        public IReadOnlyList<DiagnosticFlowAnalysis> Analyses => _analyses;
        public IReadOnlyList<string> Messages => _messages;

        public void Emit(DiagnosticFlowAnalysis analysis, string formattedMessage)
        {
            if (analysis != null)
                _analyses.Add(analysis);
            if (!string.IsNullOrEmpty(formattedMessage))
                _messages.Add(formattedMessage);
        }
    }
}
