namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Визначає, як часто volume visual updater застосовує накопичені зміни.
    /// </summary>
    public enum FogVolumeUpdateMode
    {
        /// <summary>
        /// Об'єднує зміни та перебудовує volume не частіше одного разу за кадр.
        /// </summary>
        DebouncePerFrame = 0,

        /// <summary>
        /// Перебудовує volume з інтервалом у секундах.
        /// </summary>
        Interval = 1,

        /// <summary>
        /// Застосовує visual update одразу після зміни fog state.
        /// </summary>
        Immediate = 2,
    }
}
