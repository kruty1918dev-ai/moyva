namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за bootstrap і оновлення world context для fog visuals.
    /// </summary>
    public interface IFogVisualWorldBootstrapper
    {
        /// <summary>
        /// Ініціалізує visual representation для світу заданого розміру.
        /// </summary>
        /// <param name="width">Ширина fog map у клітинках.</param>
        /// <param name="height">Висота fog map у клітинках.</param>
        /// <param name="context">Світовий контекст, потрібний для побудови visuals.</param>
        void Initialize(int width, int height, FogWorldVisualContext context);

        /// <summary>
        /// Оновлює world/grid context без зміни gameplay state.
        /// Викликається, коли змінились bounds, cell size або height/terrain maps.
        /// </summary>
        /// <param name="context">Оновлений контекст світу для visual presentation.</param>
        void SetWorldContext(FogWorldVisualContext context);
    }
}
