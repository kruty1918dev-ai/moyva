using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionLocalConfirmExecutor : IConstructionConfirmRequestExecutor
    {
        private readonly IConstructionService _constructionService;
        private readonly IConstructionDiagnostics _diagnostics;
        private readonly IConstructionDiagnosticsSession _diagnosticsSession;

        public ConstructionLocalConfirmExecutor(
            IConstructionService constructionService,
            [Zenject.InjectOptional] IConstructionDiagnostics diagnostics = null,
            [Zenject.InjectOptional] IConstructionDiagnosticsSession diagnosticsSession = null)
        {
            _constructionService = constructionService;
            _diagnostics = diagnostics;
            _diagnosticsSession = diagnosticsSession;
        }

        public int Priority => 0;

        public bool TryHandleConfirmRequest()
        {
            _diagnostics?.CompleteStep(_diagnosticsSession?.CurrentFlow, ConstructionDiagnosticSteps.BuildConfirmed, "executor=local");
            _constructionService.Confirm();
            return true;
        }
    }
}
