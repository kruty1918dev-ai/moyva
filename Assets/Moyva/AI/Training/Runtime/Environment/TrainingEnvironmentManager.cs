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
        public TrainingMetricsHub Metrics { get; private set; }
        public TrainingDecisionJournal Decisions { get; private set; }
        private TrainingReadinessReport _readiness;
        private TrainingAutonomyCoordinator _autonomy;
        private TrainingObserverServer _observerServer;
        private TrainingArenaSnapshotPublisher _snapshotPublisher;
        private string _sessionId;
        public TrainingReadinessReport Readiness => _environments.Count > 0 ? _environments[0].Readiness : _readiness;

        public void Initialize(TrainingConfig config, ITrainingSimulationFactory factory)
        {
            if (_environments.Count != 0) throw new InvalidOperationException("Manager is already initialized.");
            config.Validate();
            if (factory is ScaffoldSimulationFactory && !config.allowScaffoldSimulation)
                throw new InvalidOperationException("Scaffold simulation requires allowScaffoldSimulation=true.");
            _sessionId = Guid.NewGuid().ToString("N");
            Metrics = new TrainingMetricsHub(config.metricsHistoryCapacity);
            if (config.enableDecisionJournal || config.observerEnabled)
                Decisions = new TrainingDecisionJournal(config.decisionJournalCapacity,
                    config.enableDecisionJournal ? ResolveJournalPath(config) : null,
                    256L * 1024L * 1024L);
            if (config.curriculum?.autonomous?.enabled == true)
                _autonomy = new TrainingAutonomyCoordinator(config);
            if (config.environmentCount > 1 && !factory.SupportsIndependentEnvironments)
                throw new InvalidOperationException("Simulation factory cannot isolate multiple gameplay environments. Use environmentCount=1.");
            for (int id = 0; id < config.environmentCount; id++)
            {
                var simulation = factory.Create(id);
                var environment = new TrainingEnvironment(id, config, simulation);
                environment.SetObserverSessionId(_sessionId);
                environment.SetDecisionJournal(Decisions);
                environment.EpisodeReset += OnEpisodeReset;
                _environments.Add(environment);
                Metrics.Attach(environment);
                environment.CheckReadiness(factory);
                _autonomy?.Attach(environment);
                environment.BeginEpisode();
                _readiness = environment.CheckReadiness(factory);
                if (!config.allowScaffoldSimulation && !Readiness.IsReady)
                    throw new InvalidOperationException(Readiness.ToString());
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
                if (config.enableEditorTelemetry && config.presentationMode != TrainingPresentationMode.HeadlessFast)
                    go.AddComponent<BotTelemetryView>().ConfigureProvider(() => environment.Bridge.Orchestrator);
                go.SetActive(true);
                if (id == 0 && !string.IsNullOrEmpty(environment.Limitation))
                    Debug.LogWarning(environment.Limitation, this);
            }

            if (config.observerEnabled)
            {
                _observerServer = new TrainingObserverServer(_sessionId, config.observerEndpoint,
                    config.observerQueueCapacity, config.observerMaxMessageBytes);
                if (Decisions != null) Decisions.Appended += _observerServer.PublishDecision;
                _snapshotPublisher = new TrainingArenaSnapshotPublisher(() => Environments, _observerServer,
                    Decisions, config.observerSnapshotHz);
                _observerServer.Start();
                Debug.Log("Moyva training observer IPC: " + _observerServer.ResolvedEndpoint, this);
            }
        }

        private void Update()
        {
            _snapshotPublisher?.Tick(Time.realtimeSinceStartup);
        }

        private void OnEpisodeReset(int arenaId, long episodeId)
        {
            _observerServer?.MarkEpisodeReset(arenaId, episodeId, Decisions?.CurrentSequence ?? 0);
        }

        public void Shutdown()
        {
            _autonomy?.Dispose();
            _autonomy = null;
            if (Decisions != null && _observerServer != null) Decisions.Appended -= _observerServer.PublishDecision;
            _snapshotPublisher = null;
            _observerServer?.Dispose();
            _observerServer = null;
            Metrics?.Dispose();
            foreach (var go in _agentObjects)
                if (go != null) { go.SetActive(false); Destroy(go); }
            _agentObjects.Clear();
            foreach (var environment in _environments)
            {
                environment.EpisodeReset -= OnEpisodeReset;
                environment.Dispose();
            }
            _environments.Clear();
            Decisions = null;
        }

        private void OnDestroy() => Shutdown();

        private static string ResolveJournalPath(TrainingConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config.decisionJournalPath))
                return config.decisionJournalPath;
            return System.IO.Path.Combine(Application.persistentDataPath, "MoyvaTraining", "agent-decisions.jsonl");
        }
    }
}
