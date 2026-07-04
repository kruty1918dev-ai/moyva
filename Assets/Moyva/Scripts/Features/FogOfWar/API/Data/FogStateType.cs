namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Визначає gameplay-стан клітинки туману.
    /// Source of truth для цих станів зберігається у <see cref="IFogOfWarService"/>,
    /// а visual updaters лише відображають його.
    /// </summary>
    public enum FogStateType : byte
    {
        /// <summary>
        /// Клітинка ще жодного разу не була відкрита гравцю.
        /// </summary>
        Unexplored = 0,

        /// <summary>
        /// Клітинка вже була відкрита раніше, але зараз не входить у поточну видимість.
        /// </summary>
        Explored   = 1,

        /// <summary>
        /// Клітинка видима прямо зараз.
        /// </summary>
        Visible    = 2
    }
}
