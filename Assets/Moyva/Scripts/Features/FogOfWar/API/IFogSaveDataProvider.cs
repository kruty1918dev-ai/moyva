namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Ізолює джерело збережених explored-даних для туману.
    /// Реалізації може викликати <see cref="Runtime.FogOfWarSaveModule"/>, але gameplay fog service
    /// не повинен напряму залежати від конкретного save storage.
    /// </summary>
    public interface IFogSaveDataProvider
    {
        /// <summary>
        /// Завантажує snapshot explored-клітинок, якщо він є в поточному джерелі даних.
        /// </summary>
        /// <returns>
        /// Двовимірний explored snapshot або <see langword="null"/>, якщо дані відсутні.
        /// </returns>
        bool[,] LoadExploredData();

        /// <summary>
        /// Зберігає explored snapshot у вибране джерело даних.
        /// </summary>
        /// <param name="explored">Стан explored-клітинок, який потрібно записати.</param>
        void SaveExploredData(bool[,] explored);
    }
}
