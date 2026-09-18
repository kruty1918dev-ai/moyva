using System.IO;
using Kruty1918.Moyva.Marketing.Content;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using Kruty1918.Moyva.Marketing.Validation;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Tests
{
    /// <summary>Integration tests over the shipped JSON presets + manifest
    /// serialization contract.</summary>
    public sealed class MarketingPresetsAndManifestTests
    {
        [Test]
        public void ShippedRecipes_Parse_AndValidate()
        {
            var store = new MarketingPresetStore();
            store.LoadAll();
            Assert.GreaterOrEqual(store.Recipes.Count, 6, "Expected built-in recipes");

            foreach (var r in store.Recipes)
            {
                var issues = MarketingRecipeValidator.Validate(r);
                Assert.AreEqual(0, issues.FindAll(i => i.blocking).Count,
                    $"Recipe '{r.id}' failed validation: {string.Join(";", issues.ConvertAll(i => i.message))}");
            }
        }

        [Test]
        public void ShippedPlatforms_Parse()
        {
            var store = new MarketingPresetStore();
            store.LoadAll();
            Assert.GreaterOrEqual(store.Platforms.Count, 8, "Expected platform profiles");
            var steam = store.FindPlatform("steam-screenshot");
            Assert.IsNotNull(steam);
            Assert.IsTrue(steam.gameplayOnly);
            Assert.IsFalse(steam.textAllowed);
        }

        [Test]
        public void SteamRecipe_ProducesNoMarketingText()
        {
            var store = new MarketingPresetStore();
            store.LoadAll();
            var recipe = store.FindRecipe("steam-screenshots");
            var platform = store.FindPlatform(recipe.platformProfileId);
            var effective = PlatformCompliance.ForceCompliant(recipe, platform);
            Assert.IsFalse(effective.includeMarketingText, "Steam screenshots must stay clean");
            Assert.IsFalse(effective.includeLogo);
        }

        [Test]
        public void StringEnums_InRecipes_ParseCorrectly()
        {
            var store = new MarketingPresetStore();
            store.LoadAll();
            var steam = store.FindRecipe("steam-screenshots");
            Assert.AreEqual(MarketingContentType.Screenshots, steam.contentType);
            Assert.AreEqual(MarketingUiVisibility.Full, steam.uiVisibility);
            Assert.AreEqual(MarketingCameraStyle.Gameplay, steam.cameraStyle);
        }

        [Test]
        public void Manifest_RoundTrips()
        {
            var m = new MarketingManifest
            {
                runId = "run-1", recipeId = "r", status = "ok",
                resolutionWidth = 1920, resolutionHeight = 1080, frameRate = 30,
                worldSeed = 42,
            };
            m.shots.Add(new ShotRecord { shotId = "s1", status = "ok", score = 0.7f });
            m.filesGenerated.Add("Steam/a.png");
            string json = MarketingPresetStore.SerializeJson(m);
            var back = MarketingPresetStore.DeserializeJson<MarketingManifest>(json);
            Assert.AreEqual("run-1", back.runId);
            Assert.AreEqual(42, back.worldSeed);
            Assert.AreEqual(1, back.shots.Count);
            Assert.AreEqual("s1", back.shots[0].shotId);
        }

        [Test]
        public void RecipeJson_RoundTrips_WithStringEnums()
        {
            var r = new MarketingCaptureRecipe
            {
                id = "x", contentType = MarketingContentType.GameplayTrailer,
                outputKind = MarketingOutputKind.VideoWithAudio,
                uiVisibility = MarketingUiVisibility.Minimal,
                cameraStyle = MarketingCameraStyle.Mixed,
                lightingStyle = MarketingLightingStyle.GoldenHour,
            };
            string json = MarketingPresetStore.SerializeJson(r);
            var back = MarketingPresetStore.DeserializeJson<MarketingCaptureRecipe>(json);
            Assert.AreEqual(MarketingContentType.GameplayTrailer, back.contentType);
            Assert.AreEqual(MarketingOutputKind.VideoWithAudio, back.outputKind);
            Assert.AreEqual(MarketingUiVisibility.Minimal, back.uiVisibility);
            Assert.AreEqual(MarketingLightingStyle.GoldenHour, back.lightingStyle);
        }

        [Test]
        public void OutputLayout_SanitizesNames()
        {
            Assert.AreEqual("steam-screenshots", MarketingOutputLayout.Sanitize("steam-screenshots"));
            Assert.AreEqual("a-b-c", MarketingOutputLayout.Sanitize("a/b\\c"));
            Assert.AreEqual("run", MarketingOutputLayout.Sanitize("  "));
        }
    }
}
