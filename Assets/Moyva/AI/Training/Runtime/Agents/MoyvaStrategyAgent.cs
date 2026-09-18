using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    [RequireComponent(typeof(BehaviorParameters))]
    public sealed class MoyvaStrategyAgent : Agent
    {
        public const string BehaviorName = "MoyvaStrategy";
        private TrainingEnvironment _environment;
        private TrainingConfig _config;
        private int _ticks;
        private bool _pendingDecision;

        public void Configure(TrainingEnvironment environment, TrainingConfig config)
        {
            _environment = environment;
            _config = config.Snapshot();
            MaxStep = 0;
            _environment.Rewards.RewardApplied += ApplyReward;
            _environment.EpisodeEnded += OnEnvironmentEnded;
        }

        public override void OnEpisodeBegin()
        {
            _ticks = 0;
            _pendingDecision = false;
            if (_environment == null) return;
            // Manager prepares the initial episode before enabling this Agent.
            if (_environment.IsReady) return;
            if (_environment.EpisodeId == 0 || (_config.autoReset
                && _environment.Result != TrainingEpisodeResult.InvalidState))
                _environment.BeginEpisode();
        }

        private void FixedUpdate()
        {
            if (_environment == null) return;
            _environment.Tick(Time.fixedDeltaTime);
            // Single-candidate frames are engine-forced inside Tick and must never
            // produce an ML-Agents step; only >=2 real candidates is a decision.
            if (!_environment.HasTrainableDecision || _pendingDecision) return;
            if (_config.inspectorMode
                && !TrainingModelInspectorController.ShouldRequestModelDecision(_environment)) return;
            if (++_ticks < _config.decisionInterval) return;
            _ticks = 0;
            _pendingDecision = true;
            if (_config.inspectorMode)
                TrainingModelInspectorController.NotifyDecisionRequested(_environment);
            RequestDecision();
        }

        public override void CollectObservations(VectorSensor sensor) => _environment.Observations.Collect(sensor);
        public override void WriteDiscreteActionMask(IDiscreteActionMask mask) => _environment.Actions.WriteMask(mask);

        public override void OnActionReceived(ActionBuffers actions)
        {
            _pendingDecision = false;
            int slot = actions.DiscreteActions[0];
            if (_config.inspectorMode)
                TrainingModelInspectorController.NotifyModelAction(_environment, slot);
            if (_environment.IsReady) _environment.Step(slot);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var actions = actionsOut.DiscreteActions;
            actions[0] = _environment.Actions.FirstLegalAction();
        }

        private void ApplyReward(float value) => AddReward(value);

        private void OnEnvironmentEnded(TrainingEpisodeResult result)
        {
            var d = _environment.Diagnostics;
            if (_config.enableStats)
            {
                var stats = Academy.Instance.StatsRecorder;
                stats.Add("MoyvaAI/Reward", d.TotalReward);
                stats.Add("MoyvaAI/Shaping", d.ShapingReward);
                stats.Add("MoyvaAI/EpisodeLength", d.Decisions);
                stats.Add("MoyvaAI/Turns", d.Turns);
                stats.Add("MoyvaAI/InvalidRate", d.InvalidActions / (float)System.Math.Max(1, d.Decisions));
                stats.Add("MoyvaAI/StaleRate", d.StaleActions / (float)System.Math.Max(1, d.Decisions));
                stats.Add("MoyvaAI/WinRate", result == TrainingEpisodeResult.Victory ? 1 : 0);
                stats.Add("MoyvaAI/TimeoutRate", result == TrainingEpisodeResult.Timeout ? 1 : 0);
                // Candidate metrics aggregate real candidates over trainable
                // decisions; the final frame alone would misreport the episode.
                stats.Add("MoyvaAI/MeanCandidates", d.MeanCandidates);
                stats.Add("MoyvaAI/TrainableDecisions", d.Decisions);
                stats.Add("MoyvaAI/ForcedActions", d.ForcedActions);
                stats.Add("MoyvaAI/ForcedActionRate", d.ForcedActionRate);
                int finalCandidates = _environment.Bridge.Frame?.RealCandidateCount ?? 0;
                stats.Add("MoyvaAI/FinalFrameCandidates", finalCandidates);
                stats.Add("MoyvaAI/FinalFrameMaskedRatio",
                    1 - finalCandidates / (float)BotDecisionContract.MaxCandidateSlots);
            }
            if ((_config.verboseLogging && _config.presentationMode != TrainingPresentationMode.HeadlessFast)
                || result == TrainingEpisodeResult.InvalidState)
                Debug.Log($"Training environment {_environment.EnvironmentId}, episode {d.EpisodeNumber}: "
                    + $"{result}, decisions={d.Decisions}, turns={d.Turns}, reward={d.TotalReward}. {d.LastError}", this);
            if (result == TrainingEpisodeResult.InvalidState) EpisodeInterrupted();
            else EndEpisode();
        }

        private void OnDestroy()
        {
            if (_environment == null) return;
            _environment.Rewards.RewardApplied -= ApplyReward;
            _environment.EpisodeEnded -= OnEnvironmentEnded;
        }
    }
}
