using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionSceneSettingsProvider :
        IConstructionSceneSettingsProvider,
        IConstructionPlacementRulesProvider,
        IConstructionVisualSettingsProvider,
        IConstructionInputSettingsProvider,
        IConstructionWallSettingsProvider,
        IConstructionDiagnosticsSettingsProvider
    {
        private readonly ConstructionSceneContext _sceneContext;
        private readonly int _fallbackMinSpacing;
        private readonly int _fallbackTownHallBuildRadius;

        [Inject]
        public ConstructionSceneSettingsProvider(
            [InjectOptional] ConstructionSceneContext sceneContext = null,
            [Inject(Id = "fallbackMinSpacing")] int fallbackMinSpacing = 0,
            [Inject(Id = "fallbackTownHallBuildRadius")] int fallbackTownHallBuildRadius = 0)
        {
            _sceneContext = sceneContext;
            _fallbackMinSpacing = Mathf.Max(0, fallbackMinSpacing);
            _fallbackTownHallBuildRadius = Mathf.Max(0, fallbackTownHallBuildRadius);
        }

        public ConstructionSystemProfileSO SystemProfile => _sceneContext != null ? _sceneContext.SystemProfile : null;

        public int MinSpacing => _sceneContext?.ResolvePlacementRulesProfile() != null
            ? _sceneContext.ResolvePlacementRulesProfile().MinSpacing
            : _fallbackMinSpacing;

        public int TownHallBuildRadius => _sceneContext?.ResolvePlacementRulesProfile() != null
            ? _sceneContext.ResolvePlacementRulesProfile().TownHallBuildRadius
            : _fallbackTownHallBuildRadius;

        public bool EnableInfluenceZoneRules => _sceneContext?.ResolvePlacementRulesProfile()?.EnableInfluenceZoneRules ?? true;
        public bool EnableTerrainRules => _sceneContext?.ResolvePlacementRulesProfile()?.EnableTerrainRules ?? true;
        public bool EnableFogRules => _sceneContext?.ResolvePlacementRulesProfile()?.EnableFogRules ?? true;
        public bool RequireVisibleFogTile => _sceneContext?.ResolvePlacementRulesProfile()?.RequireVisibleFogTile ?? true;
        public bool AllowBuildingOnWater => _sceneContext?.ResolvePlacementRulesProfile()?.AllowBuildingOnWater ?? false;
        public bool AllowBuildingOnHills => _sceneContext?.ResolvePlacementRulesProfile()?.AllowBuildingOnHills ?? true;
        public string[] BlockedTileIds => _sceneContext?.ResolvePlacementRulesProfile()?.BlockedTileIds ?? System.Array.Empty<string>();
        public TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges => _sceneContext?.ResolvePlacementRulesProfile()?.BlockedTerrainLevelRanges ?? System.Array.Empty<TerrainLevelRestrictionRange>();

        public int BuildingLayerMinSortingOrder => _sceneContext?.ResolveVisualProfile()?.BuildingLayerMinSortingOrder ?? 5;
        public float BuildingSurfaceOffsetY => _sceneContext?.ResolveVisualProfile()?.BuildingSurfaceOffsetY ?? 0.5f;
        public float PreviewSurfaceOffsetY => _sceneContext?.ResolveVisualProfile()?.PreviewSurfaceOffsetY ?? 0.7f;
        public float GhostAlpha => _sceneContext?.ResolveVisualProfile()?.GhostAlpha ?? 0.55f;
        public float BlockedFlashDurationSeconds => _sceneContext?.ResolveVisualProfile()?.BlockedFlashDurationSeconds ?? 0.35f;
        public bool UseInfluenceRadiusOverlay => _sceneContext?.ResolveVisualProfile()?.UseInfluenceRadiusOverlay ?? true;
        public float InfluenceRadiusFillAlpha => _sceneContext?.ResolveVisualProfile()?.InfluenceRadiusFillAlpha ?? 0.055f;
        public float InfluenceRadiusBorderWidth => _sceneContext?.ResolveVisualProfile()?.InfluenceRadiusBorderWidth ?? 0.5f;
        public string InfluenceRadiusShaderName2D => _sceneContext?.ResolveVisualProfile()?.InfluenceRadiusShaderName2D ?? "Moyva/2D/InfluenceRadius";
        public string InfluenceRadiusShaderName3D => _sceneContext?.ResolveVisualProfile()?.InfluenceRadiusShaderName3D ?? "Moyva/3D/InfluenceRadiusExistingMeshOverlay";
        public string PreviewRootName => _sceneContext?.ResolveVisualProfile()?.PreviewRootName ?? "ConstructionPreviewRoot";
        public string PlacedRootName => _sceneContext?.ResolveVisualProfile()?.PlacedRootName ?? "PlayerBuildingsRoot";
        public string RadiusRootName => _sceneContext?.ResolveVisualProfile()?.RadiusRootName ?? "ConstructionRadiusRoot";

        public float TouchTapMaxMovePixels => _sceneContext?.ResolveInputProfile()?.TouchTapMaxMovePixels ?? 18f;
        public float TouchTapMaxDurationSeconds => _sceneContext?.ResolveInputProfile()?.TouchTapMaxDurationSeconds ?? 0.45f;
        public bool EnableMousePendingPreviewDrag => _sceneContext?.ResolveInputProfile()?.EnableMousePendingPreviewDrag ?? true;
        public bool EnableTouchPendingPreviewDrag => _sceneContext?.ResolveInputProfile()?.EnableTouchPendingPreviewDrag ?? true;
        public bool EnableMultiTouchCancel => _sceneContext?.ResolveInputProfile()?.EnableMultiTouchCancel ?? true;
        public bool BlockInteractiveUI => _sceneContext?.ResolveInputProfile()?.BlockInteractiveUI ?? true;
        public bool AllowClicksThroughNonInteractiveUI => _sceneContext?.ResolveInputProfile()?.AllowClicksThroughNonInteractiveUI ?? true;

        public bool AllowGateReplacement => _sceneContext?.ResolveWallProfile()?.AllowGateReplacement ?? true;
        public bool GateRequiresHorizontalWall => _sceneContext?.ResolveWallProfile()?.GateRequiresHorizontalWall ?? true;
        public bool AllowWallPathThroughExistingWalls => _sceneContext?.ResolveWallProfile()?.AllowWallPathThroughExistingWalls ?? true;
        public bool AllowWallPathThroughPendingWalls => _sceneContext?.ResolveWallProfile()?.AllowWallPathThroughPendingWalls ?? true;
        public bool AllowWallPathThroughGates => _sceneContext?.ResolveWallProfile()?.AllowWallPathThroughGates ?? false;
        public bool ShowWallHandles => _sceneContext?.ResolveWallProfile()?.ShowWallHandles ?? true;
        public ConstructionWallPathMode WallPathMode => _sceneContext?.ResolveWallProfile()?.WallPathMode ?? ConstructionWallPathMode.OrthogonalOnly;

        public bool EnableVerboseLogs => _sceneContext?.ResolveDiagnosticsProfile()?.EnableVerboseLogs ?? (Application.isEditor && Debug.isDebugBuild);
        public bool EnablePlacementDebug => _sceneContext?.ResolveDiagnosticsProfile()?.EnablePlacementDebug ?? true;
        public bool EnableResourceDebug => _sceneContext?.ResolveDiagnosticsProfile()?.EnableResourceDebug ?? true;
        public bool EnableVisualDebug => _sceneContext?.ResolveDiagnosticsProfile()?.EnableVisualDebug ?? true;
        public bool EnableWallDebug => _sceneContext?.ResolveDiagnosticsProfile()?.EnableWallDebug ?? true;
        public bool DrawSceneGizmos => _sceneContext?.ResolveDiagnosticsProfile()?.DrawSceneGizmos ?? true;
        public bool DrawBlockedTiles => _sceneContext?.ResolveDiagnosticsProfile()?.DrawBlockedTiles ?? false;
        public bool DrawInfluenceZones => _sceneContext?.ResolveDiagnosticsProfile()?.DrawInfluenceZones ?? true;

        public Transform ResolvePreviewRoot()
        {
            EnsureSceneRoots();
            return _sceneContext?.SceneRoots?.PreviewRoot;
        }

        public Transform ResolvePlacedRoot()
        {
            EnsureSceneRoots();
            return _sceneContext?.SceneRoots?.PlacedRoot;
        }

        public Transform ResolveRadiusRoot()
        {
            EnsureSceneRoots();
            return _sceneContext?.SceneRoots?.RadiusRoot;
        }

        public Transform ResolveUiRoot()
        {
            EnsureSceneRoots();
            return _sceneContext?.SceneRoots?.UIRoot;
        }

        public Transform ResolveDebugRoot()
        {
            EnsureSceneRoots();
            return _sceneContext?.SceneRoots?.DebugRoot;
        }

        public void EnsureSceneRoots()
        {
            _sceneContext?.CreateMissingRoots();
        }
    }
}
