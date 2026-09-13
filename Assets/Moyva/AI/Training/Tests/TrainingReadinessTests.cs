using System;
using NUnit.Framework;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public sealed class TrainingReadinessTests
    {
        [Test]
        public void MissingGameplayScopeFailsInsteadOfReturningScaffold()
        {
            var factory = new GameplayTrainingSimulationFactory();
            Assert.IsFalse(factory.SupportsIndependentEnvironments);
            StringAssert.Contains("GAMEPLAY_SCOPE_BLOCKED", Assert.Throws<InvalidOperationException>(() => factory.Create(0)).Message);
        }
        [Test]
        public void ReadyScaffoldCannotPassRealReadiness()
        {
            var factory = new ScaffoldSimulationFactory();
            using var simulation = factory.Create(0);
            simulation.Reset(new TrainingResetContext(0, 1, 1918, TrainingCurriculumStage.BasicLifecycle));
            var report = TrainingReadinessValidator.Validate(factory, simulation, null);
            Assert.AreEqual("NOT_READY_FOR_REAL_TRAINING", report.Verdict);
            CollectionAssert.Contains(report.Blockers, "REAL_PERCEPTION_BLOCKED");
            CollectionAssert.Contains(report.Blockers, "TERMINAL_OUTCOME_BLOCKED");
        }
        [Test]
        public void ExplicitVisualCliOverridesBatchDefaultWithoutChangingAuthoredConfig()
        {
            var config = new TrainingConfig();
            Assert.AreEqual(TrainingPresentationMode.Visual,
                TrainingPresentationModeResolver.Resolve(config, true, new[] { "-moyvaTrainingMode", "Visual" }));
            Assert.AreEqual(TrainingPresentationMode.Visual, config.presentationMode);
            Assert.IsFalse(config.allowScaffoldSimulation);
        }
    }
}
