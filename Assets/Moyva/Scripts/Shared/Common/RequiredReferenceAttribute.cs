using System;

namespace Kruty1918.Moyva.Shared
{
    /// <summary>
    /// Визначає допустиме походження обов'язкового Unity-посилання.
    /// </summary>
    public enum RequiredReferenceKind
    {
        /// <summary>
        /// Дозволяє будь-який серіалізований Unity-об'єкт.
        /// </summary>
        Any,

        /// <summary>
        /// Вимагає посилання на prefab asset або його компонент.
        /// </summary>
        Prefab,

        /// <summary>
        /// Вимагає посилання на об'єкт поточної сцени.
        /// </summary>
        Scene,
    }

    /// <summary>
    /// Позначає серіалізоване поле, без якого runtime-функція не може працювати.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredReferenceAttribute : Attribute
    {
        /// <summary>
        /// Ініціалізує вимогу до Unity-посилання.
        /// </summary>
        /// <param name="kind">Допустиме походження посилання.</param>
        public RequiredReferenceAttribute(RequiredReferenceKind kind = RequiredReferenceKind.Any)
        {
            Kind = kind;
        }

        /// <summary>
        /// Повертає допустиме походження посилання.
        /// </summary>
        public RequiredReferenceKind Kind { get; }
    }
}
