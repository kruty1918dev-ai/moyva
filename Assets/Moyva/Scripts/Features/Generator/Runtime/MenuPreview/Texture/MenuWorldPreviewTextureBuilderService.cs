using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuWorldPreviewTextureBuilderService : IMenuWorldPreviewTextureBuilderService
    {
        private readonly IMenuPreviewTextureSettingsResolver _settingsResolver;
        private readonly IMenuPreviewTextureLayoutService _layoutService;
        private readonly IMenuPreviewSpriteCacheFactory _spriteCacheFactory;
        private readonly IMenuPreviewMapLayerRenderer _layerRenderer;

        public MenuWorldPreviewTextureBuilderService(
            IMenuPreviewTextureSettingsResolver settingsResolver,
            IMenuPreviewTextureLayoutService layoutService,
            IMenuPreviewSpriteCacheFactory spriteCacheFactory,
            IMenuPreviewMapLayerRenderer layerRenderer)
        {
            _settingsResolver = settingsResolver;
            _layoutService = layoutService;
            _spriteCacheFactory = spriteCacheFactory;
            _layerRenderer = layerRenderer;
        }

        public Texture2D Build(MenuWorldPreviewTextureBuildRequest request)
        {
            if (request.PreviewData == null || request.PreviewData.Width <= 0 || request.PreviewData.Height <= 0)
                return null;

            var settings = _settingsResolver.Resolve(request.ProjectSettings);
            var layout = _layoutService.Create(request, settings);
            var canvas = new Color[layout.TextureWidth * layout.TextureHeight];
            var tileSprites = _spriteCacheFactory.BuildTiles(request.TileRegistry, settings, out var fallback);
            var context = new MenuPreviewTextureBuildContext(request, settings, layout, canvas, tileSprites, fallback);

            _layerRenderer.DrawTerrain(context);
            _layerRenderer.DrawObjects(context);
            _layerRenderer.DrawBuildings(context);

            return CreateTexture(context);
        }

        private static Texture2D CreateTexture(MenuPreviewTextureBuildContext context)
        {
            var settings = context.Settings;
            var texture = new Texture2D(context.Layout.TextureWidth, context.Layout.TextureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = settings.Uses3DProjectMode() ? settings.PreviewFilterMode : FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixels(context.Canvas);
            texture.Apply(false, false);
            return texture;
        }
    }
}
