using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Diagnostics.Runtime.Sinks;
using Zenject;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    public static class DiagnosticsInstaller
    {
        public static void InstallProjectCore(DiContainer container)
        {
            if (container == null)
                return;

            if (!container.HasBinding<IDiagnosticClock>())
                container.Bind<IDiagnosticClock>().To<UnityDiagnosticClock>().AsSingle();
            if (!container.HasBinding<IDiagnosticRuntimeOptions>())
                container.Bind<IDiagnosticRuntimeOptions>().To<DiagnosticRuntimeOptions>().AsSingle();
            if (!container.HasBinding<IDiagnosticFlowAnalyzer>())
                container.Bind<IDiagnosticFlowAnalyzer>().To<DiagnosticFlowAnalyzer>().AsSingle();
            if (!container.HasBinding<IDiagnosticFlowFormatter>())
                container.Bind<IDiagnosticFlowFormatter>().To<DiagnosticFlowFormatter>().AsSingle();
            if (!container.HasBinding<IDiagnosticSink>())
                container.Bind<IDiagnosticSink>().To<UnityConsoleDiagnosticSink>().AsSingle();
            if (!container.HasBinding<InMemoryDiagnosticSink>())
                container.Bind<InMemoryDiagnosticSink>().AsSingle();
            if (!container.HasBinding<IDiagnosticsEnvironmentState>())
                container.Bind<IDiagnosticsEnvironmentState>().To<global::Kruty1918.Moyva.Diagnostics.Runtime.DiagnosticsEnvironmentState>().AsSingle();
            if (!container.HasBinding<ISaveLoadDiagnosticsSession>())
                container.Bind<ISaveLoadDiagnosticsSession>().To<SaveLoadDiagnosticsSession>().AsSingle();
            if (!container.HasBinding<IConstructionDiagnosticsSession>())
                container.Bind<IConstructionDiagnosticsSession>().To<ConstructionDiagnosticsSession>().AsSingle();
            if (!container.HasBinding<IDiagnosticFlowService>())
                container.BindInterfacesAndSelfTo<DiagnosticFlowService>().AsSingle().NonLazy();
            if (!container.HasBinding<IFogStartupDiagnostics>())
                container.Bind<IFogStartupDiagnostics>().To<FogStartupDiagnostics>().AsSingle();
            if (!container.HasBinding<IConstructionDiagnostics>())
                container.Bind<IConstructionDiagnostics>().To<ConstructionDiagnostics>().AsSingle();
            if (!container.HasBinding<ISaveLoadDiagnostics>())
                container.Bind<ISaveLoadDiagnostics>().To<SaveLoadDiagnostics>().AsSingle();
            if (!container.HasBinding<IMultiplayerSessionDiagnostics>())
                container.Bind<IMultiplayerSessionDiagnostics>().To<MultiplayerSessionDiagnostics>().AsSingle();
        }
    }
}
