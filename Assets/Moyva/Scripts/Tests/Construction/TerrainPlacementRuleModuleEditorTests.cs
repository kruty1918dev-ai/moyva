#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using System.Linq;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Editor;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Tests.Construction
{
    public sealed class TerrainPlacementRuleModuleEditorTests
    {
        [Test]
        public void NewModule_InheritsGlobalTerrainRulesByDefault()
        {
            var module = new TerrainPlacementRuleModule();

            Assert.That(module.MergeMode, Is.EqualTo(PlacementRuleMergeMode.Inherit));
            Assert.That(module.AllowedTerrainIds, Is.Empty);
            Assert.That(module.BlockedTerrainIds, Is.Empty);
            Assert.That(module.AllowedTerrainTags, Is.Empty);
            Assert.That(module.BlockedTerrainTags, Is.Empty);
            Assert.That(module.AllowedTerrainLevels, Is.Empty);
            Assert.That(module.BlockedTerrainLevels, Is.Empty);
            Assert.That(module.RequiredNeighborOffsets, Is.Empty);
            Assert.That(module.AllowHills, Is.True);
            Assert.That(module.BlockEdgeTerrainTiles, Is.False);
            Assert.That(module.RequiresFlatGround, Is.False);
        }

        [Test]
        public void BlockWaterPreset_ReplacesAllPreviousTerrainSettings()
        {
            TerrainPlacementRuleModule module = CreatePopulatedModule();

            TerrainPlacementRuleModuleEditorGUI.ApplyBlockWaterPreset(module);

            AssertResetOverride(module);
            Assert.That(module.AllowedTerrainTags, Is.Empty);
            Assert.That(module.BlockedTerrainTags, Is.EqualTo(new[] { "water" }));
        }

        [Test]
        public void LandOnlyPreset_ReplacesAllPreviousTerrainSettings()
        {
            TerrainPlacementRuleModule module = CreatePopulatedModule();

            TerrainPlacementRuleModuleEditorGUI.ApplyLandOnlyPreset(module);

            AssertResetOverride(module);
            Assert.That(module.AllowedTerrainTags, Is.EqualTo(new[] { "land" }));
            Assert.That(module.BlockedTerrainTags, Is.Empty);
        }

        [Test]
        public void WaterOnlyPreset_ReplacesAllPreviousTerrainSettings()
        {
            TerrainPlacementRuleModule module = CreatePopulatedModule();

            TerrainPlacementRuleModuleEditorGUI.ApplyWaterOnlyPreset(module);

            AssertResetOverride(module);
            Assert.That(module.AllowedTerrainTags, Is.EqualTo(new[] { "water" }));
            Assert.That(module.BlockedTerrainTags, Is.Empty);
        }

        [Test]
        public void AnyTerrainPreset_ClearsAllPreviousTerrainSettings()
        {
            TerrainPlacementRuleModule module = CreatePopulatedModule();

            TerrainPlacementRuleModuleEditorGUI.ApplyAnyTerrainPreset(module);

            AssertResetOverride(module);
            Assert.That(module.AllowedTerrainTags, Is.Empty);
            Assert.That(module.BlockedTerrainTags, Is.Empty);
        }

        [Test]
        public void ValueHelpers_DoNotDiscardUnknownCatalogValues()
        {
            string[] values = { " custom-volcanic ", "WATER" };

            string[] withLand = TerrainRuleEditorContext.AddValue(values, " land ");
            string[] withoutWater = TerrainRuleEditorContext.RemoveValue(withLand, "water");
            string[] afterMissingRemoval = TerrainRuleEditorContext.RemoveValue(
                withoutWater,
                "not-in-the-catalog");

            Assert.That(
                withLand,
                Is.EqualTo(new[] { "custom-volcanic", "land", "WATER" }));
            Assert.That(
                TerrainRuleEditorContext.Contains(withLand, "CUSTOM-VOLCANIC"),
                Is.True);
            Assert.That(withoutWater, Is.EqualTo(new[] { "custom-volcanic", "land" }));
            Assert.That(afterMissingRemoval, Is.EqualTo(withoutWater));
        }

        [Test]
        public void BuildCatalog_FromSerializedProfile_IsStableAndCaseInsensitiveUnique()
        {
            TerrainLayerProfileSO asset = MoyvaJsonObjectFactory.Create<TerrainLayerProfileSO>();
            try
            {
                var serialized = new SerializedObject(asset);
                SerializedProperty profiles = serialized.FindProperty("_profiles");
                profiles.arraySize = 3;
                SetProfile(
                    profiles.GetArrayElementAtIndex(0),
                    "z-land",
                    "Zulu",
                    " land ",
                    "water");
                SetProfile(
                    profiles.GetArrayElementAtIndex(1),
                    "A-water",
                    "Alpha",
                    " WATER ",
                    "custom-volcanic");
                SetProfile(
                    profiles.GetArrayElementAtIndex(2),
                    "a-WATER",
                    "Duplicate ID",
                    "CUSTOM-VOLCANIC");
                SetProfile(
                    serialized.FindProperty("_fallback"),
                    "fallback",
                    "Default",
                    "land");
                serialized.ApplyModifiedPropertiesWithoutUndo();

                TerrainRuleEditorCatalogSnapshot catalog =
                    TerrainRuleEditorContext.BuildCatalog(asset);

                Assert.That(
                    catalog.Tags.Single(option => option.Value == "land").IsPresentInProfile,
                    Is.True);
                Assert.That(
                    catalog.Tags.Single(option => option.Value == "water").IsPresentInProfile,
                    Is.True);
                Assert.That(
                    catalog.Tags.Count(option =>
                        string.Equals(
                            option.Value,
                            "custom-volcanic",
                            System.StringComparison.OrdinalIgnoreCase)),
                    Is.EqualTo(1));
                Assert.That(
                    catalog.Layers.Select(option => option.Value).ToArray(),
                    Is.EqualTo(new[] { "A-water", "fallback", "z-land" }));
                Assert.That(
                    catalog.Layers.Select(option => option.MenuLabel).ToArray(),
                    Is.EqualTo(new[]
                    {
                        "Alpha (A-water)",
                        "Default (fallback)",
                        "Zulu (z-land)",
                    }));
            }
            finally
            {
                MoyvaJsonObjectFactory.DestroyImmediate(asset);
            }
        }

        private static TerrainPlacementRuleModule CreatePopulatedModule()
        {
            return new TerrainPlacementRuleModule
            {
                MergeMode = PlacementRuleMergeMode.Disabled,
                AllowedTerrainIds = new[] { "allowed-layer" },
                BlockedTerrainIds = new[] { "blocked-layer" },
                AllowedTerrainTags = new[] { "forest" },
                BlockedTerrainTags = new[] { "mountain" },
                AllowedTerrainLevels = new[] { 1 },
                BlockedTerrainLevels = new[] { 2 },
                RequiredNeighborOffsets = new[] { new Vector2Int(1, -1) },
                AllowHills = false,
                BlockEdgeTerrainTiles = true,
                RequiresFlatGround = true,
            };
        }

        private static void AssertResetOverride(TerrainPlacementRuleModule module)
        {
            Assert.That(module.MergeMode, Is.EqualTo(PlacementRuleMergeMode.Override));
            Assert.That(module.AllowedTerrainIds, Is.Empty);
            Assert.That(module.BlockedTerrainIds, Is.Empty);
            Assert.That(module.AllowedTerrainLevels, Is.Empty);
            Assert.That(module.BlockedTerrainLevels, Is.Empty);
            Assert.That(module.RequiredNeighborOffsets, Is.Empty);
            Assert.That(module.AllowHills, Is.True);
            Assert.That(module.BlockEdgeTerrainTiles, Is.False);
            Assert.That(module.RequiresFlatGround, Is.False);
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
