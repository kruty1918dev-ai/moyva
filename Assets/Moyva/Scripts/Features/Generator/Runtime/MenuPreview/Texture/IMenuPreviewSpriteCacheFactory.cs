using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewSpriteCacheFactory
    {
        Dictionary<string, MenuPreviewSpriteData> BuildTiles(
            TileRegistrySO registry,
            MoyvaProjectSettingsSO settings,
            out MenuPreviewSpriteData fallback);

        Dictionary<string, MenuPreviewSpriteData> BuildObjects(
            MapObjectRegistrySO registry,
            MoyvaProjectSettingsSO settings);

        Dictionary<string, MenuPreviewSpriteData> BuildBuildings(
            BuildingRegistrySO registry,
            MoyvaProjectSettingsSO settings);
    }
}
