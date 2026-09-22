using System.Collections.Generic;
using Kruty1918.Localization;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.Localization.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="LocalizationFontService"/>.
    /// Regression guard: the runtime-created dynamic fallback must sit at index 0
    /// of primary fallback tables. Serialized dynamic fallbacks (e.g. TMP's
    /// "LiberationSans SDF - Fallback") are persistent assets — when their atlas
    /// grows at runtime the editor reimports them and destroys the live object,
    /// which surfaces as MissingReferenceException in TMP_MaterialManager.
    /// </summary>
    [TestFixture]
    public sealed class LocalizationFontServiceTests
    {
        private const string RuntimeFallbackName = "LocalizationFallback (Dynamic)";

        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            // The service mutates the live TMP_Settings and default font asset;
            // strip the runtime fallback back out so the editor state stays clean.
            TMP_Settings.fallbackFontAssets?.RemoveAll(IsRuntimeFallback);
            var defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null && defaultFont.fallbackFontAssetTable != null)
                defaultFont.fallbackFontAssetTable.RemoveAll(IsRuntimeFallback);
            foreach (Object asset in _created)
                if (asset != null)
                    Object.DestroyImmediate(asset);
            _created.Clear();
        }

        private static bool IsRuntimeFallback(TMP_FontAsset asset)
            => asset == null || asset.name == RuntimeFallbackName;

        [Test]
        public void RegisterPrimaryFont_InsertsRuntimeFallbackBeforeExistingEntries()
        {
            var service = new LocalizationFontService(MoyvaLocalizationDefaults.CreateFontOptions());
            var existing = CreateFontAsset();
            var primary = CreateFontAsset();
            primary.fallbackFontAssetTable = new List<TMP_FontAsset> { existing };

            service.RegisterPrimaryFont(primary);

            Assert.AreEqual(2, primary.fallbackFontAssetTable.Count);
            var inserted = primary.fallbackFontAssetTable[0];
            _created.Add(inserted);
            Assert.IsNotNull(inserted);
            Assert.AreNotSame(existing, inserted);
            Assert.IsFalse(EditorUtility.IsPersistent(inserted),
                "Index 0 must be the non-persistent runtime fallback so lookups never " +
                "mutate a persistent dynamic font asset.");
            Assert.AreSame(existing, primary.fallbackFontAssetTable[1]);
        }

        [Test]
        public void RegisterPrimaryFont_IsIdempotent_KeepsRuntimeFallbackFirst()
        {
            var service = new LocalizationFontService(MoyvaLocalizationDefaults.CreateFontOptions());
            var primary = CreateFontAsset();

            service.RegisterPrimaryFont(primary);
            service.RegisterPrimaryFont(primary);

            Assert.AreEqual(1, primary.fallbackFontAssetTable.Count);
            var inserted = primary.fallbackFontAssetTable[0];
            _created.Add(inserted);
            Assert.AreEqual(RuntimeFallbackName, inserted.name);
        }

        private TMP_FontAsset CreateFontAsset()
        {
            var asset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            _created.Add(asset);
            return asset;
        }
    }
}
