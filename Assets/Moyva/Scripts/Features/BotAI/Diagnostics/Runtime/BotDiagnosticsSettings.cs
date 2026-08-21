using System;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsSettings
    {
        public bool Enabled { get; set; } = true;
        public bool IncludeTrace { get; set; } = true;
        public bool IncludeStackTraceForExceptions { get; set; } = true;
        public bool LogBotHeartbeat { get; set; } = true;
        public bool LogSnapshotHeartbeat { get; set; } = true;
        public bool LogReasoningFactors { get; set; } = true;

        public int BufferCapacity { get; set; } =
            BotDiagnosticsConstants.DefaultBufferCapacity;

        public double HeartbeatSeconds { get; set; } =
            BotDiagnosticsConstants.DefaultHeartbeatSeconds;

        public double HealthProbeSeconds { get; set; } =
            BotDiagnosticsConstants.DefaultHealthProbeSeconds;

        public double SnapshotProbeSeconds { get; set; } =
            BotDiagnosticsConstants.DefaultSnapshotProbeSeconds;

        public void Normalize()
        {
            BufferCapacity = Math.Max(128, BufferCapacity);
            HeartbeatSeconds = Math.Max(0.25, HeartbeatSeconds);
            HealthProbeSeconds = Math.Max(0.25, HealthProbeSeconds);
            SnapshotProbeSeconds = Math.Max(0.25, SnapshotProbeSeconds);
        }
    }
}
