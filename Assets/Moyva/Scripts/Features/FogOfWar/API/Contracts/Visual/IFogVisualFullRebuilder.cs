namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за повний rebuild fog visual presentation.
    /// </summary>
    public interface IFogVisualFullRebuilder
    {
        /// <summary>
        /// Повністю перебудовує visual presentation зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        void RebuildFullVisual(IFogOfWarService fogService);
    }
}
