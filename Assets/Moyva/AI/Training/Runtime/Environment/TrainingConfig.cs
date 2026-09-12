using System;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    public sealed class TrainingConfig
    {
        public int environmentCount = 1;
        public int baseSeed = 1918;
        public bool deterministicMode = true;
        public bool autoReset = true;
        public int maxTurnsPerEpisode = 200;
        public int maxDecisionsPerEpisode = 1000;
        public int decisionInterval = 5;
        public BehaviorType behaviorType = BehaviorType.Default;
        public TrainingCurriculumConfig curriculum = new TrainingCurriculumConfig();
        public TrainingRewardConfig rewards = new TrainingRewardConfig();
        public bool enableStats = true;
        public bool verboseLogging = false;
        public float trainingTimeScale = 1;
        public bool disableRenderingWhenPossible = false;

        public static TrainingConfig Load(TextAsset json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var result = JsonUtility.FromJson<TrainingConfig>(json.text);
            result.Validate();
            return result;
        }

        public TrainingConfig Snapshot()
        {
            var copy = JsonUtility.FromJson<TrainingConfig>(JsonUtility.ToJson(this));
            copy.Validate();
            return copy;
        }

        public void Validate()
        {
            if (environmentCount < 1 || maxTurnsPerEpisode < 1 || maxDecisionsPerEpisode < 1
                || decisionInterval < 1 || !Finite(trainingTimeScale) || trainingTimeScale <= 0
                || curriculum == null || rewards == null
                || !Enum.IsDefined(typeof(TrainingCurriculumStage), curriculum.stage)
                || !Enum.IsDefined(typeof(BehaviorType), behaviorType))
                throw new ArgumentException("Invalid training configuration.");
            if (behaviorType == BehaviorType.InferenceOnly)
                throw new ArgumentException("InferenceOnly requires a future model adapter; use Default or HeuristicOnly.");
            rewards.Validate();
        }

        internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
