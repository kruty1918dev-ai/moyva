namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Реєстр IFogOfWarService для кожної фракції.
    /// Дозволяє отримати окремий сервіс туману для конкретної фракції.
    /// </summary>
    public interface IFogOfWarServiceRegistry
    {
        /// <summary>
        /// Зареєструвати сервіс туману для фракції.
        /// Перезапише попередній, якщо вже є.
        /// </summary>
        /// <param name="factionId">Ідентифікатор фракції.</param>
        /// <param name="service">Fog service, який буде асоційовано з фракцією.</param>
        void Register(string factionId, IFogOfWarService service);

        /// <summary>
        /// Спробувати отримати сервіс туману за factionId.
        /// Повертає false якщо фракція не зареєстрована.
        /// </summary>
        /// <param name="factionId">Ідентифікатор фракції.</param>
        /// <param name="service">Повернутий fog service для фракції.</param>
        /// <returns><see langword="true"/>, якщо фракція вже має зареєстрований fog service.</returns>
        bool TryGetFor(string factionId, out IFogOfWarService service);
    }
}
