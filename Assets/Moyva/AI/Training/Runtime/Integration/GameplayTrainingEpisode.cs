using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Animations.Runtime;
using Kruty1918.Moyva.Calendar.Runtime;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Economy;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.ObjectsMap.Runtime;
using Kruty1918.Moyva.Pathfinding.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Turns.Runtime;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    internal sealed class GameplayTrainingEpisode : IDisposable
    {
        private readonly DiContainer _container = new DiContainer();
        private readonly GameObject _root;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private SignalBus _signals;
        private BotDecisionOrchestrator _opponent;
        private OwnedGameplayMap _world;
        private bool _disposed;
        public bool EconomyInstalled { get; private set; }
        public string FullGameSetupError { get; private set; }
        public ITurnService Turns { get; private set; }
        public IBotTurnGateway Gateway { get; private set; }
        public BotCapabilityRegistry Capabilities { get; private set; }
        public IBotPerceptionSource Perception { get; private set; }
        public TrainingGameplayEventBridge Outcomes { get; private set; }
        public IGridService Grid => _container.Resolve<IGridService>();
        public IGridProjection Projection => _container.Resolve<IGridProjection>();
        public GameObject Root => _root;
        internal IConstructionSaveSnapshotSource Placements => _container.Resolve<IConstructionSaveSnapshotSource>();
        internal IConstructionPlacedBuildingDestruction Destruction => _container.Resolve<IConstructionPlacedBuildingDestruction>();

        public GameplayTrainingEpisode(TrainingConfig config, TrainingResetContext context)
        {
            _root = new GameObject("TrainingWorld_" + context.EpisodeId);
            var randomState = UnityEngine.Random.state;
            try
            {
                UnityEngine.Random.InitState(context.Seed);
                MoyvaJsonRuntime.EnsureLoaded();
                var graph = MoyvaJsonRuntime.Get<GraphAsset>(config.generatorGraphId);
                Install<Kruty1918.Moyva.Signals.SignalBusInstaller>();
                var tiles = graph.TileRegistry ?? Required<TileRegistrySO>();
                GridInstaller.InstallPreviewBindings(_container, tiles, Required<MoyvaProjectSettingsSO>(), config.worldSize, config.worldSize);
                _world = new OwnedGameplayMap(_container, _root, graph, tiles, Required<MapObjectRegistrySO>(), config.worldSize, context.Seed);
                var world = _world.Data;
                Install<ObjectsMapInstaller>();
                Install<AnimationsInstaller>();
                Install<PathfinderInstaller>();
                CalendarInstaller.InstallDefaultIfMissing(_container);
                _container.Bind<ITurnAuthorityPolicy>().To<TrainingTurnAuthority>().AsSingle();
                TurnBindings.Install(_container);
                UnitsInstaller.InstallSimulationBindings(_container, Required<UnitRegistrySO>());
                FogOfWarInstaller.InstallSimulationBindings(_container);
                GameModeInstaller.InstallSimulationBindings(_container);
                if ((int)context.CurriculumStage >= 3)
                {
                    var economy = Required<EconomyDatabaseSO>();
                    EconomyInstaller.InstallSimulationBindings(_container, economy);
                    ConstructionInstaller.InstallSimulationBindings(_container, Required<BuildingRegistrySO>(),
                        economy.RulesConfig.Settlement.MinTownHallDistance);
                    EconomyInstalled = true;
                }

                var signals = _container.Resolve<SignalBus>();
                var grid = _container.Resolve<IGridService>();
                for (int y = 0; y < world.Height; y++)
                    for (int x = 0; x < world.Width; x++) grid.SetTileData(new Vector2Int(x, y), world.BiomeMap[x, y]);
                // Force the complete graph before recording disposal, including turn participants.
                Turns = _container.Resolve<ITurnService>();
                var unitService = _container.Resolve<IUnitService>();
                var owners = _container.Resolve<IUnitOwnershipQuery>();
                var fog = _container.Resolve<IFogOwnerStateReader>();
                var movement = _container.Resolve<IUnitMovementService>();
                var movementQuery = _container.Resolve<IUnitMovementQuery>();
                _container.ResolveAll<ITurnParticipant>();
                _container.ResolveAll<ITurnBlocker>();
                var initializers = _container.ResolveAll<IInitializable>();
                _disposables.AddRange(_container.ResolveAll<IDisposable>());
                foreach (var initializer in initializers) initializer.Initialize();
                _signals = signals;
                _signals.Subscribe<UnitCreatedSignal>(OwnUnitObject);
                _container.Resolve<IGameStateService>().StartGame();
                signals.Fire(new WorldGeneratedDataSignal { Width = world.Width, Height = world.Height,
                    TileMap = world.BiomeMap, ObjectMap = world.ObjectMap, HeightMap = world.HeightMap,
                    Source = WorldGeneratedDataSource.GeneratedHost, StartupSessionId = "training:" + context.EpisodeId });

                var placement = _container.Resolve<IUnitPlacementValidator>();
                var spawnCells = new List<Vector2Int>();
                var spawnRejections = new HashSet<string>();
                for (int y = 1; y < world.Height - 1; y++)
                    for (int x = 1; x < world.Width - 1; x++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (placement.CanDeployUnit(config.startingUnitTypeId, cell, out var rejection)) spawnCells.Add(cell);
                        else if (spawnRejections.Count < 4) spawnRejections.Add(grid.GetTileData(cell) + ": " + rejection);
                    }
                if (spawnCells.Count < 2) throw new InvalidOperationException("Generated world has fewer than two legal unit spawns. " + string.Join("; ", spawnRejections));
                var first = spawnCells[0];
                var second = spawnCells.OrderByDescending(c => (c - first).sqrMagnitude).First();
                Spawn(config.startingUnitTypeId, first, TrainingGameplayScope.LearnerId);
                Spawn(config.startingUnitTypeId, second, TrainingGameplayScope.OpponentId);
                signals.Fire(new WorldSpawnPositionsSignal { Source = WorldSpawnPositionsSource.GeneratedHost,
                    Assignments = new[] {
                        new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = TrainingGameplayScope.LearnerId, Position = first },
                        new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = TrainingGameplayScope.OpponentId, Position = second } } });
                signals.Fire(new WorldBuiltSignal());

                Gateway = new MoyvaBotTurnAdapter(Turns, _container.Resolve<ITurnAuthorityPolicy>());
                Capabilities = BotRuntimeInstaller.CreateGameplayRegistry(_container, Gateway);
                Perception = new MoyvaBotPerceptionSource(Turns, unitService, owners, fog, _container.TryResolve<IEconomyInfoMediator>());
                if (EconomyInstalled)
                {
                    var starter = new BootstrapStarterPackGrantService(Required<BootstrapInstallerConfigSO>().GameSettings, signals);
                    foreach (string owner in new[] { TrainingGameplayScope.LearnerId, TrainingGameplayScope.OpponentId })
                    {
                        starter.TryGrant(null, owner);
                        var bootstrap = _container.Resolve<IConstructionBootstrapQuery>();
                        if (!bootstrap.RequiresInitialCastle(owner, out string castle))
                            FullGameSetupError = "No required initial castle was found in the production registry.";
                        else
                        {
                            var action = Capabilities.Get(BotCapabilityId.Construction).Enumerate(owner)
                                .FirstOrDefault(c => c.TargetKey == castle);
                            if (action == null || !_container.Resolve<IAuthoritativeConstructionPlacementExecutor>()
                                .TryPlaceAuthoritatively(castle, new Vector2Int(action.X, action.Y), owner, ConstructionPlacementCommitIntent.None))
                                FullGameSetupError = "No legal initial castle placement for " + owner + ".";
                        }
                        var registry = _container.Resolve<IBuildingRegistry>();
                        var recruitmentSource = Capabilities.Get(BotCapabilityId.Construction).Enumerate(owner)
                            .FirstOrDefault(c => BuildingDefinitionCapabilities.TryGetEnabledModule(
                                registry.GetById(c.TargetKey), out UnitRecruitmentBuildingModule _));
                        if (recruitmentSource == null || !_container.Resolve<IAuthoritativeConstructionPlacementExecutor>()
                            .TryPlaceAuthoritatively(recruitmentSource.TargetKey, new Vector2Int(recruitmentSource.X, recruitmentSource.Y),
                                owner, ConstructionPlacementCommitIntent.None))
                            FullGameSetupError = "No affordable legal recruitment source can be constructed for " + owner + ".";
                        if (_container.Resolve<IEconomyRuntimeApi>().GetSettlementIdsForOwner(owner).Count == 0)
                            FullGameSetupError = "Initial castle did not create an active economic settlement for " + owner + ".";
                        if (!Turns.TryEndTurn(owner, out string turnReason))
                            throw new InvalidOperationException("Initial settlement turn could not finish: " + turnReason);
                    }
                }
                Outcomes = new TrainingGameplayEventBridge(signals, _container.Resolve<ITurnHistoryQuery>(), TrainingGameplayScope.LearnerId,
                    _container.Resolve<IUnitCombatService>(), owners, context.EpisodeId, _container.TryResolve<IBuildingRegistry>());
                _opponent = new BotDecisionOrchestrator(Gateway, Capabilities, Perception, new HeuristicBotPolicyDriver(),
                    new BotRuntimeConfig { curriculumStage = (int)context.CurriculumStage, visibleDelay = 0 }, new BotTelemetryHub(4));
                if (!Turns.CanOwnerAct(TrainingGameplayScope.LearnerId, out var reason))
                    throw new InvalidOperationException("Training world did not enter the learner turn: " + reason);
                Debug.Log($"MOYVA_EPISODE_BEGIN episode={context.EpisodeId} seed={context.Seed} units={unitService.GetAllUnitIds().Count} turn={Turns.GlobalTurn}");
            }
            catch { Dispose(); throw; }
            finally { UnityEngine.Random.state = randomState; }
        }
        private static T Required<T>() where T : class
            => MoyvaJsonRuntime.GetAll<T>().FirstOrDefault() ?? throw new InvalidOperationException("Missing JSON configuration: " + typeof(T).Name);
        private void Install<T>() where T : MonoInstaller
        {
            var installer = _root.AddComponent<T>();
            _container.Inject(installer);
            installer.InstallBindings();
            installer.enabled = false;
        }
        private void Spawn(string type, Vector2Int cell, string owner)
        {
            string id = _container.Resolve<IUnitFactory>().CreateUnit(type, cell, owner);
            if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("UnitFactory failed for " + owner);
            var unit = _container.Resolve<IUnitService>().GetUnitObject(id);
            unit.transform.SetParent(_root.transform, true);
        }
        private void OwnUnitObject(UnitCreatedSignal signal)
        {
            if (signal.UnitObject != null) signal.UnitObject.transform.SetParent(_root.transform, true);
        }
        public void Tick(float seconds)
        {
            if (Turns.IsOwnerActive(TrainingGameplayScope.OpponentId))
                _opponent.BeginTurn(TrainingGameplayScope.OpponentId);
            _opponent.Tick(seconds);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _signals?.TryUnsubscribe<UnitCreatedSignal>(OwnUnitObject);
            _opponent?.Dispose();
            Outcomes?.Dispose();
            for (int i = _disposables.Count - 1; i >= 0; i--) _disposables[i].Dispose();
            _world?.Dispose();
            _root.SetActive(false);
            if (Application.isPlaying) UnityEngine.Object.Destroy(_root);
            else UnityEngine.Object.DestroyImmediate(_root);
        }
        private sealed class TrainingTurnAuthority : ITurnAuthorityPolicy { public bool IsAuthoritative => true; }
    }
}
