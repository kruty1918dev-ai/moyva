namespace Kruty1918.Moyva.WorldCreation.API
{
    /// <summary>
    /// Рівень складності гри. Впливає на поведінку ботів, стартові ресурси та
    /// агресивність ворогів.
    /// </summary>
    public enum DifficultyLevel
    {
        /// <summary>Боти пасивні; стартові ресурси збільшені.</summary>
        Easy   = 0,

        /// <summary>Збалансований виклик. Значення за замовчуванням.</summary>
        Normal = 1,

        /// <summary>Боти агресивніші; стартових ресурсів менше.</summary>
        Hard   = 2,

        /// <summary>Максимальна складність; мінімальні ресурси та найвища агресія.</summary>
        Brutal = 3
    }
}
