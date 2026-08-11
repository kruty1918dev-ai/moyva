#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using System.Linq;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Tests.Construction
{
    public sealed class TerrainRuleEditorContextTests
    {
        [Test]
        public void ValueHelpers_TrimDeduplicateSortAndCompareCaseInsensitively()
        {
            string[] normalized = TerrainRuleEditorContext.NormalizeValues(new[]
            {
                " water ",
                "WATER",
                null,
                "land",
                string.Empty,
            });

            Assert.That(normalized, Is.EqualTo(new[] { "land", "water" }));
            Assert.That(TerrainRuleEditorContext.Contains(normalized, " WATER "), Is.True);
            Assert.That(
                TerrainRuleEditorContext.AddValue(normalized, "Forest"),
                Is.EqualTo(new[] { "Forest", "land", "water" }));
            Assert.That(
                TerrainRuleEditorContext.AddValue(normalized, "LAND"),
                Is.EqualTo(new[] { "land", "water" }));
            Assert.That(
                TerrainRuleEditorContext.RemoveValue(
                    new[] { "water", " WATER ", "land" },
                    "Water"),
                Is.EqualTo(new[] { "land" }));
        }

        [Test]
        public void BuildCatalog_IncludesSuggestionsProfilesFallbackAndStableLayerLabels()
        {
            TerrainLayerProfileSO asset = MoyvaJsonObjectFactory.Create<TerrainLayerProfileSO>();
            try
            {
                var serialized = new SerializedObject(asset);
                SerializedProperty profiles = serialized.FindProperty("_profiles");
                profiles.arraySize = 2;
                SetProfile(profiles.GetArrayElementAtIndex(0), " layer-b ", "Beta", " water ", "WATER");
                SetProfile(profiles.GetArrayElementAtIndex(1), "layer-a", "Alpha", "volcanic");
                SetProfile(serialized.FindProperty("_fallback"), "fallback", "Default", " shore ");
                serialized.ApplyModifiedPropertiesWithoutUndo();

                TerrainRuleEditorCatalogSnapshot catalog = TerrainRuleEditorContext.BuildCatalog(asset);

                Assert.That(catalog.Tags.Any(option => option.Value == "land" && option.IsBuiltIn), Is.True);
                Assert.That(
                    catalog.Tags.Single(option => option.Value == "water").IsPresentInProfile,
                    Is.True);
                Assert.That(
                    catalog.Tags.Single(option => option.Value == "shore").IsPresentInProfile,
                    Is.True,
                    "Fallback tags must be included in the picker catalogue.");
                Assert.That(
                    catalog.Tags.Any(option => option.Value == "volcanic" && !option.IsBuiltIn),
                    Is.True);

                Assert.That(
                    catalog.Layers.Select(option => option.MenuLabel).ToArray(),
                    Is.EqualTo(new[]
                    {
                        "Alpha (layer-a)",
                        "Beta (layer-b)",
                        "Default (fallback)",
                    }));
                Assert.That(catalog.Layers.Single(option => option.Value == "fallback").IsFallback, Is.True);
            }
            finally
            {
                MoyvaJsonObjectFactory.DestroyImmediate(asset);
            }
        }

        private static void SetProfile(
            SerializedProperty profile,
            string layerId,
            string displayName,
            params string[] tags)
        {
            profile.FindPropertyRelative("_layerId").stringValue = layerId;
            profile.FindPropertyRelative("_displayName").stringValue = displayName;

            SerializedProperty tagsProperty = profile.FindPropertyRelative("_tags");
            tagsProperty.arraySize = tags.Length;
            for (int index = 0; index < tags.Length; index++)
                tagsProperty.GetArrayElementAtIndex(index).stringValue = tags[index];
        }
    }
}

#endif
