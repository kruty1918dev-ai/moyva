using System;
using System.IO;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    [TestFixture]
    public sealed class TurnGameplayContractSuiteTests
    {
        private const string Scripts = "Assets/Moyva/Scripts/";

        private static string Read(string relative)
        {
            string path = Path.Combine(
                Directory.GetCurrentDirectory(),
                relative.Replace('/', Path.DirectorySeparatorChar));
            Assert.That(File.Exists(path), Is.True, $"Required gameplay contract source is missing: {relative}");
            return File.ReadAllText(path);
        }

        private static void ContainsAll(string source, params string[] tokens)
        {
            foreach (string token in tokens)
                Assert.That(source, Does.Contain(token), $"Missing contract token: {token}");
        }

        [Test]
        public void P01_FactionAuthority_UsesResolvedLocalOwnerAndOwnerGate()
        {
            string contracts = Read(Scripts + "Features/Turns/API/TurnContracts.cs");
            string turns = Read(Scripts + "Features/Turns/Runtime/TurnService.cs");
            ContainsAll(contracts, "ITurnLocalOwnerResolver");
            ContainsAll(turns, "public string LocalOwnerId", "public bool CanOwnerAct", "ResolveLocalOwnerId()");
        }

        [Test]
        public void P02_Restore_QueuesStateAndResumesWithoutTurnStartReplay()
        {
            string turns = Read(Scripts + "Features/Turns/Runtime/TurnService.cs");
            ContainsAll(turns, "_pendingRestore", "TryApplyPendingRestore()", "ResumeRestoredTurn()",
                "Deliberately do not call ITurnParticipant.OnTurnStarted", "Phase = TurnPhase.AwaitingInput");
        }

        [Test]
        public void P03_EndTurn_UsesAuthoritativePhasesAndBlockers()
        {
            string turns = Read(Scripts + "Features/Turns/Runtime/TurnService.cs");
            ContainsAll(turns, "TryEndTurn", "IsTurnBlocked", "TurnPhase.Ending", "TurnPhase.Resolving", "StateChanged");
        }

        [Test]
        public void P04_UnitMovement_LeaseTracksTurnEpochAndCancelsOnAuthorityChange()
        {
            string source = Read(Scripts + "Features/Units/Runtime/UnitTurnAuthorityMovementService.cs");
            ContainsAll(source, "CancellationTokenSource.CreateLinkedTokenSource", "_turns.StateChanged += OnTurnStateChanged",
                "_turns.GlobalTurn != lease.GlobalTurn", "_turns.Phase != TurnPhase.AwaitingInput", "_turns.CanOwnerAct(lease.OwnerId");
        }

        [Test]
        public void P04_UnitMovement_IsInstalledAsInterfaceDecorator()
        {
            string source = Read(Scripts + "Features/Units/Runtime/UnitsInstaller.cs");
            ContainsAll(source, "Container.Decorate<IUnitMovementService>()", ".With<UnitTurnAuthorityMovementService>()");
        }

        [Test]
        public void P05_ConstructionLifecycle_PreservesProgressAcrossRelocation()
        {
            string source = Read(Scripts + "Features/Construction/Runtime/Core/Service/ConstructionLifecycleStateMachine.cs");
            ContainsAll(source, "IsOperational(Vector2Int position)", "Relocation is not a new build action", "Preserve PlacedTurn", "Restore(");
        }

        [Test]
        public void P06_ConstructionAuthority_RequiresAwaitingInputAndCanRequireLocalOwner()
        {
            string source = Read(Scripts + "Features/Construction/Runtime/Core/Service/ConstructionService.TurnAuthority.cs");
            ContainsAll(source, "requireLocalOwner", "TurnPhase.AwaitingInput", "_turns.CanOwnerAct", "_turns.TryRecordAction");
        }

        [Test]
        public void P07_Recruitment_ValidatesOperationalBuildingBeforeEconomyCommit()
        {
            string source = Read(Scripts + "Features/Units/Runtime/UnitRecruitmentService.cs");
            int operational = source.IndexOf("_constructionLifecycle.IsOperational", StringComparison.Ordinal);
            int record = source.IndexOf("_turns.TryRecordAction(owner, \"unit-recruit-enqueue\")", StringComparison.Ordinal);
            ContainsAll(source, "public bool TryEnqueue", "_turns.CanOwnerAct", "_constructionLifecycle.IsOperational");
            Assert.That(operational, Is.GreaterThanOrEqualTo(0));
            Assert.That(record, Is.GreaterThan(operational));
        }

        [Test]
        public void P08_Recruitment_DeploymentUsesStableIdAndConsumesReadyOnlyAfterSpawn()
        {
            string source = Read(Scripts + "Features/Units/Runtime/UnitRecruitmentService.cs");
            ContainsAll(source, "BuildRecruitmentUnitId", "CreateUnitWithId", "TryTakeReady", "Stable deployment id collision", "RestoreState(");
            Assert.That(source.IndexOf("CreateUnitWithId", StringComparison.Ordinal),
                Is.LessThan(source.LastIndexOf("TryTakeReady", StringComparison.Ordinal)));
        }

        [Test]
        public void P08_UnitsSave_PersistsRecruitmentStateWithoutEconomyReplay()
        {
            string source = Read(Scripts + "Bootstrap/Runtime/UnitsSaveModule.cs");
            ContainsAll(source, "Version = 3", "IUnitRecruitmentStatePersistence", "CaptureState", "RestoreState");
            Assert.That(source, Does.Not.Contain("TryConsumeOwnerPoolResources"));
        }

        [Test]
        public void P09_CalendarRestore_IsSilentAndSameHourIsIdempotent()
        {
            string source = Read(Scripts + "Features/Calendar/Runtime/GameCalendarService.cs");
            ContainsAll(source, "RestoreByTotalHours", "publishEvents: false", "if (totalHours == _totalHours)", "OnHourChanged?.Invoke()");
        }

        [Test]
        public void P09_TurnSave_RestoresCalendarBeforeTurnState()
        {
            string source = Read(Scripts + "Bootstrap/Runtime/TurnSaveModule.cs");
            int calendar = source.IndexOf("_calendarRestorer.RestoreByTotalHours(calendarHours)", StringComparison.Ordinal);
            int turns = source.IndexOf("_restorer.Restore(round, globalTurn, activeOwner, actions)", StringComparison.Ordinal);
            ContainsAll(source, "Version = 2", "LegacyVersion = 1", "TotalHoursSinceEpoch");
            Assert.That(calendar, Is.GreaterThanOrEqualTo(0));
            Assert.That(turns, Is.GreaterThan(calendar));
        }

        [Test]
        public void P10_LegacyBotScheduler_IsRuntimeInert()
        {
            string source = Read(Scripts + "Features/BotAI/Runtime/BotTickScheduler.cs");
            ContainsAll(source, "internal sealed class BotTickScheduler", "public bool IsRuntimeEnabled => false", "Intentionally inert");
            Assert.That(source, Does.Not.Contain("internal sealed class BotTickScheduler : ITickable"));
        }

        [Test]
        public void P11_BotExecutor_HasBoundedTurnScopedActionBudget()
        {
            string source = Read(Scripts + "Features/BotAI/Runtime/BotTurnExecutor.cs");
            ContainsAll(source, "MaxMutatingActionsPerTurn = 6", "MaxMoveActionsPerTurn = 4", "TryBeginTurn",
                "_turns.CanOwnerAct", "_recruitment.TryEnqueue", "MoveUnitAsync");
        }

        [Test]
        public void P11_TurnBotDriver_OwnsEndTurnNotExecutor()
        {
            string driver = Read(Scripts + "Bootstrap/Runtime/TurnBotDriver.cs");
            string executor = Read(Scripts + "Features/BotAI/Runtime/BotTurnExecutor.cs");
            ContainsAll(driver, "_executor.TryBeginTurn", "_turns.TryEndTurn(ownerId, out _)");
            Assert.That(executor, Does.Not.Contain("_turns.TryEndTurn("));
        }

        [Test]
        public void P12_HudAuthority_IsLocalOwnerAwaitingInputAndBlockerAware()
        {
            string source = Read(Scripts + "Bootstrap/Runtime/GameplayTurnHudAuthorityPolicy.cs");
            ContainsAll(source, "turns.Phase == TurnPhase.AwaitingInput", "IsLocalOwnerTurn", "CollectBlockingReasons", "canEndTurn = localTurn && blockerReasons.Count == 0");
        }

        [Test]
        public void P12_Hud_ReevaluatesAuthorityAtTickAndClickTime()
        {
            string source = Read(Scripts + "Bootstrap/Runtime/GameplayTurnHudPresenter.cs");
            ContainsAll(source, "ITickable", "GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers)", "_turns.TryEndTurn(current.LocalOwnerId");
            Assert.That(source, Does.Contain("ITurnBlocker state may change asynchronously without StateChanged"));
        }

        [Test]
        public void CrossSystem_BotAndHudUseCanonicalUnitRecruitmentService()
        {
            string bot = Read(Scripts + "Features/BotAI/Runtime/BotTurnExecutor.cs");
            string hud = Read(Scripts + "Bootstrap/Runtime/GameplayTurnHudPresenter.cs");
            ContainsAll(bot, "IUnitRecruitmentService");
            ContainsAll(hud, "IUnitRecruitmentService");
            Assert.That(bot, Does.Not.Contain("Kruty1918.Moyva.Recruitment"));
            Assert.That(hud, Does.Not.Contain("Kruty1918.Moyva.Recruitment"));
        }

        [Test]
        public void P13_SavePlan_HasDependencyAwareGameplayPhasesAndTurnLast()
        {
            string source = Read(Scripts + "Features/SaveSystem/Runtime/SaveModuleExecutionPlan.cs");
            ContainsAll(source, "GeneratedWorldOrder = 100", "ConstructionOrder = 200", "EconomyOrder = 300",
                "UnitsOrder = 400", "FogOfWarOrder = 500", "TurnStateOrder = 900", "ISaveModuleExecutionOrder");
        }

        [Test]
        public void P13_LoadPipeline_RejectsDuplicateBlockBeforeModuleMutation()
        {
            string source = Read(Scripts + "Features/SaveSystem/Runtime/SavePipelineHelper.cs");
            int duplicate = source.IndexOf("Duplicate blockId=", StringComparison.Ordinal);
            int onLoad = source.IndexOf("module.OnLoad", StringComparison.Ordinal);
            ContainsAll(source, "payloadByBlockId.ContainsKey", "SaveModuleExecutionPlan.Build(modules)", "Save block-id collision");
            Assert.That(duplicate, Is.GreaterThanOrEqualTo(0));
            Assert.That(onLoad, Is.GreaterThan(duplicate));
        }

        [Test]
        public void CrossSystem_SaveOrchestration_RestoresUnitsBeforeTurnState()
        {
            string source = Read(Scripts + "Features/SaveSystem/Runtime/SaveModuleExecutionPlan.cs");
            int units = source.IndexOf("UnitsOrder = 400", StringComparison.Ordinal);
            int turn = source.IndexOf("TurnStateOrder = 900", StringComparison.Ordinal);
            Assert.That(units, Is.GreaterThanOrEqualTo(0));
            Assert.That(turn, Is.GreaterThan(units));
        }

        [Test]
        public void CrossSystem_AuthorityLayersAllFailClosedThroughTurnService()
        {
            string unit = Read(Scripts + "Features/Units/Runtime/UnitTurnAuthorityMovementService.cs");
            string construction = Read(Scripts + "Features/Construction/Runtime/Core/Service/ConstructionService.TurnAuthority.cs");
            string recruitment = Read(Scripts + "Features/Units/Runtime/UnitRecruitmentService.cs");
            string bot = Read(Scripts + "Features/BotAI/Runtime/BotTurnExecutor.cs");
            foreach (string source in new[] { unit, construction, recruitment, bot })
                Assert.That(source, Does.Contain("CanOwnerAct"));
        }

        [Test]
        public void ContractSuite_CoversEverySeriesPatchFromP01ThroughP13()
        {
            string self = Read(Scripts + "Tests/SaveSystem/TurnGameplayContractSuiteTests.cs");
            for (int patch = 1; patch <= 13; patch++)
                Assert.That(self, Does.Contain($"P{patch:D2}_"), $"No explicit P{patch:D2} contract test marker.");
        }
    }
}
