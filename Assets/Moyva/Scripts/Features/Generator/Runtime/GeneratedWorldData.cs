namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Контейнер згенерованих даних світу: карти біомів, об'єктів, висот та будівель.
    /// Підтримує глибоке клонування для безпечної передачі між підсистемами.
    /// </summary>
    internal sealed class GeneratedWorldData
    {
        public int Width;
        public int Height;
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
        public string[,] BuildingMap;
        public float[,] ShoreDistanceMap;
        public int[,] ShoreMask;
        public int[,] NeighborMask;

        public GeneratedWorldData Clone()
        {
            return new GeneratedWorldData
            {
                Width = Width,
                Height = Height,
                BiomeMap = MapArrayUtils.CloneStringMap(BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(HeightMap),
                BuildingMap = MapArrayUtils.CloneStringMap(BuildingMap),
                ShoreDistanceMap = MapArrayUtils.CloneFloatMap(ShoreDistanceMap),
                ShoreMask = MapArrayUtils.CloneIntMap(ShoreMask),
                NeighborMask = MapArrayUtils.CloneIntMap(NeighborMask),
            };
        }
    }
}