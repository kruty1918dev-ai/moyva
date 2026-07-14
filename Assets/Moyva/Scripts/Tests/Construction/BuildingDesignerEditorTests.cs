using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;
using NUnit.Framework;
using Sirenix.OdinInspector;
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
