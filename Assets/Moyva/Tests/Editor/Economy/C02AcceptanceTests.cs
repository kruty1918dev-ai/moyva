using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C02 acceptance: tool/material resources that were mislabeled Food
    /// (axe-silver, bolt-blue, sticks-brown) must not satisfy a food need —
    /// neither as raw JSON nor through the runtime-resolved category — while
    /// real food keeps working and stable ids stay unchanged.
    /// </summary>
    public class C02AcceptanceTests
    {
        private static readonly string[] ToolIds =
        {
            "axe-silver-food-resources",
            "bolt-blue-food-resources",
            "sticks-brown-food-resources",
        };

        private static readonly string[] RealFoodIds =
        {
            "wheat-bundle-food-resources",
            "steak-food-resources",
            "banana-food-resources",
        };

        private static string PresetPath(string id)
            => Path.Combine(
                Application.dataPath,
                "Moyva/Presets/Economy/economy-resource",
                id + ".json");

        private static string RawCategory(string id)
        {
            string path = PresetPath(id);
            Assert.IsTrue(File.Exists(path), $"Preset missing on disk: {path}");
            var marker = "\"category\"";
            foreach (string line in File.ReadLines(path))
            {
                int at = line.IndexOf(marker, System.StringComparison.Ordinal);
                if (at < 0)
                    continue;
                int colon = line.IndexOf(':', at + marker.Length);
                int first = line.IndexOf('"', colon + 1);
                int last = line.IndexOf('"', first + 1);
                return line.Substring(first + 1, last - first - 1);
            }
            Assert.Fail($"No category field in {path}");
            return null;
        }

        [Test]
        public void Tools_RawJsonCategory_IsNotFood_AndStableIdsUnchanged()
        {
            foreach (string id in ToolIds)
            {
                string raw = RawCategory(id);
                Assert.AreNotEqual("Food", raw,
                    $"{id} must not deserialize into the Food category.");
                Assert.IsTrue(System.Enum.TryParse(raw, out EconomyResourceCategory _),
                    $"{id} category '{raw}' is not a known EconomyResourceCategory.");
            }
        }

        [Test]
        public void Tools_ResolvedCategory_MatchesJson_AndIsNotFood()
        {
            JsonConfigRuntime.EnsureLoaded();
            Assert.IsTrue(JsonConfigRuntime.IsLoaded);

            foreach (string id in ToolIds)
            {
                var def = JsonConfigRuntime.Get<EconomyResourceDefinition>(id);
                Assert.NotNull(def, $"Stable id '{id}' must still resolve — no id renames.");
                Assert.AreNotEqual(EconomyResourceCategory.Food, def.Category,
                    $"{id} resolved category must not be Food.");
                Assert.AreEqual(RawCategory(id), def.Category.ToString(),
                    $"{id}: runtime resolved category diverges from the JSON source.");
            }
        }

        [Test]
        public void RealFood_StillResolvesAsFood()
        {
            JsonConfigRuntime.EnsureLoaded();

            foreach (string id in RealFoodIds)
            {
                var def = JsonConfigRuntime.Get<EconomyResourceDefinition>(id);
                Assert.NotNull(def, $"Expected real food resource '{id}' to resolve.");
                Assert.AreEqual(EconomyResourceCategory.Food, def.Category,
                    $"{id} must remain edible.");
            }
        }

        [Test]
        public void FoodNeedLookup_ExcludesTools_IncludesRealFood()
        {
            JsonConfigRuntime.EnsureLoaded();
            var database = new EconomyDatabaseSO();
            typeof(EconomyDatabaseSO)
                .GetField("_resources", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(database,
                    new List<EconomyResourceDefinition>(
                        JsonConfigRuntime.GetAll<EconomyResourceDefinition>()));

            var consumption = new EconomyConsumptionService();
            var ids = consumption.ResolveNeedResourceIds(database, "Food");

            foreach (string id in ToolIds)
                Assert.IsFalse(ids.Contains(id),
                    $"Food need must never resolve to tool id '{id}'.");
            CollectionAssert.IsNotEmpty(ids, "Food need must still resolve to real food ids.");
            foreach (string id in RealFoodIds)
                Assert.IsTrue(ids.Contains(id), $"Real food '{id}' dropped out of the Food lookup.");
        }
    }
}
