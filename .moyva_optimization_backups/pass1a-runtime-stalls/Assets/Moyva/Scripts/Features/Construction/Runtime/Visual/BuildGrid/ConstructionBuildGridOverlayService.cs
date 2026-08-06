
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridOverlayService : IConstructionBuildGridOverlayService
    {
        private readonly List<ConstructionBuildGridOverlayEntry> _entries = new();
        private readonly IConstructionVisualRootService _roots;
        private readonly IConstructionBuildGridTileCollector _tileCollector;
        private readonly IConstructionBuildGridTileFilter _tileFilter;
        private readonly IConstructionBuildGridDiagnostics _diagnostics;
        private readonly IConstructionBuildGridOverlayRenderer _renderer;
        private readonly IConstructionBuildGridChunkSurfaceService _chunkSurfaceService;
        private readonly IGameModeService _gameModeService;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly IGridProjection _gridProjection;
        private readonly IMapVisualChunkRegistry _chunkRegistry;
        private readonly BuildModeGridStateController _stateController;
        private readonly IConstructionService _constructionService;

        private bool _isConstructionModeActive;
        private bool _dirty = true;
        private int _lastChunkVisibilityVersion = -1;

        [Inject]
        public ConstructionBuildGridOverlayService(
            IConstructionVisualRootService roots,
            IConstructionBuildGridTileCollector tileCollector,
            IConstructionBuildGridTileFilter tileFilter,
            IConstructionBuildGridDiagnostics diagnostics,
            IConstructionBuildGridOverlayRenderer renderer,
            BuildModeGridStateController stateController,
            [InjectOptional] IConstructionBuildGridChunkSurfaceService chunkSurfaceService = null,
            [InjectOptional] IGameModeService gameModeService = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null,
            [InjectOptional] IMapVisualChunkRegistry chunkRegistry = null,
            [InjectOptional] IConstructionService constructionService = null)
        {
            _roots = roots;
            _tileCollector = tileCollector;
            _tileFilter = tileFilter;
            _diagnostics = diagnostics;
            _renderer = renderer;
            _chunkSurfaceService = chunkSurfaceService;
            _gameModeService = gameModeService;
            _gridProjection = gridProjection;
            _settingsProvider = settingsProvider;
            _chunkRegistry = chunkRegistry;
            _stateController = stateController;
            _constructionService = constructionService;
        }

        public void Initialize()
        {
            string shaderName = ResolveShaderName();

            _renderer.Initialize(_roots.RadiusRoot, shaderName);
            _chunkSurfaceService?.Initialize(shaderName);
            ApplyGridStyleToChunkSurface();

            ApplyInitialModeState();

            _diagnostics.LogInitialized(shaderName, ResolveActiveMaterialReady(), _gridProjection != null);
        }

        public void SetConstructionModeActive(bool active)
        {
            BuildModeGridState previousState = _stateController.State;
            string previousBuildingId = _stateController.SelectedBuildingId;
            bool stateChanged = _stateController.SetConstructionModeActive(active);
            if (active && _constructionService != null)
            {
                stateChanged |= _stateController.SetSelection(
                    _constructionService.State == BuildingPlacementState.Placing
                        ? _constructionService.GetSelectedBuildingId()
                        : null,
                    _constructionService.IsDemolishMode);
            }

            if (_isConstructionModeActive == active && !stateChanged)
                return;

            _isConstructionModeActive = active;
            _dirty = true;
            _diagnostics.LogStateTransition(
                previousState,
                _stateController.State,
                previousBuildingId,
                _stateController.SelectedBuildingId,
                active ? "construction-mode-enter" : "construction-mode-exit");

            if (UseChunkSurfaceMode())
                _chunkSurfaceService?.SetVisible(active);

            if (!active)
                Hide();
        }

        public void SetSelectedBuilding(string buildingId, bool isDemolishMode)
        {
            BuildModeGridState previousState = _stateController.State;
            string previousBuildingId = _stateController.SelectedBuildingId;
            if (!_stateController.SetSelection(buildingId, isDemolishMode))
                return;

            if (!UsesUnfilteredChunkSurface())
                _dirty = true;
            _diagnostics.LogStateTransition(
                previousState,
                _stateController.State,
                previousBuildingId,
                _stateController.SelectedBuildingId,
                isDemolishMode ? "demolish-mode" : "building-selection");
        }

        public void MarkDirty()
        {
            if (UsesUnfilteredChunkSurface())
                return;

            if (_dirty)
                return;

            _dirty = true;
            _diagnostics.LogFullRefreshRequested(_stateController.State, _stateController.SelectedBuildingId);
        }

        public void MarkDirty(Vector2Int position, int radius)
        {
            if (UsesUnfilteredChunkSurface())
                return;

            _diagnostics.LogPartialRefreshRequested(position, Mathf.Max(0, radius));
            if (UseChunkSurfaceMode())
            {
                _chunkSurfaceService?.InvalidateRegion(position, radius);
                return;
            }

            _dirty = true;
        }

        public void ResetWorld()
        {
            _dirty = true;
            _lastChunkVisibilityVersion = ResolveChunkVisibilityVersion();
            if (!UseChunkSurfaceMode())
                return;

            ApplyGridStyleToChunkSurface();
            _chunkSurfaceService?.ResetWorld();
            _chunkSurfaceService?.SetVisible(_isConstructionModeActive);
        }

        private void ApplyInitialModeState()
        {
            if (_gameModeService == null)
                return;

            _isConstructionModeActive = _gameModeService.CurrentMode == GameModeType.Construction;
            _stateController.SetConstructionModeActive(_isConstructionModeActive);
            if (_isConstructionModeActive && _constructionService != null)
            {
                _stateController.SetSelection(
                    _constructionService.State == BuildingPlacementState.Placing
                        ? _constructionService.GetSelectedBuildingId()
                        : null,
                    _constructionService.IsDemolishMode);
            }
            _dirty = true;

            if (UseChunkSurfaceMode())
                _chunkSurfaceService?.SetVisible(_isConstructionModeActive);
        }

        public void Tick()
        {
            RefreshChunkVisibilityDirtyFlag();

            if (UseChunkSurfaceMode())
            {
                TickChunkSurfaceMode();
                return;
            }

            if (_isConstructionModeActive && _dirty)
                Rebuild();

            Draw();
        }

        public void Hide()
        {
            _entries.Clear();
            _renderer.SetVisible(false);
            _chunkSurfaceService?.Hide();
        }

        public void Dispose()
        {
            Hide();
            _chunkSurfaceService?.Dispose();
        }

        private void TickChunkSurfaceMode()
        {
            if (_dirty)
                RebuildChunkSurface();

            float updateBudget = ResolveUpdateBudgetMilliseconds();
            _chunkSurfaceService?.ProcessUpdates(
                _isConstructionModeActive ? updateBudget : Mathf.Min(0.5f, updateBudget));
        }

        private void RebuildChunkSurface()
        {
            _dirty = false;

            if (!_isConstructionModeActive)
            {
                Hide();
                return;
            }

            if (TryGetSkipReason(out string reason))
            {
                _diagnostics.LogRebuildSkipped(reason);
                Hide();
                return;
            }

            _entries.Clear();
            _renderer.SetVisible(false);
            ApplyGridStyleToChunkSurface();
            _chunkSurfaceService?.EnsureVisibleChunks(invalidateMasks: false);
            if (!UsesUnfilteredChunkSurface())
                _chunkSurfaceService?.InvalidateAllMasks();
            _chunkSurfaceService?.SetVisible(_isConstructionModeActive);
            _lastChunkVisibilityVersion = ResolveChunkVisibilityVersion();
        }

        private void Rebuild()
        {
            _dirty = false;
            if (TryGetSkipReason(out string reason))
            {
                _diagnostics.LogRebuildSkipped(reason);
                Hide();
                return;
            }

            _chunkSurfaceService?.Hide();
            _tileCollector.Collect(_entries, _tileFilter.ResolveVisualState, out ConstructionBuildGridCollectionStats stats);
            _lastChunkVisibilityVersion = ResolveChunkVisibilityVersion();
            if (_entries.Count == 0)
            {
                _diagnostics.LogRebuildCompleted(stats);
                Hide();
                return;
            }

            _renderer.ApplyStyle(
                new Color(0.70f, 0.95f, 1f, ResolveLineAlpha()),
                new Color(0.70f, 0.95f, 1f, ResolveFillAlpha()),
                ResolveLineWidth());
            _renderer.SetVisible(true);
            _diagnostics.LogRebuildCompleted(stats);
        }

        private void Draw()
        {
            if (!_isConstructionModeActive)
                return;

            _renderer.Draw(_entries, _diagnostics);
        }

        private bool TryGetSkipReason(out string reason)
        {
            reason = !_isConstructionModeActive
                ? "construction mode is inactive"
                : !ResolveUseOverlay()
                    ? "visual profile disabled build-grid overlay"
                    : !ResolveActiveMaterialReady()
                        ? "material is missing"
                        : _gridProjection != null && !GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                            ? $"unsupported grid world plane '{_gridProjection.WorldPlane}'"
                            : null;
            return reason != null;
        }

        private bool ResolveUseOverlay() => _settingsProvider?.UseBuildGridOverlay ?? true;
        private float ResolveFillAlpha() => Mathf.Clamp01(_settingsProvider?.BuildGridFillAlpha ?? 0.045f);
        private float ResolveLineAlpha() => Mathf.Clamp01(_settingsProvider?.BuildGridLineAlpha ?? 0.22f);
        private float ResolveLineWidth() => Mathf.Clamp(_settingsProvider?.BuildGridLineWidthNormalized ?? 0.035f, 0.005f, 0.49f);
        private float ResolveUpdateBudgetMilliseconds() => Mathf.Max(0.1f, _settingsProvider?.BuildGridUpdateBudgetMilliseconds ?? 2f);
        private string ResolveShaderName() => _settingsProvider?.BuildGridShaderName ?? "Moyva/Overlay/ConstructionBuildGrid";

        private bool UseChunkSurfaceMode()
            => _settingsProvider?.BuildGridRenderMode == ConstructionBuildGridRenderMode.ChunkSurfacePlane;

        private bool UsesUnfilteredChunkSurface()
            => UseChunkSurfaceMode()
               && !(_settingsProvider?.BuildGridSurfacePlaneUseBuildableFilter ?? false);

        private bool ResolveActiveMaterialReady()
            => UseChunkSurfaceMode()
                ? _chunkSurfaceService?.MaterialReady == true
                : _renderer.MaterialReady;

        private void ApplyGridStyleToChunkSurface()
        {
            _chunkSurfaceService?.ApplyStyle(
                new Color(0.70f, 0.95f, 1f, ResolveLineAlpha()),
                new Color(0.70f, 0.95f, 1f, ResolveFillAlpha()),
                ResolveLineWidth());
        }

        private void RefreshChunkVisibilityDirtyFlag()
        {
            int currentVersion = ResolveChunkVisibilityVersion();
            if (currentVersion == _lastChunkVisibilityVersion)
                return;

            _lastChunkVisibilityVersion = currentVersion;

            if (UseChunkSurfaceMode())
            {
                _chunkSurfaceService?.EnsureVisibleChunks(invalidateMasks: false);
                _chunkSurfaceService?.ApplyChunkVisibility();
                return;
            }

            _dirty = true;
        }

        private int ResolveChunkVisibilityVersion()
            => _chunkRegistry?.CameraVisibilityVersion ?? -1;
    }
}
