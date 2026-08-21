using System;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsSignalObserver : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IBotDiagnosticsLogger _log;

        public BotDiagnosticsSignalObserver(
            SignalBus signalBus,
            IBotDiagnosticsLogger log)
        {
            _signalBus = signalBus;
            _log = log;
        }

        public void Initialize()
        {
            if (_signalBus == null)
            {
                _log.Error(
                    BotDiagnosticCategory.Bootstrap,
                    "SIGNAL.BUS_MISSING",
                    "SignalBus is unavailable; startup/spawn diagnostics cannot subscribe.");
                return;
            }

            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);

            _log.Info(
                BotDiagnosticCategory.Bootstrap,
                "SIGNAL.SUBSCRIBED",
                "Bot diagnostics subscribed to world generation and spawn assignment signals.");
        }

        public void Dispose()
        {
            _signalBus?.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus?.TryUnsubscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            _log.Info(
                BotDiagnosticCategory.Bootstrap,
                "WORLD.GENERATED",
                "World generation data became available.",
                details:
                    $"size={signal.Width}x{signal.Height}; " +
                    $"startupSequence={signal.StartupSequence}; " +
                    $"session='{signal.StartupSessionId}'; source={signal.Source}; " +
                    $"tileMap={signal.TileMap != null}; heightMap={signal.HeightMap != null}; terrainLevelMap={signal.TerrainLevelMap != null}");
        }

        private void OnSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            int count = signal.Assignments?.Length ?? 0;
            int botCount = 0;
            int humanCount = 0;

            var parts = new string[count];
            for (int i = 0; i < count; i++)
            {
                var assignment = signal.Assignments[i];
                if (assignment.IsBot)
                    botCount++;
                else
                    humanCount++;

                parts[i] =
                    $"slot={assignment.SlotIndex},id='{assignment.ParticipantId}',bot={assignment.IsBot},pos={assignment.Position}";
            }

            BotDiagnosticLevel level =
                botCount > 0 ? BotDiagnosticLevel.Info : BotDiagnosticLevel.Error;

            Write(
                level,
                BotDiagnosticCategory.Spawn,
                botCount > 0
                    ? "SPAWN.ASSIGNMENTS_WITH_BOT"
                    : "SPAWN.ASSIGNMENTS_NO_BOT",
                "Starting position assignments published.",
                botCount > 0
                    ? "At least one bot slot exists."
                    : "No assignment is marked IsBot=true; TurnService cannot register an AI faction from these assignments.",
                $"source={signal.Source}; startupSequence={signal.StartupSequence}; " +
                $"count={count}; humans={humanCount}; bots={botCount}; assignments=[{string.Join(" | ", parts)}]");
        }

        private void Write(
            BotDiagnosticLevel level,
            BotDiagnosticCategory category,
            string code,
            string message,
            string reason,
            string details)
        {
            switch (level)
            {
                case BotDiagnosticLevel.Error:
                case BotDiagnosticLevel.Critical:
                    _log.Error(category, code, message, reason, details);
                    break;
                case BotDiagnosticLevel.Warning:
                    _log.Warning(category, code, message, reason, details);
                    break;
                default:
                    _log.Info(category, code, message, reason, details);
                    break;
            }
        }
    }
}
