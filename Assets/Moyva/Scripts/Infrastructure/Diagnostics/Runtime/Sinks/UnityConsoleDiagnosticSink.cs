using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Diagnostics.Runtime.Sinks
{
    internal sealed class UnityConsoleDiagnosticSink : IDiagnosticSink
    {
        private readonly IDiagnosticRuntimeOptions _options;

        public UnityConsoleDiagnosticSink(IDiagnosticRuntimeOptions options)
        {
            _options = options ?? DiagnosticRuntimeOptions.Default;
        }

        public void Emit(DiagnosticFlowAnalysis analysis, string formattedMessage)
        {
            if (analysis == null || string.IsNullOrEmpty(formattedMessage))
                return;

            if (analysis.Status == DiagnosticFlowStatus.Ok)
            {
                if (_options.EmitOkFlows)
                    Debug.Log(formattedMessage);
                return;
            }

            if (analysis.Status == DiagnosticFlowStatus.Failed)
                Debug.LogError(formattedMessage);
            else
                Debug.LogWarning(formattedMessage);
        }
    }
}
