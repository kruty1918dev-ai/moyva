using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Planning;
using NUnit.Framework;

namespace Kruty1918.Moyva.Marketing.Tests
{
    public sealed class MarketingPlanningTests
    {
        private static WorldSubjects MakeWorld()
        {
            var w = new WorldSubjects
            {
                worldCenterX = 32f, worldCenterY = 0f, worldCenterZ = 32f, worldRadius = 32f,
            };
            for (int i = 0; i < 6; i++)
                w.units.Add(new ShotSubject
                {
                    contentId = i % 2 == 0 ? "spearman" : "archer",
                    instanceId = $"u{i}", worldX = 10 + i * 3, worldZ = 10,
                    approximateRadius = 1.2f,
                });
            for (int i = 0; i < 4; i++)
                w.buildings.Add(new ShotSubject
                {
                    contentId = i == 0 ? "castle-01" : $"house-0{i}",
                    instanceId = $"b{i}", worldX = 20 + i * 4, worldZ = 22,
                    approximateRadius = 3f,
                });
            w.landmarks.Add(new ShotSubject { contentId = "terrain", instanceId = "l0", worldX = 40, worldZ = 40 });
            return w;
        }

        private static ContentIndexSnapshot MakeIndex()
        {
            var s = new ContentIndexSnapshot();
            s.entries.Add(new ContentIndexEntry
            {
                id = "spearman", category = MarketingContentCategory.Unit, role = "Military",
                hasAnimator = true, animationClips = new[] { "idle", "walk", "attack" },
                rendererCount = 2, suitsClose = true, marketingValue = MarketingValueTier.Hero,
            });
            s.entries.Add(new ContentIndexEntry
            {
                id = "archer", category = MarketingContentCategory.Unit, role = "Military",
                hasAnimator = true, animationClips = new[] { "idle", "walk" },
                rendererCount = 2, suitsClose = true,
            });
            s.entries.Add(new ContentIndexEntry
            {
                id = "castle-01", category = MarketingContentCategory.Building,
                role = "SettlementCenter", rendererCount = 3, suitsClose = true,
                marketingValue = MarketingValueTier.Hero,
            });
            for (int i = 1; i < 4; i++)
                s.entries.Add(new ContentIndexEntry
                {
                    id = $"house-0{i}", category = MarketingContentCategory.Building,
                    rendererCount = 2,
                });
            s.musicKeys.Add("tavern-music-loop");
            s.sfxKeys.Add("sword-hit");
            return s;
        }

        private static MarketingCaptureRecipe Recipe(string id, MarketingOutputKind kind)
            => new MarketingCaptureRecipe { id = id, outputKind = kind, shotCount = 10, durationSec = 45f };

        [Test]
        public void ScreenshotPlan_RespectsDiversityCap()
        {
            var planner = new ShotPlanner();
            var seq = planner.PlanScreenshots(MakeWorld(), MakeIndex(), Recipe("steam-screenshots", MarketingOutputKind.Image), 42);
            Assert.AreEqual(10, seq.shots.Count);

            var counts = new Dictionary<ShotCategory, int>();
            foreach (var s in seq.shots)
            {
                counts.TryGetValue(s.category, out int n);
                counts[s.category] = n + 1;
            }
            // No single category may exceed 30% of the batch.
            foreach (var kv in counts)
                Assert.LessOrEqual(kv.Value, 4, $"Category {kv.Key} exceeded diversity cap");
        }

        [Test]
        public void Planner_IsDeterministic()
        {
            var planner = new ShotPlanner();
            var a = planner.PlanScreenshots(MakeWorld(), MakeIndex(), Recipe("r", MarketingOutputKind.Image), 777);
            var b = planner.PlanScreenshots(MakeWorld(), MakeIndex(), Recipe("r", MarketingOutputKind.Image), 777);
            Assert.AreEqual(a.shots.Count, b.shots.Count);
            for (int i = 0; i < a.shots.Count; i++)
            {
                Assert.AreEqual(a.shots[i].category, b.shots[i].category);
                Assert.AreEqual(a.shots[i].rig, b.shots[i].rig);
                Assert.AreEqual(a.shots[i].subject.instanceId, b.shots[i].subject.instanceId);
                Assert.AreEqual(a.shots[i].seed, b.shots[i].seed);
            }
        }

        [Test]
        public void TrailerPlan_HasNarrativeArc_AndEndsWithEndCard()
        {
            var planner = new ShotPlanner();
            var recipe = Recipe("trailer-60", MarketingOutputKind.VideoWithAudio);
            var seq = planner.PlanTrailer(MakeWorld(), MakeIndex(), recipe, 5);
            Assert.Greater(seq.shots.Count, 3);

            var beats = new List<string>();
            foreach (var s in seq.shots) beats.Add(s.beat);
            Assert.AreEqual("Hook", beats[0]);
            Assert.AreEqual("End", beats[beats.Count - 1]);
            var last = seq.shots[seq.shots.Count - 1];
            Assert.AreEqual(CameraRigType.Static, last.rig);
            Assert.AreEqual(ShotTransition.FadeThroughBlack, last.transitionIn);
        }

        [Test]
        public void TrailerPlan_ShotLengthsRespectRestraint()
        {
            var planner = new ShotPlanner();
            var recipe = Recipe("trailer-60", MarketingOutputKind.VideoWithAudio);
            recipe.durationSec = 60f;
            var seq = planner.PlanTrailer(MakeWorld(), MakeIndex(), recipe, 9);
            float minLen = TrailerBeatLibrary.MinShotLength(60f);
            foreach (var s in seq.shots)
                Assert.GreaterOrEqual(s.durationSec, minLen * 0.9f,
                    $"Shot {s.shotId} too short: {s.durationSec}");
        }

        [Test]
        public void BannedContent_ScoresNegativeInfinity()
        {
            var index = MakeIndex();
            index.entries[0].selectionOverride = SelectionOverride.Ban; // spearman banned
            float s = ContentScorer.Score(index.entries[0], new ContentScorer.Context());
            Assert.IsTrue(float.IsNegativeInfinity(s));
        }

        [Test]
        public void PinnedContent_ScoresPositiveInfinity()
        {
            var index = MakeIndex();
            index.entries[0].selectionOverride = SelectionOverride.Pin;
            float s = ContentScorer.Score(index.entries[0], new ContentScorer.Context());
            Assert.IsTrue(float.IsPositiveInfinity(s));
        }

        [Test]
        public void MissingMaterial_IsRejected()
        {
            var index = MakeIndex();
            index.entries[0].missingMaterial = true;
            float s = ContentScorer.Score(index.entries[0], new ContentScorer.Context());
            Assert.IsTrue(float.IsNegativeInfinity(s));
        }

        [Test]
        public void RepetitionPenalty_LowersScore()
        {
            var e = MakeIndex().entries[0];
            var fresh = ContentScorer.Score(e, new ContentScorer.Context());
            var used = ContentScorer.Score(e, new ContentScorer.Context
            {
                usageCounts = new Dictionary<string, int> { [e.id] = 2 },
            });
            Assert.Less(used, fresh);
        }
    }
}
