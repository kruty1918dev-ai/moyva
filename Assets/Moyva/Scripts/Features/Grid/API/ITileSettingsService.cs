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

        /// <summary>
        /// Чи заблоковано тайл для будівництва (наприклад вода). За замовчуванням <c>false</c>.
        /// </summary>
        /// <param name="tileId">Ідентифікатор типу тайла / шару.</param>
        bool IsBuildBlocked(string tileId);

        /// <summary>
        /// Вертикальний зсув поверхні шару (для розміщення об'єктів/юнітів на тайлі).
        /// Повертає <c>0</c>, якщо профіль шару відсутній.
        /// </summary>
        /// <param name="tileId">Ідентифікатор типу тайла / шару.</param>
        float GetSurfaceOffset(string tileId);
    }
}