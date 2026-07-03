using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime.Flows
{
    public static class WorldGenerationDiagnosticSteps
    {
        public const string ProjectContextInstalled = "ProjectContext.Installed";
        public const string SceneContextInstalled = "SceneContext.Installed";
        public const string DirectLaunchConfigured = "DirectLaunch.Configured";
        public const string BootstrapSubscribeWorldSignals = "Bootstrap.SubscribeWorldSignals";
        public const string GeneratorInstallerInstallBindings = "GeneratorInstaller.InstallBindings";
        public const string GeneratorStartupInitialize = "GeneratorStartup.Initialize";
        public const string MapVisualBuildWorld = "MapVisual.BuildWorld";
        public const string GraphMapDataGenerate = "GraphMapData.Generate";
        public const string TwcBuild = "TWC.Build";
        public const string WorldGeneratedDataSignalFire = "WorldGeneratedDataSignal.Fire";
        public const string BootstrapWorldGeneratedReceive = "Bootstrap.WorldGenerated.Receive";
        public const string FogSnapshotValidated = "FogSnapshotValidated";
        public const string FogRepairApply = "FogRepair.Apply";
        public const string FogRepairSkip = "FogRepair.Skip";
        public const string SpawnPositionsSelected = "SpawnPositions.Selected";
        public const string WorldSpawnPositionsSignalFire = "WorldSpawnPositionsSignal.Fire";
        public const string FogRevealApply = "FogReveal.Apply";
        public const string CameraFocusStart = "Camera.FocusStart";
    }

    public static class WorldGenerationDiagnosticFlows
    {
        public static readonly DiagnosticFlowDefinition WorldBuildFromGeneratorStartup = new DiagnosticFlowDefinition(
            "WorldBuildFromGeneratorStartup",
            "WorldBuildFromGeneratorStartup",
            "WG",
            new[]
            {
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.ProjectContextInstalled),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.SceneContextInstalled),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.DirectLaunchConfigured),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.BootstrapSubscribeWorldSignals),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.GeneratorInstallerInstallBindings),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.GeneratorStartupInitialize),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.MapVisualBuildWorld),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.GraphMapDataGenerate),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.TwcBuild),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.WorldGeneratedDataSignalFire),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive),
            },
            timeoutMilliseconds: 6000d,
            strictOrder: true);

        public static readonly DiagnosticFlowDefinition WorldBuildFromSavedDataStartup = new DiagnosticFlowDefinition(
            "WorldBuildFromSavedDataStartup",
            "WorldBuildFromSavedDataStartup",
            "WG",
            new[]
            {
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.ProjectContextInstalled),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.SceneContextInstalled),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.DirectLaunchConfigured),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.BootstrapSubscribeWorldSignals),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.GeneratorInstallerInstallBindings),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.GeneratorStartupInitialize),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.MapVisualBuildWorld),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.TwcBuild),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.WorldGeneratedDataSignalFire),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive),
            },
            timeoutMilliseconds: 6000d,
            strictOrder: true);

        public static readonly DiagnosticFlowDefinition StartingPositionStartupNewGame = new DiagnosticFlowDefinition(
            "StartingPositionStartupNewGame",
            "StartingPositionStartupNewGame",
            "SP",
            new[]
            {
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.SpawnPositionsSelected),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.WorldSpawnPositionsSignalFire),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.FogRevealApply),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.CameraFocusStart),
            },
            timeoutMilliseconds: 6000d,
            strictOrder: true);

        public static readonly DiagnosticFlowDefinition StartingPositionStartupLoadRecovery = new DiagnosticFlowDefinition(
            "StartingPositionStartupLoadRecovery",
            "StartingPositionStartupLoadRecovery",
            "SP",
            new[]
            {
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.FogSnapshotValidated),
                DiagnosticStepDefinition.Optional(WorldGenerationDiagnosticSteps.FogRepairApply),
                DiagnosticStepDefinition.Optional(WorldGenerationDiagnosticSteps.FogRepairSkip),
                DiagnosticStepDefinition.Required(WorldGenerationDiagnosticSteps.CameraFocusStart),
            },
            timeoutMilliseconds: 6000d,
            strictOrder: true);
    }

    public interface IWorldGenerationDiagnostics
    {
        void ReplayProjectContextInstalledFromEnvironment();
        void ProjectContextInstalled(string details = null);
        void SceneContextInstalled(string details = null);
        void DirectLaunchConfigured(string details = null);
        void BootstrapSubscribedWorldSignals(string details = null);
        void GeneratorInstallerInstalled(string details = null);
        void GeneratorStartupInitialized(string details = null);
        void MapVisualBuildWorldCalled(string details = null);
        void GraphMapDataGenerated(string details = null);
        void TwcBuildCompleted(string details = null);
        void WorldGeneratedSignalFired(string details = null);
        void BootstrapWorldGeneratedReceived(string details = null);
        void BeginStartingPositionNewGame(string details = null);
        void BeginStartingPositionLoadRecovery(string details = null);
        void FogSnapshotValidated(string details = null);
        void FogRepairApplied(string details = null);
        void FogRepairSkipped(string details = null);
        void SpawnPositionsSelected(string details = null);
        void WorldSpawnPositionsSignalFired(string details = null);
        void FogRevealApplied(string details = null);
        void CameraFocused(string details = null);
        void SkipStartupStep(string stepId, string reason, string details = null);
        void FailStartupStep(string stepId, string reason, string details = null);
        void ReportStartup(bool forceTimeout = false);
    }

    public sealed class SceneWorldGenerationDiagnostics : IWorldGenerationDiagnostics
    {
        private readonly IDiagnosticFlowService _flowService;
        private readonly IDiagnosticsEnvironmentState _environmentState;
        private readonly System.Collections.Generic.List<StepEntry> _worldBuildHistory =
            new System.Collections.Generic.List<StepEntry>();
        private IDiagnosticFlow _worldBuildFlow;
        private IDiagnosticFlow _startingPositionFlow;
        private string _launchMode = "Unknown";
        private bool _hasWorldSettings;
        private bool _autoLoad;
        private int _maxPlayers;

        public SceneWorldGenerationDiagnostics(
            IDiagnosticFlowService flowService,
            IDiagnosticsEnvironmentState environmentState)
        {
            _flowService = flowService;
            _environmentState = environmentState;
        }

        public void ProjectContextInstalled(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.ProjectContextInstalled, details));
        }

        public void SceneContextInstalled(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.SceneContextInstalled, details));
        }

        public void DirectLaunchConfigured(string details = null)
        {
            UpdateLaunchState(details);
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.DirectLaunchConfigured, details));
            EnsureWorldBuildFlow(details);
        }

        public void BootstrapSubscribedWorldSignals(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.BootstrapSubscribeWorldSignals, details));
        }

        public void GeneratorInstallerInstalled(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.GeneratorInstallerInstallBindings, details));
        }

        public void GeneratorStartupInitialized(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.GeneratorStartupInitialize, details));
        }

        public void MapVisualBuildWorldCalled(string details = null)
        {
            EnsureWorldBuildFlow(details);
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.MapVisualBuildWorld, details));
        }

        public void GraphMapDataGenerated(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.GraphMapDataGenerate, details));
        }

        public void TwcBuildCompleted(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.TwcBuild, details));
        }

        public void WorldGeneratedSignalFired(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.WorldGeneratedDataSignalFire, details));
        }

        public void BootstrapWorldGeneratedReceived(string details = null)
        {
            RecordWorldBuildStep(StepEntry.Complete(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive, details));
            _flowService.ReportFlow(_worldBuildFlow);
        }

        public void BeginStartingPositionNewGame(string details = null)
        {
            if (_startingPositionFlow != null
                && !_startingPositionFlow.IsSummaryReported
                && _startingPositionFlow.Definition.Id == WorldGenerationDiagnosticFlows.StartingPositionStartupNewGame.Id)
            {
                return;
            }

            _startingPositionFlow = _flowService.StartFlow(
                WorldGenerationDiagnosticFlows.StartingPositionStartupNewGame,
                "starting-position:new-game",
                DiagnosticContext.FromDetails(details));
            _startingPositionFlow.CompleteStep(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive, details);
        }

        public void BeginStartingPositionLoadRecovery(string details = null)
        {
            if (_startingPositionFlow != null
                && !_startingPositionFlow.IsSummaryReported
                && _startingPositionFlow.Definition.Id == WorldGenerationDiagnosticFlows.StartingPositionStartupLoadRecovery.Id)
            {
                return;
            }

            _startingPositionFlow = _flowService.StartFlow(
                WorldGenerationDiagnosticFlows.StartingPositionStartupLoadRecovery,
                "starting-position:load-recovery",
                DiagnosticContext.FromDetails(details));
            _startingPositionFlow.CompleteStep(WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive, details);
        }

        public void FogSnapshotValidated(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.FogSnapshotValidated, details);
        }

        public void FogRepairApplied(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.FogRepairApply, details);
        }

        public void FogRepairSkipped(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.FogRepairSkip, details);
        }

        public void SpawnPositionsSelected(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.SpawnPositionsSelected, details);
        }

        public void WorldSpawnPositionsSignalFired(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.WorldSpawnPositionsSignalFire, details);
        }

        public void FogRevealApplied(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.FogRevealApply, details);
        }

        public void CameraFocused(string details = null)
        {
            _startingPositionFlow?.CompleteStep(WorldGenerationDiagnosticSteps.CameraFocusStart, details);
        }

        public void SkipStartupStep(string stepId, string reason, string details = null)
        {
            if (_startingPositionFlow != null && !IsWorldBuildStep(stepId))
                _startingPositionFlow.SkipStep(stepId, reason, details);
            else
                RecordWorldBuildStep(StepEntry.Skip(stepId, reason, details));
        }

        public void FailStartupStep(string stepId, string reason, string details = null)
        {
            if (_startingPositionFlow != null && !IsWorldBuildStep(stepId))
                _startingPositionFlow.FailStep(stepId, reason, details);
            else
                RecordWorldBuildStep(StepEntry.Fail(stepId, reason, details));
        }

        public void ReportStartup(bool forceTimeout = false)
        {
            if (_startingPositionFlow != null && !_startingPositionFlow.IsSummaryReported)
                _flowService.ReportFlow(_startingPositionFlow, forceTimeout);

            if (_worldBuildFlow != null && !_worldBuildFlow.IsSummaryReported)
                _flowService.ReportFlow(_worldBuildFlow, forceTimeout);
        }

        public void ReplayProjectContextInstalledFromEnvironment()
        {
            if (_environmentState.IsProjectContextInstalled)
                ProjectContextInstalled(_environmentState.ProjectContextInstallDetails);
        }

        private void RecordWorldBuildStep(StepEntry entry)
        {
            _worldBuildHistory.Add(entry);
            ApplyWorldBuildStep(entry);
        }

        private void ApplyWorldBuildStep(StepEntry entry)
        {
            if (_worldBuildFlow == null)
                return;

            switch (entry.Operation)
            {
                case StepOperation.Complete:
                    _worldBuildFlow.CompleteStep(entry.StepId, entry.Details);
                    break;
                case StepOperation.Skip:
                    _worldBuildFlow.SkipStep(entry.StepId, entry.Reason, entry.Details);
                    break;
                case StepOperation.Fail:
                    _worldBuildFlow.FailStep(entry.StepId, entry.Reason, entry.Details);
                    break;
            }
        }

        private void EnsureWorldBuildFlow(string detailsHint)
        {
            DiagnosticFlowDefinition targetDefinition = ResolveWorldBuildDefinition(detailsHint);
            if (_worldBuildFlow != null
                && _worldBuildFlow.Definition.Id == targetDefinition.Id
                && !_worldBuildFlow.IsSummaryReported)
            {
                return;
            }

            _worldBuildFlow = _flowService.StartFlow(
                targetDefinition,
                targetDefinition.DisplayName,
                BuildWorldBuildContext(detailsHint));

            for (int index = 0; index < _worldBuildHistory.Count; index++)
                ApplyWorldBuildStep(_worldBuildHistory[index]);
        }

        private DiagnosticContext BuildWorldBuildContext(string detailsHint)
        {
            var context = new DiagnosticContext()
                .Add("mode", _launchMode)
                .Add("hasWorldSettings", _hasWorldSettings)
                .Add("autoLoad", _autoLoad)
                .Add("maxPlayers", _maxPlayers);

            if (!string.IsNullOrWhiteSpace(detailsHint))
                context.Add("details", detailsHint);

            return context;
        }

        private DiagnosticFlowDefinition ResolveWorldBuildDefinition(string detailsHint)
        {
            string source = TryParseSource(detailsHint);
            if (source == "pending-save" || source == "autoload" || source == "pending-world-data")
                return WorldGenerationDiagnosticFlows.WorldBuildFromSavedDataStartup;

            if (source == "new" || source == "direct-test")
                return WorldGenerationDiagnosticFlows.WorldBuildFromGeneratorStartup;

            return _autoLoad
                ? WorldGenerationDiagnosticFlows.WorldBuildFromSavedDataStartup
                : WorldGenerationDiagnosticFlows.WorldBuildFromGeneratorStartup;
        }

        private static bool IsWorldBuildStep(string stepId)
        {
            return stepId == WorldGenerationDiagnosticSteps.ProjectContextInstalled
                || stepId == WorldGenerationDiagnosticSteps.SceneContextInstalled
                || stepId == WorldGenerationDiagnosticSteps.DirectLaunchConfigured
                || stepId == WorldGenerationDiagnosticSteps.BootstrapSubscribeWorldSignals
                || stepId == WorldGenerationDiagnosticSteps.GeneratorInstallerInstallBindings
                || stepId == WorldGenerationDiagnosticSteps.GeneratorStartupInitialize
                || stepId == WorldGenerationDiagnosticSteps.MapVisualBuildWorld
                || stepId == WorldGenerationDiagnosticSteps.GraphMapDataGenerate
                || stepId == WorldGenerationDiagnosticSteps.TwcBuild
                || stepId == WorldGenerationDiagnosticSteps.WorldGeneratedDataSignalFire
                || stepId == WorldGenerationDiagnosticSteps.BootstrapWorldGeneratedReceive;
        }

        private static string TryParseSource(string details)
        {
            if (string.IsNullOrWhiteSpace(details))
                return null;

            const string marker = "source=";
            int start = details.IndexOf(marker, System.StringComparison.Ordinal);
            if (start < 0)
                return null;

            start += marker.Length;
            int end = details.IndexOf(',', start);
            return end >= 0
                ? details.Substring(start, end - start).Trim()
                : details.Substring(start).Trim();
        }

        private void UpdateLaunchState(string details)
        {
            if (string.IsNullOrWhiteSpace(details))
                return;

            string mode = TryParseValue(details, "mode");
            if (!string.IsNullOrWhiteSpace(mode))
                _launchMode = mode;

            if (bool.TryParse(TryParseValue(details, "hasWorldSettings"), out bool hasWorldSettings))
                _hasWorldSettings = hasWorldSettings;

            if (bool.TryParse(TryParseValue(details, "autoLoad"), out bool autoLoad))
                _autoLoad = autoLoad;

            if (int.TryParse(TryParseValue(details, "maxPlayers"), out int maxPlayers))
                _maxPlayers = maxPlayers;
        }

        private string TryParseValue(string details, string key)
        {
            const System.StringComparison Comparison = System.StringComparison.Ordinal;
            string marker = key + "=";
            int start = details.IndexOf(marker, Comparison);
            if (start < 0)
                return null;

            start += marker.Length;
            int end = details.IndexOf(',', start);
            return end >= 0
                ? details.Substring(start, end - start).Trim()
                : details.Substring(start).Trim();
        }

        private enum StepOperation
        {
            Complete,
            Skip,
            Fail,
        }

        private readonly struct StepEntry
        {
            private StepEntry(StepOperation operation, string stepId, string reason, string details)
            {
                Operation = operation;
                StepId = stepId;
                Reason = reason;
                Details = details;
            }

            public StepOperation Operation { get; }
            public string StepId { get; }
            public string Reason { get; }
            public string Details { get; }

            public static StepEntry Complete(string stepId, string details)
            {
                return new StepEntry(StepOperation.Complete, stepId, null, details);
            }

            public static StepEntry Skip(string stepId, string reason, string details)
            {
                return new StepEntry(StepOperation.Skip, stepId, reason, details);
            }

            public static StepEntry Fail(string stepId, string reason, string details)
            {
                return new StepEntry(StepOperation.Fail, stepId, reason, details);
            }
        }
    }
}
