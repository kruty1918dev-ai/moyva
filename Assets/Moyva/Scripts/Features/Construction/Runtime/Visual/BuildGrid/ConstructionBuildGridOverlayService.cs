
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
            [InjectOptional] IConstructionBuildGridChunkSurfaceService chunkSurfaceService = null,
            [InjectOptional] IGameModeService gameModeService = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null,
            [InjectOptional] IMapVisualChunkRegistry chunkRegistry = null)
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
        }

        public void Initialize()
        {
            string shaderName = ResolveShaderName();

            _renderer.Initialize(_roots.RadiusRoot, shaderName);
            _chunkSurfaceService?.Initialize(shaderName);

            ApplyInitialModeState();

            _diagnostics.LogInitialized(shaderName, ResolveActiveMaterialReady(), _gridProjection != null);
        }

        public void SetConstructionModeActive(bool active)
        {
            _isConstructionModeActive = active;
            _dirty = true;
            _diagnostics.LogModeChanged(active);

            if (UseChunkSurfaceMode())
                _chunkSurfaceService?.SetVisible(active);

            if (!active)
                Hide();
        }

        public void MarkDirty() => _dirty = true;

        private void ApplyInitialModeState()
        {
            if (_gameModeService == null)
                return;

            _isConstructionModeActive = _gameModeService.CurrentMode == GameModeType.Construction;
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

        private void TickChunkSurfaceMode()
        {
            if (!_isConstructionModeActive)
                return;

            if (_dirty)
            {
                RebuildChunkSurface();
                return;
            }

            _chunkSurfaceService?.ApplyChunkVisibility();
        }

        private void RebuildChunkSurface()
        {
            _dirty = false;

            if (TryGetSkipReason(out string reason))
            {
                _diagnostics.LogRebuildSkipped(reason);
                Hide();
                return;
            }

            _entries.Clear();
            _renderer.SetVisible(false);
            ApplyGridStyleToChunkSurface();
            _chunkSurfaceService?.Rebuild();
            _chunkSurfaceService?.SetVisible(true);
            _chunkSurfaceService?.ApplyChunkVisibility();
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
            _tileCollector.Collect(_entries, _tileFilter.ShouldRender, out ConstructionBuildGridCollectionStats stats);
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
        private string ResolveShaderName() => _settingsProvider?.BuildGridShaderName ?? "Moyva/Overlay/ConstructionBuildGrid";

        private bool UseChunkSurfaceMode()
            => _settingsProvider?.BuildGridRenderMode == ConstructionBuildGridRenderMode.ChunkSurfacePlane;

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
            if (!_isConstructionModeActive)
                return;

            int currentVersion = ResolveChunkVisibilityVersion();
            if (currentVersion == _lastChunkVisibilityVersion)
                return;

            _lastChunkVisibilityVersion = currentVersion;

            if (UseChunkSurfaceMode())
            {
                _chunkSurfaceService?.ApplyChunkVisibility();
                return;
            }

            _dirty = true;
        }

        private int ResolveChunkVisibilityVersion()
            => _chunkRegistry?.CameraVisibilityVersion ?? -1;
    }
}