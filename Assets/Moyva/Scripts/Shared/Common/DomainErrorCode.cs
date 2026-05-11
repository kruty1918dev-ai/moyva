namespace Kruty1918.Moyva.Shared.Common
{
    /// <summary>
    /// Canonical domain error identifiers for expected runtime failures.
    /// </summary>
    public enum DomainErrorCode
    {
        None = 0,
        Unknown = 1,
        Cancelled = 2,
        Validation = 3,
        NotFound = 4,
        WrongPassword = 5,
        PermissionDenied = 6,
        Conflict = 7,
        Timeout = 8,
        Network = 9,
        RoomFull = 10,
        SessionExpired = 11,
        TransportFailure = 12
    }
}
