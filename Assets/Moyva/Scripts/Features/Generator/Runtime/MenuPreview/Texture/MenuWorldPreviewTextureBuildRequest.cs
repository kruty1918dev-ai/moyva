using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public readonly struct MenuWorldPreviewTextureBuildRequest
    {
        public MenuWorldPreviewTextureBuildRequest(
            MenuWorldPreviewData previewData,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry,
            BuildingRegistrySO buildingRegistry,
            int pixelsPerTile,
            int maxTextureEdge,
            MoyvaProjectSettingsSO projectSettings)
        {
            PreviewData = previewData;
            TileRegistry = tileRegistry;
            ObjectRegistry = objectRegistry;
            BuildingRegistry = buildingRegistry;
            PixelsPerTile = pixelsPerTile;
            MaxTextureEdge = maxTextureEdge;
            ProjectSettings = projectSettings;
        }

        public MenuWorldPreviewData PreviewData { get; }
        public TileRegistrySO TileRegistry { get; }
        public MapObjectRegistrySO ObjectRegistry { get; }
        public BuildingRegistrySO BuildingRegistry { get; }
        public int PixelsPerTile { get; }
        public int MaxTextureEdge { get; }
        public MoyvaProjectSettingsSO ProjectSettings { get; }
    }
}
