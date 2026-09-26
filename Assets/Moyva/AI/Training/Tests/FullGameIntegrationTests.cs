using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using System.Linq;
using System.Threading;
using Kruty1918.Moyva.AI.Bot;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public sealed class FullGameIntegrationTests
    {
        [UnityTearDown]
        public IEnumerator RestoreEditModeAfterFailure()
        {
            if (Application.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator CastleLessonStartsWithoutLearnerBuildingsAndOffersRealPlacement()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
            config.curriculum.stage = TrainingCurriculumStage.Building;
            config.learnInitialCastle = true;
            var container = new DiContainer();
            new TrainingInstaller().Install(container, config);
            using (var simulation = (GameplayTrainingSimulation)container.Resolve<ITrainingSimulationFactory>().Create(0))
            using (var environment = new TrainingEnvironment(0, config, simulation))
            {
                environment.BeginEpisode();
                Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
                var world = simulation.Episode;
                Assert.IsEmpty(world.Placements.GetSavedPlacements().Where(p => p.OwnerId == simulation.PlayerId));
                Assert.IsTrue(world.Placements.GetSavedPlacements().Any(p => p.OwnerId == TrainingGameplayScope.OpponentId));
                var construction = simulation.Capabilities.Get(BotCapabilityId.Construction);
                Assert.IsTrue(construction.Enumerate(simulation.PlayerId).Any(c => c.TargetKey == "castle-01"));
                Assert.IsTrue(world.Turns.CanOwnerAct(simulation.PlayerId, out _));
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator RealUnitsCaptureDefendedSettlementsAndRewardsResetForBothOwners()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            float previousScale = Time.timeScale;
            Time.timeScale = 8;
            try
            {
                var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
                config.curriculum.stage = TrainingCurriculumStage.FullGame;
                var container = new DiContainer();
                new TrainingInstaller().Install(container, config);
                var factory = container.Resolve<ITrainingSimulationFactory>();
                using var simulation = (GameplayTrainingSimulation)factory.Create(0);
                using var environment = new TrainingEnvironment(0, config, simulation);
                int captures = 0;
                int rewardEvents = 0;
                GameObject oldRoot = null;
                simulation.GameplayReward += reward =>
                {
                    if (reward.Type == TrainingRewardEventType.ObjectiveCaptured || reward.Type == TrainingRewardEventType.ObjectiveLost)
                        rewardEvents++;
                };
                for (int episode = 0; episode < 2; episode++)
                {
                    environment.BeginEpisode();
                    yield return null;
                    Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
                    Assert.IsTrue(TrainingReadinessValidator.Validate(factory, simulation, environment).IsReady);
                    if (episode > 0) Assert.IsTrue(oldRoot == null, "Previous capture world survived reset.");
                    Assert.AreEqual(0, environment.Rewards.TotalReward);
                    var world = simulation.Episode;
                    string player = episode == 0 ? simulation.PlayerId : TrainingGameplayScope.OpponentId;
                    string defender = episode == 0 ? TrainingGameplayScope.OpponentId : simulation.PlayerId;
                    Assert.AreEqual(1, world.Economy.GetSettlementIdsForOwner(player).Count);
                    Assert.AreEqual(1, world.Economy.GetSettlementIdsForOwner(defender).Count);
                    var castle = world.Placements.GetSavedPlacements().Single(p => p.OwnerId == defender && p.BuildingId == "castle-01");
                    string target = $"{castle.BuildingId}@{castle.Position.x},{castle.Position.y}";
                    string settlement = world.Economy.GetSettlementIdsForOwner(defender).Single();
                    string actor = world.Units.GetAllUnitIds().Single(id => world.UnitOwners.GetUnitOwnerId(id) == player);
                    var capture = simulation.Capabilities.Get(BotCapabilityId.Capture);
                    Assert.IsInstanceOf<CaptureBotCapability>(capture);
                    Assert.IsNull(capture.UnavailableReason(player));
                    Assert.IsEmpty(capture.Enumerate(player), "Full defenses must prevent capture.");
                    world.Signals.Subscribe<SettlementCapturedSignal>(signal =>
                    {
                        captures++;
                        Assert.AreEqual(settlement, signal.SettlementId);
                        Assert.AreEqual(defender, signal.PreviousOwnerId);
                        Assert.AreEqual(player, signal.NewOwnerId);
                    });
                    EnsureTurn(simulation, player);
                    // Test setup advances a real unit through production traversal, movement query and command.
                    yield return MoveToSettlement(simulation, player, actor, castle.Position);
                    Assert.IsEmpty(capture.Enumerate(player), "Adjacency alone must not bypass defenses.");
                    Assert.IsTrue(world.Health.TryGet(target, out var health));
                    var combat = simulation.Capabilities.Get(BotCapabilityId.Combat);
                    for (int attack = 0; health.CurrentHp > Mathf.CeilToInt(health.MaxHp * 0.25f) && attack < 20; attack++)
                    {
                        var action = combat.Enumerate(player).FirstOrDefault(c => c.ActorKey == actor && c.TargetKey == target);
                        if (action == null)
                        {
                            AdvanceRound(simulation, player);
                            action = combat.Enumerate(player).FirstOrDefault(c => c.ActorKey == actor && c.TargetKey == target);
                        }
                        Assert.IsNotNull(action, "No real legal building attack can reduce the settlement defenses.");
                        yield return Complete(combat.Execute(player, action, CancellationToken.None));
                        Assert.Greater(health.CurrentHp, 0, "The test must capture a living settlement.");
                    }
                    var candidate = capture.Enumerate(player).FirstOrDefault(c => c.ActorKey == actor && c.TargetKey == target);
                    Assert.IsNotNull(candidate, "Production capture never became legal after movement and combat.");
                    // A later turn must invalidate the old identity even when geometry still matches.
                    AdvanceRound(simulation, player);
                    Assert.IsFalse(capture.Validate(player, candidate, out _));
                    candidate = capture.Enumerate(player).First(c => c.ActorKey == actor && c.TargetKey == target);
                    var movement = simulation.Capabilities.Get(BotCapabilityId.Movement);
                    var leave = movement.Enumerate(player).FirstOrDefault(c => c.ActorKey == actor
                        && Math.Max(Math.Abs(c.X - castle.Position.x), Math.Abs(c.Y - castle.Position.y)) > 1);
                    Assert.IsNotNull(leave, "A real move must be available to test stale capture range.");
                    yield return Complete(movement.Execute(player, leave, CancellationToken.None));
                    Assert.IsFalse(capture.Validate(player, candidate, out _));
                    Assert.IsEmpty(capture.Enumerate(player), "Capture must disappear outside adjacency.");
                    yield return MoveToSettlement(simulation, player, actor, castle.Position);
                    candidate = capture.Enumerate(player).First(c => c.ActorKey == actor && c.TargetKey == target);
                    var ids = capture.Enumerate(player).Select(c => c.Id).ToArray();
                    var resources = world.Economy.GetSettlementResourceTotals(settlement);
                    int hp = health.CurrentHp;
                    long turn = simulation.Turns.GlobalTurn;
                    world.Units.TryGetUnitPosition(actor, out var position);
                    CollectionAssert.AreEqual(ids, capture.Enumerate(player).Select(c => c.Id).ToArray());
                    CollectionAssert.AreEquivalent(resources, world.Economy.GetSettlementResourceTotals(settlement));
                    Assert.AreEqual(hp, health.CurrentHp);
                    Assert.AreEqual(turn, simulation.Turns.GlobalTurn);
                    Assert.IsTrue(world.Units.TryGetUnitPosition(actor, out var unchanged) && unchanged == position);
                    Assert.AreEqual(episode, captures);
                    Assert.AreEqual(episode, rewardEvents);
                    Assert.IsTrue(world.Economy.GetSettlementIdsForOwner(defender).Contains(settlement));
                    Assert.IsTrue(capture.Validate(player, candidate, out var reason), reason);
                    float shapingBefore = environment.Rewards.ShapingReward;
                    yield return Complete(capture.Execute(player, candidate, CancellationToken.None));
                    Assert.AreEqual(episode + 1, captures, "Capture signal must occur exactly once.");
                    Assert.AreEqual(episode + 1, rewardEvents, "Capture reward bridge must observe exactly one event.");
                    Assert.AreEqual(episode == 0 ? config.rewards.objectiveCaptured : config.rewards.objectiveLost,
                        environment.Rewards.ShapingReward - shapingBefore, 0.00001f);
                    Assert.IsTrue(world.Economy.GetSettlementIdsForOwner(player).Contains(settlement));
                    Assert.IsFalse(world.Economy.GetSettlementIdsForOwner(defender).Contains(settlement));
                    Assert.IsTrue(world.Placements.GetSavedPlacements().Any(p => p.Position == castle.Position && p.OwnerId == player));
                    CollectionAssert.AreEquivalent(resources, world.Economy.GetSettlementResourceTotals(settlement));
                    Assert.AreEqual(2, world.Economy.GetOwnerSettlementSnapshots(player).Count);
                    Assert.IsFalse(capture.Validate(player, candidate, out _));
                    Assert.AreEqual(BotExecutionStatus.Rejected, capture.Execute(player, candidate, CancellationToken.None).Result.Status);
                    Assert.AreEqual(episode + 1, captures);
                    Assert.AreEqual(episode + 1, rewardEvents);
                    // This is the last enemy settlement: normal capture elimination must end the match.
                    environment.Tick(0);
                    Assert.AreEqual(episode == 0 ? TrainingEpisodeResult.Victory : TrainingEpisodeResult.Defeat, environment.Result);
                    Assert.Greater(Math.Abs(environment.Rewards.TotalReward), environment.Rewards.AbsoluteShaping);
                    oldRoot = world.Root;
                }
            }
            finally { Time.timeScale = previousScale; }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator PerceptionPublishesSpatialIntelAndEconomyFeatures()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
            config.curriculum.stage = TrainingCurriculumStage.FullGame;
            var container = new DiContainer();
            new TrainingInstaller().Install(container, config);
            using (var simulation = (GameplayTrainingSimulation)container.Resolve<ITrainingSimulationFactory>().Create(0))
            using (var environment = new TrainingEnvironment(0, config, simulation))
            {
                environment.BeginEpisode();
                Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
                var snapshot = simulation.Perception.Capture(simulation.PlayerId);
                Assert.AreEqual(1f, snapshot.Global[BotObservationSchema.SpatialAvailable],
                    "Spatial block must be marked available when grid and fog are bound.");
                Assert.Greater(snapshot.Global[BotObservationSchema.OwnSettlements], 0f,
                    "FullGame scaffold must report the learner's castle settlement.");
                Assert.Greater(snapshot.Global[BotObservationSchema.OwnUnits], 0f);
                int explored = 0, visible = 0, ownUnits = 0, ownBuildings = 0;
                for (int cell = 0; cell < 64; cell++)
                {
                    if (snapshot.Spatial[BotSpatialChannel.Explored * 64 + cell] > 0) explored++;
                    if (snapshot.Spatial[BotSpatialChannel.Visible * 64 + cell] > 0) visible++;
                    if (snapshot.Spatial[BotSpatialChannel.OwnUnits * 64 + cell] > 0) ownUnits++;
                    if (snapshot.Spatial[BotSpatialChannel.OwnBuildings * 64 + cell] > 0) ownBuildings++;
                    Assert.LessOrEqual(snapshot.Spatial[BotSpatialChannel.Visible * 64 + cell],
                        snapshot.Spatial[BotSpatialChannel.Explored * 64 + cell],
                        "Visible-only intel must never appear on unexplored cells.");
                }
                Assert.Greater(explored, 0, "Opening reveal must land on the spatial grid.");
                Assert.Greater(visible, 0);
                Assert.Greater(ownUnits, 0, "Own units must land on the spatial grid.");
                Assert.Greater(ownBuildings, 0, "Own castle must land on the spatial grid.");
                // Fog legality: enemy spatial channels must contain no hidden state.
                int enemyVisible = 0, enemyRemembered = 0;
                for (int cell = 0; cell < 64; cell++)
                {
                    if (snapshot.Spatial[BotSpatialChannel.VisibleEnemyUnits * 64 + cell] > 0) enemyVisible++;
                    if (snapshot.Spatial[BotSpatialChannel.RememberedEnemyUnits * 64 + cell] > 0) enemyRemembered++;
                }
                int actualVisibleEnemies = simulation.Episode.Units.GetAllUnitIds()
                    .Count(id => simulation.Episode.UnitOwners.GetUnitOwnerId(id) != simulation.PlayerId
                        && simulation.Episode.Units.TryGetUnitPosition(id, out var position)
                        && simulation.Episode.Fog.IsVisible(simulation.PlayerId, position));
                Assert.GreaterOrEqual(enemyVisible + enemyRemembered, 0);
                if (actualVisibleEnemies > 0) Assert.Greater(enemyVisible, 0,
                    "Visible enemy units must appear on the spatial grid.");
            }
            yield return new ExitPlayMode();
        }

        private static IEnumerator Complete(Task<BotExecutionResult> task)
        {
            float deadline = Time.realtimeSinceStartup + 20;
            while (!task.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(task.IsCompleted, "Authoritative gameplay command timed out.");
            var result = task.GetAwaiter().GetResult();
            Assert.AreEqual(BotExecutionStatus.Completed, result.Status, result.Reason);
        }

        private static void EnsureTurn(GameplayTrainingSimulation simulation, string player)
        {
            if (!simulation.Turns.IsOwnerActive(player))
                Assert.IsTrue(simulation.Turns.TryEndTurn(simulation.Turns.ActiveOwnerId, out var reason), reason);
            Assert.IsTrue(simulation.Turns.CanOwnerAct(player, out var allowed), allowed);
        }

        private static void AdvanceRound(GameplayTrainingSimulation simulation, string player)
        {
            Assert.IsTrue(simulation.Turns.TryEndTurn(player, out var reason), reason);
            EnsureTurn(simulation, player);
        }

        private static IEnumerator MoveToSettlement(GameplayTrainingSimulation simulation, string player, string actor, Vector2Int target)
        {
            var world = simulation.Episode;
            var tried = new HashSet<Vector2Int>();
            while (true)
            {
                Assert.IsTrue(world.Units.TryGetUnitPosition(actor, out var start));
                var previous = new Dictionary<Vector2Int, Vector2Int>();
                var queue = new Queue<Vector2Int>();
                queue.Enqueue(start);
                previous[start] = start;
                Vector2Int? goal = null;
                while (queue.Count > 0 && previous.Count <= 4096)
                {
                    var from = queue.Dequeue();
                    if (Math.Max(Math.Abs(from.x - target.x), Math.Abs(from.y - target.y)) <= 1
                        && !tried.Contains(from))
                    { goal = from; break; }
                    for (int y = -1; y <= 1; y++)
                        for (int x = -1; x <= 1; x++)
                        {
                            var next = from + new Vector2Int(x, y);
                            if (previous.ContainsKey(next) || !world.Traversal.TryEvaluateStep(actor, from, next,
                                float.MaxValue, UnitTraversalMode.Pathfinding, out _, out _)) continue;
                            previous[next] = from;
                            queue.Enqueue(next);
                        }
                }
                Assert.IsTrue(goal.HasValue,
                    $"Generated world has no legal unit route to the settlement. start={start} target={target} visited={previous.Count} tried={tried.Count}");
                var path = new Stack<Vector2Int>();
                for (var cell = goal.Value; cell != start; cell = previous[cell]) path.Push(cell);
                while (path.Count > 0)
                {
                    var next = path.Pop();
                    if (!world.MovementQuery.GetMovementTiles(actor).Any(c => c.Position == next && c.IsReachable))
                        AdvanceRound(simulation, player);
                    var step = world.MovementQuery.GetMovementTiles(actor).FirstOrDefault(c => c.Position == next);
                    Assert.IsTrue(step.IsReachable, $"Production movement cannot reach {next}: {step.Reason}; stamina={world.Units.GetStamina(actor)}");
                    using var cancellation = new CancellationTokenSource();
                    var move = world.Movement.MoveUnitAsync(actor, next, cancellation.Token);
                    float deadline = Time.realtimeSinceStartup + 20;
                    while (!move.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
                    if (!move.IsCompleted) cancellation.Cancel();
                    Assert.IsTrue(move.IsCompleted, "Production movement timed out.");
                    move.GetAwaiter().GetResult();
                    Assert.IsTrue(world.Units.TryGetUnitPosition(actor, out var actual) && actual == next);
                }

                // Height-aware fog can hide an adjacent cell across a terrain
                // edge; ring the target until it enters the owner's vision.
                if (world.Fog == null || world.Fog.IsVisible(player, target))
                    yield break;
                tried.Add(goal.Value);
                Assert.Less(tried.Count, 8,
                    $"No target-adjacent cell is visible to '{player}'. target={target}");
            }
        }

        [UnityTest]
        public IEnumerator OwnedWorldMovesOpponentProgressesAndSettlementLossEndsMatchAcrossReset()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            float previousScale = Time.timeScale;
            Time.timeScale = 8;
            try
            {
                var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
                config.curriculum.stage = TrainingCurriculumStage.FullGame;
                var container = new DiContainer();
                new TrainingInstaller().Install(container, config);
                var factory = container.Resolve<ITrainingSimulationFactory>();
                using var simulation = (GameplayTrainingSimulation)factory.Create(0);
                using var environment = new TrainingEnvironment(0, config, simulation);
                GameObject oldRoot = null;
                for (int episode = 0; episode < 2; episode++)
                {
                    environment.BeginEpisode();
                    yield return null; // Destroy from the previous scope is deferred in PlayMode.
                    Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
                    if (episode > 0) Assert.IsTrue(oldRoot == null, "Previous episode objects survived reset.");
                    Assert.IsNull(simulation.Episode.FullGameSetupError);
                    Assert.AreEqual(4, simulation.Episode.Placements.GetSavedPlacements().Count);
                    Assert.AreEqual(0, environment.Rewards.TotalReward);
                    var report = TrainingReadinessValidator.Validate(factory, simulation, environment);
                    Assert.AreEqual(TrainingMechanicStatus.Ready, report.Mechanics["ECONOMY"]);
                    Assert.AreEqual(TrainingMechanicStatus.Ready, report.Mechanics["CAPTURE"]);
                    Assert.IsTrue(report.IsReady, report.ToString());

                    var movement = simulation.Capabilities.Get(BotCapabilityId.Movement);
                    var candidate = movement.Enumerate(simulation.PlayerId).FirstOrDefault();
                    Assert.IsNotNull(candidate, "No real legal movement candidate.");
                    using var cancellation = new CancellationTokenSource();
                    var move = movement.Execute(simulation.PlayerId, candidate, cancellation.Token);
                    float deadline = Time.realtimeSinceStartup + 15;
                    while (!move.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
                    if (!move.IsCompleted) cancellation.Cancel();
                    Assert.IsTrue(move.IsCompleted, "Real movement did not complete.");
                    Assert.AreEqual(BotExecutionStatus.Completed, move.GetAwaiter().GetResult().Status);

                    long turn = simulation.Turns.GlobalTurn;
                    int endTurn = Enumerable.Range(0, environment.Bridge.Frame.Candidates.Count)
                        .First(i => environment.Bridge.Frame.Candidates[i].Intent == BotIntentType.EndTurn);
                    Assert.IsTrue(environment.Step(endTurn));
                    deadline = Time.realtimeSinceStartup + 20;
                    while ((simulation.Turns.GlobalTurn < turn + 2 || !simulation.Turns.IsOwnerActive(simulation.PlayerId))
                        && Time.realtimeSinceStartup < deadline)
                    {
                        environment.Tick(Time.deltaTime);
                        yield return null;
                    }
                    Assert.GreaterOrEqual(simulation.Turns.GlobalTurn, turn + 2, "Shared heuristic opponent did not finish its turn.");
                    Assert.IsTrue(simulation.Turns.IsOwnerActive(simulation.PlayerId));

                    // Exercise the production settlement destruction -> elimination -> GameEndedSignal chain.
                    // This fixture tests the terminal trigger, not an artificial training win rule.
                    string losingOwner = episode == 0 ? TrainingGameplayScope.OpponentId : simulation.PlayerId;
                    var castle = simulation.Episode.Placements.GetSavedPlacements()
                        .Single(p => p.OwnerId == losingOwner && p.BuildingId == "castle-01");
                    Assert.IsTrue(simulation.Episode.Destruction.TryDestroyPlacedBuilding(castle.Position, "scope-sanity"));
                    environment.Tick(0);
                    Assert.AreEqual(episode == 0 ? TrainingEpisodeResult.Victory : TrainingEpisodeResult.Defeat, environment.Result);
                    oldRoot = simulation.Episode.Root;
                }
            }
            finally { Time.timeScale = previousScale; }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator FullGameCastleToEconomyKeepsEpisodeAndWorldInstance()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
            config.curriculum.stage = TrainingCurriculumStage.FullGame;
            var container = new DiContainer();
            new TrainingInstaller().Install(container, config);
            using (var simulation = (GameplayTrainingSimulation)container.Resolve<ITrainingSimulationFactory>().Create(0))
            using (var environment = new TrainingEnvironment(0, config, simulation))
            {
                environment.SetScenario(TrainingScenarioCatalog.BuiltIn().Get("full-game"));
                environment.BeginEpisode();
                Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
                long episode = environment.EpisodeId;
                GameObject world = simulation.Episode.Root;
                var tracker = environment.ScenarioProgress;
                tracker.SetSetupPhase(false, new TrainingScenarioFacts());
                tracker.ObserveReward(new TrainingRewardEvent(episode, "castle", TrainingRewardEventType.BuildingCreated,
                    "building-type:castle-01", validated: true, meaningful: true),
                    new TrainingScenarioFacts(ownedSettlements: 1, operationalCastles: 1,
                        operationalBuildingsByType: new Dictionary<string, int> { ["castle-01"] = 1 }));
                Assert.AreEqual(1, tracker.StepIndex);
                tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed,
                    new TrainingScenarioFacts(ownedSettlements: 1, operationalCastles: 1,
                        productionPerTurn: new Dictionary<string, float> { ["walnut-wood-materials-resources"] = 6 },
                        operationalBuildingsByType: new Dictionary<string, int> { ["castle-01"] = 1, ["wood-camp"] = 1 }));
                Assert.AreEqual(2, tracker.StepIndex);
                Assert.AreEqual(episode, environment.EpisodeId);
                Assert.AreSame(world, simulation.Episode.Root);
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator ComboScenarioKeepsOneWorldAcrossStepTransition()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
            config.curriculum.stage = TrainingCurriculumStage.Economy;
            var container = new DiContainer();
            new TrainingInstaller().Install(container, config);
            using (var simulation = (GameplayTrainingSimulation)container.Resolve<ITrainingSimulationFactory>().Create(0))
            using (var environment = new TrainingEnvironment(0, config, simulation))
            {
                environment.SetScenario(TrainingScenarioCatalog.BuiltIn().Get("combo-foundation"));
                environment.BeginEpisode();
                Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
                var world = simulation.Episode.Root;
                long episode = environment.EpisodeId;
                environment.ScenarioProgress.SetSetupPhase(false, new TrainingScenarioFacts());
                environment.ScenarioProgress.ObserveReward(
                    new TrainingRewardEvent(episode, "castle", TrainingRewardEventType.BuildingCreated,
                        "building-type:castle-01", validated: true, meaningful: true),
                    new TrainingScenarioFacts(ownedSettlements: 1, operationalCastles: 1,
                        operationalBuildingsByType: new Dictionary<string, int> { ["castle-01"] = 1 }));
                Assert.AreEqual(1, environment.ScenarioProgress.StepIndex);
                Assert.AreEqual(episode, environment.EpisodeId);
                Assert.AreSame(world, simulation.Episode.Root);
            }
            yield return new ExitPlayMode();
        }
    }
}
