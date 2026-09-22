namespace Kruty1918.Telemetry.Core
{
    /// <summary>
    /// Producer-facing telemetry entry point. Implementations must be thread-safe,
    /// must not perform disk or network I/O inside <see cref="Track{T}"/>, and must
    /// never throw for malformed events — they return a diagnostic <see cref="TelemetryResult"/>.
    /// </summary>
    public interface ITelemetrySink
    {
        /// <summary>Validate, serialize and buffer a strongly typed event.</summary>
        TelemetryResult Track<T>(in T telemetryEvent) where T : struct, ITelemetryEvent;

        /// <summary>Generic low-level path for producers that cannot use structs.</summary>
        TelemetryResult TrackRaw(string eventType, string contractId, int contractVersion,
            System.Action<ITelemetryEventWriter> writePayload);

        /// <summary>Seal the in-flight batch into the local store. Safe to call any time.</summary>
        void Flush();

        /// <summary>Currently active session/identity context.</summary>
        TelemetrySessionContext Session { get; }
    }
}
