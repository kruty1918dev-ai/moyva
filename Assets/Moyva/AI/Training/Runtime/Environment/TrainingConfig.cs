using System;
using System.Globalization;
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
        public bool observerEnabled = false;
        public string observerEndpoint = "moyva-training-observer";
        public int observerQueueCapacity = 256;
        public int observerMaxMessageBytes = 1024 * 1024;
        public float observerSnapshotHz = 5f;
        public float trainingTimeScale = 1;
        public bool disableRenderingWhenPossible = false;

        // Runtime-only frozen evaluation contract. These values are injected from
        // the launcher environment and are never authored into the training preset.
        [NonSerialized] public bool evaluationMode;
        [NonSerialized] public string evaluationRunId;
        [NonSerialized] public string evaluationScenarioId;
        [NonSerialized] public string evaluationCheckpoint;
        [NonSerialized] public long evaluationCheckpointStep;
        [NonSerialized] public string evaluationContractHash;
        [NonSerialized] public int evaluationGeneration;
        [NonSerialized] public int evaluationEpisodes;
        [NonSerialized] public int evaluationSeedBase;
        [NonSerialized] public string evaluationSeedSetVersion;
        [NonSerialized] public string evaluationProgressPath;

        public static TrainingConfig Load(TextAsset json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var result = JsonUtility.FromJson<TrainingConfig>(json.text);
            if (result.curriculum == null) result.curriculum = new TrainingCurriculumConfig();
            if (result.curriculum.autonomous == null) result.curriculum.autonomous = new AutonomousTrainingConfig();
            result.ApplyRuntimeEnvironment();
            result.Validate();
            return result;
        }

        public TrainingConfig Snapshot()
        {
            var copy = JsonUtility.FromJson<TrainingConfig>(JsonUtility.ToJson(this));
            copy.evaluationMode = evaluationMode;
            copy.evaluationRunId = evaluationRunId;
            copy.evaluationScenarioId = evaluationScenarioId;
            copy.evaluationCheckpoint = evaluationCheckpoint;
            copy.evaluationCheckpointStep = evaluationCheckpointStep;
            copy.evaluationContractHash = evaluationContractHash;
            copy.evaluationGeneration = evaluationGeneration;
            copy.evaluationEpisodes = evaluationEpisodes;
            copy.evaluationSeedBase = evaluationSeedBase;
            copy.evaluationSeedSetVersion = evaluationSeedSetVersion;
            copy.evaluationProgressPath = evaluationProgressPath;
            copy.Validate();
            return copy;
        }

        private void ApplyRuntimeEnvironment()
        {
            string statePath = Environment.GetEnvironmentVariable("MOYVA_CURRICULUM_STATE_PATH");
            if (!string.IsNullOrWhiteSpace(statePath) && curriculum?.autonomous != null)
                curriculum.autonomous.statePath = statePath;
            if (Environment.GetEnvironmentVariable("MOYVA_AUTONOMOUS_TRAINING") == "1" && curriculum?.autonomous != null)
                curriculum.autonomous.enabled = true;

            if (Environment.GetEnvironmentVariable("MOYVA_EVALUATION") != "1") return;
            evaluationMode = true;
            evaluationRunId = RequiredEnvironment("MOYVA_EVAL_RUN_ID");
            evaluationScenarioId = RequiredEnvironment("MOYVA_EVAL_SCENARIO");
            evaluationCheckpoint = RequiredEnvironment("MOYVA_EVAL_CHECKPOINT");
            evaluationContractHash = RequiredEnvironment("MOYVA_EVAL_CONTRACT_HASH");
            evaluationSeedSetVersion = RequiredEnvironment("MOYVA_EVAL_SEED_SET_VERSION");
            evaluationProgressPath = RequiredEnvironment("MOYVA_EVAL_PROGRESS_PATH");
            evaluationCheckpointStep = ParseLongEnvironment("MOYVA_EVAL_CHECKPOINT_STEP");
            evaluationGeneration = ParseIntEnvironment("MOYVA_EVAL_GENERATION");
            evaluationEpisodes = ParseIntEnvironment("MOYVA_EVAL_EPISODES");
            evaluationSeedBase = ParseIntEnvironment("MOYVA_EVAL_SEED_BASE");

            // Evaluation is a standalone inference process. It never connects to a
            // trainer and its episode/decision accounting is isolated from training.
            behaviorType = BehaviorType.HeuristicOnly;
            deterministicMode = true;
            environmentCount = 1;
            baseSeed = evaluationSeedBase;
            autoReset = true;
            enableStats = false;
            enableDecisionJournal = false;
            observerEnabled = false;
            enableEditorTelemetry = false;
            presentationMode = TrainingPresentationMode.HeadlessFast;
            if (curriculum?.autonomous == null)
                throw new ArgumentException("Frozen evaluation requires autonomous curriculum configuration.");
            curriculum.autonomous.enabled = true;
            curriculum.autonomous.evaluationEpisodes = evaluationEpisodes;
        }

        private static string RequiredEnvironment(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Missing frozen evaluation environment: " + name);
            return value;
        }

        private static int ParseIntEnvironment(string name)
            => int.Parse(RequiredEnvironment(name), NumberStyles.Integer, CultureInfo.InvariantCulture);
        private static long ParseLongEnvironment(string name)
            => long.Parse(RequiredEnvironment(name), NumberStyles.Integer, CultureInfo.InvariantCulture);

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
                || observerQueueCapacity < 8 || observerQueueCapacity > 65536
                || observerMaxMessageBytes < TrainingObserverProtocol.MinimumMessageBytes || observerMaxMessageBytes > 16 * 1024 * 1024
                || !Finite(observerSnapshotHz) || observerSnapshotHz < 0.1f || observerSnapshotHz > 60f
                || (observerEnabled && string.IsNullOrWhiteSpace(observerEndpoint))
                || !Enum.IsDefined(typeof(TrainingPresentationMode), presentationMode)
                || !Enum.IsDefined(typeof(TrainingCurriculumStage), curriculum.stage)
                || !Enum.IsDefined(typeof(BehaviorType), behaviorType))
                throw new ArgumentException("Invalid training configuration.");
            if (behaviorType == BehaviorType.InferenceOnly && !evaluationMode)
                throw new ArgumentException("InferenceOnly is reserved for frozen evaluation.");
            if (evaluationMode && behaviorType != BehaviorType.HeuristicOnly)
                throw new ArgumentException("Frozen evaluation must bootstrap HeuristicOnly before first-step model binding.");
            if (evaluationMode && (evaluationEpisodes < 1 || evaluationGeneration < 1 || evaluationCheckpointStep < 0
                || string.IsNullOrWhiteSpace(evaluationRunId) || string.IsNullOrWhiteSpace(evaluationScenarioId)
                || string.IsNullOrWhiteSpace(evaluationCheckpoint) || string.IsNullOrWhiteSpace(evaluationContractHash)
                || string.IsNullOrWhiteSpace(evaluationSeedSetVersion) || string.IsNullOrWhiteSpace(evaluationProgressPath)))
                throw new ArgumentException("Frozen evaluation metadata is incomplete.");
            curriculum.autonomous?.Validate();
            rewards.Validate();
            visualTimeScale = Mathf.Clamp(visualTimeScale, 0.1f, 20f);
            headlessTimeScale = Mathf.Clamp(headlessTimeScale, 0.1f, 20f);
        }

        internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
