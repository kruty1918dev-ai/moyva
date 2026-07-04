using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public sealed partial class FogOfWarVolumeController
    {
        [TitleGroup("Preview")]
        [Button("Build Preview From Scene Grid")]
        [DisableInPlayMode]
        private void BuildPreviewFromSceneGrid()
        {
            if (_settings == null || ResolveFogManager() == null)
                return;

            if (!TryClearGeneratedFogOutput())
                return;

            var previewBuilder = ResolvePreviewBuilder();
            var sceneContextBuilder = ResolveSceneContextBuilder();
            if (previewBuilder == null || sceneContextBuilder == null)
                return;

            previewBuilder.BuildPreview(this, sceneContextBuilder.BuildContext(this));
        }

        [TitleGroup("Preview")]
        [Button("Clear Preview")]
        private void ClearGeneratedFogOutput()
        {
            TryClearGeneratedFogOutput();
        }

        [TitleGroup("Runtime Actions")]
        [Button("Rebuild Fog Volume")]
        [EnableIf(nameof(CanRequestRuntimeRebuild))]
        private void RebuildFogVolume()
        {
            _runtimeUpdater?.RequestFullRebuildFromController(this);
        }

        public float ResolveCellSize(float worldCellSize)
        {
            if (_overrideCellSize)
                return Mathf.Max(0.001f, _cellSizeOverride);

            if (_settings != null && !_settings.Volume.UseWorldCellSize)
                return Mathf.Max(0.001f, _settings.Volume.CellSizeOverride);

            return worldCellSize > 0.0001f ? worldCellSize : 1f;
        }

        private bool TryClearGeneratedFogOutput()
        {
            var outputCleaner = ResolveOutputCleaner();
            if (outputCleaner == null)
                return false;

            outputCleaner.ClearGeneratedOutput(ResolveFogManager());
            return true;
        }
    }
}
