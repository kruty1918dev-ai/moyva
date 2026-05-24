using System;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Утиліти для клонування двовимірних масивів карт (string[,], float[,] та int[,]).
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
            Array.Copy(source, clone, source.Length);
            return clone;
        }

        internal static string[,] CloneStringMapOrCreate(string[,] source, int width, int height)
        {
            return source != null
                ? CloneStringMap(source)
                : new string[Math.Max(1, width), Math.Max(1, height)];
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
            Array.Copy(source, clone, source.Length);
            return clone;
        }

        internal static float[,] CloneFloatMapOrCreate(float[,] source, int width, int height)
        {
            return source != null
                ? CloneFloatMap(source)
                : new float[Math.Max(1, width), Math.Max(1, height)];
        }

        internal static int[,] CloneIntMap(int[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new int[width, height];
            Array.Copy(source, clone, source.Length);
            return clone;
        }

        internal static bool[,] CloneBoolMap(bool[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new bool[width, height];
            Array.Copy(source, clone, source.Length);
            return clone;
        }

        internal static bool[,] CloneBoolMapOrCreate(bool[,] source, int width, int height)
        {
            return source != null
                ? CloneBoolMap(source)
                : new bool[Math.Max(1, width), Math.Max(1, height)];
        }
    }
}
