namespace Kruty1918.Moyva.WorldCreation.API
{
    /// <summary>
    /// Контракт сервісу, що зберігає поточний конфіг створення світу
    /// та надає утилітарні методи для UI.
    ///
    /// Реєструється у Zenject через <see cref="Runtime.WorldCreationInstaller"/>.
    /// </summary>
    public interface IWorldCreationService
    {
        /// <summary>
        /// Поточна конфігурація, яку редагує гравець на екрані створення світу.
        /// </summary>
        WorldCreationConfig CurrentConfig { get; }

        /// <summary>
        /// Замінює поточну конфігурацію новою.
        /// </summary>
        void UpdateConfig(WorldCreationConfig config);

        /// <summary>
        /// Скидає конфігурацію до значень з <see cref="WorldCreationDefaultsSO"/>.
        /// </summary>
        void ResetToDefaults();

        /// <summary>
        /// Генерує випадковий seed (≠ 0) і зберігає його в <see cref="CurrentConfig"/>.
        /// </summary>
        /// <returns>Згенерований seed.</returns>
        int GenerateRandomSeed();

        /// <summary>
        /// Перевіряє коректність конфігурації.
        /// </summary>
        /// <param name="errorMessage">
        /// Локалізоване повідомлення про помилку, або <c>null</c> якщо конфіг валідний.
        /// </param>
        /// <returns><c>true</c> якщо конфіг валідний.</returns>
        bool ValidateConfig(WorldCreationConfig config, out string errorMessage);

        /// <summary>
        /// Конвертує <see cref="WorldCreationConfig"/> у плоску структуру
        /// <see cref="Kruty1918.Moyva.Signals.WorldCreationConfigData"/> для сигналу.
        /// </summary>
        Kruty1918.Moyva.Signals.WorldCreationConfigData ToSignalData(WorldCreationConfig config);
    }
}
