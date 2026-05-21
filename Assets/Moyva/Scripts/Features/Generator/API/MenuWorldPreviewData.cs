namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Публічний DTO для menu-only прев'ю світу.
    /// Містить лише фінальні карти, без внутрішніх runtime-структур генератора.
    /// </summary>
    public sealed class MenuWorldPreviewData
    {
        public MenuWorldPreviewData(
            int width,
            int height,
            int seed,
            string[,] biomeMap,
            string[,] objectMap,
            float[,] heightMap,
            string[,] buildingMap)
        {
            Width = width;
            Height = height;
            Seed = seed;
            BiomeMap = biomeMap;
            ObjectMap = objectMap;
            HeightMap = heightMap;
            BuildingMap = buildingMap;
        }

        public int Width { get; }
        public int Height { get; }
        public int Seed { get; }

        public string[,] BiomeMap { get; }
        public string[,] ObjectMap { get; }
        public float[,] HeightMap { get; }
        public string[,] BuildingMap { get; }
    }
}