using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    public sealed class DiagnosticsEnvironmentState : IDiagnosticsEnvironmentState
    {
        public bool IsProjectContextInstalled { get; private set; }
        public string ProjectContextInstallDetails { get; private set; }

        public void MarkProjectContextInstalled(string details = null)
        {
            IsProjectContextInstalled = true;
            ProjectContextInstallDetails = details;
        }
    }
}
