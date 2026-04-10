namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Logging abstraction for the multiplayer subsystem.
    /// </summary>
    public interface IMultiplayerLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Trace(string message);
    }
}
