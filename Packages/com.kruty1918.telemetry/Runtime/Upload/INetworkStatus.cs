namespace Kruty1918.Telemetry.Upload
{
    /// <summary>Connectivity abstraction so the core stays Unity-free.</summary>
    public interface INetworkStatus
    {
        /// <summary>True when uploads should be attempted (cheap heuristic only).</summary>
        bool IsOnline { get; }
    }

    /// <summary>Always-online (tests, desktop tools).</summary>
    public sealed class AlwaysOnlineStatus : INetworkStatus
    {
        public bool IsOnline => true;
    }

    /// <summary>Manually switched — used by tests to simulate offline→online recovery.</summary>
    public sealed class ManualNetworkStatus : INetworkStatus
    {
        public bool IsOnline { get; set; } = true;
    }
}
