using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public static class TrainingReadinessValidator
    {
        public static TrainingReadinessReport Validate(ITrainingSimulationFactory factory,
            ITrainingSimulation simulation, TrainingEnvironment environment)
        {
            var report = new TrainingReadinessReport();
            if (!(factory is GameplayTrainingSimulationFactory)) report.Block("REAL_FACTORY_BLOCKED: SCAFFOLD / NOT REAL GAMEPLAY.");
            if (!(simulation is GameplayTrainingSimulation)) report.Block("REAL_SIMULATION_BLOCKED: no owned gameplay simulation.");
            if (string.IsNullOrWhiteSpace(simulation?.PlayerId)) report.Block("PLAYER_ID_BLOCKED");
            if (simulation?.Turns == null) report.Block("TURN_SERVICE_BLOCKED");
            var source = simulation as ITrainingBotRuntimeSource;
            if (source?.Perception == null || source.Perception is EmptyBotPerceptionSource) report.Block("REAL_PERCEPTION_BLOCKED");
            if (source?.TurnGateway == null || source.Capabilities?.Get(BotCapabilityId.Turn) == null)
                report.Block("ENDTURN_CAPABILITY_BLOCKED");
            var movement = source?.Capabilities?.Get(BotCapabilityId.Movement);
            if (movement == null) report.Block("MOVEMENT_STATUS_UNKNOWN");
            else if (movement.UnavailableReason(simulation.PlayerId) is string reason)
                report.Warn("Movement unavailable: " + reason);
            if (simulation?.IsReady != true || environment?.IsReady != true)
                report.Block("GAMEPLAY_RESET_BLOCKED: reset/episode begin has not succeeded.");
            if (environment?.Result == TrainingEpisodeResult.InvalidState && !string.IsNullOrEmpty(environment.Diagnostics.LastError))
                report.Block(environment.Diagnostics.LastError);
            if (environment?.Rewards == null) report.Block("REWARD_TRACKER_BLOCKED");
            if (environment?.Bridge.Telemetry.NonFiniteValues > 0) report.Block("NONFINITE_SOURCE_OBSERVATIONS");
            var outcomes = (simulation as GameplayTrainingSimulation)?.Outcomes;
            if (outcomes == null || !outcomes.IsConnected) report.Block("TERMINAL_OUTCOME_BLOCKED");
            var frame = environment?.Bridge.Frame;
            if (frame == null) report.Block("DECISION_FRAME_BLOCKED: episode cannot request a policy decision.");
            else
            {
                if (frame.ContractHash != BotDecisionContract.Hash || frame.Observations.Count != BotDecisionContract.ObservationCount)
                    report.Block("SHARED_BOT_CONTRACT_BLOCKED");
                foreach (float value in frame.Observations)
                    if (float.IsNaN(value) || float.IsInfinity(value)) { report.Block("NONFINITE_OBSERVATIONS"); break; }
                bool legalGameplay = false;
                for (int slot = 0; slot < frame.Candidates.Count; slot++)
                    if (frame.Candidates.IsLegal(slot) && frame.Candidates[slot].Intent != BotIntentType.Wait) legalGameplay = true;
                if (!legalGameplay) report.Block("REAL GAMEPLAY CANDIDATES NOT AVAILABLE");
                if (frame.Candidates.Count <= 1) report.Warn("REAL GAMEPLAY CANDIDATES NOT AVAILABLE: only one candidate; verify curriculum and real movement state.");
                foreach (var unavailable in frame.Unavailable) report.Warn(unavailable.Key + ": " + unavailable.Value);
            }
            report.Warn("Gameplay reward sources: terminal result adapter only; unit/building/objective rewards are disconnected.");
            report.Warn("Visual/headless share the Bot path; runtime parity has not been verified.");
            return report;
        }
    }
}
