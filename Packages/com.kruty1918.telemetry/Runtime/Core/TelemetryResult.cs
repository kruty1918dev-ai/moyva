namespace Kruty1918.Telemetry.Core
{
    /// <summary>Outcome of a Track() call. Never throws for data problems; inspect Status/Code.</summary>
    public readonly struct TelemetryResult
    {
        public readonly TelemetryResultStatus Status;
        /// <summary>Machine-readable reason, e.g. "contract.missing", "field.invalid:level".</summary>
        public readonly string Code;

        public TelemetryResult(TelemetryResultStatus status, string code = null)
        {
            Status = status;
            Code = code;
        }

        public bool Accepted => Status == TelemetryResultStatus.Accepted;

        public static readonly TelemetryResult Ok = new TelemetryResult(TelemetryResultStatus.Accepted);
        public static TelemetryResult Disabled() => new TelemetryResult(TelemetryResultStatus.Disabled, "disabled");
        public static TelemetryResult Rejected(string code) => new TelemetryResult(TelemetryResultStatus.Rejected, code);
        public static TelemetryResult Dropped(string code) => new TelemetryResult(TelemetryResultStatus.Dropped, code);
        public static TelemetryResult Faulted(string code) => new TelemetryResult(TelemetryResultStatus.Faulted, code);

        public override string ToString() => Code == null ? Status.ToString() : $"{Status}:{Code}";
    }

    public enum TelemetryResultStatus
    {
        /// <summary>Event validated, serialized and buffered into the current batch.</summary>
        Accepted = 0,
        /// <summary>Sink is disabled (consent/config); event intentionally not recorded.</summary>
        Disabled = 1,
        /// <summary>Event failed validation or contract resolution; counted as invalid.</summary>
        Rejected = 2,
        /// <summary>Event valid but dropped by sampling/priority policy; counted.</summary>
        Dropped = 3,
        /// <summary>Internal failure; event lost. Check diagnostics.</summary>
        Faulted = 4,
    }
}
