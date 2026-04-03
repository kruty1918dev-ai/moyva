namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GeneratedWorldData
    {
        public int Width;
        public int Height;
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;

        public GeneratedWorldData Clone()
        {
            return new GeneratedWorldData
            {
                Width = Width,
                Height = Height,
                BiomeMap = CloneStringMap(BiomeMap),
                ObjectMap = CloneStringMap(ObjectMap),
                HeightMap = CloneFloatMap(HeightMap),
            };
        }

        private static string[,] CloneStringMap(string[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new string[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    clone[x, y] = source[x, y];

            return clone;
        }

        private static float[,] CloneFloatMap(float[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new float[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    clone[x, y] = source[x, y];

            return clone;
        }
    }
}