namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Інтерфейс для вузлів графа, що надають seed (зерно) генерації.
    /// Дозволяє GraphBasedMapDataGenerator отримати seed без рефлексії,
    /// забезпечуючи стабільний контракт між генератором та нодами.
    /// </summary>
    public interface ISeedProvider
    {
        /// <summary>Seed, що використовується для ініціалізації генерації.</summary>
        int Seed { get; }
    }
}
