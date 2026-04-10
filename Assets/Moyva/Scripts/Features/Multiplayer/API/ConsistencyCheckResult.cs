namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Result of comparing a client world snapshot against the host's.
    /// </summary>
    public enum ConsistencyCheckResult
    {
        Equal,
        ConfigMismatch,
        WorldMismatch
    }
}
