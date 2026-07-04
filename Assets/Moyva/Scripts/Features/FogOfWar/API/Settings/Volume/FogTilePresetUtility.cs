using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public static class FogTilePresetUtility
    {
        public static bool HasUsableDualGridPreset(TilePreset preset)
        {
            return preset != null
                && (preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null);
        }
    }
}
