using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Diagnostics.API
{
    public interface IDiagnosticClock
    {
        double NowMilliseconds { get; }
    }

    public interface IDiagnosticStepScope : IDisposable
    {
        void Complete(DiagnosticContext context);
        void Complete(string details = null);
        void Fail(string reason, DiagnosticContext context);
        void Fail(string reason, string details = null);
        void Skip(string reason, DiagnosticContext context);
        void Skip(string reason, string details = null);
    }

    public interface IDiagnosticFlow
    {
        DiagnosticFlowId Id { get; }
        string TraceId { get; }
        DiagnosticFlowDefinition Definition { get; }
        string Subject { get; }
        DiagnosticContext Context { get; }
        double StartedAtMilliseconds { get; }
        bool IsSummaryReported { get; }
        IReadOnlyDictionary<string, DiagnosticStepRecord> Records { get; }

        IDiagnosticStepScope BeginStep(string stepId, DiagnosticContext context);
        IDiagnosticStepScope BeginStep(string stepId, string details = null);
        void CompleteStep(string stepId, DiagnosticContext context);
        void CompleteStep(string stepId, string details = null);
        void FailStep(string stepId, string reason, DiagnosticContext context);
        void FailStep(string stepId, string reason, string details = null);
        void SkipStep(string stepId, string reason, DiagnosticContext context);
        void SkipStep(string stepId, string reason, string details = null);
        void ReportSummary(bool forceTimeout = false);
    }

    public interface IDiagnosticFlowService
    {
        IDiagnosticFlow StartFlow(DiagnosticFlowDefinition definition, string subject, DiagnosticContext context);
        IDiagnosticFlow StartFlow(DiagnosticFlowDefinition definition);
        IDiagnosticFlow GetOrStartFlow(string flowKey, DiagnosticFlowDefinition definition, string subject, DiagnosticContext context);
        IDiagnosticFlow GetOrStartFlow(string flowKey, DiagnosticFlowDefinition definition);
        void ReportFlow(IDiagnosticFlow flow, bool forceTimeout = false);
    }

    public interface IDiagnosticRuntimeOptions
    {
        bool IsEnabled { get; }
        bool EmitOkFlows { get; }
    }

    public interface IDiagnosticFlowReporter
    {
        void Report(IDiagnosticFlow flow, bool forceTimeout = false);
    }

    public interface IDiagnosticFlowAnalyzer
    {
        DiagnosticFlowAnalysis Analyze(IDiagnosticFlow flow, bool forceTimeout = false);
    }

    public interface IDiagnosticFlowFormatter
    {
        string Format(DiagnosticFlowAnalysis analysis);
    }

    public interface IDiagnosticSink
    {
        void Emit(DiagnosticFlowAnalysis analysis, string formattedMessage);
    }

    public interface IDiagnosticsEnvironmentState
    {
        bool IsProjectContextInstalled { get; }
        string ProjectContextInstallDetails { get; }

        void MarkProjectContextInstalled(string details = null);
    }

    public interface ISaveLoadDiagnosticsSession
    {
        IDiagnosticFlow CurrentFlow { get; }
        bool HasActiveFlow { get; }

        void Begin(IDiagnosticFlow flow);
        void Clear(IDiagnosticFlow flow = null);
    }

    public interface IConstructionDiagnosticsSession
    {
        IDiagnosticFlow CurrentFlow { get; }
        bool HasActiveFlow { get; }

        void Begin(IDiagnosticFlow flow);
        void Clear(IDiagnosticFlow flow = null);
    }
}
