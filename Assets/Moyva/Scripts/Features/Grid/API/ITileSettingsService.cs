namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Контракт читання параметрів тайла (вага руху тощо) за його ідентифікатором.
    /// Зазвичай реалізація залежить від <see cref="TileRegistrySO"/>.
    /// </summary>
    public interface ITileSettingsService
    {
        /// <summary>
        /// Повертає вагу проходження тайла для алгоритмів руху/пошуку шляху.
        /// </summary>
        /// <param name="tileId">Ідентифікатор типу тайла.</param>
        /// <returns>Вага руху (чим більше, тим дорожче проходження).</returns>
        float GetTileWeight(string tileId);
    }
}