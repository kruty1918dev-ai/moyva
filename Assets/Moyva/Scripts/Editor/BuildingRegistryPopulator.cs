// ═══════════════════════════════════════════════════════════════════════════
// BuildingRegistryPopulator.cs
// Утиліта для наповнення BuildingRegistrySO стандартним набором будівель.
// Запуск: Unity → Moyva / Інструменти / Наповнити реєстр будівель
// ═══════════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Наповнює <see cref="BuildingRegistrySO"/> стандартним набором
    /// будівель з модулями відповідно до economy-runtime-logic-plan.
    /// <para>
    /// Запуск: <b>Moyva → Інструменти → Наповнити реєстр будівель</b>.
    /// </para>
    /// <para>
    /// Існуючі будівлі з тим самим ID не дублюються — оновлюються
    /// DisplayName, Category, Modules та правила розміщення.
    /// </para>
    /// </summary>
    public static class BuildingRegistryPopulator
    {
        // ─── Resource ID ─────────────────────────────────────────────
        // Мають відповідати _id у EconomyResourceDefinition-ассетах.
        // Якщо ресурсу ще немає — Id зберігається як placeholder.
        private const string Res_Wood       = "wood-materials-resources";
        private const string Res_Stone      = "stone-materials-resources";
        private const string Res_RawMeat    = "raw-meat-food-resources";
        private const string Res_Berries    = "raspberries-food-resources";
        private const string Res_Water      = "water";
        private const string Res_Coal       = "dark-armor-materials-resources";
        private const string Res_Wheat      = "wheat-bundle-food-resources";
        private const string Res_Planks     = "wooden-board-materials-resources";
        private const string Res_Tools      = "iron-ingot-materials-resources";

        // ─── Tile ID ─────────────────────────────────────────────────
        private const string Tile_Forest    = "forest-dense";
        private const string Tile_Water     = "water";

        // ═════════════════════════════════════════════════════════════
        //  Menu Item
        // ═════════════════════════════════════════════════════════════
        [MenuItem("Moyva/Інструменти/Наповнити реєстр будівель", priority = 200)]
        private static void Execute()
        {
            var registry = FindRegistry();
            if (registry == null)
            {
                EditorUtility.DisplayDialog(
                    "Помилка",
                    "BuildingRegistrySO не знайдено в проєкті.\n" +
                    "Створіть його через Create → Moyva → Construction → BuildingRegistry.",
                    "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Наповнити реєстр будівель",
                $"Буде додано/оновлено стандартний набір будівель у:\n{AssetDatabase.GetAssetPath(registry)}\n\n" +
                "Існуючі будівлі з відповідним ID будуть оновлені.\nНові будівлі будуть додані в кінець списку.\n\n" +
                "Продовжити?",
                "Так, наповнити", "Скасувати");

            if (!confirmed) return;

            PopulateAndSave(registry);

            Debug.Log($"[BuildingRegistryPopulator] Реєстр наповнено. Всього будівель: {registry.Buildings.Length}");
            EditorUtility.DisplayDialog(
                "Готово",
                $"Реєстр будівель оновлено.\nВсього записів: {registry.Buildings.Length}",
                "OK");
        }

        public static void PopulateAndSave(BuildingRegistrySO registry)
        {
            if (registry == null)
                return;

            Undo.RecordObject(registry, "Наповнити реєстр будівель");
            PopulateBuildings(registry);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }

        // ═════════════════════════════════════════════════════════════
        //  Визначення будівель
        // ═════════════════════════════════════════════════════════════
        private static void PopulateBuildings(BuildingRegistrySO registry)
        {
            var existing = new Dictionary<string, BuildingDefinition>(StringComparer.OrdinalIgnoreCase);
            if (registry.Buildings != null)
            {
                foreach (var b in registry.Buildings)
                {
                    if (b != null && !string.IsNullOrWhiteSpace(b.Id))
                        existing[b.Id] = b;
                }
            }

            var definitions = CreateStandardBuildings();

            foreach (var def in definitions)
            {
                if (existing.TryGetValue(def.Id, out var prev))
                {
                    // Оновити поля (зберігаємо Prefab та Icon)
                    prev.DisplayName = def.DisplayName;
                    prev.Category = def.Category;
                    prev.Modules = def.Modules;
                    prev.UseCustomTownHallRules = def.UseCustomTownHallRules;
                    prev.RequireTownHallInRange = def.RequireTownHallInRange;
                    prev.BlockIfTownHallAlreadyInRange = def.BlockIfTownHallAlreadyInRange;
                    prev.TownHallProximityRadiusOverride = def.TownHallProximityRadiusOverride;
                }
                else
                {
                    existing[def.Id] = def;
                }
            }

            var result = new List<BuildingDefinition>(existing.Values);
            result.Sort((a, b) =>
            {
                int catCmp = a.Category.CompareTo(b.Category);
                return catCmp != 0 ? catCmp : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
            });

            registry.Buildings = result.ToArray();
        }

        // ═════════════════════════════════════════════════════════════
        //  Фабрика стандартних будівель
        // ═════════════════════════════════════════════════════════════
        private static List<BuildingDefinition> CreateStandardBuildings()
        {
            return new List<BuildingDefinition>
            {
                // ─────────── Цивільні (Civilian) ──────────────────────
                Civilian("town-hall", "Ратуша",
                    requireTownHall: false,
                    blockTownHall: true,
                    modules: new BuildingModuleDefinition[]
                    {
                        new TownHallBuildingModule { BuildRadius = 25, IsCentral = true },
                        new WorkerlessBuildingModule(),
                    }),

                Civilian("house", "Хата",
                    modules: new BuildingModuleDefinition[]
                    {
                        new HousingBuildingModule { Capacity = 6, IsGarrisonCapable = false },
                    }),

                Civilian("castle", "Замок",
                    requireTownHall: false,
                    blockTownHall: true,
                    modules: new BuildingModuleDefinition[]
                    {
                        new CastleBuildingModule
                        {
                            IsCapital = true,
                            GarrisonCapacity = 20,
                            ExclusionRadius = 30,
                        },
                        new WorkerlessBuildingModule(),
                    }),

                Civilian("warehouse", "Склад",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WarehouseBuildingModule
                        {
                            ResourceIds = Array.Empty<string>(),
                            MaxCapacity = -1,
                        },
                    }),

                Civilian("barn", "Амбар",
                    modules: new BuildingModuleDefinition[]
                    {
                        new BarnBuildingModule
                        {
                            FoodResourceIds = Array.Empty<string>(),
                        },
                    }),

                Civilian("logistics-post", "Логістичний пост",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WorkerlessBuildingModule(),
                    }),

                Civilian("bridge", "Міст",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WorkerlessBuildingModule(),
                        new TileRequirementBuildingModule
                        {
                            Requirements = new[]
                            {
                                new TileRequirementDefinition { TileId = Tile_Water, Radius = 1, MinimumTileCount = 1 },
                            },
                        },
                    }),

                // ─────────── Індустріальні (Industrial) ───────────────
                Industrial("gatherer-hut", "Хатина збирача", Res_Berries,
                    workers: 2, priority: 50,
                    tileReqs: new[]
                    {
                        new TileRequirementDefinition { TileId = Tile_Forest, Radius = 4, MinimumTileCount = 2 },
                    }),

                Industrial("well", "Криниця", Res_Water,
                    workers: 1, priority: 60,
                    tileReqs: new[]
                    {
                        new TileRequirementDefinition { TileId = Tile_Water, Radius = 3, MinimumTileCount = 1 },
                    }),

                Industrial("hunter-hut", "Хатина мисливця", Res_RawMeat,
                    workers: 2, priority: 55,
                    tileReqs: new[]
                    {
                        new TileRequirementDefinition { TileId = Tile_Forest, Radius = 5, MinimumTileCount = 3 },
                    }),

                Industrial("lumberjack-hut", "Хатина лісоруба", Res_Wood,
                    workers: 2, priority: 45,
                    tileReqs: new[]
                    {
                        new TileRequirementDefinition { TileId = Tile_Forest, Radius = 4, MinimumTileCount = 3 },
                    }),

                Industrial("quarry", "Каменоломня", Res_Stone,
                    workers: 3, priority: 40),

                Industrial("coal-mine", "Вугільна шахта", Res_Coal,
                    workers: 3, priority: 35),

                Industrial("farm", "Ферма", Res_Wheat,
                    workers: 3, priority: 65),

                Industrial("sawmill", "Пилорама", Res_Planks,
                    workers: 2, priority: 50),

                Industrial("smithy", "Кузня", Res_Tools,
                    workers: 2, priority: 45),

                // ─────────── Військові (Military) ─────────────────────
                Military("barracks", "Казарма",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WorkerlessBuildingModule(),
                    }),

                Military("stable", "Конюшня",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WorkerlessBuildingModule(),
                    }),

                Military("archery-range", "Стрілецький майдан",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WorkerlessBuildingModule(),
                    }),

                Military("watchtower", "Оглядова вежа",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WorkerlessBuildingModule(),
                    }),

                // ─────────── Стіни / Ворота (Walls) ──────────────────
                Wall("wall", "Стіна",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WallBuildingModule { HitPoints = 500, IsPassable = false },
                    }),

                Wall("gate", "Ворота",
                    modules: new BuildingModuleDefinition[]
                    {
                        new GateBuildingModule { HitPoints = 400, OpenSpeed = 1.0f },
                    }),

                Wall("moat", "Рів",
                    modules: new BuildingModuleDefinition[]
                    {
                        new WallBuildingModule { HitPoints = 0, IsPassable = false },
                        new WorkerlessBuildingModule(),
                    }),
            };
        }

        // ═════════════════════════════════════════════════════════════
        //  Хелпери для скорочення
        // ═════════════════════════════════════════════════════════════

        private static BuildingDefinition Civilian(
            string id, string displayName,
            bool requireTownHall = true,
            bool blockTownHall = false,
            BuildingModuleDefinition[] modules = null)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = displayName,
                Category = BuildingCategory.Civilian,
                Modules = modules != null ? new List<BuildingModuleDefinition>(modules) : new List<BuildingModuleDefinition>(),
                RequireTownHallInRange = requireTownHall,
                BlockIfTownHallAlreadyInRange = blockTownHall,
            };
        }

        /// <summary>
        /// Створює індустріальну будівлю з <see cref="ProductionBuildingModule"/>
        /// і опціональними вимогами до тайлів.
        /// </summary>
        private static BuildingDefinition Industrial(
            string id, string displayName,
            string resourceId,
            int workers, int priority,
            TileRequirementDefinition[] tileReqs = null)
        {
            var modules = new List<BuildingModuleDefinition>
            {
                new ProductionBuildingModule
                {
                    ResourceId = resourceId,
                    WorkersRequired = workers,
                    Priority = priority,
                },
            };

            if (tileReqs != null && tileReqs.Length > 0)
            {
                modules.Add(new TileRequirementBuildingModule { Requirements = tileReqs });
            }

            return new BuildingDefinition
            {
                Id = id,
                DisplayName = displayName,
                Category = BuildingCategory.Industrial,
                Modules = modules,
                RequireTownHallInRange = true,
            };
        }

        private static BuildingDefinition Military(
            string id, string displayName,
            BuildingModuleDefinition[] modules = null)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = displayName,
                Category = BuildingCategory.Military,
                Modules = modules != null ? new List<BuildingModuleDefinition>(modules) : new List<BuildingModuleDefinition>(),
                RequireTownHallInRange = true,
            };
        }

        private static BuildingDefinition Wall(
            string id, string displayName,
            BuildingModuleDefinition[] modules = null)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = displayName,
                Category = BuildingCategory.Walls,
                Modules = modules != null ? new List<BuildingModuleDefinition>(modules) : new List<BuildingModuleDefinition>(),
                RequireTownHallInRange = true,
            };
        }

        // ═════════════════════════════════════════════════════════════
        //  Пошук реєстру в проєкті
        // ═════════════════════════════════════════════════════════════
        private static BuildingRegistrySO FindRegistry()
        {
            var guids = AssetDatabase.FindAssets("t:BuildingRegistrySO");
            if (guids.Length == 0) return null;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
        }
    }
}
