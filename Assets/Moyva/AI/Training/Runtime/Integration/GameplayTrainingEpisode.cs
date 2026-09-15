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
        private readonly HashSet<string> _legitimatelyRecruitedUnitIds = new HashSet<string>(StringComparer.Ordinal);
        private bool _disposed;
        private bool _setupPhase = true;
        public bool EconomyInstalled { get; private set; }
        public string FullGameSetupError { get; private set; }
        public ITurnService Turns { get; private set; }
        public IBotTurnGateway Gateway { get; private set; }
        public BotCapabilityRegistry Capabilities { get; private set; }
        public IBotPerceptionSource Perception { get; private set; }
        public TrainingGameplayEventBridge Outcomes { get; private set; }
        internal MenuWorldPreviewData GeneratedWorld => _world?.Data;
        public IGridService Grid => _container.Resolve<IGridService>();
        public IGridProjection Projection => _container.Resolve<IGridProjection>();
        public GameObject Root => _root;
        internal IBuildingRegistry Buildings => _container.TryResolve<IBuildingRegistry>();
        internal IConstructionSaveSnapshotSource Placements => _container.Resolve<IConstructionSaveSnapshotSource>();
        internal IUnitMovementService Movement => _container.Resolve<IUnitMovementService>();
        internal IUnitMovementQuery MovementQuery => _container.Resolve<IUnitMovementQuery>();
        internal IUnitService Units => _container.Resolve<IUnitService>();
        internal IUnitOwnershipQuery UnitOwners => _container.Resolve<IUnitOwnershipQuery>();
        internal IUnitTraversalPolicy Traversal => _container.Resolve<IUnitTraversalPolicy>();
        internal IHealthRegistry Health => _container.Resolve<IHealthRegistry>();
        internal IEconomyRuntimeApi Economy => _container.Resolve<IEconomyRuntimeApi>();
        internal IFogOwnerStateReader Fog => _container.TryResolve<IFogOwnerStateReader>();
        internal SignalBus Signals => _signals;
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
                int worldSize = context.WorldSize > 0 ? context.WorldSize : config.worldSize;
                GridInstaller.InstallPreviewBindings(_container, tiles, Required<MoyvaProjectSettingsSO>(), worldSize, worldSize);
                _world = new OwnedGameplayMap(_container, _root, graph, tiles, Required<MapObjectRegistrySO>(), worldSize, context.Seed);
                var world = _world.Data;
                EnrichTrainingBiomes(world, context.Seed);
                Install<ObjectsMapInstaller>();
                Install<AnimationsInstaller>();
                Install<PathfinderInstaller>();
                CalendarInstaller.InstallDefaultIfMissing(_container);
                _container.Bind<ITurnAuthorityPolicy>().To<TrainingTurnAuthority>().AsSingle();
                TurnBindings.Install(_container);
                UnitsInstaller.InstallSimulationBindings(_container, Required<UnitRegistrySO>());
                FogOfWarInstaller.InstallSimulationBindings(_container);
                GameModeInstaller.InstallSimulationBindings(_container);
                var economy = Required<EconomyDatabaseSO>();
                EconomyInstaller.InstallSimulationBindings(_container, economy);
                ConstructionInstaller.InstallSimulationBindings(_container, Required<BuildingRegistrySO>(),
                    economy.RulesConfig.Settlement.MinTownHallDistance);
                EconomyInstalled = true;

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
                _signals.Subscribe<UnitRecruitmentDeployedSignal>(TrackRecruitmentDeployment);
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
                var first = SelectLearnerSpawn(world, spawnCells);
                var reachable = ReachableLandSpawnCells(world, first, spawnCells);
                if (reachable.Count < 2) throw new InvalidOperationException("Generated world has no connected legal opponent spawn.");
                var second = reachable.Where(c => c != first)
                    .OrderByDescending(c => (c - first).sqrMagnitude + ResourcePotentialScore(world, c) * 8)
                    .First();
                var assignments = new List<SpawnPositionAssignment>
                {
                    new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = TrainingGameplayScope.LearnerId, Position = first },
                    new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = TrainingGameplayScope.OpponentId, Position = second }
                };
                signals.Fire(new WorldSpawnPositionsSignal { Source = WorldSpawnPositionsSource.GeneratedHost,
                    Assignments = assignments.ToArray() });
                signals.Fire(new WorldBuiltSignal());

                Gateway = new MoyvaBotTurnAdapter(Turns, _container.Resolve<ITurnAuthorityPolicy>());
                Capabilities = BotRuntimeInstaller.CreateGameplayRegistry(_container, Gateway);
                Perception = new MoyvaBotPerceptionSource(Turns, unitService, owners, fog,
                    profiles: _container.TryResolve<IUnitGameplayProfileService>(),
                    terrain: _container.TryResolve<IGeneratedTerrainLevelQuery>(),
                    economy: _container.TryResolve<IEconomyInfoMediator>());
                _setupPhase = false;
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
        internal TrainingScenarioFacts CaptureTrainingFacts()
        {
            const string learner = TrainingGameplayScope.LearnerId;
            var stocks = new Dictionary<string, float>(StringComparer.Ordinal);
            var production = new Dictionary<string, float>(StringComparer.Ordinal);
            var deployedByType = new Dictionary<string, int>(StringComparer.Ordinal);
            var operationalByType = new Dictionary<string, int>(StringComparer.Ordinal);
            var unitCells = new Dictionary<string, Vector2Int>(StringComparer.Ordinal);
            var ownedSettlementIds = new List<string>();

            var recruitedByType = new Dictionary<string, int>(StringComparer.Ordinal);
            var setupByType = new Dictionary<string, int>(StringComparer.Ordinal);
            var legitimateRecruitmentSources = new List<string>();

            int ownedSettlements = 0;
            int operationalCastles = 0;
            if (EconomyInstalled)
            {
                var ids = Economy.GetSettlementIdsForOwner(learner);
                if (ids != null)
                {
                    ownedSettlements = ids.Count;
                    ownedSettlementIds.AddRange(ids);
                }

                var totals = Economy.GetOwnerResourceTotals(learner);
                if (totals != null)
                    foreach (var pair in totals)
                        if (!float.IsNaN(pair.Value) && !float.IsInfinity(pair.Value))
                            stocks[pair.Key] = pair.Value;

                var productionSnapshot = EconomyProductionReadModel.Capture(_container, learner);
                foreach (var pair in productionSnapshot.ProductionPerTurn)
                    production[pair.Key] = pair.Value;
                foreach (var pair in productionSnapshot.ActiveProducerBuildingsByType)
                    operationalByType[pair.Key] = pair.Value;

                if (ownedSettlements > 0)
                {
                    foreach (var placement in Placements.GetSavedPlacements())
                    {
                        if (!string.Equals(placement.OwnerId, learner, StringComparison.Ordinal)
                            || !string.Equals(placement.BuildingId, "castle-01", StringComparison.Ordinal))
                            continue;
                        operationalCastles++;
                    }
                }
                if (operationalCastles > 0)
                    operationalByType["castle-01"] = operationalCastles;

                var buildingRegistry = _container.TryResolve<IBuildingRegistry>();
                var recruitmentQuery = _container.TryResolve<IUnitRecruitmentQuery>();
                if (buildingRegistry != null && recruitmentQuery != null)
                {
                    foreach (var placement in Placements.GetSavedPlacements())
                    {
                        if (!string.Equals(placement.OwnerId, learner, StringComparison.Ordinal)) continue;
                        var buildingDef = buildingRegistry.GetById(placement.BuildingId);
                        if (buildingDef != null && BuildingDefinitionCapabilities.TryGetEnabledModule(buildingDef, out UnitRecruitmentBuildingModule _))
                        {
                            string key = placement.BuildingId + "@" + placement.Position.x + "," + placement.Position.y;
                            legitimateRecruitmentSources.Add(key);
                        }
                    }
                }
            }

            int ownedUnits = 0;
            int deployedUnits = 0;
            int visibleEnemies = 0;
            var fog = _container.TryResolve<IFogOwnerStateReader>();

            foreach (string unitId in Units.GetAllUnitIds())
            {
                string owner = UnitOwners.GetUnitOwnerId(unitId);
                bool positioned = Units.TryGetUnitPosition(unitId, out var cell);
                if (string.Equals(owner, learner, StringComparison.Ordinal))
                {
                    ownedUnits++;
                    if (!positioned) continue;
                    deployedUnits++;
                    unitCells[unitId] = cell;
                    string type = Units.GetUnitTypeId(unitId);
                    if (!string.IsNullOrWhiteSpace(type))
                    {
                        deployedByType[type] = deployedByType.TryGetValue(type, out var count) ? count + 1 : 1;

                        if (_legitimatelyRecruitedUnitIds.Contains(unitId))
                            recruitedByType[type] = recruitedByType.TryGetValue(type, out var rc) ? rc + 1 : 1;
                        else
                            setupByType[type] = setupByType.TryGetValue(type, out var sc) ? sc + 1 : 1;
                    }
                }
                else if (positioned && fog != null && fog.IsVisible(learner, cell))
                {
                    visibleEnemies++;
                }
            }
            int exploredCells = 0;
            var exploration = _container.TryResolve<IFogOwnerExplorationSnapshotStore>();
            var explored = exploration?.GetExploredSnapshot(learner);
            if (explored != null)
                for (int y = 0; y < explored.GetLength(1); y++)
                    for (int x = 0; x < explored.GetLength(0); x++)
                        if (explored[x, y]) exploredCells++;

            int currentTurn = Turns == null ? 0
                : Turns.GlobalTurn > int.MaxValue ? int.MaxValue
                : Turns.GlobalTurn < int.MinValue ? int.MinValue
                : (int)Turns.GlobalTurn;
            var world = _world?.Data;

            return new TrainingScenarioFacts(
                isSetup: _setupPhase,
                ownedSettlements: ownedSettlements,
                operationalCastles: operationalCastles,
                ownedUnits: ownedUnits,
                deployedUnits: deployedUnits,
                resourceStock: stocks,
                productionPerTurn: production,
                visibleEnemyUnitCount: visibleEnemies,
                capturedObjectiveIds: ownedSettlementIds,
                currentTurn: currentTurn,
                exploredCells: exploredCells,
                deployedUnitsByType: deployedByType,
                operationalBuildingsByType: operationalByType,
                unitCells: unitCells,
                ownedSettlementIds: ownedSettlementIds,
                reachableLandCellsFromLearner: CountReachableLandFromLearner(world),
                totalLandCells: CountLandCells(world),
                learnerResourcePotential: CountLearnerResourcePotential(world),
                recruitedUnitsByType: recruitedByType,
                setupUnitsByType: setupByType,
                legitimateRecruitmentSources: legitimateRecruitmentSources);
        }

        private Vector2Int SelectLearnerSpawn(MenuWorldPreviewData world, IReadOnlyCollection<Vector2Int> spawnCells)
        {
            return spawnCells
                .OrderByDescending(c => FloodLand(world, c) + ResourcePotentialScore(world, c) * 12 + CoastalScore(world, c) * 4)
                .ThenBy(c => c.y)
                .ThenBy(c => c.x)
                .First();
        }

        private static void EnrichTrainingBiomes(MenuWorldPreviewData world, int seed)
        {
            if (world?.BiomeMap == null) return;
            var range = ResolveHeightRange(world);
            for (int y = 0; y < world.Height; y++)
                for (int x = 0; x < world.Width; x++)
                {
                    string current = world.BiomeMap[x, y] ?? string.Empty;
                    if (IsWater(current)) continue;
                    var cell = new Vector2Int(x, y);
                    float height = Height01(world, cell, range);
                    float noise = Hash01(seed, x, y);
                    bool coast = AdjacentToWater(world, cell);
                    if (coast && height < 0.62f)
                        world.BiomeMap[x, y] = "sand";
                    else if (height > 0.82f)
                        world.BiomeMap[x, y] = "mountain";
                    else if (height > 0.68f || noise > 0.86f)
                        world.BiomeMap[x, y] = "hill";
                    else if (noise > 0.58f)
                        world.BiomeMap[x, y] = "forest-sparse";
                    else if (height < 0.42f && noise > 0.30f)
                        world.BiomeMap[x, y] = "lowland";
                    else
                        world.BiomeMap[x, y] = "grass";
        }
        }

        private static Vector2 ResolveHeightRange(MenuWorldPreviewData world)
        {
            if (world?.HeightMap == null) return new Vector2(0f, 1f);
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            for (int y = 0; y < world.Height; y++)
                for (int x = 0; x < world.Width; x++)
                {
                    float value = world.HeightMap[x, y];
                    if (float.IsNaN(value) || float.IsInfinity(value)) continue;
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                }
            return max > min ? new Vector2(min, max) : new Vector2(0f, 1f);
        }

        private static float Height01(MenuWorldPreviewData world, Vector2Int cell, Vector2 range)
        {
            if (world?.HeightMap == null || !InBounds(world, cell)) return 0.5f;
            float raw = world.HeightMap[cell.x, cell.y];
            return Mathf.Approximately(range.x, range.y)
                ? Mathf.Clamp01(raw)
                : Mathf.Clamp01((raw - range.x) / (range.y - range.x));
        }

        private static float Hash01(int seed, int x, int y)
            {
            unchecked
                    {
                uint h = 2166136261;
                h = (h ^ (uint)seed) * 16777619;
                h = (h ^ (uint)x) * 16777619;
                h = (h ^ (uint)y) * 16777619;
                h ^= h >> 13;
                h *= 1274126177;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }

        private static List<Vector2Int> ReachableLandSpawnCells(
            MenuWorldPreviewData world,
            Vector2Int start,
            IReadOnlyCollection<Vector2Int> candidates)
        {
            var candidateSet = new HashSet<Vector2Int>(candidates);
            var result = new List<Vector2Int>();
            var seen = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            while (queue.Count > 0 && seen.Count <= 4096)
            {
                var from = queue.Dequeue();
                if (candidateSet.Contains(from)) result.Add(from);
                for (int i = 0; i < 4; i++)
                {
                    var next = from + (i == 0 ? Vector2Int.right : i == 1 ? Vector2Int.left : i == 2 ? Vector2Int.up : Vector2Int.down);
                    if (!InBounds(world, next) || !seen.Add(next) || IsWater(world.BiomeMap[next.x, next.y]))
                        continue;
                    queue.Enqueue(next);
                }
            }
            return result;
        }

        private int CountLearnerResourcePotential(MenuWorldPreviewData world)
        {
            if (world == null || !EconomyInstalled) return 0;
            int score = 0;
            foreach (var placement in Placements.GetSavedPlacements())
                if (string.Equals(placement.OwnerId, TrainingGameplayScope.LearnerId, StringComparison.Ordinal))
                    score = Math.Max(score, ResourcePotentialScore(world, placement.Position) + CoastalScore(world, placement.Position));
            return score;
        }

        private static int ResourcePotentialScore(MenuWorldPreviewData world, Vector2Int center)
        {
            if (world?.BiomeMap == null) return 0;
            int score = 0;
            foreach (var cell in NearbyCells(center, 5))
            {
                if (!InBounds(world, cell)) continue;
                string tile = world.BiomeMap[cell.x, cell.y] ?? string.Empty;
                string key = tile.ToLowerInvariant();
                if (key.Contains("forest") || key.Contains("wood")) score += 3;
                else if (key.Contains("mountain") || key.Contains("hill") || key.Contains("rock")) score += 3;
                else if (key.Contains("grass") || key.Contains("lowland")) score += 1;
            }
            return Math.Min(score, 64);
        }

        private static int CoastalScore(MenuWorldPreviewData world, Vector2Int center)
        {
            if (world?.BiomeMap == null) return 0;
            int score = 0;
            foreach (var cell in NearbyCells(center, 4))
            {
                if (!InBounds(world, cell) || !IsWater(world.BiomeMap[cell.x, cell.y])) continue;
                if (HasAdjacentLand(world, cell)) score += 2;
            }
            return Math.Min(score, 24);
        }

        private int CountReachableLandFromLearner(MenuWorldPreviewData world)
        {
            if (world == null) return 0;
            Vector2Int? start = null;
            foreach (var placement in Placements.GetSavedPlacements())
                if (string.Equals(placement.OwnerId, TrainingGameplayScope.LearnerId, StringComparison.Ordinal))
                {
                    start = placement.Position;
                    break;
                }
            if (!start.HasValue)
            {
                foreach (string unitId in Units.GetAllUnitIds())
                {
                    if (!string.Equals(UnitOwners.GetUnitOwnerId(unitId), TrainingGameplayScope.LearnerId, StringComparison.Ordinal))
                        continue;
                    if (Units.TryGetUnitPosition(unitId, out var position))
                    {
                        start = position;
                        break;
                    }
                }
            }
            return start.HasValue ? FloodLand(world, start.Value) : 0;
        }

        private static int CountLandCells(MenuWorldPreviewData world)
        {
            if (world?.BiomeMap == null) return 0;
            int count = 0;
            for (int y = 0; y < world.Height; y++)
                for (int x = 0; x < world.Width; x++)
                    if (!IsWater(world.BiomeMap[x, y])) count++;
            return count;
        }

        private static int FloodLand(MenuWorldPreviewData world, Vector2Int start)
        {
            if (world?.BiomeMap == null || !InBounds(world, start) || IsWater(world.BiomeMap[start.x, start.y]))
                return 0;
            var seen = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var from = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    var next = from + (i == 0 ? Vector2Int.right : i == 1 ? Vector2Int.left : i == 2 ? Vector2Int.up : Vector2Int.down);
                    if (!InBounds(world, next) || !seen.Add(next) || IsWater(world.BiomeMap[next.x, next.y]))
                        continue;
                    queue.Enqueue(next);
                }
            }
            return seen.Count;
        }

        private static bool InBounds(MenuWorldPreviewData world, Vector2Int cell)
            => cell.x >= 0 && cell.y >= 0 && cell.x < world.Width && cell.y < world.Height;

        private static bool HasAdjacentLand(MenuWorldPreviewData world, Vector2Int cell)
        {
            for (int i = 0; i < 4; i++)
            {
                var next = cell + (i == 0 ? Vector2Int.right : i == 1 ? Vector2Int.left : i == 2 ? Vector2Int.up : Vector2Int.down);
                if (InBounds(world, next) && !IsWater(world.BiomeMap[next.x, next.y])) return true;
            }
            return false;
        }

        private static bool AdjacentToWater(MenuWorldPreviewData world, Vector2Int cell)
        {
            for (int i = 0; i < 4; i++)
            {
                var next = cell + (i == 0 ? Vector2Int.right : i == 1 ? Vector2Int.left : i == 2 ? Vector2Int.up : Vector2Int.down);
                if (InBounds(world, next) && IsWater(world.BiomeMap[next.x, next.y])) return true;
            }
            return false;
        }

        private static IEnumerable<Vector2Int> NearbyCells(Vector2Int center, int radius)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (Mathf.Abs(cell.x - center.x) + Mathf.Abs(cell.y - center.y) <= radius)
                        yield return cell;
                }
        }

        private static bool IsWater(string tile)
        {
            string key = (tile ?? string.Empty).ToLowerInvariant();
            return key.Contains("water") || key.Contains("ocean") || key.Contains("river") || key.Contains("lake");
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
        private void OwnUnitObject(UnitCreatedSignal signal)
        {
            if (signal.UnitObject != null) signal.UnitObject.transform.SetParent(_root.transform, true);
        }
        private void TrackRecruitmentDeployment(UnitRecruitmentDeployedSignal signal)
        {
            if (signal.QueueId > 0
                && string.Equals(signal.OwnerId, TrainingGameplayScope.LearnerId, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(signal.UnitId))
                _legitimatelyRecruitedUnitIds.Add(signal.UnitId);
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
            _signals?.TryUnsubscribe<UnitRecruitmentDeployedSignal>(TrackRecruitmentDeployment);
            _opponent?.Dispose();
            Outcomes?.Dispose();
            for (int i = _disposables.Count - 1; i >= 0; i--) _disposables[i].Dispose();
            _world?.Dispose();
            if (_root != null)
            {
                _root.SetActive(false);
                if (Application.isPlaying) UnityEngine.Object.Destroy(_root);
                else UnityEngine.Object.DestroyImmediate(_root);
            }
        }
        private sealed class TrainingTurnAuthority : ITurnAuthorityPolicy { public bool IsAuthoritative => true; }
    }
}
