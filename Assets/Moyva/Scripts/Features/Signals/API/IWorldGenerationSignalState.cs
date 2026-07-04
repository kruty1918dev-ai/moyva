namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Зберігає останні startup-critical world signals як replayable scene state.
    /// Дає пізнім підписникам змогу відновити world/spawn payload, навіть якщо вони пропустили SignalBus.Fire.
    /// </summary>
    public interface IWorldGenerationSignalState
    {
        /// <summary>
        /// Починає новий startup world cycle і очищає cached snapshots попереднього світу.
        /// Викликається на старті побудови нового/іншого світу в тій самій gameplay-сцені.
        /// </summary>
        /// <param name="sessionId">Стабільний session/world fingerprint для цього циклу.</param>
        /// <returns>Новий монотонний startup sequence.</returns>
        long BeginWorldSnapshotCycle(string sessionId);

        /// <summary>
        /// Повністю очищає cached world/spawn snapshots.
        /// </summary>
        void Clear();

        /// <summary>
        /// Повертає поточну world identity для metadata нових startup signals.
        /// </summary>
        bool TryGetCurrentWorldIdentity(out long startupSequence, out string sessionId);

        /// <summary>
        /// Зберігає останній payload побудованого світу.
        /// </summary>
        /// <param name="signal">Snapshot generated світу.</param>
        /// <returns>Нормалізований payload із заповненими metadata полями.</returns>
        WorldGeneratedDataSignal StoreWorldGeneratedData(WorldGeneratedDataSignal signal);

        /// <summary>
        /// Повертає останній payload побудованого світу, якщо він уже був опублікований.
        /// </summary>
        /// <param name="signal">Останній world snapshot.</param>
        /// <returns><see langword="true"/>, якщо payload доступний.</returns>
        bool TryGetWorldGeneratedData(out WorldGeneratedDataSignal signal);

        /// <summary>
        /// Зберігає останній payload стартових spawn assignments.
        /// </summary>
        /// <param name="signal">Snapshot стартових позицій.</param>
        /// <param name="storedSignal">Нормалізований accepted snapshot.</param>
        /// <returns><see langword="true"/>, якщо snapshot accepted і став latest state.</returns>
        bool TryStoreWorldSpawnPositions(WorldSpawnPositionsSignal signal, out WorldSpawnPositionsSignal storedSignal);

        /// <summary>
        /// Повертає останній payload стартових spawn assignments, якщо він уже був опублікований.
        /// </summary>
        /// <param name="signal">Останні spawn assignments.</param>
        /// <returns><see langword="true"/>, якщо payload доступний.</returns>
        bool TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal signal);
    }
}
