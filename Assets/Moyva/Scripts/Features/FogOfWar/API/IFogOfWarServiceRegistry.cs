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
        void Register(string factionId, IFogOfWarService service);

        /// <summary>
        /// Спробувати отримати сервіс туману за factionId.
        /// Повертає false якщо фракція не зареєстрована.
        /// </summary>
        bool TryGetFor(string factionId, out IFogOfWarService service);
    }
}
