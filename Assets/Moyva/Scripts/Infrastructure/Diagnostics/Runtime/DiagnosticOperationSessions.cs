using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    public sealed class SaveLoadDiagnosticsSession : ISaveLoadDiagnosticsSession
    {
        public IDiagnosticFlow CurrentFlow { get; private set; }
        public bool HasActiveFlow => CurrentFlow != null && !CurrentFlow.IsSummaryReported;

        public void Begin(IDiagnosticFlow flow)
        {
            CurrentFlow = flow;
        }

        public void Clear(IDiagnosticFlow flow = null)
        {
            if (flow == null || ReferenceEquals(CurrentFlow, flow))
                CurrentFlow = null;
        }
    }

    public sealed class ConstructionDiagnosticsSession : IConstructionDiagnosticsSession
    {
        public IDiagnosticFlow CurrentFlow { get; private set; }
        public bool HasActiveFlow => CurrentFlow != null && !CurrentFlow.IsSummaryReported;

        public void Begin(IDiagnosticFlow flow)
        {
            CurrentFlow = flow;
        }

        public void Clear(IDiagnosticFlow flow = null)
        {
            if (flow == null || ReferenceEquals(CurrentFlow, flow))
                CurrentFlow = null;
        }
    }
}
