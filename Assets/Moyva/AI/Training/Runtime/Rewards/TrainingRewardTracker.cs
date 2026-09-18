using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingRewardTracker
    {
        private readonly ITrainingRewardPolicy _policy;
        private readonly float _cap;
        private readonly string _playerId;
        private readonly HashSet<string> _events = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _subjects = new HashSet<string>(StringComparer.Ordinal);
        private long _episodeId;
        private bool _ended;
        public event Action<float> RewardApplied;
        public event Action<TrainingEpisodeResult> EpisodeCompleted;
        public float TotalReward { get; private set; }
        public float ShapingReward { get; private set; }
        public float AbsoluteShaping { get; private set; }

        public TrainingRewardTracker(TrainingRewardConfig config, string playerId)
        {
            _policy = new TrainingRewardPolicy(config);
            _cap = config.shapingCap;
            _playerId = playerId;
        }

        public void Reset(long episodeId)
        {
            _episodeId = episodeId;
            _ended = false;
            _events.Clear();
            _subjects.Clear();
            TotalReward = ShapingReward = AbsoluteShaping = 0;
        }

        public float Record(TrainingRewardEvent e)
        {
            if (_ended || e.EpisodeId != _episodeId || string.IsNullOrEmpty(e.EventId)) return 0;
            if (!e.Validated && e.Type != TrainingRewardEventType.InvalidAction) return 0;
            if (!_events.Add(e.EventId)) return 0;
            TrainingEpisodeResult terminal = e.Type switch
            {
                TrainingRewardEventType.EpisodeWon => TrainingEpisodeResult.Victory,
                TrainingRewardEventType.EpisodeLost => TrainingEpisodeResult.Defeat,
                TrainingRewardEventType.EpisodeDraw => TrainingEpisodeResult.Draw,
                TrainingRewardEventType.EpisodeTimeout => TrainingEpisodeResult.Timeout,
                _ => TrainingEpisodeResult.None
            };
            if (terminal != TrainingEpisodeResult.None) return Complete(terminal);
            float reward = _policy.Evaluate(e, _playerId);
            if (reward == 0) return 0;
            if (!string.IsNullOrEmpty(e.SubjectId)
                && !_subjects.Add(((int)e.Type).ToString() + ":" + e.SubjectId)) return 0;
            float amount = Math.Min(Math.Abs(reward), Math.Max(0, _cap - AbsoluteShaping));
            reward = Math.Sign(reward) * amount;
            AbsoluteShaping = Math.Min(_cap, AbsoluteShaping + amount);
            ShapingReward += reward;
            return Apply(reward);
        }

        public float Complete(TrainingEpisodeResult result)
        {
            if (_ended || result == TrainingEpisodeResult.None) return 0;
            _ended = true;
            if (result == TrainingEpisodeResult.InvalidState)
            {
                float correction = -TotalReward;
                ShapingReward = AbsoluteShaping = 0;
                Apply(correction);
                EpisodeCompleted?.Invoke(result);
                return correction;
            }
            float reward = Apply(_policy.Terminal(result));
            EpisodeCompleted?.Invoke(result);
            return reward;
        }

        private float Apply(float reward)
        {
            TotalReward += reward;
            if (reward != 0) RewardApplied?.Invoke(reward);
            return reward;
        }
    }
}
