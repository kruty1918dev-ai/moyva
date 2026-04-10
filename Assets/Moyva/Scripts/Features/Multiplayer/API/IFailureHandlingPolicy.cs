namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Describes what the system should do when a multiplayer failure occurs.
    /// </summary>
    public interface IFailureHandlingPolicy
    {
        /// <summary>
        /// Called when a recoverable error occurs. Returns true if the system should retry.
        /// </summary>
        bool HandleRecoverable(FailureCategory category, string details);

        /// <summary>
        /// Called when a non-recoverable error occurs.
        /// </summary>
        void HandleNonRecoverable(FailureCategory category, string details);
    }
}
