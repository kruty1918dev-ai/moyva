using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorTerrainPresetUtility
    {
        public static bool ShouldUseDualGrid(TilePreset preset, bool mappingPreference)
            => mappingPreference
                || preset != null && preset.gridtype == TilePreset.GridType.dual
                || preset != null && !HasUsableTilePreset(preset, false) && HasUsableTilePreset(preset, true);

        public static bool HasUsableTilePreset(TilePreset preset, bool useDualGrid)
        {
            if (preset == null)
                return false;

            if (useDualGrid)
            {
                return preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null;
            }

            return preset.NRMGRD_fillTile != null
                || preset.NRMGRD_singleTile != null
                || preset.NRMGRD_edgeFillTile != null
                || preset.NRMGRD_cornerFillTile != null
                || preset.NRMGRD_interiorCornerTile != null
                || preset.NRMGRD_doubleCornerTile != null
                || preset.NRMGRD_threeWayFillTile != null
                || preset.NRMGRD_edgeCornerFillTile != null;
        }
    }
}
