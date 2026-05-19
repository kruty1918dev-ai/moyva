namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Тип ідентифікатора, який використовується для входу в кімнату.
    /// Залежності: використовується JoinRoomResolver та JoinRoomPanelService.
    /// </summary>
    public enum JoinRoomTargetKind
    {
        /// <summary>Ціль не визначена.</summary>
        None,

        /// <summary>Код підключення до кімнати.</summary>
        JoinCode,

        /// <summary>Глобальний ідентифікатор lobby.</summary>
        LobbyId
    }
}
