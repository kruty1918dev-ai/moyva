using System;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Telemetry.Core;
using Zenject;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>
    /// Bridges BotTelemetryHub recordings into the telemetry pipeline.
    /// Reuses the existing hub as single authority — never duplicates its buffers.
    /// Hub is optional: absent when the bot runtime is not installed.
    /// </summary>
    public sealed class MoyvaBotTelemetryAdapter : IInitializable, IDisposable
    {
        private readonly ITelemetrySink _sink;
        private readonly BotTelemetryHub _bound;
        private BotTelemetryHub _hub;

        public MoyvaBotTelemetryAdapter(ITelemetrySink sink,
            [InjectOptional] BotTelemetryHub bound = null)
        {
            _sink = sink;
            _bound = bound;
        }

        public void Initialize() => Attach(_bound);

        /// <summary>Attach a hub; also usable standalone for ad-hoc training hubs.</summary>
        public void Attach(BotTelemetryHub hub)
        {
            Detach();
            _hub = hub;
            if (_hub == null) return;
            _hub.TraceRecorded += OnTrace;
            _hub.Metrics.EpisodeRecorded += OnEpisode;
        }

        public void Detach()
        {
            if (_hub == null) return;
            _hub.TraceRecorded -= OnTrace;
            _hub.Metrics.EpisodeRecorded -= OnEpisode;
            _hub = null;
        }

        public void Dispose() => Detach();

        private void OnTrace(BotDecisionTrace trace)
            => _sink.Track(MoyvaBotDecisionEvent.FromTrace(trace));

        private void OnEpisode(BotEpisodeMetrics m)
            => _sink.Track(MoyvaBotEpisodeEvent.From(m));
    }
}
