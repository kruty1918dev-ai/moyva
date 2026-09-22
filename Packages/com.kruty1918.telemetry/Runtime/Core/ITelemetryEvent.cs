namespace Kruty1918.Telemetry.Core
{
    /// <summary>
    /// Strongly typed telemetry event. Implement as a struct for allocation-free tracking.
    /// <see cref="WriteTo"/> must be a deterministic, reflection-free projection of the
    /// event payload; it is exercised at Track() time on the caller thread.
    /// </summary>
    public interface ITelemetryEvent
    {
        /// <summary>Stable machine name, e.g. "match.started". Never rename in place; bump version.</summary>
        string EventType { get; }

        /// <summary>Id of the registered <see cref="Contracts.EventContract"/> that governs this event.</summary>
        string ContractId { get; }

        /// <summary>Version of the contract the payload conforms to.</summary>
        int ContractVersion { get; }

        /// <summary>Serialize event payload fields. Must not allocate unboundedly or throw on bad data;
        /// report invalid values via <paramref name="writer"/> which validates against the contract.</summary>
        void WriteTo(ITelemetryEventWriter writer);
    }

    /// <summary>
    /// Reflection-free field sink used by <see cref="ITelemetryEvent.WriteTo"/>.
    /// All writes are validated against the active contract; invalid fields are
    /// recorded as violations and skipped rather than throwing.
    /// </summary>
    public interface ITelemetryEventWriter
    {
        void Field(string name, int value);
        void Field(string name, long value);
        void Field(string name, float value);
        void Field(string name, double value);
        void Field(string name, bool value);
        void Field(string name, string value);
        void Field(string name, int[] values);
        void Field(string name, long[] values);
        void Field(string name, float[] values);
        void Field(string name, string[] values);
        /// <summary>Write a nested object field using a scoped sub-writer.</summary>
        void FieldObject(string name, System.Action<ITelemetryEventWriter> write);
        /// <summary>Write an array of nested objects.</summary>
        void FieldObjectArray(string name, System.Action<ITelemetryEventWriter>[] write);
    }
}
