using Kruty1918.Moyva.Diagnostics.API;
using UnityEngine;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    internal sealed class DiagnosticRuntimeOptions : IDiagnosticRuntimeOptions
    {
        public static DiagnosticRuntimeOptions Default { get; } = new DiagnosticRuntimeOptions();

        public bool IsEnabled => true;
        public bool EmitOkFlows => Debug.isDebugBuild;
    }
}
