namespace Kruty1918.Moyva.Signals
{
    /// <summary>Тип ігрового режиму. Використовується в GameModeChangedSignal.</summary>
    public enum GameModeType
    {
        Normal       = 0,
        Construction = 1
    }

    /// <summary>
    /// Надсилається GameModeService при зміні ігрового режиму.
    /// Отримується: TileInteractionService (вимикається в Construction),
    ///              ConstructionService (активується в Construction).
    /// </summary>
    public struct GameModeChangedSignal
    {
        public GameModeType NewMode;
    }

    /// <summary>
    /// Запит на зміну режиму гри від UI/інших модулів.
    /// Обробляється в GameMode модулі окремим сервісом-маршрутизатором.
    /// </summary>
    public struct GameModeChangeRequestedSignal
    {
        public GameModeType RequestedMode;
    }
}
