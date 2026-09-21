using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
#endif

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingAutonomyCoordinator : IDisposable
    {
        [Serializable]
        private sealed class FrozenEvaluationProgress
        {
            public string state;
            public string run_id;
            public string source_checkpoint;
            public long checkpoint_step;
            public string contract_hash;
            public string scenario_id;
            public string seed_set_version;
            public int evaluation_generation;
            public int episode_count;
            public int completed_episodes;
            public int successes;
            public int failures;
            public float success_rate;
            public string started_utc;
            public string ended_utc;
            public string result;
            public bool mastery_changed;
            public string reason;
        }

        private readonly TrainingCurriculumController _controller;
        private readonly TrainingConfig _config;
        private readonly List<TrainingEnvironment> _environments = new List<TrainingEnvironment>();
        private readonly Dictionary<TrainingEnvironment, Action<TrainingEpisodeResult>> _handlers =
            new Dictionary<TrainingEnvironment, Action<TrainingEpisodeResult>>();
        private int _evaluationCompleted;
        private int _evaluationSuccesses;
        private readonly string _evaluationStartedUtc;
        private Action<int> _modelBindingHandler;
        private bool _frozenModelBound;
        public TrainingCurriculumController Controller => _controller;

        public TrainingAutonomyCoordinator(TrainingConfig config)
        {
            if (config == null || config.curriculum == null || config.curriculum.autonomous == null)
                throw new ArgumentNullException(nameof(config));
            _config = config.Snapshot();
            _controller = new TrainingCurriculumController(_config.curriculum.autonomous, _config.baseSeed);
            _evaluationStartedUtc = DateTime.UtcNow.ToString("O");
            if (_config.evaluationMode || _config.inspectorMode)
            {
                if (Academy.Instance.IsCommunicatorOn)
                    throw new InvalidOperationException("Frozen inference must not connect to an ML-Agents trainer.");
                _modelBindingHandler = _ => BindFrozenModelBeforeDecision();
                Academy.Instance.AgentPreStep += _modelBindingHandler;
                if (_config.evaluationMode)
                    WriteEvaluationProgress("EVALUATING", null, false, null);
            }
        }

        public void Attach(TrainingEnvironment environment)
        {
            if (environment == null || _environments.Contains(environment)) return;
            if ((_config.evaluationMode || _config.inspectorMode) && _environments.Count != 0)
                throw new InvalidOperationException("Frozen inference supports exactly one deterministic environment.");
            _environments.Add(environment);
            if (_config.evaluationMode || _config.inspectorMode)
            {
                string scenarioId = _config.evaluationMode
                    ? _config.evaluationScenarioId : _config.inspectorScenarioId;
                var scenario = _controller.GetScenario(scenarioId);
                if (scenario == null) throw new InvalidOperationException("Unknown frozen-inference scenario: " + scenarioId);
                environment.SetScenario(scenario);
            }
            else
            {
                environment.SetScenario(_controller.ChooseNext());
            }
            Action<TrainingEpisodeResult> handler = result => OnEpisodeEnded(environment, result);
            _handlers.Add(environment, handler);
            environment.EpisodeEnded += handler;
        }


        private void BindFrozenModelBeforeDecision()
        {
            if ((!_config.evaluationMode && !_config.inspectorMode) || _frozenModelBound) return;
            try
            {
                if (Academy.Instance.IsCommunicatorOn)
                    throw new InvalidOperationException("Frozen evaluation communicator became active.");
                var model = Resources.Load("MoyvaFrozenEvaluationModel");
                if (model == null)
                    throw new InvalidOperationException("Frozen evaluation model resource is missing from the player.");

                PropertyInfo modelProperty = typeof(BehaviorParameters).GetProperty("Model", BindingFlags.Instance | BindingFlags.Public);
                if (modelProperty == null)
                    throw new MissingMemberException("BehaviorParameters.Model is unavailable.");
                var behaviors = UnityEngine.Object.FindObjectsByType<BehaviorParameters>(FindObjectsInactive.Include);
                int bound = 0;
                foreach (var behavior in behaviors)
                {
                    if (behavior == null || behavior.BehaviorName != MoyvaStrategyAgent.BehaviorName) continue;
                    // Evaluation boots HeuristicOnly so an Agent can initialize without a
                    // model. AgentPreStep runs before state collection/decision, therefore
                    // no heuristic action can occur before this frozen model is installed.
                    modelProperty.SetValue(behavior, model);
                    behavior.BehaviorType = BehaviorType.InferenceOnly;
                    bound++;
                }
                if (bound != 1)
                    throw new InvalidOperationException("Frozen evaluation requires exactly one MoyvaStrategy BehaviorParameters; found " + bound + ".");
                _frozenModelBound = true;
                if (_modelBindingHandler != null) Academy.Instance.AgentPreStep -= _modelBindingHandler;
                _modelBindingHandler = null;
            }
            catch (Exception exception)
            {
                if (_modelBindingHandler != null) Academy.Instance.AgentPreStep -= _modelBindingHandler;
                _modelBindingHandler = null;
                WriteEvaluationProgress("INTERRUPTED", "INTERRUPTED", false, "FROZEN_EVALUATION_MODEL_BIND_FAILED: " + exception.Message);
                Debug.LogError("FROZEN_EVALUATION_MODEL_BIND_FAILED: " + exception);
                if (!Application.isEditor) Application.Quit(22);
            }
        }

        private void OnEpisodeEnded(TrainingEnvironment environment, TrainingEpisodeResult result)
        {
            if (_config.inspectorMode)
            {
                // Inspector episodes never count as training/evaluation and never
                // mutate mastery/curriculum state.
                return;
            }
            if (_config.evaluationMode)
            {
                OnEvaluationEpisodeEnded(environment, result);
                return;
            }

            bool success = result == TrainingEpisodeResult.ScenarioSuccess
                || (environment.Scenario != null && environment.Scenario.fullGame && result == TrainingEpisodeResult.Victory);
            if (result != TrainingEpisodeResult.InvalidState && environment.Scenario != null)
                _controller.RecordTrainingEpisode(environment.Scenario.id, success, environment.Diagnostics.Decisions);

            // EvaluationDue is consumed by the Python launcher at an exact trainer
            // checkpoint boundary. Training episodes never call RecordEvaluation.
            environment.SetScenario(_controller.ChooseNext());
        }

        private void OnEvaluationEpisodeEnded(TrainingEnvironment environment, TrainingEpisodeResult result)
        {
            if (result == TrainingEpisodeResult.InvalidState)
            {
                WriteEvaluationProgress("INTERRUPTED", "INTERRUPTED", false,
                    string.IsNullOrWhiteSpace(environment.Diagnostics.LastError) ? "Evaluation episode entered InvalidState." : environment.Diagnostics.LastError);
                if (!Application.isEditor) Application.Quit(20);
                return;
            }

            bool success = result == TrainingEpisodeResult.ScenarioSuccess
                || (environment.Scenario != null && environment.Scenario.fullGame && result == TrainingEpisodeResult.Victory);
            _evaluationCompleted++;
            if (success) _evaluationSuccesses++;

            if (_evaluationCompleted < _config.evaluationEpisodes)
            {
                WriteEvaluationProgress("EVALUATING", null, false, null);
                environment.SetScenario(_controller.GetScenario(_config.evaluationScenarioId));
                return;
            }

            if (_evaluationCompleted != _config.evaluationEpisodes)
            {
                WriteEvaluationProgress("INTERRUPTED", "INTERRUPTED", false, "Evaluation episode count exceeded its frozen contract.");
                if (!Application.isEditor) Application.Quit(21);
                return;
            }

            string evaluationId = _config.evaluationCheckpoint + "|" + _config.evaluationScenarioId + "|"
                + _config.evaluationGeneration.ToString(System.Globalization.CultureInfo.InvariantCulture);
            bool masteryChanged = _controller.RecordEvaluation(
                _config.evaluationScenarioId, _evaluationSuccesses, _evaluationCompleted,
                _config.evaluationCheckpointStep, _config.evaluationCheckpoint, evaluationId);
            float rate = _evaluationSuccesses / (float)_evaluationCompleted;
            string outcome = rate >= _config.curriculum.autonomous.masteryThreshold ? "PASSED" : "FAILED";
            WriteEvaluationProgress("COMPLETED", outcome, masteryChanged, null);
            if (!Application.isEditor) Application.Quit(0);
        }

        private void WriteEvaluationProgress(string state, string result, bool masteryChanged, string reason)
        {
            if (!_config.evaluationMode || string.IsNullOrWhiteSpace(_config.evaluationProgressPath)) return;
            var progress = new FrozenEvaluationProgress
            {
                state = state,
                run_id = _config.evaluationRunId,
                source_checkpoint = _config.evaluationCheckpoint,
                checkpoint_step = _config.evaluationCheckpointStep,
                contract_hash = _config.evaluationContractHash,
                scenario_id = _config.evaluationScenarioId,
                seed_set_version = _config.evaluationSeedSetVersion,
                evaluation_generation = _config.evaluationGeneration,
                episode_count = _config.evaluationEpisodes,
                completed_episodes = _evaluationCompleted,
                successes = _evaluationSuccesses,
                failures = Math.Max(0, _evaluationCompleted - _evaluationSuccesses),
                success_rate = _evaluationCompleted == _config.evaluationEpisodes
                    ? _evaluationSuccesses / (float)_evaluationCompleted : 0f,
                started_utc = _evaluationStartedUtc,
                ended_utc = state == "EVALUATING" ? null : DateTime.UtcNow.ToString("O"),
                result = result,
                mastery_changed = masteryChanged,
                reason = reason
            };
            try
            {
                string path = Path.GetFullPath(_config.evaluationProgressPath);
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(progress, true));
                if (File.Exists(path)) File.Delete(path);
                File.Move(temporary, path);
            }
            catch (Exception exception)
            {
                Debug.LogError("Cannot write frozen evaluation progress: " + exception.Message);
            }
        }

        public void Dispose()
        {
            if (_modelBindingHandler != null) Academy.Instance.AgentPreStep -= _modelBindingHandler;
            _modelBindingHandler = null;
            foreach (var pair in _handlers) pair.Key.EpisodeEnded -= pair.Value;
            _handlers.Clear();
            _environments.Clear();
            _controller.Dispose();
        }
    }

    // Build helper for an evaluation-only player. The frozen ONNX is copied into
    // a temporary Resources folder so runtime binding never reads mutable external weights.
    public static class TrainingEvaluationModelBinder
    {
#if UNITY_EDITOR
        public static void BuildFrozenEvaluationPlayer()
        {
            const string sourceScene = "Assets/Moyva/AI/Training/Scenes/MoyvaTraining.unity";
            const string temporaryRoot = "Assets/Moyva/AI/Training/FrozenEvaluationBuildTemp";
            const string temporaryResources = temporaryRoot + "/Resources";
            const string temporaryModel = temporaryResources + "/MoyvaFrozenEvaluationModel.onnx";
            string modelSource = Environment.GetEnvironmentVariable("MOYVA_EVAL_MODEL_SOURCE");
            string output = Environment.GetEnvironmentVariable("MOYVA_EVAL_BUILD_OUTPUT");
            string targetName = Environment.GetEnvironmentVariable("MOYVA_EVAL_BUILD_TARGET");
            if (string.IsNullOrWhiteSpace(modelSource) || !File.Exists(modelSource))
                throw new FileNotFoundException("MOYVA_EVAL_MODEL_SOURCE is missing.", modelSource);
            if (string.IsNullOrWhiteSpace(output)) throw new InvalidOperationException("MOYVA_EVAL_BUILD_OUTPUT is missing.");
            if (!Enum.TryParse(targetName, out BuildTarget target))
                throw new InvalidOperationException("Invalid MOYVA_EVAL_BUILD_TARGET: " + targetName);

            try
            {
                Directory.CreateDirectory(Path.GetFullPath(temporaryResources));
                File.Copy(modelSource, Path.GetFullPath(temporaryModel), true);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var model = AssetDatabase.LoadMainAssetAtPath(temporaryModel);
                if (model == null) throw new InvalidOperationException("Unity could not import the frozen ONNX as a model asset.");

                // Runtime self-play reads .sentis (ONNX conversion is editor-only);
                // write it next to the frozen checkpoint so verified checkpoints
                // become loadable opponents without an extra editor launch.
                try
                {
                    string sentisPath = Path.ChangeExtension(modelSource, ".sentis");
                    Unity.InferenceEngine.ModelWriter.Save(sentisPath, model as Unity.InferenceEngine.ModelAsset);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[MoyvaTraining] Sentis export for self-play pool failed: " + e.Message);
                }

                string directory = Path.GetDirectoryName(output);
                if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { sourceScene },
                    locationPathName = output,
                    target = target,
                    options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Frozen evaluation player build failed: " + report.summary.result);
            }
            finally
            {
                AssetDatabase.DeleteAsset(temporaryRoot);
                AssetDatabase.Refresh();
            }
        }
#endif
    }
}
