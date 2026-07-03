using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime.Flows
{
    public static class FogStartupDiagnosticSteps
    {
        public const string WorldGenerated = "WorldGenerated";
        public const string FogServiceInitializeMap = "FogService.InitializeMap";
        public const string SpawnResolved = "SpawnResolved";
        public const string RevealArea = "RevealArea";
        public const string RegisterCoreVision = "RegisterCoreVision";
        public const string FlushVisual = "FlushVisual";
        public const string VolumeUpdaterRebuild = "VolumeUpdater.Rebuild";
    }

    public static class ConstructionDiagnosticSteps
    {
        public const string PlayerClickedBuild = "PlayerClickedBuild";
        public const string BuildRequestCreated = "BuildRequestCreated";
        public const string ResourcesChecked = "ResourcesChecked";
        public const string GridCellValidated = "GridCellValidated";
        public const string TerrainValidated = "TerrainValidated";
        public const string PreviewShown = "PreviewShown";
        public const string BuildConfirmed = "BuildConfirmed";
        public const string ResourcesReserved = "ResourcesReserved";
        public const string BuildingSpawned = "BuildingSpawned";
        public const string BuildingRegistered = "BuildingRegistered";
        public const string ConstructionSignalFired = "ConstructionSignalFired";
        public const string UiUpdated = "UIUpdated";
    }

    public static class SaveLoadDiagnosticSteps
    {
        public const string LoadRequested = "LoadRequested";
        public const string SlotResolved = "SlotResolved";
        public const string FileReadStarted = "FileReadStarted";
        public const string FileReadCompleted = "FileReadCompleted";
        public const string SaveDataDeserialized = "SaveDataDeserialized";
        public const string WorldDataRestored = "WorldDataRestored";
        public const string MapVisualBuildRequested = "MapVisualBuildRequested";
        public const string WorldGeneratedDataSignalFired = "WorldGeneratedDataSignalFired";
        public const string FogSnapshotRestored = "FogSnapshotRestored";
        public const string UnitsRestored = "UnitsRestored";
        public const string CameraFocused = "CameraFocused";
        public const string LoadCompleted = "LoadCompleted";
    }

    public static class MultiplayerSessionDiagnosticSteps
    {
        public const string ConnectRequested = "ConnectRequested";
        public const string TransportSelected = "TransportSelected";
        public const string LobbyResolved = "LobbyResolved";
        public const string SessionStarted = "SessionStarted";
        public const string LocalPlayerRegistered = "LocalPlayerRegistered";
        public const string ParticipantsSynced = "ParticipantsSynced";
        public const string GameplayReady = "GameplayReady";
    }

    public static class RuntimeDiagnosticFlowDefinitions
    {
        public static readonly DiagnosticFlowDefinition FogStartupReveal = new DiagnosticFlowDefinition(
            "FogStartupReveal",
            "FogStartupReveal",
            "FOG",
            new[]
            {
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.WorldGenerated),
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.FogServiceInitializeMap),
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.SpawnResolved),
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.RevealArea),
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.RegisterCoreVision),
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.FlushVisual),
                DiagnosticStepDefinition.Required(FogStartupDiagnosticSteps.VolumeUpdaterRebuild),
            },
            timeoutMilliseconds: 5000d,
            strictOrder: true);

        public static readonly DiagnosticFlowDefinition ConstructionPlacement = new DiagnosticFlowDefinition(
            "ConstructionPlacement",
            "ConstructionPlacement",
            "BP",
            new[]
            {
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.PlayerClickedBuild),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.BuildRequestCreated),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.ResourcesChecked),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.GridCellValidated),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.TerrainValidated),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.PreviewShown),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.BuildConfirmed),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.ResourcesReserved),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.BuildingSpawned),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.BuildingRegistered),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.ConstructionSignalFired),
                DiagnosticStepDefinition.Required(ConstructionDiagnosticSteps.UiUpdated),
            },
            timeoutMilliseconds: 6000d,
            strictOrder: true);

        public static readonly DiagnosticFlowDefinition SaveLoad = new DiagnosticFlowDefinition(
            "SaveLoad",
            "SaveLoad",
            "LOAD",
            new[]
            {
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.LoadRequested),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.SlotResolved),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.FileReadStarted),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.FileReadCompleted, timeoutMilliseconds: 6000d),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.SaveDataDeserialized),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.WorldDataRestored),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.MapVisualBuildRequested),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.WorldGeneratedDataSignalFired),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.FogSnapshotRestored),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.UnitsRestored),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.CameraFocused),
                DiagnosticStepDefinition.Required(SaveLoadDiagnosticSteps.LoadCompleted),
            },
            timeoutMilliseconds: 15000d,
            strictOrder: true);

        public static readonly DiagnosticFlowDefinition MultiplayerSession = new DiagnosticFlowDefinition(
            "MultiplayerSession",
            "MultiplayerSession",
            "MP",
            new[]
            {
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.ConnectRequested),
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.TransportSelected),
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.LobbyResolved),
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.SessionStarted),
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.LocalPlayerRegistered),
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.ParticipantsSynced),
                DiagnosticStepDefinition.Required(MultiplayerSessionDiagnosticSteps.GameplayReady),
            },
            timeoutMilliseconds: 12000d,
            strictOrder: true);
    }

    public interface IFogStartupDiagnostics : IFeatureFlowDiagnostics { }
    public interface IConstructionDiagnostics : IFeatureFlowDiagnostics { }
    public interface ISaveLoadDiagnostics : IFeatureFlowDiagnostics { }
    public interface IMultiplayerSessionDiagnostics : IFeatureFlowDiagnostics { }

    public interface IFeatureFlowDiagnostics
    {
        IDiagnosticFlow StartFlow(string subject = null, DiagnosticContext context = null);
        IDiagnosticFlow GetOrStartFlow(string flowKey, string subject = null, DiagnosticContext context = null);
        void CompleteStep(IDiagnosticFlow flow, string stepId, string details = null);
        void CompleteStep(IDiagnosticFlow flow, string stepId, DiagnosticContext context);
        void SkipStep(IDiagnosticFlow flow, string stepId, string reason, string details = null);
        void SkipStep(IDiagnosticFlow flow, string stepId, string reason, DiagnosticContext context);
        void FailStep(IDiagnosticFlow flow, string stepId, string reason, string details = null);
        void FailStep(IDiagnosticFlow flow, string stepId, string reason, DiagnosticContext context);
        void Report(IDiagnosticFlow flow, bool forceTimeout = false);
    }

    internal abstract class FeatureFlowDiagnostics : IFeatureFlowDiagnostics
    {
        private readonly IDiagnosticFlowService _flowService;
        private readonly DiagnosticFlowDefinition _definition;

        protected FeatureFlowDiagnostics(IDiagnosticFlowService flowService, DiagnosticFlowDefinition definition)
        {
            _flowService = flowService;
            _definition = definition;
        }

        public IDiagnosticFlow StartFlow(string subject = null, DiagnosticContext context = null)
        {
            return _flowService.StartFlow(_definition, subject, context ?? DiagnosticContext.Empty);
        }

        public IDiagnosticFlow GetOrStartFlow(string flowKey, string subject = null, DiagnosticContext context = null)
        {
            return _flowService.GetOrStartFlow(flowKey, _definition, subject, context ?? DiagnosticContext.Empty);
        }

        public void CompleteStep(IDiagnosticFlow flow, string stepId, string details = null)
        {
            (flow ?? StartFlow()).CompleteStep(stepId, details);
        }

        public void CompleteStep(IDiagnosticFlow flow, string stepId, DiagnosticContext context)
        {
            (flow ?? StartFlow()).CompleteStep(stepId, context);
        }

        public void SkipStep(IDiagnosticFlow flow, string stepId, string reason, string details = null)
        {
            (flow ?? StartFlow()).SkipStep(stepId, reason, details);
        }

        public void SkipStep(IDiagnosticFlow flow, string stepId, string reason, DiagnosticContext context)
        {
            (flow ?? StartFlow()).SkipStep(stepId, reason, context);
        }

        public void FailStep(IDiagnosticFlow flow, string stepId, string reason, string details = null)
        {
            (flow ?? StartFlow()).FailStep(stepId, reason, details);
        }

        public void FailStep(IDiagnosticFlow flow, string stepId, string reason, DiagnosticContext context)
        {
            (flow ?? StartFlow()).FailStep(stepId, reason, context);
        }

        public void Report(IDiagnosticFlow flow, bool forceTimeout = false)
        {
            (flow ?? StartFlow()).ReportSummary(forceTimeout);
        }
    }

    internal sealed class FogStartupDiagnostics : FeatureFlowDiagnostics, IFogStartupDiagnostics
    {
        public FogStartupDiagnostics(IDiagnosticFlowService flowService)
            : base(flowService, RuntimeDiagnosticFlowDefinitions.FogStartupReveal)
        {
        }
    }

    internal sealed class ConstructionDiagnostics : FeatureFlowDiagnostics, IConstructionDiagnostics
    {
        public ConstructionDiagnostics(IDiagnosticFlowService flowService)
            : base(flowService, RuntimeDiagnosticFlowDefinitions.ConstructionPlacement)
        {
        }
    }

    internal sealed class SaveLoadDiagnostics : FeatureFlowDiagnostics, ISaveLoadDiagnostics
    {
        public SaveLoadDiagnostics(IDiagnosticFlowService flowService)
            : base(flowService, RuntimeDiagnosticFlowDefinitions.SaveLoad)
        {
        }
    }

    internal sealed class MultiplayerSessionDiagnostics : FeatureFlowDiagnostics, IMultiplayerSessionDiagnostics
    {
        public MultiplayerSessionDiagnostics(IDiagnosticFlowService flowService)
            : base(flowService, RuntimeDiagnosticFlowDefinitions.MultiplayerSession)
        {
        }
    }
}
