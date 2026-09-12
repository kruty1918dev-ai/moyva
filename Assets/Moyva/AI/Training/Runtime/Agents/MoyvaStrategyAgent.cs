using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

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
            if (_environment.IsReady) _environment.ResetEnvironment();
            if (_environment.EpisodeId == 0 || (_config.autoReset
                && _environment.Result != TrainingEpisodeResult.InvalidState))
                _environment.BeginEpisode();
        }

        private void FixedUpdate()
        {
            if (_environment == null || !_environment.IsReady || _pendingDecision) return;
            if (++_ticks < _config.decisionInterval) return;
            _ticks = 0;
            _pendingDecision = true;
            RequestDecision();
        }

        public override void CollectObservations(VectorSensor sensor) => _environment.Observations.Collect(sensor);
        public override void WriteDiscreteActionMask(IDiscreteActionMask mask) => _environment.Actions.WriteMask(mask);

        public override void OnActionReceived(ActionBuffers actions)
        {
            _pendingDecision = false;
            if (_environment.IsReady) _environment.Step(actions.DiscreteActions[0]);
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
                stats.Add("Moyva/Reward", d.TotalReward);
                stats.Add("Moyva/Shaping", d.ShapingReward);
                stats.Add("Moyva/Decisions", d.Decisions);
                stats.Add("Moyva/Turns", d.Turns);
                stats.Add("Moyva/InvalidActions", d.InvalidActions);
            }
            if (_config.verboseLogging || result == TrainingEpisodeResult.InvalidState)
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
