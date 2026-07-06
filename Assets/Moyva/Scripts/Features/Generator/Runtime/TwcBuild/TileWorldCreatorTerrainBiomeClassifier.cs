namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorTerrainBiomeClassifier
    {
        public static bool IsWater(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            string lower = id.ToLowerInvariant();
            return lower.StartsWith("water") || lower.Contains("ocean") || lower == "sea";
        }

        public static bool IsSand(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            string lower = id.ToLowerInvariant();
            return lower.StartsWith("sand") || lower.StartsWith("grass-coast") || lower == "beach" || lower == "coast";
        }

        public static bool HasWaterNeighbour(string[,] biomeMap, int x, int y, int width, int height)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                if (IsWater(biomeMap[nx, ny]))
                    return true;
            }

            return false;
        }
    }
}
