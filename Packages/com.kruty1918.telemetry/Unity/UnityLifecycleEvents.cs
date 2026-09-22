using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Core;

namespace Kruty1918.Telemetry.Unity
{
    /// <summary>app.lifecycle — phase + uptime + platform + version + exit reason.</summary>
    [TelemetryEvent("app.lifecycle", 1)]
    public struct AppLifecycleEvent : ITelemetryEvent
    {
        public string Phase;
        public long UptimeMs;
        public string Platform;
        public string AppVersion;
        public string ExitReason;

        public string EventType => "app.lifecycle";
        public string ContractId => "app.lifecycle";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("phase", Phase);
            w.Field("uptimeMs", UptimeMs);
            w.Field("platform", Platform);
            w.Field("appVersion", AppVersion);
            if (ExitReason != null) w.Field("exitReason", ExitReason);
        }
    }

    /// <summary>app.error — sanitized exception metadata; message is hashed, never raw.</summary>
    [TelemetryEvent("app.error", 1)]
    public struct AppErrorEvent : ITelemetryEvent
    {
        public string ExceptionType;
        public string MessageHash;
        public string Condition;
        public bool IsFatal;
        public long UptimeMs;

        public string EventType => "app.error";
        public string ContractId => "app.error";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("exceptionType", ExceptionType);
            w.Field("messageHash", MessageHash);
            if (Condition != null) w.Field("condition", Condition);
            w.Field("isFatal", IsFatal);
            w.Field("uptimeMs", UptimeMs);
        }
    }

    /// <summary>perf.sample — aggregate window stats, never per-frame detail.</summary>
    [TelemetryEvent("perf.sample", 1)]
    public struct PerfSampleEvent : ITelemetryEvent
    {
        public float FrameMsP50;
        public float FrameMsP95;
        public long AllocKb;
        public long WindowMs;

        public string EventType => "perf.sample";
        public string ContractId => "perf.sample";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("frameMsP50", FrameMsP50);
            w.Field("frameMsP95", FrameMsP95);
            w.Field("allocKb", AllocKb);
            w.Field("windowMs", WindowMs);
        }
    }

    /// <summary>net.transition — connectivity state change observed by the host.</summary>
    [TelemetryEvent("net.transition", 1)]
    public struct NetTransitionEvent : ITelemetryEvent
    {
        public string State;
        public long UptimeMs;

        public string EventType => "net.transition";
        public string ContractId => "net.transition";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("state", State);
            w.Field("uptimeMs", UptimeMs);
        }
    }

    /// <summary>session.started / session.ended — gameplay scope boundaries.</summary>
    [TelemetryEvent("session.started", 1)]
    public struct SessionStartedEvent : ITelemetryEvent
    {
        public string SessionKind;
        public string Mode;
        public string ScenarioId;

        public string EventType => "session.started";
        public string ContractId => "session.started";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("sessionKind", SessionKind);
            if (Mode != null) w.Field("mode", Mode);
            if (ScenarioId != null) w.Field("scenarioId", ScenarioId);
        }
    }

    [TelemetryEvent("session.ended", 1)]
    public struct SessionEndedEvent : ITelemetryEvent
    {
        public string SessionKind;
        public string Outcome;
        public long DurationMs;

        public string EventType => "session.ended";
        public string ContractId => "session.ended";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("sessionKind", SessionKind);
            w.Field("outcome", Outcome);
            w.Field("durationMs", DurationMs);
        }
    }

    /// <summary>telemetry.drop — self-describing drop accounting emitted on pressure/consent drops.</summary>
    [TelemetryEvent("telemetry.drop", 1)]
    public struct TelemetryDropEvent : ITelemetryEvent
    {
        public string Reason;
        public long Count;
        public string Scope;

        public string EventType => "telemetry.drop";
        public string ContractId => "telemetry.drop";
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("reason", Reason);
            w.Field("count", Count);
            if (Scope != null) w.Field("scope", Scope);
        }
    }
}
