using System;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingBootstrap : MonoBehaviour
    {
        [SerializeField] private TextAsset configuration;
        [SerializeField] private TrainingEnvironmentManager environmentManager;
        private float _previousTimeScale;
        private bool _ownsSettings;

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
                var config = TrainingConfig.Load(configuration);
                var container = new DiContainer();
                new TrainingInstaller().Install(container, config);
                _previousTimeScale = Time.timeScale;
                _ownsSettings = true;
                Time.timeScale = config.trainingTimeScale;
                if (config.disableRenderingWhenPossible)
                {
                    foreach (var root in gameObject.scene.GetRootGameObjects())
                        foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                            camera.enabled = false;
                }
                environmentManager.Initialize(config, container.Resolve<ITrainingSimulationFactory>());
            }
            catch (Exception exception)
            {
                Debug.LogError("Training initialization failed: " + exception.Message, this);
                environmentManager?.Shutdown();
                RestoreSettings();
                enabled = false;
            }
        }

        private void RestoreSettings()
        {
            if (!_ownsSettings) return;
            Time.timeScale = _previousTimeScale;
            _ownsSettings = false;
        }

        private void OnDestroy() => RestoreSettings();
    }
}
