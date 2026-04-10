namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Categorises a multiplayer failure for policy routing.
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
