namespace Kruty1918.Moyva.Faction.API
{
    /// <summary>
    /// Тип контролера фракції.
    /// Human — керується локальним гравцем.
    /// Bot    — керується AI.
    /// Network — керується віддаленим гравцем через relay (для майбутнього мультиплеєру).
    /// </summary>
    public enum FactionType
    {
        Human   = 0,
        Bot     = 1,
        Network = 2,
    }
}
