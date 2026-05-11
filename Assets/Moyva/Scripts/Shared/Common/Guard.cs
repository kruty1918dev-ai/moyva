using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Shared.Common
{
    public static class Guard
    {
        public static T NotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
            return value;
        }

        public static string NotNullOrWhiteSpace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", paramName);
            return value;
        }

        /// <summary>
        /// Перевіряє довільну умову. При порушенні — <see cref="ArgumentException"/> з <paramref name="message"/>.
        /// </summary>
        public static void Requires(bool condition, string message, string paramName = null)
        {
            if (!condition)
                throw new ArgumentException(message ?? "Precondition failed.", paramName ?? string.Empty);
        }

        /// <summary>
        /// Перевіряє, що значення лежить у діапазоні [<paramref name="min"/>, <paramref name="max"/>] включно.
        /// </summary>
        public static T IsInRange<T>(T value, T min, T max, string paramName)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
            return value;
        }

        /// <summary>
        /// Перевіряє, що рядковий ідентифікатор не порожній і не перевищує максимальну довжину.
        /// </summary>
        public static string IsValidId(string value, string paramName, int maxLength = 128)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Id cannot be null or whitespace.", paramName);
            if (value.Length > maxLength)
                throw new ArgumentException($"Id exceeds maximum length of {maxLength} characters.", paramName);
            return value;
        }

        /// <summary>
        /// Перевіряє, що колекція не null і не порожня.
        /// </summary>
        public static IEnumerable<T> NotEmpty<T>(IEnumerable<T> collection, string paramName)
        {
            if (collection == null)
                throw new ArgumentNullException(paramName);

            // Перебираємо один елемент, щоб перевірити наявність хоча б одного.
            using var e = collection.GetEnumerator();
            if (!e.MoveNext())
                throw new ArgumentException("Collection must not be empty.", paramName);

            return collection;
        }
    }
}
