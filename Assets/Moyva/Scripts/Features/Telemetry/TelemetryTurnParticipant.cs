using Kruty1918.Moyva.Turns.API;
using Kruty1918.Telemetry.Core;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>
    /// Observes turn lifecycle through ITurnParticipant — the canonical turn boundary.
    /// Runs last (high TurnOrder) so it records the fully-committed state.
    /// </summary>
    public sealed class TelemetryTurnParticipant : ITurnParticipant
    {
        private readonly ITelemetrySink _sink;

        public TelemetryTurnParticipant(ITelemetrySink sink) => _sink = sink;

        public int TurnOrder => 10_000;

        public void OnTurnStarted(TurnContext context) => Track("started", context);
        public void OnTurnEnding(TurnContext context) => Track("ending", context);

        public void OnRoundCompleted(int completedRound)
            => _sink.Track(new MoyvaTurnLifecycleEvent { Phase = "roundCompleted", Round = completedRound });

        private void Track(string phase, TurnContext c)
            => _sink.Track(new MoyvaTurnLifecycleEvent
            {
                Phase = phase,
                Round = c.Round,
                GlobalTurn = c.GlobalTurn,
                FactionIndex = c.FactionIndex,
                OwnerId = c.Faction.OwnerId,
            });
    }
}
