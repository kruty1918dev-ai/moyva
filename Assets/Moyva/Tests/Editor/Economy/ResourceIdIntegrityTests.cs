using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// Цілісність посилань на ресурси: кожен "resourceId" у пресетах будівель,
    /// правил економіки та стартових ресурсах бутстрапа повинен існувати в
    /// реєстрі ресурсів Presets/Economy/resources.json. Раніше market.basePrices
    /// посилалися на неіснуючі id ("Wood", "Grain"), що мовчки ламало ринок.
    /// </summary>
    public sealed class ResourceIdIntegrityTests
    {
        private static readonly Regex IdRegex =
            new Regex("\"id\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);
        private static readonly Regex ResourceIdRegex =
            new Regex("\"resourceId\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);

        private static string PresetsDir
            => Path.Combine(Application.dataPath, "Moyva", "Presets");

        private static HashSet<string> LoadKnownResourceIds()
        {
            string path = Path.Combine(PresetsDir, "Economy", "resources.json");
            Assert.IsTrue(File.Exists(path), $"Resource registry missing: {path}");

            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (Match m in IdRegex.Matches(File.ReadAllText(path)))
                ids.Add(m.Groups[1].Value);
            return ids;
        }

        private static IEnumerable<string> PresetFilesWithResourceRefs()
        {
            foreach (string dir in new[]
            {
                Path.Combine(PresetsDir, "Buildings"),
                Path.Combine(PresetsDir, "Economy", "economy-rules-config"),
                Path.Combine(PresetsDir, "Systems", "bootstrap-installer-config"),
            })
            {
                if (!Directory.Exists(dir))
                    continue;
                foreach (string file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
                    yield return file;
            }
        }

        [Test]
        public void AllPresetResourceIds_ExistInResourceRegistry()
        {
            HashSet<string> known = LoadKnownResourceIds();
            Assert.Greater(known.Count, 10, "Resource registry parsed suspiciously few ids.");

            var failures = new List<string>();
            foreach (string file in PresetFilesWithResourceRefs())
            {
                string text = File.ReadAllText(file);
                foreach (Match m in ResourceIdRegex.Matches(text))
                {
                    string id = m.Groups[1].Value;
                    if (!known.Contains(id))
                        failures.Add($"{Path.GetFileName(file)}: unknown resourceId '{id}'");
                }
            }

            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        [Test]
        public void MarketBasePrices_ReferenceKnownResources()
        {
            HashSet<string> known = LoadKnownResourceIds();
            string path = Path.Combine(
                PresetsDir, "Economy", "economy-rules-config", "economyrulesconfig.json");
            Assert.IsTrue(File.Exists(path));

            string text = File.ReadAllText(path);
            int marketStart = text.IndexOf("\"market\"", System.StringComparison.Ordinal);
            int listStart = text.IndexOf("\"resourceBasePrices\"", marketStart, System.StringComparison.Ordinal);
            Assert.Greater(listStart, marketStart, "market.resourceBasePrices block not found");

            int marketCount = 0;
            foreach (Match m in ResourceIdRegex.Matches(text, listStart))
            {
                Assert.IsTrue(known.Contains(m.Groups[1].Value),
                    $"market.basePrices references unknown resource '{m.Groups[1].Value}'");
                marketCount++;
            }
            Assert.Greater(marketCount, 0, "market.resourceBasePrices is empty");
        }
    }
}
