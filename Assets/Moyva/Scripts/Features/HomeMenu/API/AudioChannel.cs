namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Логічні канали звуку, якими керує <see cref="IAudioSettingsService"/>.
    /// Значення відповідають параметрам у <c>AudioMixer</c>.
    /// </summary>
    public enum AudioChannel
    {
        /// <summary>Майстер-гучність.</summary>
        Master = 0,
        /// <summary>Фонова музика.</summary>
        Music  = 1,
        /// <summary>Звукові ефекти (SFX).</summary>
        Sfx    = 2,
        /// <summary>Інтерфейсні звуки (кліки, підтвердження).</summary>
        Ui     = 3
    }
}