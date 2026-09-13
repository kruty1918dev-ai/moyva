using System;
using System.Collections;
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
                    Assert.AreEqual(TrainingMechanicStatus.Blocked, report.Mechanics["CAPTURE"]);
                    Assert.IsFalse(report.IsReady, "Raw capture transfer must not count as a guarded player action.");

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
    }
}
