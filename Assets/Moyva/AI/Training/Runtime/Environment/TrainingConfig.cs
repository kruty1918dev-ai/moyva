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
        public bool randomizeWorldSize = true;
        public int minWorldSize = 20;
        public int maxWorldSize = 40;
        public string generatorGraphId = "testgeneratorgraph";
        public string spawnValidationUnitTypeId = "warrior";
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
        public float visualCameraZoomSensitivity = 1;
        public float visualCameraPanSpeed = 1;
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
        // Fraction of training episodes where training-only opaque scenario hints
        // (ScenarioGoal/Step/Progress) are zeroed so the policy cannot key on the
        // scenario id hash. Frozen evaluation always suppresses them instead.
        public float scenarioHintDropout = 0.30f;
        public bool watchdogEnabled = true;
        // Submissions per watchdog evaluation window (forced + trainable).
        public int watchdogWindow = 200;
        // Global ceiling; a scenario may tighten it via maxForcedActionRatio.
        public float watchdogMaxForcedRatio = 0.95f;
        // Trainable+forced submissions without any scenario progress before abort.
        public int watchdogMaxStagnantSubmissions = 400;

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

        // Runtime-only frozen Model Inspector contract.
        [NonSerialized] public bool inspectorMode;
        [NonSerialized] public string inspectorRunId;
        [NonSerialized] public string inspectorScenarioId;
        [NonSerialized] public string inspectorCheckpoint;
        [NonSerialized] public long inspectorCheckpointStep;
        [NonSerialized] public string inspectorContractHash;
        [NonSerialized] public string inspectorModelSha256;
        [NonSerialized] public int inspectorSeed;
        [NonSerialized] public bool inspectorAutoPlay;
        [NonSerialized] public string inspectorSessionPath;
        [NonSerialized] public string inspectorJournalPath;
        [NonSerialized] public string inspectorStatePath;

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
            copy.inspectorMode = inspectorMode;
            copy.inspectorRunId = inspectorRunId;
            copy.inspectorScenarioId = inspectorScenarioId;
            copy.inspectorCheckpoint = inspectorCheckpoint;
            copy.inspectorCheckpointStep = inspectorCheckpointStep;
            copy.inspectorContractHash = inspectorContractHash;
            copy.inspectorModelSha256 = inspectorModelSha256;
            copy.inspectorSeed = inspectorSeed;
            copy.inspectorAutoPlay = inspectorAutoPlay;
            copy.inspectorSessionPath = inspectorSessionPath;
            copy.inspectorJournalPath = inspectorJournalPath;
            copy.inspectorStatePath = inspectorStatePath;
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

            string runtimeJournalPath = Environment.GetEnvironmentVariable("MOYVA_DECISION_JOURNAL_PATH");
            if (!string.IsNullOrWhiteSpace(runtimeJournalPath))
            {
                enableDecisionJournal = true;
                decisionJournalPath = runtimeJournalPath;
            }

            if (Environment.GetEnvironmentVariable("MOYVA_MODEL_INSPECTOR") == "1")
            {
                inspectorMode = true;
                inspectorRunId = RequiredEnvironment("MOYVA_INSPECT_RUN_ID");
                inspectorScenarioId = RequiredEnvironment("MOYVA_INSPECT_SCENARIO");
                inspectorCheckpoint = RequiredEnvironment("MOYVA_INSPECT_CHECKPOINT");
                inspectorContractHash = RequiredEnvironment("MOYVA_INSPECT_CONTRACT_HASH");
                inspectorModelSha256 = RequiredEnvironment("MOYVA_INSPECT_MODEL_SHA256");
                inspectorSessionPath = RequiredEnvironment("MOYVA_INSPECT_SESSION_PATH");
                inspectorJournalPath = RequiredEnvironment("MOYVA_INSPECT_JOURNAL_PATH");
                inspectorStatePath = RequiredEnvironment("MOYVA_INSPECT_STATE_PATH");
                inspectorCheckpointStep = ParseLongEnvironment("MOYVA_INSPECT_CHECKPOINT_STEP");
                inspectorSeed = ParseIntEnvironment("MOYVA_INSPECT_SEED");
                inspectorAutoPlay = Environment.GetEnvironmentVariable("MOYVA_INSPECT_AUTOPLAY") == "1";

                // Bootstrap HeuristicOnly; TrainingAutonomyCoordinator swaps the
                // embedded frozen ONNX to InferenceOnly before the first decision.
                behaviorType = BehaviorType.HeuristicOnly;
                deterministicMode = true;
                environmentCount = 1;
                baseSeed = inspectorSeed;
                autoReset = false;
                enableStats = false;
                enableDecisionJournal = false;
                observerEnabled = false;
                enableEditorTelemetry = false;
                presentationMode = TrainingPresentationMode.Visual;
                enableSceneOverlay = false;
                if (curriculum?.autonomous == null)
                    throw new ArgumentException("Model Inspector requires autonomous curriculum configuration.");
                curriculum.autonomous.enabled = true;
                curriculum.autonomous.statePath = inspectorStatePath;
                return;
            }

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
                || minWorldSize < 12 || maxWorldSize > 128 || minWorldSize > maxWorldSize
                || string.IsNullOrWhiteSpace(spawnValidationUnitTypeId)
                || decisionInterval < 1 || !Finite(trainingTimeScale) || trainingTimeScale <= 0
                || !Finite(visualTimeScale) || !Finite(headlessTimeScale)
                || !Finite(visualCameraZoomSensitivity) || !Finite(visualCameraPanSpeed)
                || visualTimeScale <= 0 || headlessTimeScale <= 0
                || visualCameraZoomSensitivity <= 0 || visualCameraPanSpeed <= 0
                || metricsHistoryCapacity < 1 || metricsHistoryCapacity > 5000
                || decisionJournalCapacity < 64 || decisionJournalCapacity > 100000
                || observerQueueCapacity < 8 || observerQueueCapacity > 65536
                || observerMaxMessageBytes < TrainingObserverProtocol.MinimumMessageBytes || observerMaxMessageBytes > 16 * 1024 * 1024
                || !Finite(observerSnapshotHz) || observerSnapshotHz < 0.1f || observerSnapshotHz > 60f
                || (observerEnabled && string.IsNullOrWhiteSpace(observerEndpoint))
                || !Finite(scenarioHintDropout) || scenarioHintDropout < 0f || scenarioHintDropout > 1f
                || watchdogWindow < 1 || watchdogMaxStagnantSubmissions < 1
                || !Finite(watchdogMaxForcedRatio) || watchdogMaxForcedRatio < 0f || watchdogMaxForcedRatio > 1f
                || !Enum.IsDefined(typeof(TrainingPresentationMode), presentationMode)
                || !Enum.IsDefined(typeof(TrainingCurriculumStage), curriculum.stage)
                || !Enum.IsDefined(typeof(BehaviorType), behaviorType))
                throw new ArgumentException("Invalid training configuration.");
            if (behaviorType == BehaviorType.InferenceOnly && !evaluationMode && !inspectorMode)
                throw new ArgumentException("InferenceOnly is reserved for frozen evaluation/model inspection.");
            if ((evaluationMode || inspectorMode) && behaviorType != BehaviorType.HeuristicOnly)
                throw new ArgumentException("Frozen inference must bootstrap HeuristicOnly before first-step model binding.");
            if (inspectorMode && (inspectorCheckpointStep < 0
                || string.IsNullOrWhiteSpace(inspectorRunId)
                || string.IsNullOrWhiteSpace(inspectorScenarioId)
                || string.IsNullOrWhiteSpace(inspectorCheckpoint)
                || string.IsNullOrWhiteSpace(inspectorContractHash)
                || string.IsNullOrWhiteSpace(inspectorModelSha256)
                || string.IsNullOrWhiteSpace(inspectorSessionPath)
                || string.IsNullOrWhiteSpace(inspectorJournalPath)
                || string.IsNullOrWhiteSpace(inspectorStatePath)))
                throw new ArgumentException("Model Inspector metadata is incomplete.");
            if (evaluationMode && (evaluationEpisodes < 1 || evaluationGeneration < 1 || evaluationCheckpointStep < 0
                || string.IsNullOrWhiteSpace(evaluationRunId) || string.IsNullOrWhiteSpace(evaluationScenarioId)
                || string.IsNullOrWhiteSpace(evaluationCheckpoint) || string.IsNullOrWhiteSpace(evaluationContractHash)
                || string.IsNullOrWhiteSpace(evaluationSeedSetVersion) || string.IsNullOrWhiteSpace(evaluationProgressPath)))
                throw new ArgumentException("Frozen evaluation metadata is incomplete.");
            curriculum.autonomous?.Validate();
            rewards.Validate();
            visualTimeScale = Mathf.Clamp(visualTimeScale, 0.1f, 20f);
            visualCameraZoomSensitivity = Mathf.Clamp(visualCameraZoomSensitivity, 0.1f, 5f);
            visualCameraPanSpeed = Mathf.Clamp(visualCameraPanSpeed, 0.1f, 5f);
            headlessTimeScale = Mathf.Clamp(headlessTimeScale, 0.1f, 20f);
        }

        internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
