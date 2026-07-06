using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MapVisualTileFallbacks
    {
        public static IEnumerable<string> Resolve(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                yield break;

            string normalized = tileId.ToLowerInvariant();
            if (normalized.Contains("deep-depth") || normalized.Contains("ocean-deep"))
            {
                yield return "water-deep-depth-tile-001";
                yield return "water-deep-depth-tile-002";
                yield return "ocean-deep";
            }
            else if (normalized.StartsWith("water") || normalized.Contains("water-"))
            {
                yield return "water-middle-depth-tile-002";
                yield return "water-deep-depth-tile-001";
                yield return "ocean-shallow";
            }
            else if (normalized.Contains("stone-hill") || normalized.Contains("hill") || normalized.Contains("mountain"))
            {
                yield return "grass-tile-level-3-001";
                yield return "grass-tile-level-2-001";
                yield return "hill";
            }
            else if (normalized.Contains("snow"))
            {
                yield return "snow-tile-001";
                yield return "grass-tile-level-1-001";
            }
            else if (normalized.Contains("sand") || normalized.Contains("coast"))
            {
                yield return "sand-tile-003";
                yield return "sand-tile-001";
                yield return "beach";
            }
            else if (normalized.Contains("grass") || normalized.Contains("lowland"))
            {
                yield return "grass-tile-001";
                yield return "texture-grass-tile-001";
                yield return "grass-tile-level-1-001";
                yield return "grass";
            }
        }
    }
}
