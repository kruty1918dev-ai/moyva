namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Утиліти для клонування двовимірних масивів карт (string[,] та float[,]).
    /// Єдина реалізація для GeneratedWorldData, MapVisualInstantiator
    /// та GeneratedWorldSaveModule — усуває дублювання Clone-методів.
    /// </summary>
    internal static class MapArrayUtils
    {
        /// <summary>
        /// Створює глибоку копію рядкового масиву карти.
        /// Повертає null, якщо джерело null.
        /// </summary>
        internal static string[,] CloneStringMap(string[,] source)
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

        /// <summary>
        /// Створює глибоку копію числового масиву карти висот.
        /// Повертає null, якщо джерело null.
        /// </summary>
        internal static float[,] CloneFloatMap(float[,] source)
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

        internal static int[,] CloneIntMap(int[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new int[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    clone[x, y] = source[x, y];

            return clone;
        }
    }
}
