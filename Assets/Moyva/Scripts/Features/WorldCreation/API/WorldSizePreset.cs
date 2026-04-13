namespace Kruty1918.Moyva.WorldCreation.API
{
    /// <summary>
    /// Розмір генерованого світу.
    /// Визначає розміри карти в тайлах.
    /// </summary>
    public enum WorldSizePreset
    {
        /// <summary>32 × 32 тайли — підходить для швидких ігор.</summary>
        Small   = 0,

        /// <summary>64 × 64 тайли — збалансований розмір за замовчуванням.</summary>
        Medium  = 1,

        /// <summary>128 × 128 тайли — для тривалих стратегічних партій.</summary>
        Large   = 2,

        /// <summary>Гравець задає ширину та висоту вручну.</summary>
        Custom  = 3
    }
}
