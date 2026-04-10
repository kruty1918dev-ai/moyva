namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Categorizes a multiplayer failure for policy routing.
    /// </summary>
    public enum FailureCategory
    {
        Unknown,
        NetworkDisconnect,
        ConfigMismatch,
        WorldMismatch,
        ParticipantRejected,
        HostMigrationFailed,
        SessionFull,
        StrictLockViolation
    }
}
