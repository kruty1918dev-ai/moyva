using System.Text;
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class ConstructionSceneContext : MonoBehaviour
    {
        [TabGroup("Overview", "Setup"), Required]
        [SerializeField] private ConstructionSystemProfileSO _systemProfile;

        [TabGroup("Overview", "Setup"), Required]
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        [TabGroup("Overview", "Setup")]
        [SerializeField] private ConstructionSceneRoots _sceneRoots = new();

        [TabGroup("Overview", "Setup")]
        [SerializeField] private ConstructionSceneOverrides _sceneOverrides = new();

        public ConstructionSystemProfileSO SystemProfile => _systemProfile;
        public BuildingRegistrySO BuildingRegistry => _buildingRegistry;
        public ConstructionSceneRoots SceneRoots => _sceneRoots;
        public ConstructionSceneOverrides SceneOverrides => _sceneOverrides;

        [TabGroup("Profiles", "Resolved"), ShowInInspector, ReadOnly]
        private ConstructionPlacementRulesProfileSO ResolvedPlacementRulesProfile => ResolvePlacementRulesProfile();

        [TabGroup("Profiles", "Resolved"), ShowInInspector, ReadOnly]
        private ConstructionVisualProfileSO ResolvedVisualProfile => ResolveVisualProfile();

        [TabGroup("Profiles", "Resolved"), ShowInInspector, ReadOnly]
        private ConstructionInputProfileSO ResolvedInputProfile => ResolveInputProfile();

        [TabGroup("Profiles", "Resolved"), ShowInInspector, ReadOnly]
        private ConstructionWallProfileSO ResolvedWallProfile => ResolveWallProfile();

        [TabGroup("Profiles", "Resolved"), ShowInInspector, ReadOnly]
        private ConstructionDiagnosticsProfileSO ResolvedDiagnosticsProfile => ResolveDiagnosticsProfile();

        [TabGroup("Overview", "Status"), ShowInInspector, ReadOnly, PropertyOrder(-5)]
        private string ValidationSummary => BuildValidationSummary();

        [TabGroup("Overview", "Actions"), Button]
        public void CreateMissingRoots()
        {
            EnsureRoot(ref _sceneRoots.PreviewRoot, ResolveRootName(_sceneOverrides?.VisualProfileOverride?.PreviewRootName, _systemProfile?.VisualProfile?.PreviewRootName, "ConstructionPreviewRoot"));
            EnsureRoot(ref _sceneRoots.PlacedRoot, ResolveRootName(_sceneOverrides?.VisualProfileOverride?.PlacedRootName, _systemProfile?.VisualProfile?.PlacedRootName, "PlayerBuildingsRoot"));
            EnsureRoot(ref _sceneRoots.RadiusRoot, ResolveRootName(_sceneOverrides?.VisualProfileOverride?.RadiusRootName, _systemProfile?.VisualProfile?.RadiusRootName, "ConstructionRadiusRoot"));
            EnsureRoot(ref _sceneRoots.UIRoot, "ConstructionUIRoot");
            EnsureRoot(ref _sceneRoots.DebugRoot, "ConstructionDebugRoot");
        }

        [TabGroup("Overview", "Actions"), Button]
        public void SyncRootNamesFromProfiles()
        {
            if (_sceneRoots == null)
                _sceneRoots = new ConstructionSceneRoots();

            CreateMissingRoots();
        }

        [TabGroup("Overview", "Actions"), Button]
        public void AutoFindRoots()
        {
            if (_sceneRoots == null)
                _sceneRoots = new ConstructionSceneRoots();

            _sceneRoots.PreviewRoot ??= transform.Find(ResolveRootName(_sceneOverrides?.VisualProfileOverride?.PreviewRootName, _systemProfile?.VisualProfile?.PreviewRootName, "ConstructionPreviewRoot"));
            _sceneRoots.PlacedRoot ??= transform.Find(ResolveRootName(_sceneOverrides?.VisualProfileOverride?.PlacedRootName, _systemProfile?.VisualProfile?.PlacedRootName, "PlayerBuildingsRoot"));
            _sceneRoots.RadiusRoot ??= transform.Find(ResolveRootName(_sceneOverrides?.VisualProfileOverride?.RadiusRootName, _systemProfile?.VisualProfile?.RadiusRootName, "ConstructionRadiusRoot"));
            _sceneRoots.UIRoot ??= transform.Find("ConstructionUIRoot");
            _sceneRoots.DebugRoot ??= transform.Find("ConstructionDebugRoot");
        }

        [TabGroup("Overview", "Actions"), Button]
        public void ClearEmptyRoots()
        {
            _sceneRoots?.ClearEmptyRoots();
        }

        public ConstructionPlacementRulesProfileSO ResolvePlacementRulesProfile() => _sceneOverrides?.PlacementRulesProfileOverride != null
            ? _sceneOverrides.PlacementRulesProfileOverride
            : _systemProfile != null ? _systemProfile.PlacementRulesProfile : null;

        public ConstructionVisualProfileSO ResolveVisualProfile() => _sceneOverrides?.VisualProfileOverride != null
            ? _sceneOverrides.VisualProfileOverride
            : _systemProfile != null ? _systemProfile.VisualProfile : null;

        public ConstructionInputProfileSO ResolveInputProfile() => _sceneOverrides?.InputProfileOverride != null
            ? _sceneOverrides.InputProfileOverride
            : _systemProfile != null ? _systemProfile.InputProfile : null;

        public ConstructionWallProfileSO ResolveWallProfile() => _sceneOverrides?.WallProfileOverride != null
            ? _sceneOverrides.WallProfileOverride
            : _systemProfile != null ? _systemProfile.WallProfile : null;

        public ConstructionDiagnosticsProfileSO ResolveDiagnosticsProfile() => _sceneOverrides?.DiagnosticsProfileOverride != null
            ? _sceneOverrides.DiagnosticsProfileOverride
            : _systemProfile != null ? _systemProfile.DiagnosticsProfile : null;

        private void OnValidate()
        {
            if (_buildingRegistry == null && _systemProfile != null && _systemProfile.BuildingRegistry != null)
                _buildingRegistry = _systemProfile.BuildingRegistry;

            _sceneRoots ??= new ConstructionSceneRoots();
            _sceneOverrides ??= new ConstructionSceneOverrides();
        }

        private string BuildValidationSummary()
        {
            var builder = new StringBuilder();
            if (_systemProfile == null)
                builder.AppendLine("- System profile is missing.");
            if (_buildingRegistry == null)
                builder.AppendLine("- Building registry is missing.");
            if (_systemProfile != null && _systemProfile.BuildingRegistry != null && _buildingRegistry != null && _systemProfile.BuildingRegistry != _buildingRegistry)
                builder.AppendLine("- Scene registry differs from system profile registry.");
            if (_systemProfile != null && _systemProfile.EconomyRulesProfile == null)
                builder.AppendLine("- Economy rules profile is missing.");
            if (_systemProfile != null && _systemProfile.FogOfWarSettings == null)
                builder.AppendLine("- Fog settings profile is missing.");
            if (ResolvePlacementRulesProfile() == null)
                builder.AppendLine("- Placement rules profile is missing.");
            if (ResolveVisualProfile() == null)
                builder.AppendLine("- Visual profile is missing.");
            if (ResolveInputProfile() == null)
                builder.AppendLine("- Input profile is missing.");
            if (ResolveWallProfile() == null)
                builder.AppendLine("- Wall profile is missing.");
            if (ResolveDiagnosticsProfile() == null)
                builder.AppendLine("- Diagnostics profile is missing.");
            if (_sceneRoots?.PreviewRoot == null)
                builder.AppendLine("- Preview root is not assigned.");
            if (_sceneRoots?.PlacedRoot == null)
                builder.AppendLine("- Placed root is not assigned.");
            if (_sceneRoots?.RadiusRoot == null)
                builder.AppendLine("- Radius root is not assigned.");
            if (_sceneRoots?.UIRoot == null)
                builder.AppendLine("- UI root is not assigned.");
            if (_sceneRoots?.DebugRoot == null)
                builder.AppendLine("- Debug root is not assigned.");
            return builder.Length == 0 ? "Scene context looks valid." : builder.ToString().TrimEnd();
        }

        private static string ResolveRootName(string overrideValue, string profileValue, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(overrideValue))
                return overrideValue;
            if (!string.IsNullOrWhiteSpace(profileValue))
                return profileValue;
            return fallback;
        }

        private void EnsureRoot(ref Transform target, string rootName)
        {
            if (target != null)
                return;

            Transform found = transform.Find(rootName);
            if (found != null)
            {
                target = found;
                return;
            }

            var go = new GameObject(rootName);
            go.transform.SetParent(transform, false);
            target = go.transform;
        }
    }
}
