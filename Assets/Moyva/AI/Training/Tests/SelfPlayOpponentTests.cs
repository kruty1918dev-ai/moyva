using System.Collections.Generic;
using System.IO;
using System.Threading;
using Kruty1918.Moyva.AI.Bot;
using NUnit.Framework;
using Unity.InferenceEngine;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    // Self-play opponents load a verified checkpoint at runtime in .sentis
    // serialization (ONNX conversion is editor-only). The shipped model doubles
    // as the fixture: it shares the exact observation/action contract the pool
    // checkpoints use, and ModelWriter round-trips it to the runtime format the
    // frozen-eval exporter produces.
    public sealed class SelfPlayOpponentTests
    {
        private const string ShippedModelAssetPath =
            "Assets/Moyva/Presets/AI/Resources/AI/Models/MoyvaStrategy_Normal.onnx";

        private static string ShippedModelPath =>
            Path.GetFullPath(ShippedModelAssetPath);

        private static string _fixtureSentis;

        private static string FixtureModelPath
        {
            get
            {
                if (_fixtureSentis != null) return _fixtureSentis;
                var asset = AssetDatabase.LoadAssetAtPath<ModelAsset>(ShippedModelAssetPath);
                Assert.NotNull(asset, "Fixture model did not import as a ModelAsset: " + ShippedModelPath);
                _fixtureSentis = Path.Combine(Path.GetTempPath(), "moyva-selfplay-fixture.sentis");
                ModelWriter.Save(_fixtureSentis, asset);
                return _fixtureSentis;
            }
        }

        private static BotDecisionFrame Frame(params BotCandidateAction[] candidates)
            => new BotDecisionFrame("enemy", 1, new BotGameStamp("enemy", 1, 0, true),
                new BotCandidateSet(candidates), new float[BotObservationSchema.Size],
                new Dictionary<BotCapabilityId, string>());

        private static BotCandidateAction Candidate(BotCapabilityId capability, BotIntentType intent)
            => new BotCandidateAction("c-" + intent, capability, intent);

        [Test]
        public void SentisModelLoadsAsSelfPlayOpponent()
        {
            using (var driver = SelfPlayOpponentDriver.TryLoad(FixtureModelPath, out string reason))
                Assert.NotNull(driver, reason);
        }

        [Test]
        public void SelfPlayDriverDecodesOnlyLegalSlots()
        {
            using (var driver = SelfPlayOpponentDriver.TryLoad(FixtureModelPath, out string reason))
            {
                Assert.NotNull(driver, reason);
                var candidates = new[]
                {
                    Candidate(BotCapabilityId.Turn, BotIntentType.EndTurn),
                    Candidate(BotCapabilityId.Movement, BotIntentType.Move),
                    Candidate(BotCapabilityId.Combat, BotIntentType.Attack),
                };
                var frame = Frame(candidates);
                var decision = driver.Decide(frame, CancellationToken.None).Result;
                Assert.AreEqual(BotPolicyMode.MLAgentsInference, decision.Mode);
                Assert.IsTrue(frame.Candidates.IsLegal(decision.Slot),
                    $"Model returned illegal slot {decision.Slot}.");
            }
        }

        [Test]
        public void SelfPlayDriverHonoursActionMask()
        {
            using (var driver = SelfPlayOpponentDriver.TryLoad(FixtureModelPath, out string reason))
            {
                Assert.NotNull(driver, reason);
                // Slot 5 is the only legal action; the mask must force the pick.
                var candidates = new BotCandidateAction[6];
                candidates[5] = Candidate(BotCapabilityId.Turn, BotIntentType.EndTurn);
                var frame = Frame(candidates);
                var decision = driver.Decide(frame, CancellationToken.None).Result;
                Assert.AreEqual(5, decision.Slot,
                    "Masked model must return the only legal candidate.");
            }
        }

        [Test]
        public void SelfPlayDriverRejectsOnnxWithFormatHint()
        {
            Assert.IsNull(SelfPlayOpponentDriver.TryLoad(ShippedModelPath, out string reason));
            StringAssert.Contains(".sentis", reason);
        }

        [Test]
        public void SelfPlayDriverRejectsMissingModel()
        {
            Assert.IsNull(SelfPlayOpponentDriver.TryLoad("does/not/exist.sentis", out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsNull(SelfPlayOpponentDriver.TryLoad(null, out reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void PolicyFactorySelectsSelfPlayDriver()
        {
            var driver = TrainingOpponentPolicy.Create("self-play", FixtureModelPath);
            Assert.IsInstanceOf<SelfPlayOpponentDriver>(driver);
            using (driver as System.IDisposable)
                Assert.AreEqual(BotPolicyMode.MLAgentsInference, driver.Mode);
            var onnx = TrainingOpponentPolicy.Create("onnx", FixtureModelPath);
            Assert.IsInstanceOf<SelfPlayOpponentDriver>(onnx);
            (onnx as System.IDisposable)?.Dispose();
        }

        [Test]
        public void PolicyFactoryDegradesToHeuristicWithoutModel()
        {
            var driver = TrainingOpponentPolicy.Create("self-play", null);
            Assert.IsInstanceOf<BoundedHeuristicOpponentDriver>(driver);
            Assert.AreEqual(BotPolicyMode.Heuristic, driver.Mode);
        }
    }
}
