using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Content;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Planning;
using Kruty1918.Moyva.Marketing.Text;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Tests
{
    public sealed class MarketingClaimsAndFramesTests
    {
        private static ContentIndexSnapshot RichIndex()
        {
            var s = new ContentIndexSnapshot();
            s.entries.Add(new ContentIndexEntry
            {
                id = "castle-01", category = MarketingContentCategory.Building,
                role = "SettlementCenter", rendererCount = 3,
            });
            s.entries.Add(new ContentIndexEntry
            {
                id = "archer", category = MarketingContentCategory.Unit, role = "Military",
                rendererCount = 2,
            });
            s.entries.Add(new ContentIndexEntry
            {
                id = "farm", category = MarketingContentCategory.Building,
                role = "Production", rendererCount = 2,
            });
            s.entries.Add(new ContentIndexEntry
            {
                id = "wall", category = MarketingContentCategory.Building,
                role = "Defense", rendererCount = 2,
            });
            s.musicKeys.Add("m");
            s.sfxKeys.Add("s");
            return s;
        }

        private static MarketingClaimCatalog Catalog()
            => new MarketingClaimCatalog(new MarketingClaimCatalogData
            {
                claims = new List<MarketingClaim>
                {
                    new MarketingClaim { id = "a", en = "BUILD", requires = new[] { "settlement" } },
                    new MarketingClaim { id = "b", en = "STRIKE FROM AFAR", requires = new[] { "ranged" } },
                    new MarketingClaim { id = "c", en = "Embark on an epic journey", requires = new string[0] },
                },
            });

        [Test]
        public void Facts_DetectFeatures()
        {
            var f = MarketingFeatureFacts.FromIndex(RichIndex());
            Assert.IsTrue(f.hasSettlementCenter);
            Assert.IsTrue(f.hasCombatUnits);
            Assert.IsTrue(f.hasEconomyBuildings);
            Assert.IsTrue(f.hasDefenseBuildings);
            Assert.IsTrue(f.hasRangedUnits); // archer id
            Assert.AreEqual(1, f.musicCount);
        }

        [Test]
        public void Claims_RequireSatisfiedPredicates()
        {
            var facts = MarketingFeatureFacts.FromIndex(RichIndex());
            var eligible = Catalog().Eligible(facts, "en");
            Assert.IsTrue(eligible.Exists(c => c.id == "a"));
            Assert.IsTrue(eligible.Exists(c => c.id == "b")); // ranged present
            // Cliché phrase is banned even though predicates pass
            Assert.IsFalse(eligible.Exists(c => c.id == "c"));
        }

        [Test]
        public void Claims_MissingFeature_BlocksClaim()
        {
            var empty = MarketingFeatureFacts.FromIndex(new ContentIndexSnapshot());
            var eligible = Catalog().Eligible(empty, "en");
            Assert.AreEqual(0, eligible.Count);
        }

        [Test]
        public void BannedPhrases_AreRejected()
        {
            Assert.IsTrue(MarketingClaimCatalog.IsBanned("Embark on an epic journey"));
            Assert.IsTrue(MarketingClaimCatalog.IsBanned("Unleash your power"));
            Assert.IsFalse(MarketingClaimCatalog.IsBanned("BUILD"));
        }

        [Test]
        public void ClaimLocalization_FallsBackToEnglish()
        {
            var c = new MarketingClaim { id = "x", en = "BUILD", uk = "БУДУЙ", de = "" };
            Assert.AreEqual("БУДУЙ", c.For("uk"));
            Assert.AreEqual("BUILD", c.For("de"));
            Assert.AreEqual("BUILD", c.For("fr"));
        }

        [Test]
        public void GoldenFrame_RejectsDeadFrames()
        {
            var bad = new FrameMetrics { blackFraction = 0.95f, luminanceVariance = 0.0f };
            var s = GoldenFrameEvaluator.Evaluate(bad);
            Assert.IsTrue(s.Rejected);

            var magenta = new FrameMetrics { magentaFraction = 0.02f, luminanceVariance = 0.2f, luminanceMean = 0.4f };
            Assert.IsTrue(GoldenFrameEvaluator.Evaluate(magenta).Rejected);
        }

        [Test]
        public void GoldenFrame_PickBest_SelectsComposedFrame()
        {
            var frames = new List<FrameMetrics>
            {
                new FrameMetrics { subjectScreenArea = 0.2f, subjectCenterOffset = 0.6f, luminanceMean = 0.4f, luminanceVariance = 0.05f, subjectInsideSafeArea = true },
                new FrameMetrics { subjectScreenArea = 0.25f, subjectCenterOffset = 0.1f, luminanceMean = 0.45f, luminanceVariance = 0.08f, subjectInsideSafeArea = true },
                new FrameMetrics { blackFraction = 1f },
            };
            int best = GoldenFrameEvaluator.PickBest(frames);
            Assert.AreEqual(1, best);
        }

        [Test]
        public void AspectCrop_Portrait_KeepsFocusInside()
        {
            // 16:9 → 9:16; focus at x=0.75 must stay inside the crop.
            var crop = AspectCrop.Compute(1920, 1080, 9f / 16f, new Vector2(0.75f, 0.5f));
            Assert.AreEqual(1080, crop.height);
            Assert.Less(crop.width, 1080); // ~607
            float focusX = 0.75f * 1920;
            Assert.GreaterOrEqual(focusX, crop.x);
            Assert.LessOrEqual(focusX, crop.x + crop.width);
            Assert.GreaterOrEqual(crop.x, 0);
            Assert.LessOrEqual(crop.x + crop.width, 1920);
        }

        [Test]
        public void AspectCrop_SameAspect_ReturnsFullFrame()
        {
            var crop = AspectCrop.Compute(1920, 1080, 16f / 9f, new Vector2(0.2f, 0.8f));
            Assert.AreEqual(new RectInt(0, 0, 1920, 1080), crop);
        }

        [Test]
        public void Typography_SafeMargin_GrowsForPortrait()
        {
            float landscape = MarketingTypography.SafeMargin(16f / 9f, 0.05f);
            float portrait = MarketingTypography.SafeMargin(9f / 16f, 0.05f);
            Assert.Greater(portrait, landscape);
        }

        [Test]
        public void Typography_RejectsOverflowCopy()
        {
            Assert.IsTrue(MarketingTypography.Fits("BUILD", TextTemplate.BeatWord));
            Assert.IsFalse(MarketingTypography.Fits(
                "This is a very long marketing sentence that would never fit", TextTemplate.BeatWord));
        }

        [Test]
        public void Fingerprint_IsStable_AndSensitive()
        {
            var a = ContentFingerprint.Compute(new[] { "u:a:1", "u:b:2" });
            var b = ContentFingerprint.Compute(new[] { "u:b:2", "u:a:1" });
            var c = ContentFingerprint.Compute(new[] { "u:a:1", "u:b:3" });
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
        }
    }
}
