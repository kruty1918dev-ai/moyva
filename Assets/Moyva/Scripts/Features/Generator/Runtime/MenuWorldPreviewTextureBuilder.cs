using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Compatibility facade for menu-only world preview texture generation.
    /// Runtime code should prefer IMenuWorldPreviewTextureBuilderService via Zenject.
    /// </summary>
    public static class MenuWorldPreviewTextureBuilder
    {
        private const int DefaultPixelsPerTile = 4;
        private const int DefaultMaxTextureEdge = 1024;

        public static Texture2D Build(
            MenuWorldPreviewData previewData,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry = null,
            BuildingRegistrySO buildingRegistry = null,
            int pixelsPerTile = DefaultPixelsPerTile,
            int maxTextureEdge = DefaultMaxTextureEdge,
            MoyvaProjectSettingsSO projectSettings = null)
        {
            var request = new MenuWorldPreviewTextureBuildRequest(
                previewData,
                tileRegistry,
                objectRegistry,
                buildingRegistry,
                pixelsPerTile,
                maxTextureEdge,
                projectSettings);

            return MenuWorldPreviewTextureBuilderComposition.Create().Build(request);
        }
    }
}
