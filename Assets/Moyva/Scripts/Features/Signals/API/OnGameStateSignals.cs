namespace Kruty1918.Moyva.Signals
{
    /// <summary>Надсилається GameStateService при старті нової гри.</summary>
    public struct GameStartedSignal { }

    /// <summary>Надсилається GameStateService при завершенні гри (перемога/поразка).</summary>
    public struct GameEndedSignal
    {
        /// <summary>FactionId.Value переможця, або null якщо нічия/скасування.</summary>
        public string WinnerId;
    }

    /// <summary>Надсилається коли гра ставиться на паузу/знімається з паузи.</summary>
    public struct GamePausedSignal
    {
        public bool IsPaused;
    }
}
