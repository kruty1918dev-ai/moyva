using System.IO;
using Kruty1918.Localization;
using NUnit.Framework;

namespace Kruty1918.Moyva.Shared.Localization.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="LocalizationService"/> against the real JSON catalogs.
    /// Persistence is redirected to a temp file via the internal test-seam ctor.
    /// </summary>
    [TestFixture]
    public sealed class LocalizationServiceTests
    {
        private string _persistPath;

        [SetUp]
        public void SetUp()
        {
            _persistPath = Path.Combine(Path.GetTempPath(),
                $"moyva-language-test-{System.Guid.NewGuid():N}.txt");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_persistPath)) File.Delete(_persistPath);
        }

        private LocalizationService Create() => new LocalizationService(
            MoyvaLocalizationDefaults.CreateOptions(_persistPath));

        [Test]
        public void SupportedLanguages_ContainsEnglishAndUkrainian()
        {
            var service = Create();
            Assert.AreEqual(2, service.SupportedLanguages.Count);
            Assert.AreEqual("en", service.SupportedLanguages[0].Id);
            Assert.AreEqual("uk", service.SupportedLanguages[1].Id);
        }

        [Test]
        public void T_MissingKey_ReturnsSourceText()
        {
            var service = Create();
            service.TrySetLanguage("en");
            Assert.AreEqual("some untranslated text", service.T("some untranslated text"));
        }

        [Test]
        public void T_Ukrainian_TranslatesKnownKey()
        {
            var service = Create();
            Assert.IsTrue(service.TrySetLanguage("uk"));
            Assert.AreEqual("Назад", service.T("Back"));
        }

        [Test]
        public void T_Ukrainian_UnknownKey_FallsBackToSource()
        {
            var service = Create();
            service.TrySetLanguage("uk");
            Assert.AreEqual("Unmapped label", service.T("Unmapped label"));
        }

        [Test]
        public void TF_FormatsPlaceholders()
        {
            var service = Create();
            service.TrySetLanguage("uk");
            Assert.AreEqual("Показано 1-10 з 42", service.TF("Showing {0}-{1} of {2}", 1, 10, 42));
        }

        [Test]
        public void TN_English_OneVsOther()
        {
            var service = Create();
            service.TrySetLanguage("en");
            Assert.AreEqual("turn", service.TN("turn", "turns", 1));
            Assert.AreEqual("turns", service.TN("turn", "turns", 2));
        }

        [Test]
        public void TN_Ukrainian_OneFewMany()
        {
            var service = Create();
            service.TrySetLanguage("uk");
            Assert.AreEqual("хід", service.TN("turn", "turns", 1));
            Assert.AreEqual("ходи", service.TN("turn", "turns", 2));
            Assert.AreEqual("ходи", service.TN("turn", "turns", 4));
            Assert.AreEqual("ходів", service.TN("turn", "turns", 5));
            Assert.AreEqual("ходів", service.TN("turn", "turns", 11));
            Assert.AreEqual("хід", service.TN("turn", "turns", 21));
        }

        [Test]
        public void TrySetLanguage_RaisesLanguageChangedOnce()
        {
            var service = Create();
            service.TrySetLanguage("en");
            int raised = 0;
            service.LanguageChanged += () => raised++;

            Assert.IsTrue(service.TrySetLanguage("uk"));
            Assert.IsTrue(service.TrySetLanguage("uk"));
            Assert.AreEqual(1, raised);
            Assert.AreEqual("uk", service.CurrentLanguageId);
        }

        [Test]
        public void TrySetLanguage_UnknownId_ReturnsFalse()
        {
            var service = Create();
            Assert.IsFalse(service.TrySetLanguage("xx"));
        }

        [Test]
        public void PersistedLanguage_IsRestoredOnNextInstance()
        {
            var first = Create();
            first.TrySetLanguage("uk");

            var second = Create();
            Assert.AreEqual("uk", second.CurrentLanguageId);
        }
    }
}
