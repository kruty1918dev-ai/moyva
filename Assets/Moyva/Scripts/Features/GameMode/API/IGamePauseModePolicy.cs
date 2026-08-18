namespace Kruty1918.Moyva.GameMode.API
{
    /// <summary>
    /// Reports whether pause must remain local because a network session is active.
    /// </summary>
    public interface IGamePauseModePolicy
    {
        bool IsMultiplayerSessionActive { get; }
    }
}
