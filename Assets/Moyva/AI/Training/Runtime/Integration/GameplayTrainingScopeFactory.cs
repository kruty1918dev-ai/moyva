using System;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class GameplayTrainingScopeFactory : ITrainingGameplayScopeFactory
    {
        private readonly TrainingConfig _config;
        public GameplayTrainingScopeFactory(TrainingConfig config) { _config = config.Snapshot(); }
        public TrainingGameplayScope Create(int environmentId)
        {
            if (environmentId != 0) throw new ArgumentOutOfRangeException(nameof(environmentId));
            return new TrainingGameplayScope(_config);
        }
    }
}
