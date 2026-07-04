using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogLegacyOverlaySettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            settings.FogTileSpritePixelSize = ClampSpritePixelSize(settings.FogTileSpritePixelSize);
            settings.FogTileSizeInCells = ClampTileSizeInCells(settings.FogTileSizeInCells);
            settings.FogTileSeamOverlapPixels = Mathf.Max(0f, settings.FogTileSeamOverlapPixels);
            settings.FogMapEdgePaddingPixels = Mathf.Max(0f, settings.FogMapEdgePaddingPixels);
            settings.FogMapEdgeOverhangCells = Mathf.Max(0f, settings.FogMapEdgeOverhangCells);
            settings.FogTileTiling = Mathf.Max(1f, settings.FogTileTiling);
            settings.FogIconSpritePixelSize = ClampSpritePixelSize(settings.FogIconSpritePixelSize);
            settings.FogIconScale = Mathf.Max(0.1f, settings.FogIconScale);
            settings.Fog3DTopClearance = Mathf.Max(0f, settings.Fog3DTopClearance);
            settings.ShaderFogCullThreshold = Mathf.Clamp(settings.ShaderFogCullThreshold, 0f, 0.25f);
        }

        private static Vector2Int ClampSpritePixelSize(Vector2Int size)
        {
            return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }

        private static Vector2 ClampTileSizeInCells(Vector2 size)
        {
            return new Vector2(Mathf.Max(0.001f, size.x), Mathf.Max(0.001f, size.y));
        }
    }
}
