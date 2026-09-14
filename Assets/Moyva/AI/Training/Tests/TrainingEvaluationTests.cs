using System;
using System.IO;
using NUnit.Framework;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public sealed class TrainingEvaluationTests
    {
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "MoyvaTrainingEvaluationTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        private TrainingCurriculumController Controller(int evaluationEpisodes = 50)
        {
            return new TrainingCurriculumController(new AutonomousTrainingConfig
            {
                enabled = true,
                evaluationEverySteps = 10000,
                evaluationEpisodes = evaluationEpisodes,
                masteryThreshold = 0.80f,
                regressionThreshold = 0.65f,
                masteryChecksRequired = 3,
                statePath = Path.Combine(_directory, "state-" + Guid.NewGuid().ToString("N") + ".json")
            }, 1918);
        }

        [Test]
        public void SeventyNinePercentDoesNotGiveMastery()
        {
            using var controller = Controller(100);
            Assert.IsFalse(controller.RecordEvaluation("castle", 79, 100, 10000));
            var skill = controller.GetSkill("castle");
            Assert.IsFalse(skill.mastered);
            Assert.AreEqual(0, skill.consecutivePasses);
        }

        [Test]
        public void EightyPercentGivesOneConsecutivePass()
        {
            using var controller = Controller();
            controller.RecordEvaluation("castle", 40, 50, 10000);
            var skill = controller.GetSkill("castle");
            Assert.IsFalse(skill.mastered);
            Assert.AreEqual(1, skill.consecutivePasses);
        }

        [Test]
        public void ThreeMasteryEvaluationsMarkSkillMastered()
        {
            using var controller = Controller();
            controller.RecordEvaluation("castle", 40, 50, 10000);
            controller.RecordEvaluation("castle", 45, 50, 20000);
            Assert.IsTrue(controller.RecordEvaluation("castle", 50, 50, 30000));
            Assert.IsTrue(controller.GetSkill("castle").mastered);
            Assert.AreEqual(3, controller.GetSkill("castle").consecutivePasses);
        }

        [Test]
        public void SixtyFourPercentRegressesMasteredSkill()
        {
            using var controller = Controller();
            controller.RecordEvaluation("castle", 50, 50, 10000);
            controller.RecordEvaluation("castle", 50, 50, 20000);
            controller.RecordEvaluation("castle", 50, 50, 30000);
            Assert.IsTrue(controller.GetSkill("castle").mastered);
            Assert.IsTrue(controller.RecordEvaluation("castle", 32, 50, 40000));
            Assert.IsFalse(controller.GetSkill("castle").mastered);
            Assert.AreEqual(0, controller.GetSkill("castle").consecutivePasses);
        }

        [Test]
        public void SeventyPercentKeepsMasteredButResetsPasses()
        {
            using var controller = Controller();
            controller.RecordEvaluation("castle", 50, 50, 10000);
            controller.RecordEvaluation("castle", 50, 50, 20000);
            controller.RecordEvaluation("castle", 50, 50, 30000);
            Assert.IsTrue(controller.GetSkill("castle").mastered);
            Assert.IsFalse(controller.RecordEvaluation("castle", 35, 50, 40000));
            Assert.IsTrue(controller.GetSkill("castle").mastered);
            Assert.AreEqual(0, controller.GetSkill("castle").consecutivePasses);
        }

        [Test]
        public void TrainingEpisodeDoesNotAdvanceMastery()
        {
            using var controller = Controller();
            for (int i = 0; i < 10; i++) controller.RecordTrainingEpisode("castle", true, 1000);
            var skill = controller.GetSkill("castle");
            Assert.IsTrue(controller.EvaluationDue);
            Assert.IsFalse(skill.mastered);
            Assert.AreEqual(0, skill.consecutivePasses);
            Assert.AreEqual(0, skill.evaluationEpisodes);
        }

        [Test]
        public void InterruptedEvaluationDoesNotChangeMastery()
        {
            using var controller = Controller();
            controller.RecordEvaluation("castle", 50, 50, 10000);
            controller.RecordEvaluation("castle", 50, 50, 20000);
            controller.RecordEvaluation("castle", 50, 50, 30000);
            var before = controller.GetSkill("castle");
            Assert.IsTrue(before.mastered);
            Assert.AreEqual(3, before.consecutivePasses);
            Assert.IsFalse(controller.RecordEvaluation("castle", 17, 17, 40000));
            var after = controller.GetSkill("castle");
            Assert.IsTrue(after.mastered);
            Assert.AreEqual(3, after.consecutivePasses);
            Assert.AreEqual(50, after.evaluationEpisodes);
            Assert.AreEqual(50, after.evaluationSuccesses);
        }

        [Test]
        public void RetriedCompletedEvaluationIsIdempotent()
        {
            using var controller = Controller();
            const string evaluationId = "checkpoint|castle|1";
            controller.RecordEvaluation("castle", 40, 50, 10000, "checkpoint", evaluationId);
            Assert.AreEqual(1, controller.GetSkill("castle").consecutivePasses);
            controller.RecordEvaluation("castle", 40, 50, 10000, "checkpoint", evaluationId);
            Assert.AreEqual(1, controller.GetSkill("castle").consecutivePasses);
        }

        [Test]
        public void EvaluationDoesNotIncreaseTrainingDecisionCounter()
        {
            using var controller = Controller();
            controller.RecordTrainingEpisode("castle", true, 1234);
            long before = controller.State.totalDecisions;
            controller.RecordEvaluation("castle", 40, 50, 10000);
            Assert.AreEqual(before, controller.State.totalDecisions);
        }
    }
}
