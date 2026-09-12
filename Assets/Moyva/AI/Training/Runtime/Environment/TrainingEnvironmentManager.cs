using System;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingEnvironmentManager : MonoBehaviour
    {
        private readonly List<TrainingEnvironment> _environments = new List<TrainingEnvironment>();
        private readonly List<GameObject> _agentObjects = new List<GameObject>();
        public IReadOnlyList<TrainingEnvironment> Environments => _environments.AsReadOnly();

        public void Initialize(TrainingConfig config, ITrainingSimulationFactory factory)
        {
            if (_environments.Count != 0) throw new InvalidOperationException("Manager is already initialized.");
            config.Validate();
            if (config.environmentCount > 1 && !factory.SupportsIndependentEnvironments)
                throw new InvalidOperationException("Simulation factory cannot isolate multiple gameplay environments. Use environmentCount=1.");
            for (int id = 0; id < config.environmentCount; id++)
            {
                var simulation = factory.Create(id);
                var environment = new TrainingEnvironment(id, config, simulation);
                _environments.Add(environment);
                var slot = transform.parent.Find("Environment_" + id);
                var go = slot != null ? slot.gameObject : new GameObject("Environment_" + id);
                go.transform.SetParent(transform.parent, false);
                go.SetActive(false);
                _agentObjects.Add(go);
                var behavior = go.AddComponent<BehaviorParameters>();
                behavior.BehaviorName = MoyvaStrategyAgent.BehaviorName;
                behavior.BehaviorType = config.behaviorType;
                behavior.BrainParameters.VectorObservationSize = TrainingObservationLayout.Size;
                behavior.BrainParameters.NumStackedVectorObservations = 1;
                behavior.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(TrainingActionMaskProvider.BranchSize);
                var agent = go.AddComponent<MoyvaStrategyAgent>();
                agent.Configure(environment, config);
                go.AddComponent<BotTelemetryView>().ConfigureProvider(() => environment.Bridge.Orchestrator);
                go.SetActive(true);
                if (id == 0 && !string.IsNullOrEmpty(environment.Limitation))
                    Debug.LogWarning(environment.Limitation, this);
            }
        }

        public void Shutdown()
        {
            foreach (var go in _agentObjects)
                if (go != null) { go.SetActive(false); Destroy(go); }
            _agentObjects.Clear();
            foreach (var environment in _environments) environment.Dispose();
            _environments.Clear();
        }

        private void OnDestroy() => Shutdown();
    }
}
