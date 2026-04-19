using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using Zenject;

namespace Kruty1918.Moyva.Tests.GameMode
{
    [TestFixture]
    public sealed class GameStateServiceTests : ZenjectUnitTestFixture
    {
        private IGameStateService _service;
        private SignalBus _signalBus;
        private int _startedCount;
        private int _pausedCount;
        private int _endedCount;
        private GamePausedSignal _lastPausedSignal;
        private GameEndedSignal _lastEndedSignal;

        [SetUp]
        public void SetUp()
        {
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameStartedSignal>();
            Container.DeclareSignal<GamePausedSignal>();
            Container.DeclareSignal<GameEndedSignal>();

            var type = typeof(IGameStateService).Assembly
                .GetType("Kruty1918.Moyva.GameMode.Runtime.GameStateService");
            Container.BindInterfacesAndSelfTo(type).AsSingle();
            Container.ResolveRoots();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<IGameStateService>();

            _startedCount = 0;
            _pausedCount = 0;
            _endedCount = 0;

            _signalBus.Subscribe<GameStartedSignal>(() => _startedCount++);
            _signalBus.Subscribe<GamePausedSignal>(s => { _pausedCount++; _lastPausedSignal = s; });
            _signalBus.Subscribe<GameEndedSignal>(s => { _endedCount++; _lastEndedSignal = s; });
        }

        // --- Initial State ---
        [Test]
        public void InitialState_IsIdle()
        {
            Assert.AreEqual(GameStateType.Idle, _service.CurrentState);
        }

        // --- StartGame ---
        [Test]
        public void StartGame_SetsStatePlaying()
        {
            _service.StartGame();
            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
        }

        [Test]
        public void StartGame_FiresGameStartedSignal()
        {
            _service.StartGame();
            Assert.AreEqual(1, _startedCount);
        }

        [Test]
        public void StartGame_CalledTwice_FiresTwice()
        {
            _service.StartGame();
            _service.StartGame();
            Assert.AreEqual(2, _startedCount);
        }

        // --- PauseGame ---
        [Test]
        public void PauseGame_WhenPlaying_SetsPaused()
        {
            _service.StartGame();
            _service.PauseGame();
            Assert.AreEqual(GameStateType.Paused, _service.CurrentState);
        }

        [Test]
        public void PauseGame_WhenPlaying_FiresPausedSignal()
        {
            _service.StartGame();
            _service.PauseGame();
            Assert.AreEqual(1, _pausedCount);
            Assert.IsTrue(_lastPausedSignal.IsPaused);
        }

        [Test]
        public void PauseGame_WhenIdle_DoesNothing()
        {
            _service.PauseGame();
            Assert.AreEqual(GameStateType.Idle, _service.CurrentState);
            Assert.AreEqual(0, _pausedCount);
        }

