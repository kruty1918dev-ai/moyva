using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionConfirmRequestRouter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly List<IConstructionConfirmRequestExecutor> _executors;
        private readonly IConstructionDiagnostics _diagnostics;
        private readonly IConstructionDiagnosticsSession _diagnosticsSession;

        public ConstructionConfirmRequestRouter(
            SignalBus signalBus,
            List<IConstructionConfirmRequestExecutor> executors,
            [InjectOptional] IConstructionDiagnostics diagnostics = null,
            [InjectOptional] IConstructionDiagnosticsSession diagnosticsSession = null)
        {
            _signalBus = signalBus;
            _executors = executors ?? new List<IConstructionConfirmRequestExecutor>();
            _diagnostics = diagnostics;
            _diagnosticsSession = diagnosticsSession;
        }

        public void Initialize()
        {
            _executors.Sort((left, right) => right.Priority.CompareTo(left.Priority));
            _signalBus.Subscribe<PlaceBuildingConfirmRequestSignal>(OnConfirmRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<PlaceBuildingConfirmRequestSignal>(OnConfirmRequested);
        }

        private void OnConfirmRequested(PlaceBuildingConfirmRequestSignal _)
        {
            IDiagnosticFlow flow = _diagnostics?.StartFlow("construction-confirm", new DiagnosticContext().Add("source", "PlaceBuildingConfirmRequestSignal"));
            _diagnosticsSession?.Begin(flow);
            _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.PlayerClickedBuild, "source=confirm-request");
            _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildRequestCreated, "executorCount=" + _executors.Count);
            _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.PreviewShown, "source=pending-preview");

            for (int i = 0; i < _executors.Count; i++)
            {
                if (_executors[i] != null && _executors[i].TryHandleConfirmRequest())
                    return;
            }

            _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, "no-executor-handled");
            _diagnostics?.Report(flow);
            _diagnosticsSession?.Clear(flow);
            UnityEngine.Debug.LogWarning("[Construction] PlaceBuildingConfirmRequestSignal received, but no executor handled it.");
        }
    }
}
