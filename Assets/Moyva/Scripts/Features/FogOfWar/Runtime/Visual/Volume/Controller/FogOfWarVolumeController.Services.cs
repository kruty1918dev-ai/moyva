using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public sealed partial class FogOfWarVolumeController
    {
        private TileWorldCreatorManager ResolveFogManager()
        {
            if (_tileWorldCreatorManager == null)
                _tileWorldCreatorManager = GetComponent<TileWorldCreatorManager>();

            return _tileWorldCreatorManager;
        }

        private IFogVolumePreviewBuilder ResolvePreviewBuilder()
        {
            if (_previewBuilder != null)
                return _previewBuilder;

            return _previewBuilder = FogVolumeControllerEditorFallbackFactory.CreatePreviewBuilder();
        }

        private IFogVolumeSceneContextBuilder ResolveSceneContextBuilder()
        {
            if (_sceneContextBuilder != null)
                return _sceneContextBuilder;

            return _sceneContextBuilder = FogVolumeControllerEditorFallbackFactory.CreateSceneContextBuilder();
        }

        private IFogVolumeOutputCleaner ResolveOutputCleaner()
        {
            if (_outputCleaner != null)
                return _outputCleaner;

            return _outputCleaner = FogVolumeControllerEditorFallbackFactory.CreateOutputCleaner();
        }

        private IFogVolumeValidationService ResolveValidationService()
        {
            if (_validationService != null)
                return _validationService;

            return _validationService = FogVolumeControllerEditorFallbackFactory.CreateValidationService();
        }

        private bool HasSettings(FogOfWarSettings settings)
            => settings != null;
    }
}
