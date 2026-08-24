using System;
using System.IO;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    [TestFixture]
    public sealed class TurnGameplayFinalAuditTests
    {
        private const string Scripts = "Assets/Moyva/Scripts/";

        private static string Read(string relative)
        {
            string path = Path.Combine(
                Directory.GetCurrentDirectory(),
                relative.Replace('/', Path.DirectorySeparatorChar));
            Assert.That(File.Exists(path), Is.True,
                $"Final-audit source missing: {relative}");
            return File.ReadAllText(path);
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        [Test]
        public void P15_LegacyRecruitmentBootstrapActivation_IsRemoved()
        {
            string source = Read(Scripts + "Bootstrap/Runtime/BootstrapInstaller.cs");
            Assert.That(source, Does.Not.Contain("RecruitmentBindings.Install"));
            Assert.That(source, Does.Not.Contain("Kruty1918.Moyva.Recruitment"));
            Assert.That(source, Does.Contain("GameplayTurnHudPresenter"));
            Assert.That(source, Does.Contain("TurnBotDriver"));
        }

        [Test]
        public void P15_BootstrapAssembly_DropsLegacyRecruitmentDependency()
        {
            string source = Read(Scripts + "Bootstrap/Kruty1918.Moyva.Bootstrap.asmdef");
            Assert.That(source, Does.Not.Contain("Kruty1918.Moyva.Recruitment"));
            Assert.That(source, Does.Contain("Kruty1918.Moyva.Units"));
            Assert.That(source, Does.Contain("Kruty1918.Moyva.BotAI"));
            Assert.That(source, Does.Contain("Kruty1918.Moyva.Turns"));
        }

        [Test]
        public void P15_CanonicalRecruitment_IsBoundExactlyOnceByUnitsInstaller()
        {
            string source = Read(Scripts + "Features/Units/Runtime/UnitsInstaller.cs");
            Assert.That(source, Does.Contain("BindInterfacesAndSelfTo<UnitRecruitmentService>()"));
            Assert.That(Count(source, "BindInterfacesAndSelfTo<UnitRecruitmentService>()"), Is.EqualTo(1));
        }

        [Test]
        public void P15_LegacyRecruitmentFeature_IsRemoved()
        {
            string legacyDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                Scripts.Replace('/', Path.DirectorySeparatorChar),
                "Features",
                "Recruitment");
            Assert.That(Directory.Exists(legacyDirectory), Is.False);

            string units = Read(Scripts + "Features/Units/Runtime/UnitsInstaller.cs");
            Assert.That(units, Does.Contain("BindInterfacesAndSelfTo<UnitRecruitmentService>()"));
        }

        [Test]
        public void P15_LegacyBotScheduler_IsRemoved()
        {
            string schedulerPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                Scripts.Replace('/', Path.DirectorySeparatorChar),
                "Features",
                "BotAI",
                "Runtime",
                "BotTickScheduler.cs");
            Assert.That(File.Exists(schedulerPath), Is.False);

            string bindings = Read(Scripts + "Features/BotAI/Runtime/BotRuntimeBindings.cs");
            Assert.That(bindings, Does.Not.Contain("BindInterfacesAndSelfTo<BotTickScheduler>"));
            Assert.That(bindings, Does.Not.Contain("BindInterfacesTo<BotTickScheduler>"));
        }

        [Test]
        public void P15_BotEndTurnBoundary_HasSingleOwnerInDriver()
        {
            string driver = Read(Scripts + "Bootstrap/Runtime/TurnBotDriver.cs");
            string executor = Read(Scripts + "Features/BotAI/Runtime/BotTurnExecutor.cs");
            Assert.That(driver, Does.Contain("_executor.TryBeginTurn"));
            Assert.That(driver, Does.Contain("_turns.TryEndTurn(ownerId, out _)"));
            Assert.That(executor, Does.Not.Contain("TryEndTurn("));
        }

        [Test]
        public void P15_UnitRuntime_DoesNotAdvanceGameplayCalendar()
        {
            string unit = Read(Scripts + "Features/Units/Runtime/UnitService.cs");
            string movement = Read(Scripts + "Features/Units/Runtime/UnitMovementService.cs");
            Assert.That(unit, Does.Not.Contain("AdvanceTurn("));
            Assert.That(movement, Does.Not.Contain("AdvanceTurn("));
            Assert.That(unit, Does.Not.Contain("OnHourChanged"));
            Assert.That(movement, Does.Not.Contain("OnHourChanged"));
        }

        [Test]
        public void P15_CalendarRestore_RemainsSilentAndTurnSaveRestoresItBeforeTurnState()
        {
            string calendar = Read(Scripts + "Features/Calendar/Runtime/GameCalendarService.cs");
            string save = Read(Scripts + "Bootstrap/Runtime/TurnSaveModule.cs");
            Assert.That(calendar, Does.Contain("RestoreByTotalHours"));
            Assert.That(calendar, Does.Contain("publishEvents: false"));
            int calendarRestore = save.IndexOf(
                "_calendarRestorer.RestoreByTotalHours(calendarHours)",
                StringComparison.Ordinal);
            int turnRestore = save.IndexOf(
                "_restorer.Restore(round, globalTurn, activeOwner, actions)",
                StringComparison.Ordinal);
            Assert.That(calendarRestore, Is.GreaterThanOrEqualTo(0));
            Assert.That(turnRestore, Is.GreaterThan(calendarRestore));
        }

        [Test]
        public void P15_RecruitmentRestore_DoesNotReplayEconomyCommit()
        {
            string recruitment = Read(Scripts + "Features/Units/Runtime/UnitRecruitmentService.cs");
            string unitsSave = Read(Scripts + "Bootstrap/Runtime/UnitsSaveModule.cs");
            Assert.That(recruitment, Does.Contain("RestoreState("));
            Assert.That(unitsSave, Does.Contain("RestoreState"));
            Assert.That(unitsSave, Does.Not.Contain("TryConsumeOwnerPoolResources"));
        }

        [Test]
        public void P15_HudAuthority_RemainsLocalOwnerPhaseAndBlockerGated()
        {
            string policy = Read(Scripts + "Bootstrap/Runtime/GameplayTurnHudAuthorityPolicy.cs");
            string presenter = Read(Scripts + "Bootstrap/Runtime/GameplayTurnHudPresenter.cs");
            Assert.That(policy, Does.Contain("TurnPhase.AwaitingInput"));
            Assert.That(policy, Does.Contain("IsLocalOwnerTurn"));
            Assert.That(policy, Does.Contain("CollectBlockingReasons"));
            Assert.That(presenter, Does.Contain("GameplayTurnHudAuthorityPolicy.Evaluate"));
            Assert.That(presenter, Does.Contain("_turns.TryEndTurn(current.LocalOwnerId"));
        }

        [Test]
        public void P15_AllMutationAuthorities_StillRouteThroughTurnService()
        {
            string movement = Read(Scripts + "Features/Units/Runtime/UnitTurnAuthorityMovementService.cs");
            string construction = Read(Scripts + "Features/Construction/Runtime/Core/Service/ConstructionService.TurnAuthority.cs");
            string recruitment = Read(Scripts + "Features/Units/Runtime/UnitRecruitmentService.cs");
            string bot = Read(Scripts + "Features/BotAI/Runtime/BotTurnExecutor.cs");
            foreach (string source in new[] { movement, construction, recruitment, bot })
                Assert.That(source, Does.Contain("CanOwnerAct"));
        }

        [Test]
        public void P15_SaveLoadPlan_RestoresTurnStateAfterGameplayWorldState()
        {
            string plan = Read(Scripts + "Features/SaveSystem/Runtime/SaveModuleExecutionPlan.cs");
            Assert.That(plan, Does.Contain("GeneratedWorldOrder = 100"));
            Assert.That(plan, Does.Contain("ConstructionOrder = 200"));
            Assert.That(plan, Does.Contain("EconomyOrder = 300"));
            Assert.That(plan, Does.Contain("UnitsOrder = 400"));
            Assert.That(plan, Does.Contain("FogOfWarOrder = 500"));
            Assert.That(plan, Does.Contain("TurnStateOrder = 900"));
        }

        [Test]
        public void P15_SaveLoadPipeline_RejectsDuplicateBlocksBeforeOnLoadMutation()
        {
            string source = Read(Scripts + "Features/SaveSystem/Runtime/SavePipelineHelper.cs");
            int duplicate = source.IndexOf("Duplicate blockId=", StringComparison.Ordinal);
            int load = source.IndexOf("module.OnLoad", StringComparison.Ordinal);
            Assert.That(duplicate, Is.GreaterThanOrEqualTo(0));
            Assert.That(load, Is.GreaterThan(duplicate));
        }

        [Test]
        public void P15_P14ContractSuite_IsPresentAndLocksP01ThroughP13()
        {
            string suite = Read(Scripts + "Tests/SaveSystem/TurnGameplayContractSuiteTests.cs");
            for (int patch = 1; patch <= 13; patch++)
                Assert.That(suite, Does.Contain($"P{patch:D2}_"), $"Missing P{patch:D2} contract marker.");
            Assert.That(Count(suite, "        [Test]"), Is.GreaterThanOrEqualTo(20));
        }

        [Test]
        public void P15_UnitMovementRangeQuery_HasSingleRuntimeOwner()
        {
            string movement = Read(Scripts + "Features/Units/Runtime/UnitMovementService.cs");
            string range = Read(Scripts + "Features/Units/Runtime/UnitMovementRangeQuery.cs");
            string installer = Read(Scripts + "Features/Units/Runtime/UnitsInstaller.cs");

            Assert.That(movement, Does.Not.Contain("IUnitMovementQuery"));
            Assert.That(movement, Does.Not.Contain("GetMovementTiles("));
            Assert.That(range, Does.Contain("IUnitMovementQuery"));
            Assert.That(range, Does.Contain("GetMovementTiles("));
            Assert.That(installer, Does.Contain("BindInterfacesAndSelfTo<UnitMovementRangeQuery>()"));
        }

        [Test]
        public void P15_RecruitmentDeployment_UsesCanonicalPlacementValidatorOnly()
        {
            string recruitment = Read(Scripts + "Features/Units/Runtime/UnitRecruitmentService.cs");
            Assert.That(recruitment, Does.Contain("_placementValidator.CanDeployUnit"));
            Assert.That(recruitment, Does.Not.Contain("_grid.ContainsCell"));
            Assert.That(recruitment, Does.Not.Contain("_objectsMap.IsOccupied"));
        }

        [Test]
        public void P15_FinalAuditSuite_DeclaresFinalSeriesMarker()
        {
            string self = Read(Scripts + "Tests/SaveSystem/TurnGameplayFinalAuditTests.cs");
            Assert.That(self, Does.Contain("P15_"));
            Assert.That(Count(self, "        [Test]"), Is.GreaterThanOrEqualTo(15));
        }
    }
}
