using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class BuildingDesignerEditorTests
    {
        [Test]
        public void ModuleCatalog_ContainsEveryConcreteModuleExactlyOnce()
        {
            Type[] moduleTypes = typeof(BuildingModuleDefinition).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract
                               && typeof(BuildingModuleDefinition).IsAssignableFrom(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToArray();
            Type[] catalogTypes = BuildingModuleEditorCatalog.Options
                .Select(option => option.ModuleType)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToArray();

            CollectionAssert.AreEqual(moduleTypes, catalogTypes);
            Assert.AreEqual(catalogTypes.Length, catalogTypes.Distinct().Count());
        }

        [Test]
        public void ModuleCatalog_ExposesPerPlayerLimitInUkrainianRulesCategory()
        {
            BuildingModuleEditorDescriptor descriptor = BuildingModuleEditorCatalog.Find(
                typeof(BuildingPerPlayerLimitModule));

            Assert.NotNull(descriptor);
            Assert.AreEqual("Правила", descriptor.Category);
            Assert.AreEqual("Ліміт будівель на гравця", descriptor.DisplayName);
            Assert.IsInstanceOf<BuildingPerPlayerLimitModule>(descriptor.Create());
        }

        [Test]
        public void ModuleCatalog_BlocksDuplicateModuleWithUkrainianReason()
        {
            var modules = new List<BuildingModuleDefinition>
            {
                new BuildingPerPlayerLimitModule { MaxBuildingsPerPlayer = 1 },
            };

            string reason = BuildingModuleEditorCatalog.GetConflictReason(
                modules,
                typeof(BuildingPerPlayerLimitModule));

            Assert.IsFalse(string.IsNullOrWhiteSpace(reason));
            StringAssert.Contains("вже додано", reason);
        }

        [Test]
        public void BuildingDesignerFields_HaveUkrainianLabelAndTooltipMetadata()
        {
            Type[] inspectedTypes =
            {
                typeof(BuildingIdentity),
                typeof(BuildingPresentation),
                typeof(BuildingFootprint),
                typeof(BuildingPlacementRules),
                typeof(BuildingConstructionData),
                typeof(BuildingRuntimeStats),
                typeof(BuildingPreviewSettings),
                typeof(BuildingModuleDefinition),
                typeof(BuildingResourceAmount),
                typeof(ProductionRecipeDefinition),
                typeof(TileRequirementDefinition),
                typeof(BuildingDefinition.BuildingConstructionCostEntry),
                typeof(BuildingValidationIssue),
            };

            Type[] moduleTypes = typeof(BuildingModuleDefinition).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract
                               && typeof(BuildingModuleDefinition).IsAssignableFrom(type))
                .ToArray();

            foreach (Type type in inspectedTypes.Concat(moduleTypes))
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (FieldInfo field in fields)
                {
                    Assert.NotNull(
                        field.GetCustomAttribute<LabelTextAttribute>(),
                        $"{type.Name}.{field.Name} не має українського LabelText.");
                    bool hasTooltip = field.GetCustomAttribute<PropertyTooltipAttribute>() != null
                                      || field.GetCustomAttribute<TooltipAttribute>() != null;
                    Assert.IsTrue(hasTooltip, $"{type.Name}.{field.Name} не має пояснення при наведенні.");
                }
            }
        }

        [Test]
        public void BuildingDefinitionAsset_UsesLocalizedTabsAndCustomModuleDrawerMarker()
        {
            FieldInfo modulesField = typeof(BuildingDefinitionAsset).GetField(
                nameof(BuildingDefinitionAsset.Modules));

            Assert.NotNull(modulesField);
            Assert.NotNull(modulesField.GetCustomAttribute<BuildingModuleListAttribute>());

            FieldInfo[] tabFields =
            {
                typeof(BuildingDefinitionAsset).GetField(nameof(BuildingDefinitionAsset.Identity)),
                typeof(BuildingDefinitionAsset).GetField(nameof(BuildingDefinitionAsset.Presentation)),
                typeof(BuildingDefinitionAsset).GetField(nameof(BuildingDefinitionAsset.Footprint)),
                typeof(BuildingDefinitionAsset).GetField(nameof(BuildingDefinitionAsset.Placement)),
                typeof(BuildingDefinitionAsset).GetField(nameof(BuildingDefinitionAsset.Construction)),
                typeof(BuildingDefinitionAsset).GetField(nameof(BuildingDefinitionAsset.RuntimeStats)),
                modulesField,
            };

            foreach (FieldInfo field in tabFields)
            {
                TabGroupAttribute tab = field?.GetCustomAttribute<TabGroupAttribute>();
                Assert.NotNull(tab, $"{field?.Name} не має TabGroup.");
                Assert.IsFalse(IsEnglishTabName(tab.GroupName), $"Вкладка {field?.Name} не локалізована.");
            }
        }

        [Test]
        public void PlacementMigration_UndoAndSaveReload_PreserveManagedReferenceModules()
        {
            string folderName =
                $"__MoyvaConstructionMigrationTests_{Guid.NewGuid():N}";
            string testFolder = $"Assets/{folderName}";
            string assetPath = null;
            try
            {
                AssetDatabase.CreateFolder("Assets", folderName);

                assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{testFolder}/Migration.asset");
                BuildingDefinitionAsset asset =
                    ScriptableObject.CreateInstance<BuildingDefinitionAsset>();
                asset.Identity.Id = "migration-regression";
                asset.Identity.DisplayName = "Migration Regression";
                asset.Placement.RequiresSettlementInfluence = true;
                asset.Placement.RequiresWaterNearby = true;
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();

                Type migrationType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(assembly => assembly.GetType(
                        "Kruty1918.Moyva.Construction.Editor.BuildingPlacementModuleMigrationUtility"))
                    .FirstOrDefault(type => type != null);
                Assert.NotNull(
                    migrationType,
                    "Construction editor migration assembly was not loaded.");
                MethodInfo migrateMethod = migrationType.GetMethod(
                    "Migrate",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof(BuildingDefinitionAsset),
                        typeof(bool),
                    },
                    null);
                FieldInfo currentVersionField = migrationType.GetField(
                    "CurrentVersion",
                    BindingFlags.Public | BindingFlags.Static);
                Assert.NotNull(migrateMethod);
                Assert.NotNull(currentVersionField);
                int currentVersion =
                    (int)currentVersionField.GetRawConstantValue();

                migrateMethod.Invoke(null, new object[] { asset, true });
                Undo.FlushUndoRecordObjects();
                Assert.AreEqual(
                    currentVersion,
                    asset.PlacementModuleMigrationVersion);
                Assert.IsTrue(asset.Modules.Count > 0);

                Undo.PerformUndo();
                Assert.AreEqual(0, asset.PlacementModuleMigrationVersion);
                Assert.AreEqual(0, asset.Modules.Count);

                migrateMethod.Invoke(null, new object[] { asset, false });
                AssetDatabase.SaveAssets();
                Resources.UnloadAsset(asset);
                AssetDatabase.ImportAsset(
                    assetPath,
                    ImportAssetOptions.ForceUpdate);

                BuildingDefinitionAsset reloaded =
                    AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(
                        assetPath);
                Assert.NotNull(reloaded);
                Assert.AreEqual(
                    currentVersion,
                    reloaded.PlacementModuleMigrationVersion);

                var influence = reloaded.Modules
                    .OfType<SettlementInfluenceRequirementBuildingModule>()
                    .Single();
                Assert.AreEqual(
                    PlacementRuleMergeMode.Override,
                    influence.MergeMode);
                Assert.IsTrue(influence.RequiresInfluence);

                var terrainRequirement = reloaded.Modules
                    .OfType<TileRequirementBuildingModule>()
                    .Single();
                Assert.AreEqual(1, terrainRequirement.Requirements.Length);
                Assert.AreEqual(
                    "water",
                    terrainRequirement.Requirements[0].TerrainTag);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(assetPath))
                    AssetDatabase.DeleteAsset(assetPath);
                if (AssetDatabase.IsValidFolder(testFolder))
                    AssetDatabase.DeleteAsset(testFolder);
            }
        }

        private static bool IsEnglishTabName(string value)
        {
            return value == "Basic"
                   || value == "Visual"
                   || value == "Footprint"
                   || value == "Placement"
                   || value == "Economy"
                   || value == "Runtime"
                   || value == "Modules";
        }
    }
}
