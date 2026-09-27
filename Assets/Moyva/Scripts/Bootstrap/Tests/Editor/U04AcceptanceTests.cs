using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Localization;
using Kruty1918.Moyva.Shared.Localization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// U04 acceptance invariants for localization catalogs: uk/en carry the
    /// same key set, placeholder-bearing entries keep identical placeholders,
    /// no empty values exist, plural entries have complete forms, and the live
    /// LocalizationService resolves both source directions with a language
    /// switch that never produces mixed-language output.
    /// </summary>
    public sealed class U04AcceptanceTests
    {
        private static readonly string CatalogDir = Path.Combine(
            Application.dataPath,
            "Moyva/Presets/Localization/Resources/MoyvaLocales");

        private static JObject Entries(string lang)
            => (JObject)JObject.Parse(
                File.ReadAllText(Path.Combine(CatalogDir, lang + ".json")))["entries"];

        private static HashSet<string> Placeholders(JToken value)
        {
            var texts = value?.Type == JTokenType.Object
                ? value.Children<JProperty>().Select(c => (string)c.Value)
                : new[] { (string)(value ?? JValue.CreateNull()) };
            var set = new HashSet<string>();
            foreach (var t in texts)
                foreach (System.Text.RegularExpressions.Match m in
                    System.Text.RegularExpressions.Regex.Matches(t ?? string.Empty, @"\{[^}]*\}"))
                    set.Add(m.Value);
            return set;
        }

        [Test]
        public void Catalogs_HaveIdenticalKeySets()
        {
            JObject uk = Entries("uk");
            JObject en = Entries("en");
            var ukKeys = new HashSet<string>(uk.Properties().Select(p => p.Name));
            var enKeys = new HashSet<string>(en.Properties().Select(p => p.Name));

            CollectionAssert.IsEmpty(ukKeys.Except(enKeys),
                "en.json is missing keys present in uk.json");
            CollectionAssert.IsEmpty(enKeys.Except(ukKeys),
                "uk.json is missing keys present in en.json");
            Assert.Greater(ukKeys.Count, 0);
        }

        [Test]
        public void Catalogs_PlaceholdersMatch_AndNoEmptyValues()
        {
            JObject uk = Entries("uk");
            JObject en = Entries("en");

            foreach (var prop in uk.Properties())
            {
                JToken ukValue = prop.Value;
                JToken enValue = en[prop.Name];
                Assert.IsNotNull(enValue, $"missing en entry for '{prop.Name}'");

                var ukPh = Placeholders(ukValue);
                var enPh = Placeholders(enValue);
                CollectionAssert.AreEquivalent(ukPh, enPh,
                    $"placeholder mismatch for '{prop.Name}'");

                bool ukEmpty = ukValue.Type == JTokenType.Object
                    ? ukValue.Children<JProperty>().Any(c => string.IsNullOrWhiteSpace((string)c.Value))
                    : string.IsNullOrWhiteSpace((string)ukValue);
                Assert.IsFalse(ukEmpty, $"empty uk value for '{prop.Name}'");
                bool enEmpty = enValue.Type == JTokenType.Object
                    ? enValue.Children<JProperty>().Any(c => string.IsNullOrWhiteSpace((string)c.Value))
                    : string.IsNullOrWhiteSpace((string)enValue);
                Assert.IsFalse(enEmpty, $"empty en value for '{prop.Name}'");
            }
        }

        [Test]
        public void Catalogs_PluralEntries_HaveCompleteForms()
        {
            foreach (string lang in new[] { "uk", "en" })
            {
                JObject entries = Entries(lang);
                foreach (var prop in entries.Properties())
                {
                    if (prop.Value.Type != JTokenType.Object)
                        continue;
                    foreach (string form in new[] { "one", "few", "many", "other" })
                        Assert.IsFalse(
                            string.IsNullOrWhiteSpace((string)prop.Value[form]),
                            $"{lang}:{prop.Name} missing plural form '{form}'");
                }
            }
        }

        [Test]
        public void Service_ResolvesBothDirections_AndSwitchesLive()
        {
            var options = MoyvaLocalizationDefaults.CreateOptions(
                Path.Combine(Path.GetTempPath(), "u04-language.txt"));
            var service = new LocalizationService(options);

            Assert.IsTrue(service.TrySetLanguage("en"));
            Assert.AreEqual("en", service.CurrentLanguageId);
            // English-authored source text passes through identity under en.
            Assert.AreEqual("A realm awaits", service.T("A realm awaits"));
            // Ukrainian-authored building names translate under en.
            Assert.AreEqual("House", service.T("Будинок"));

            bool fired = false;
            service.LanguageChanged += () => fired = true;
            Assert.IsTrue(service.TrySetLanguage("uk"));
            Assert.IsTrue(fired, "language switch must fire LanguageChanged");

            Assert.AreEqual("Царство чекає", service.T("A realm awaits"));
            Assert.AreEqual("Будинок", service.T("Будинок"));
        }

        [Test]
        public void Service_PluralForms_ResolvePerLanguage()
        {
            var options = MoyvaLocalizationDefaults.CreateOptions(
                Path.Combine(Path.GetTempPath(), "u04-language.txt"));
            var service = new LocalizationService(options);

            service.TrySetLanguage("en");
            Assert.AreEqual("turn", service.TN("turn", "turns", 1));
            Assert.AreEqual("turns", service.TN("turn", "turns", 5));

            service.TrySetLanguage("uk");
            Assert.AreEqual("хід", service.TN("turn", "turns", 1));
            Assert.AreEqual("ходів", service.TN("turn", "turns", 5));
        }
    }
}
