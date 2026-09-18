using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Validation;
using NUnit.Framework;

namespace Kruty1918.Moyva.Marketing.Tests
{
    public sealed class MarketingValidationTests
    {
        [Test]
        public void ValidRecipe_Passes()
        {
            var r = new MarketingCaptureRecipe
            {
                id = "steam-screenshots",
                outputKind = MarketingOutputKind.Image,
                shotCount = 10,
                resolutionWidth = 1920, resolutionHeight = 1080,
            };
            var issues = MarketingRecipeValidator.Validate(r);
            Assert.AreEqual(0, issues.FindAll(i => i.blocking).Count);
        }

        [Test]
        public void BadResolution_Fails()
        {
            var r = new MarketingCaptureRecipe { id = "x", resolutionWidth = 50 };
            var issues = MarketingRecipeValidator.Validate(r);
            Assert.IsTrue(issues.Exists(i => i.code == "resolution-low"));
        }

        [Test]
        public void VideoRecipe_BadDuration_Fails()
        {
            var r = new MarketingCaptureRecipe
            {
                id = "v", outputKind = MarketingOutputKind.Video,
                durationSec = 400f, frameRate = 30,
            };
            var issues = MarketingRecipeValidator.Validate(r);
            Assert.IsTrue(issues.Exists(i => i.code == "duration"));
        }

        [Test]
        public void SteamScreenshot_RejectsMarketingText()
        {
            var p = new PlatformCaptureProfile
            {
                id = "steam-screenshot", gameplayOnly = true,
                textAllowed = false, logoAllowed = false, preRenderedAllowed = false,
            };
            var r = new MarketingCaptureRecipe
            {
                id = "s", outputKind = MarketingOutputKind.Image,
                includeMarketingText = true, includeLogo = true,
            };
            var issues = PlatformCompliance.Check(r, p);
            Assert.GreaterOrEqual(issues.Count, 2);
        }

        [Test]
        public void ForceCompliant_ProducesCleanSteamRecipe()
        {
            var p = new PlatformCaptureProfile
            {
                id = "steam-screenshot", gameplayOnly = true,
                textAllowed = false, logoAllowed = false, preRenderedAllowed = false,
            };
            var r = new MarketingCaptureRecipe
            {
                id = "s", includeMarketingText = true, includeLogo = true,
                allowStagingSpawns = true, durationSec = 300f,
            };
            p.maxDurationSec = 60f;
            var clean = PlatformCompliance.ForceCompliant(r, p);
            Assert.IsFalse(clean.includeMarketingText);
            Assert.IsFalse(clean.includeLogo);
            Assert.IsFalse(clean.allowStagingSpawns);
            Assert.AreEqual(60f, clean.durationSec);
            // Original unchanged (non-destructive)
            Assert.IsTrue(r.includeMarketingText);
        }

        [Test]
        public void ContentValidator_FlagsMissingMaterial()
        {
            var e = new ContentIndexEntry
            {
                id = "broken", category = MarketingContentCategory.Building,
                editorPath = "Assets/x.prefab", missingMaterial = true, rendererCount = 1,
            };
            var issues = MarketingContentValidator.Validate(e);
            Assert.IsTrue(issues.Exists(i => i.code == "content-material"));
        }
    }
}
