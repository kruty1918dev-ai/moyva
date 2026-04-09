using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomySimulationServiceTests
    {
        [Test]
        public void Simulate_ShouldProduceDeterministicTotals()
        {
            var profileA = ScriptableObject.CreateInstance<EconomyProductionProfile>();
            SetString(profileA, "_buildingId", "farm");
            SetString(profileA, "_recipeId", "food");
            SetFloat(profileA, "_cycleDurationSeconds", 60f);
            SetInt(profileA, "_outputAmountPerCycle", 2);

            var profileB = ScriptableObject.CreateInstance<EconomyProductionProfile>();
            SetString(profileB, "_buildingId", "quarry");
            SetString(profileB, "_recipeId", "stone");
            SetFloat(profileB, "_cycleDurationSeconds", 120f);
            SetInt(profileB, "_outputAmountPerCycle", 3);

            var service = new EconomySimulationService();
            var input = new EconomySimulationInput
            {
                DurationMinutes = 10f,
                ProductionProfiles = new[] { profileA, profileB },
            };

            var first = service.Simulate(input);
            var second = service.Simulate(input);

            Assert.AreEqual(20, first.ResourceTotals["food"]);
            Assert.AreEqual(15, first.ResourceTotals["stone"]);
            Assert.AreEqual(first.ResourceTotals["food"], second.ResourceTotals["food"]);
            Assert.AreEqual(first.ResourceTotals["stone"], second.ResourceTotals["stone"]);
            Assert.AreEqual(first.Log.Count, second.Log.Count);
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            var so = new SerializedObject(target);
            so.Update();
            so.FindProperty(propertyName).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            var so = new SerializedObject(target);
            so.Update();
            so.FindProperty(propertyName).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            var so = new SerializedObject(target);
            so.Update();
            so.FindProperty(propertyName).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
