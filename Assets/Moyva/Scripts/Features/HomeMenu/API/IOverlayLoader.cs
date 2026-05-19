namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт сервісу прогрес-оверлею в меню.
    /// Залежності: реалізується UI-шаром і використовується startup/join/create-room сценаріями.
    /// </summary>
    public interface IOverlayLoader
    {
        /// <summary>
        /// Показати оверлей і створити трекер його стану.
        /// </summary>
        /// <param name="value">Поточне значення прогресу.</param>
        /// <param name="maxValue">Максимальне значення прогресу.</param>
        /// <param name="sufix">Суфікс відображення прогресу.</param>
        /// <returns>Об'єкт стану завантаження.</returns>
        OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%");

        /// <summary>
        /// Оновити значення прогресу для вже відкритого оверлею.
        /// </summary>
        /// <param name="value">Поточне значення прогресу.</param>
        /// <param name="maxValue">Максимальне значення прогресу.</param>
        /// <param name="sufix">Суфікс відображення прогресу.</param>
        void UpdateOverlay(float value, float maxValue = 100, string sufix = "%");

        // If forceImmediate == true, the implementation should hide the overlay immediately
        // (skip delayed animation) to guarantee the panel is closed when initialization completes.
        /// <summary>
        /// Зупинити і сховати оверлей.
        /// </summary>
        /// <param name="forceImmediate">True, щоб сховати оверлей миттєво без анімації.</param>
        void StopOverlay(bool forceImmediate = false);

        // Lock prevents StopOverlay from hiding the overlay (e.g. during game launch).
        // Each LockOverlay call must be matched by one UnlockOverlay call.
        /// <summary>
        /// Заблокувати закриття оверлею до явного розблокування.
        /// </summary>
        void LockOverlay();

        /// <summary>
        /// Зняти блокування закриття оверлею.
        /// </summary>
        void UnlockOverlay();
    }
}