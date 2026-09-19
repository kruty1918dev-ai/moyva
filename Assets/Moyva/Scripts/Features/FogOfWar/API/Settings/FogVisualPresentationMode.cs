namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Визначає visual path для FogOfWar.
    /// Gameplay fog state від цього режиму не залежить.
    /// </summary>
    public enum FogVisualPresentationMode
    {
        /// <summary>
        /// Screen-space fog, який накладається на готовий кадр
        /// з урахуванням camera depth і world position.
        /// </summary>
        ScreenSpace = 0
    }
}