        [Test]
        public void PauseGame_WhenAlreadyPaused_DoesNothing()
        {
            _service.StartGame();
            _service.PauseGame();
            _service.PauseGame();
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void PauseGame_WhenGameOver_DoesNothing()
        {
            _service.StartGame();
            _service.EndGame("w");
            _service.PauseGame();
            Assert.AreEqual(GameStateType.GameOver, _service.CurrentState);
            Assert.AreEqual(0, _pausedCount);
        }

        // --- ResumeGame ---
        [Test]
        public void ResumeGame_WhenPaused_SetsPlaying()
        {
            _service.StartGame();
            _service.PauseGame();
            _service.ResumeGame();
            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
        }

        [Test]
        public void ResumeGame_WhenPaused_FiresPausedSignalWithFalse()
        {
            _service.StartGame();
            _service.PauseGame();
            _service.ResumeGame();
            Assert.AreEqual(2, _pausedCount);
            Assert.IsFalse(_lastPausedSignal.IsPaused);
        }

        [Test]
        public void ResumeGame_WhenPlaying_DoesNothing()
        {
            _service.StartGame();
            _service.ResumeGame();
            Assert.AreEqual(0, _pausedCount);
        }

        [Test]
        public void ResumeGame_WhenIdle_DoesNothing()
        {
            _service.ResumeGame();
            Assert.AreEqual(GameStateType.Idle, _service.CurrentState);
        }

        [Test]
        public void ResumeGame_WhenGameOver_DoesNothing()
        {
            _service.StartGame();
            _service.EndGame("w");
            _service.ResumeGame();
            Assert.AreEqual(GameStateType.GameOver, _service.CurrentState);
        }

        // --- EndGame ---
        [Test]
        public void EndGame_SetsGameOver()
        {
            _service.StartGame();
            _service.EndGame("winner");
            Assert.AreEqual(GameStateType.GameOver, _service.CurrentState);
        }

        [Test]
        public void EndGame_FiresGameEndedSignal()
        {
            _service.StartGame();
            _service.EndGame("winner");
            Assert.AreEqual(1, _endedCount);
            Assert.AreEqual("winner", _lastEndedSignal.WinnerId);
        }

        [Test]
        public void EndGame_NullWinner_StillFires()
        {
            _service.StartGame();
            _service.EndGame(null);
            Assert.AreEqual(1, _endedCount);
            Assert.IsNull(_lastEndedSignal.WinnerId);
        }

        // --- Complex FSM Flows ---
        [Test]
        public void PauseResumeCycle_MaintainsPlaying()
        {
            _service.StartGame();
            _service.PauseGame();
            _service.ResumeGame();
            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
        }

        [Test]
        public void FullFlow_Start_Pause_Resume_End()
        {
            _service.StartGame();
            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
            _service.PauseGame();
            Assert.AreEqual(GameStateType.Paused, _service.CurrentState);
            _service.ResumeGame();
            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
            _service.EndGame("p0");
            Assert.AreEqual(GameStateType.GameOver, _service.CurrentState);
        }

        [Test]
        public void EndGame_FromPaused_SetsGameOver()
        {
            _service.StartGame();
            _service.PauseGame();
            _service.EndGame("p0");
            Assert.AreEqual(GameStateType.GameOver, _service.CurrentState);
        }

        [Test]
        public void EndGame_FromIdle_SetsGameOver()
        {
            _service.EndGame("p0");
            Assert.AreEqual(GameStateType.GameOver, _service.CurrentState);
        }

        [Test]
        public void MultipleEndCalls_FiresMultipleTimes()
        {
            _service.StartGame();
            _service.EndGame("a");
            _service.EndGame("b");
            Assert.AreEqual(2, _endedCount);
        }
    }

    [TestFixture]
    public sealed class WinConditionSOTests
    {
        [Test]
        public void DefaultCondition_IsEliminateAllEnemies()
        {
            var so = UnityEngine.ScriptableObject.CreateInstance<WinConditionSO>();
            Assert.AreEqual(WinConditionSO.ConditionType.EliminateAllEnemies, so.Condition);
        }

        [Test]
        public void DefaultControlPoints_IsThree()
        {
            var so = UnityEngine.ScriptableObject.CreateInstance<WinConditionSO>();
            Assert.AreEqual(3, so.ControlPointsRequired);
        }

        [Test]
        public void DefaultSurvivalTime_Is300()
        {
            var so = UnityEngine.ScriptableObject.CreateInstance<WinConditionSO>();
            Assert.AreEqual(300f, so.SurvivalTimeSeconds);
        }

        [Test]
        public void SetCondition_ViaSerializedObject()
        {
            var so = UnityEngine.ScriptableObject.CreateInstance<WinConditionSO>();
            var serialized = new UnityEditor.SerializedObject(so);
            serialized.FindProperty("_condition").enumValueIndex = (int)WinConditionSO.ConditionType.Survival;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(WinConditionSO.ConditionType.Survival, so.Condition);
        }

        [Test]
        public void SetControlPoints_ViaSerializedObject()
        {
            var so = UnityEngine.ScriptableObject.CreateInstance<WinConditionSO>();
            var serialized = new UnityEditor.SerializedObject(so);
            serialized.FindProperty("_controlPointsRequired").intValue = 5;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(5, so.ControlPointsRequired);
        }

        [Test]
        public void SetSurvivalTime_ViaSerializedObject()
        {
            var so = UnityEngine.ScriptableObject.CreateInstance<WinConditionSO>();
            var serialized = new UnityEditor.SerializedObject(so);
            serialized.FindProperty("_survivalTimeSeconds").floatValue = 600f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(600f, so.SurvivalTimeSeconds);
        }
    }
}
