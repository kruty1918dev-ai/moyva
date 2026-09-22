namespace Kruty1918.Telemetry.Contracts
{
    /// <summary>
    /// Built-in contracts every host gets for free: application/session lifecycle,
    /// errors, performance aggregates, network transitions. These form the
    /// minimal coverage floor; products register their own on top.
    /// </summary>
    public static class StandardContracts
    {
        /// <summary>Idempotent: skips contracts already present at the same version.
        /// Safe to call on a registry that a host pre-populated.</summary>
        public static void RegisterAll(ContractRegistry r)
        {
            Register(r, new EventContract("app.lifecycle", 1, "app.lifecycle", "telemetry.core")
            {
                Description = "Application start/pause/resume/quit with reason.",
                Priority = TelemetryPriority.High,
            }
                .Add(new ContractField("phase", ContractFieldType.String) { Semantics = "started|paused|resumed|quitting|crashed" })
                .Add(new ContractField("uptimeMs", ContractFieldType.Long) { Unit = "ms", Semantics = "monotonic app uptime" })
                .Add(new ContractField("platform", ContractFieldType.String, required: false) { Semantics = "os/device category" })
                .Add(new ContractField("appVersion", ContractFieldType.String, required: false))
                .Add(new ContractField("exitReason", ContractFieldType.String, required: false) { Semantics = "clean|crash|kill|background-timeout" }));

            Register(r, new EventContract("app.error", 1, "app.error", "telemetry.core")
            {
                Description = "Unhandled exception metadata — type and message hash only, no stack PII.",
                Priority = TelemetryPriority.Critical,
            }
                .Add(new ContractField("exceptionType", ContractFieldType.String) { Semantics = "CLR type name" })
                .Add(new ContractField("messageHash", ContractFieldType.String) { Semantics = "sha256 short hash of message" })
                .Add(new ContractField("condition", ContractFieldType.String, required: false))
                .Add(new ContractField("isFatal", ContractFieldType.Bool))
                .Add(new ContractField("uptimeMs", ContractFieldType.Long)));

            Register(r, new EventContract("session.started", 1, "session.started", "telemetry.core")
            {
                Description = "Gameplay session opened (match/level context).",
                Priority = TelemetryPriority.Critical,
            }
                .Add(new ContractField("sessionKind", ContractFieldType.String) { Semantics = "match|training|editor" })
                .Add(new ContractField("mode", ContractFieldType.String, required: false))
                .Add(new ContractField("scenarioId", ContractFieldType.String, required: false)));

            Register(r, new EventContract("session.ended", 1, "session.ended", "telemetry.core")
            {
                Description = "Gameplay session closed with outcome.",
                Priority = TelemetryPriority.Critical,
            }
                .Add(new ContractField("sessionKind", ContractFieldType.String))
                .Add(new ContractField("outcome", ContractFieldType.String) { Semantics = "completed|abandoned|error" })
                .Add(new ContractField("durationMs", ContractFieldType.Long) { Unit = "ms" }));

            Register(r, new EventContract("perf.sample", 1, "perf.sample", "telemetry.core")
            {
                Description = "Aggregated performance sample (never per-frame).",
                Priority = TelemetryPriority.Low,
                SamplingRate = 1f,
            }
                .Add(new ContractField("frameMsP50", ContractFieldType.Float) { Unit = "ms" })
                .Add(new ContractField("frameMsP95", ContractFieldType.Float) { Unit = "ms" })
                .Add(new ContractField("allocKb", ContractFieldType.Long, required: false) { Unit = "KiB" })
                .Add(new ContractField("windowMs", ContractFieldType.Long) { Unit = "ms" }));

            Register(r, new EventContract("net.transition", 1, "net.transition", "telemetry.core")
            {
                Description = "Connectivity state transition observed by the host.",
            }
                .Add(new ContractField("state", ContractFieldType.String) { Semantics = "online|offline|metered" })
                .Add(new ContractField("uptimeMs", ContractFieldType.Long)));

            Register(r, new EventContract("telemetry.drop", 1, "telemetry.drop", "telemetry.core")
            {
                Description = "Drop accounting: data discarded by policy (pressure/consent/corruption).",
                Priority = TelemetryPriority.Critical,
            }
                .Add(new ContractField("reason", ContractFieldType.String))
                .Add(new ContractField("count", ContractFieldType.Long))
                .Add(new ContractField("scope", ContractFieldType.String, required: false)));
        }

        private static void Register(ContractRegistry r, EventContract c)
        {
            if (r == null || r.TryResolve(c.ContractId, c.Version, out var _)) return;
            try { r.Register(c); } catch (System.InvalidOperationException) { /* frozen */ }
        }
    }
}
