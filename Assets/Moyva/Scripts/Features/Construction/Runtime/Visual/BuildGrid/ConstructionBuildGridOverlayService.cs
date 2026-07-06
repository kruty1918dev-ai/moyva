using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
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
        private readonly IGameModeService _gameModeService;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly IGridProjection _gridProjection;

        private bool _isConstructionModeActive;
        private bool _dirty = true;

        [Inject]
        public ConstructionBuildGridOverlayService(
            IConstructionVisualRootService roots,
            IConstructionBuildGridTileCollector tileCollector,
            IConstructionBuildGridTileFilter tileFilter,
            IConstructionBuildGridDiagnostics diagnostics,
            IConstructionBuildGridOverlayRenderer renderer,
            [InjectOptional] IGameModeService gameModeService = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _tileCollector = tileCollector;
            _tileFilter = tileFilter;
            _diagnostics = diagnostics;
            _renderer = renderer;
            _gameModeService = gameModeService;
            _gridProjection = gridProjection;
            _settingsProvider = settingsProvider;
        }

        public void Initialize()
        {
            _renderer.Initialize(_roots.RadiusRoot, ResolveShaderName());
            ApplyInitialModeState();
            _diagnostics.LogInitialized(ResolveShaderName(), _renderer.MaterialReady, _gridProjection != null);
        }

        public void SetConstructionModeActive(bool active)
        {
            _isConstructionModeActive = active;
            _dirty = true;
            _diagnostics.LogModeChanged(active);
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
        }

        public void Tick()
        {
            if (_isConstructionModeActive && _dirty)
                Rebuild();

            Draw();
        }

        public void Hide()
        {
            _entries.Clear();
            _renderer.SetVisible(false);
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

            _tileCollector.Collect(_entries, _tileFilter.ShouldRender, out ConstructionBuildGridCollectionStats stats);
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
                    : !_renderer.MaterialReady
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
    }
}
