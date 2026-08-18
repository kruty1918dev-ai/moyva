#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
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

using Kruty1918.Moyva.Jsonization;
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
        public void ModuleCatalog_EveryOptionHasRuntimeConsumer()
        {
            foreach (BuildingModuleEditorDescriptor descriptor
                     in BuildingModuleEditorCatalog.Options)
            {
                Assert.IsTrue(
                    BuildingDefinitionCapabilities.HasRuntimeConsumer(
                        descriptor.ModuleType),
                    $"Модуль '{descriptor.ModuleType.Name}' є в picker, " +
                    "але не має runtime consumer.");
            }
        }

        [Test]
        public void BuildingDefinitionAsset_RuntimeRevisionChangesOnEditorMutation()
        {
            int before = BuildingDefinitionAsset.RuntimeRevision;
            var asset =
                MoyvaJsonObjectFactory.Create<BuildingDefinitionAsset>();
            try
            {
                asset.NotifyEditorDataChanged();
                Assert.AreNotEqual(
                    before,
                    BuildingDefinitionAsset.RuntimeRevision);
            }
            finally
            {
                MoyvaJsonObjectFactory.DestroyImmediate(asset);
            }
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
                    MoyvaJsonObjectFactory.Create<BuildingDefinitionAsset>();
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
                    MoyvaJsonRuntime.GetLegacyResource<BuildingDefinitionAsset>(
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

    [TestFixture]
    public sealed class BuildingModuleRuntimeCapabilityRegressionTests
    {
        [Test]
        public void GarrisonCapacity_UsesCastleDefenseAndHousingCapabilities()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new CastleBuildingModule
                    {
                        GarrisonCapacity = 4,
                    },
                    new DefenseBuildingModule
                    {
                        GarrisonCapacity = 6,
                    },
                    new HousingBuildingModule
                    {
                        Capacity = 8,
                        IsGarrisonCapable = true,
                    },
                },
            };

            Assert.AreEqual(
                8,
                BuildingDefinitionCapabilities.GetGarrisonCapacity(
                    definition));
        }

        [Test]
        public void Castle_RemainsStrictPerOwnerUniqueRegardlessOfLimitModule()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new CastleBuildingModule
                    {
                        IsCapital = true,
                    },
                    new BuildingPerPlayerLimitModule
                    {
                        MaxBuildingsPerPlayer = 99,
                        LimitScope = BuildingLimitScope.Global,
                        OverflowPolicy = BuildingLimitOverflowPolicy.RelocateExisting,
                    },
                },
            };

            Assert.IsTrue(
                BuildingDefinitionCapabilities.IsStrictPerOwnerUnique(
                    definition));
            Assert.AreEqual(
                1,
                BuildingDefinitionCapabilities.GetMaxBuildingsPerPlayer(
                    definition));
        }
    }

    [TestFixture]
    public sealed class MoyvaConstructionModuleSchemaV2Tests
    {
        [Test]
        public void TownHallAndCastle_ModulePresenceIsAuthoritative()
        {
            var townHall = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new TownHallBuildingModule
                    {
                        IsCentral = false,
                        BuildRadius = 9,
                    },
                },
            };
            var castle = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new CastleBuildingModule
                    {
                        IsCapital = false,
                        ExclusionRadius = 7,
                    },
                },
            };

            Assert.IsTrue(
                BuildingDefinitionCapabilities.IsTownHall(townHall));
            Assert.IsTrue(
                BuildingDefinitionCapabilities.IsCastle(castle));
            Assert.AreEqual(
                9,
                BuildingDefinitionCapabilities.GetInfluenceRadius(
                    townHall,
                    0));
            Assert.AreEqual(
                7,
                BuildingDefinitionCapabilities.GetInfluenceRadius(
                    castle,
                    0));
        }

        [Test]
        public void CanonicalGarrison_OverridesLegacyGarrisonFields()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new CastleBuildingModule
                    {
                        GarrisonCapacity = 20,
                    },
                    new DefenseBuildingModule
                    {
                        GarrisonCapacity = 30,
                    },
                    new GarrisonBuildingModule
                    {
                        Capacity = 5,
                    },
                },
            };

            Assert.AreEqual(
                5,
                BuildingDefinitionCapabilities.GetGarrisonCapacity(
                    definition));
        }

        [Test]
        public void WorkforcePriority_IsAuthoritativeOverProductionFallback()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new ProductionBuildingModule
                    {
                        Priority = 99,
                    },
                    new WorkforceBuildingModule
                    {
                        Priority = 7,
                    },
                },
            };

            Assert.AreEqual(
                7,
                BuildingDefinitionCapabilities.GetEconomyPriority(
                    definition));
        }

        [Test]
        public void CanonicalStorage_EmptyExplicitListDoesNotFallBackToLegacy()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new StorageBuildingModule
                    {
                        StorageKind = BuildingStorageKind.Food,
                        AcceptedResourceIds = Array.Empty<string>(),
                    },
                    new WarehouseBuildingModule
                    {
                        ResourceIds = new[] { "stone" },
                    },
                },
            };

            Assert.IsEmpty(
                BuildingDefinitionCapabilities
                    .GetAcceptedStorageResourceIds(definition));
        }

        [Test]
        public void ProductionValidation_RejectsDuplicateRecipeIds()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new ProductionBuildingModule
                    {
                        Recipes =
                            new List<ProductionRecipeDefinition>
                            {
                                new ProductionRecipeDefinition
                                {
                                    RecipeId = "same",
                                    RequiresWorkers = false,
                                    Outputs =
                                        new List<BuildingResourceAmount>
                                        {
                                            new BuildingResourceAmount
                                            {
                                                ResourceId = "wood",
                                                Amount = 1,
                                            },
                                        },
                                },
                                new ProductionRecipeDefinition
                                {
                                    RecipeId = "same",
                                    RequiresWorkers = false,
                                    Outputs =
                                        new List<BuildingResourceAmount>
                                        {
                                            new BuildingResourceAmount
                                            {
                                                ResourceId = "stone",
                                                Amount = 1,
                                            },
                                        },
                                },
                            },
                    },
                },
            };

            IReadOnlyList<BuildingValidationIssue> issues =
                BuildingModuleValidation.Validate(definition);

            Assert.IsTrue(
                HasIssue(
                    issues,
                    "INV_PRODUCTION_RECIPE_DUPLICATE_ID"));
        }

        [Test]
        public void FogRevealValidation_RejectsEnabledModuleWithNoEffect()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new FogRevealBuildingModule
                    {
                        RevealRadius = 3,
                        RevealOnBuilt = false,
                        RevealWhileActive = false,
                    },
                },
            };

            IReadOnlyList<BuildingValidationIssue> issues =
                BuildingModuleValidation.Validate(definition);

            Assert.IsTrue(
                HasIssue(
                    issues,
                    "INV_FOG_REVEAL_NO_EFFECT"));
        }

        private static bool HasIssue(
            IReadOnlyList<BuildingValidationIssue> issues,
            string code)
        {
            for (int index = 0;
                 index < (issues?.Count ?? 0);
                 index++)
            {
                if (issues[index]?.Code == code)
                    return true;
            }

            return false;
        }
    }

    [TestFixture]
    public sealed class MoyvaConstructionModuleRuntimeRobustnessTests
    {
        [Test]
        public void CanonicalPickerModules_HaveRuntimeConsumerAndEffect()
        {
            foreach (BuildingModuleEditorDescriptor descriptor
                     in BuildingModuleEditorCatalog.Options)
            {
                if (BuildingModuleEditorCatalog.IsLegacyModule(
                        descriptor.ModuleType))
                {
                    continue;
                }

                BuildingModuleDefinition module =
                    descriptor.Create();
                Assert.NotNull(
                    module,
                    $"Picker failed to create {descriptor.ModuleType.Name}.");

                Assert.IsTrue(
                    BuildingDefinitionCapabilities.HasRuntimeConsumer(
                        descriptor.ModuleType),
                    $"Canonical module '{descriptor.ModuleType.Name}' " +
                    "has no runtime consumer.");

                string effect =
                    BuildingDefinitionCapabilities
                        .GetModuleRuntimeEffectDescription(module);
                Assert.IsFalse(
                    string.IsNullOrWhiteSpace(effect));
                StringAssert.DoesNotContain(
                    "Немає зареєстрованого runtime consumer",
                    effect);
            }
        }

        [Test]
        public void ConstructionSavedPlacement_PreservesOwnerIdentity()
        {
            var placement =
                new ConstructionSavedPlacement(
                    new Vector2Int(7, 9),
                    "castle-01",
                    "faction-a");

            Assert.AreEqual(
                new Vector2Int(7, 9),
                placement.Position);
            Assert.AreEqual(
                "castle-01",
                placement.BuildingId);
            Assert.AreEqual(
                "faction-a",
                placement.OwnerId);
        }

        [Test]
        public void ModuleStatePersistenceContract_ExposesStableKeyedPayload()
        {
            Type contract =
                typeof(IConstructionModuleStatePersistence);

            Assert.NotNull(
                contract.GetProperty(
                    nameof(IConstructionModuleStatePersistence.StateKey)));
            Assert.NotNull(
                contract.GetMethod(
                    nameof(IConstructionModuleStatePersistence.CaptureState)));
            Assert.NotNull(
                contract.GetMethod(
                    nameof(IConstructionModuleStatePersistence.RestoreState)));
        }

        [Test]
        public void ProductionRecipeThatRequiresWorkers_FailsClosedAtZeroWorkers()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new ProductionBuildingModule
                    {
                        Recipes =
                            new List<ProductionRecipeDefinition>
                            {
                                new ProductionRecipeDefinition
                                {
                                    RecipeId = "worker-recipe",
                                    RequiresWorkers = true,
                                    Outputs =
                                        new List<BuildingResourceAmount>
                                        {
                                            new BuildingResourceAmount
                                            {
                                                ResourceId = "wood",
                                                Amount = 1,
                                            },
                                        },
                                },
                            },
                    },
                    new WorkforceBuildingModule
                    {
                        WorkersRequired = 0,
                    },
                },
            };

            IReadOnlyList<BuildingValidationIssue> issues =
                BuildingModuleValidation.Validate(definition);

            Assert.IsTrue(
                issues.Any(
                    issue =>
                        issue?.Code
                        == "INV_PRODUCTION_WORKERS_REQUIRED"));
        }

        [Test]
        public void CanonicalGarrisonCapacity_IsIndependentFromHousingCapacity()
        {
            var definition = new BuildingDefinition
            {
                Modules = new List<BuildingModuleDefinition>
                {
                    new HousingBuildingModule
                    {
                        Capacity = 20,
                        IsGarrisonCapable = true,
                    },
                    new GarrisonBuildingModule
                    {
                        Capacity = 4,
                    },
                },
            };

            Assert.AreEqual(
                4,
                BuildingDefinitionCapabilities
                    .GetGarrisonCapacity(definition));
        }

        [Test]
        public void ModuleSchema_IsVersioned()
        {
            Assert.GreaterOrEqual(
                BuildingDefinitionCapabilities.ModuleSchemaVersion,
                2);
        }
    }
}

#endif
