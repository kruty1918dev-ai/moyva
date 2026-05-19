namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Етапи join-пайплайна при вході гравця до кімнати.
    /// Залежності: використовується JoinRoomPanelService та суміжною transport/lobby логікою.
    /// </summary>
    internal enum JoinPipelineState
    {
        /// <summary>Пайплайн не виконується.</summary>
        Idle = 0,

        /// <summary>Виконуються попередні перевірки залежностей і вхідних даних.</summary>
        Preflight = 1,

        /// <summary>Визначається точна ціль входу: join code або lobby id.</summary>
        ResolvingTarget = 2,

        /// <summary>Виконується приєднання до lobby-рівня.</summary>
        JoiningLobby = 3,

        /// <summary>Налаштовується або під'єднується транспорт мережі.</summary>
        ConnectingTransport = 4,

        /// <summary>Join-процес успішно завершено.</summary>
        Ready = 5,

        /// <summary>Join-процес завершився помилкою.</summary>
        Failed = 6,
    }
}
