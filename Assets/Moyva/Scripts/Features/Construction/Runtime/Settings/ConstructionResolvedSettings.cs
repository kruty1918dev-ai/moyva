using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionResolvedSettings
    {
        public ConstructionResolvedSettings(
            ConstructionPlacementRulesProfileSO placementRulesProfile,
            ConstructionVisualProfileSO visualProfile,
            ConstructionInputProfileSO inputProfile,
            ConstructionWallProfileSO wallProfile,
            ConstructionDiagnosticsProfileSO diagnosticsProfile)
        {
            PlacementRulesProfile = placementRulesProfile;
            VisualProfile = visualProfile;
            InputProfile = inputProfile;
            WallProfile = wallProfile;
            DiagnosticsProfile = diagnosticsProfile;
        }

        public ConstructionPlacementRulesProfileSO PlacementRulesProfile { get; }
        public ConstructionVisualProfileSO VisualProfile { get; }
        public ConstructionInputProfileSO InputProfile { get; }
        public ConstructionWallProfileSO WallProfile { get; }
        public ConstructionDiagnosticsProfileSO DiagnosticsProfile { get; }
    }
}
