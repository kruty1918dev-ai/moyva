using System;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    public sealed class TrainingConfig
    {
        public int environmentCount = 1;
        public bool learnInitialCastle = false;
        public int baseSeed = 1918;
        public int worldSize = 24;
        public string generatorGraphId = "testgeneratorgraph";
        public string startingUnitTypeId = "warrior";
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
        public bool allowScaffoldSimulation = false;
        public TrainingPresentationMode presentationMode = TrainingPresentationMode.Visual;
        public bool autoHeadlessInBatchMode = true;
        public float visualTimeScale = 1;
        public float headlessTimeScale = 10;
        public bool enableSceneOverlay = true;
        public bool enableEditorTelemetry = true;
        public bool enableAudioInVisualMode = false;
        public int metricsHistoryCapacity = 500;
        public bool enableDecisionJournal = true;
        public string decisionJournalPath = "";
        public int decisionJournalCapacity = 4096;
        // Retained for compatibility with existing serialized configurations.
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
            if (curriculum == null || rewards == null)
                throw new ArgumentException("Invalid training configuration: curriculum/rewards are required.");
            if (learnInitialCastle && (int)curriculum.stage < (int)TrainingCurriculumStage.Building)
                throw new ArgumentException("Initial castle lesson requires Building or later curriculum.");
            if (environmentCount < 1 || maxTurnsPerEpisode < 1 || maxDecisionsPerEpisode < 1
                || worldSize < 12 || worldSize > 128 || string.IsNullOrWhiteSpace(generatorGraphId)
                || string.IsNullOrWhiteSpace(startingUnitTypeId)
                || decisionInterval < 1 || !Finite(trainingTimeScale) || trainingTimeScale <= 0
                || !Finite(visualTimeScale) || !Finite(headlessTimeScale)
                || visualTimeScale <= 0 || headlessTimeScale <= 0
                || metricsHistoryCapacity < 1 || metricsHistoryCapacity > 5000
                || decisionJournalCapacity < 64 || decisionJournalCapacity > 100000
                || !Enum.IsDefined(typeof(TrainingPresentationMode), presentationMode)
                || !Enum.IsDefined(typeof(TrainingCurriculumStage), curriculum.stage)
                || !Enum.IsDefined(typeof(BehaviorType), behaviorType))
                throw new ArgumentException("Invalid training configuration.");
            if (behaviorType == BehaviorType.InferenceOnly)
                throw new ArgumentException("InferenceOnly requires a future model adapter; use Default or HeuristicOnly.");
            curriculum.autonomous?.Validate();
            rewards.Validate();
            visualTimeScale = Mathf.Clamp(visualTimeScale, 0.1f, 20f);
            headlessTimeScale = Mathf.Clamp(headlessTimeScale, 0.1f, 20f);
        }

        internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
