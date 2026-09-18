using System;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingBootstrap : MonoBehaviour
    {
        [SerializeField] private TextAsset configuration;
        [SerializeField] private TrainingEnvironmentManager environmentManager;
        public TextAsset Configuration => configuration;
        public TrainingConfig Config { get; private set; }
        public TrainingEnvironmentManager Environments => environmentManager;
        public TrainingPresentationController Presentation { get; private set; }
        public TrainingPerformanceController Performance { get; private set; }
        private TrainingReadinessReport _readiness;
        public TrainingReadinessReport Readiness => environmentManager?.Environments.Count > 0 ? environmentManager.Readiness : _readiness;
        public string Status { get; private set; } = "Not running";

        private void Start()
        {
            if (gameObject.scene.name != "MoyvaTraining")
            {
                Debug.LogError("TrainingBootstrap must run only in MoyvaTraining.", this);
                enabled = false;
                return;
            }
            try
            {
                Config = TrainingConfig.Load(configuration);
                TrainingCommandLine.Apply(Config, Environment.GetCommandLineArgs(), Application.isBatchMode);
                if (environmentManager == null) throw new InvalidOperationException("TrainingEnvironmentManager is missing.");
                Performance = new TrainingPerformanceController(Config, Config.presentationMode);
                var presentation = new GameObject("TrainingPresentation");
                presentation.transform.SetParent(transform.parent != null ? transform.parent : transform, false);
                Presentation = presentation.AddComponent<TrainingPresentationController>();
                Presentation.Initialize(this);
                var container = new DiContainer();
                new TrainingInstaller().Install(container, Config);
                environmentManager.Initialize(Config, container.Resolve<ITrainingSimulationFactory>());
                Presentation.RefreshCameras();
                if (Config.inspectorMode)
                {
                    var inspector = presentation.AddComponent<TrainingModelInspectorController>();
                    inspector.Initialize(this);
                }
                _readiness = environmentManager.Readiness;
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-moyvaRequireTrainer") >= 0
                    && !Unity.MLAgents.Academy.Instance.IsCommunicatorOn)
                    throw new InvalidOperationException("TRAINER_NOT_CONNECTED: no external ML-Agents trainer.");
                Status = Config.inspectorMode
                    ? "Model Inspector / Frozen Inference"
                    : Config.allowScaffoldSimulation ? "SCAFFOLD / NOT REAL GAMEPLAY" : "Running / Real Gameplay";
                Debug.Log(Readiness.Verdict + " | MoyvaStrategy | trainer=" + Unity.MLAgents.Academy.Instance.IsCommunicatorOn);
            }
            catch (Exception exception)
            {
                _readiness = environmentManager?.Readiness ?? new TrainingReadinessReport();
                Readiness.Block(exception.Message);
                Readiness.Block("TERMINAL_OUTCOME_BLOCKED: no running real episode.");
                Status = "Initialization blocked";
                Debug.LogError(Readiness.ToString(), this);
                environmentManager?.Shutdown();
                RestoreSettings();
                if (!Application.isEditor) Application.Quit(10);
            }
        }

        private void RestoreSettings()
        {
            Presentation?.RestoreCameras();
            Performance?.Dispose();
        }

        private void OnDestroy() { environmentManager?.Shutdown(); RestoreSettings(); }
    }
}
