using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Simple failure policy: logs the error and returns false for recoverable,
    /// logs error for non-recoverable. No hard crashes.
    /// </summary>
    public sealed class SimpleFailureHandlingPolicy : IFailureHandlingPolicy
    {
        private readonly IMultiplayerLogger _logger;

        public SimpleFailureHandlingPolicy(IMultiplayerLogger logger)
        {
            _logger = logger;
        }

        public bool HandleRecoverable(FailureCategory category, string details)
        {
            _logger.Warn($"Recoverable failure [{category}]: {details}");
            return false;
        }

        public void HandleNonRecoverable(FailureCategory category, string details)
        {
            _logger.Error($"Non-recoverable failure [{category}]: {details}");
        }
    }
}
