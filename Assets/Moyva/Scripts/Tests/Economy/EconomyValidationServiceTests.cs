using System.Linq;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomyValidationServiceTests
    {
        [Test]
        public void Validate_ShouldCatchDuplicateAndMissingReferences()
        {
            var database = ScriptableObject.CreateInstance<EconomyDatabaseSO>();

            var resourceA = ScriptableObject.CreateInstance<EconomyResourceDefinition>();
            SetString(resourceA, "_id", "food");
            SetEnum(resourceA, "_category", EconomyResourceCategory.None);

            var resourceB = ScriptableObject.CreateInstance<EconomyResourceDefinition>();
            SetString(resourceB, "_id", "food");
            SetEnum(resourceB, "_category", EconomyResourceCategory.Materials);

            var settlement = ScriptableObject.CreateInstance<EconomySettlementDefinition>();
            SetString(settlement, "_settlementId", "village_01");
            SetString(settlement, "_centerBuildingId", "");
            SetInt(settlement, "_buildRadius", 0);

            var warehouse = ScriptableObject.CreateInstance<EconomyWarehousePolicy>();
            AddWarehouseEntry(warehouse, "unknown_resource", priority: 0);

            var production = ScriptableObject.CreateInstance<EconomyProductionProfile>();
            SetString(production, "_buildingId", "mill");
            SetString(production, "_recipeId", "");
            SetInt(production, "_outputAmountPerCycle", 0);

            var caravan = ScriptableObject.CreateInstance<EconomyCaravanTemplate>();
            SetString(caravan, "_templateId", "caravan_1");
            SetInt(caravan, "_capacity", 0);

            AddToList(database, "_resources", resourceA);
            AddToList(database, "_resources", resourceB);
            AddToList(database, "_settlements", settlement);
            AddToList(database, "_warehousePolicies", warehouse);
            AddToList(database, "_productionProfiles", production);
            AddToList(database, "_caravanTemplates", caravan);

            var service = new EconomyValidationService();
            var issues = service.Validate(database);

            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("Duplicate resource Id")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("has no category")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("no CenterBuildingId")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("invalid BuildRadius")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("unknown resource")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("missing RecipeId")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("invalid OutputAmountPerCycle")));
            Assert.IsTrue(issues.Any(issue => issue.Message.Contains("invalid Capacity")));
        }

        private static void AddWarehouseEntry(EconomyWarehousePolicy warehouse, string resourceId, int priority)
        {
            var so = new SerializedObject(warehouse);
            so.Update();

            var entries = so.FindProperty("_entries");
            entries.InsertArrayElementAtIndex(entries.arraySize);

            var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            entry.FindPropertyRelative("_resourceId").stringValue = resourceId;
            entry.FindPropertyRelative("_priority").intValue = priority;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToList(ScriptableObject target, string listPropertyName, Object item)
        {
            var so = new SerializedObject(target);
            so.Update();

            var list = so.FindProperty(listPropertyName);
            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;

            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static void SetEnum(Object target, string propertyName, EconomyResourceCategory value)
        {
            var so = new SerializedObject(target);
            so.Update();
            so.FindProperty(propertyName).enumValueIndex = (int)value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
