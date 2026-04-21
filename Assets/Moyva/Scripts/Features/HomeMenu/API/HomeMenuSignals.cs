namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Сигнал: HomeMenu завершив початкову ініціалізацію (прелоад конфігів/аудіо).
    /// Публікується <see cref="Runtime.HomeMenuFlow"/> після приховування оверлею завантаження.
    /// </summary>
    public struct HomeMenuReadySignal { }

    /// <summary>
    /// Сигнал: користувач натиснув "Start Game" у головному меню.
    /// Обробники можуть підготувати конфіги до показу WorldCreation.
    /// </summary>
    public struct HomeMenuStartRequestedSignal { }

    /// <summary>
    /// Сигнал: користувач підтвердив вихід з гри.
    /// </summary>
    public struct HomeMenuQuitRequestedSignal { }

    /// <summary>
    /// Сигнал: користувачем видалені всі користувацькі дані.
    /// </summary>
    public struct UserDataClearedSignal
    {
        /// <summary>Кількість видалених артефактів (слоти + конфіг).</summary>
        public int DeletedCount;
    }
}
