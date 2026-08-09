using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class BuildingModuleListDrawer
        : OdinAttributeDrawer<BuildingModuleListAttribute, List<BuildingModuleDefinition>>
    {
        private readonly Dictionary<BuildingModuleDefinition, bool> _expanded = new();
        private bool _showAdvanced;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            BuildingDefinitionAsset asset = GetTargetAsset();
            List<BuildingModuleDefinition> modules = ValueEntry.SmartValue;
            if (modules == null)
            {
                modules = new List<BuildingModuleDefinition>();
                ValueEntry.SmartValue = modules;
            }

            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            GUILayout.Label(
                new GUIContent(
                    $"Модулі будівлі ({modules.Count})",
                    "Що робить: Збирає незалежні можливості та правила будівлі.\nВплив у грі: Модулі визначають житло, виробництво, оборону, видимість і ліміти."),
                EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(
                    new GUIContent(
                        "Додати модуль",
                        "Що робить: Відкриває каталог усіх модулів із пошуком.\nВплив у грі: Додає нову можливість або правило до цієї будівлі."),
                    GUILayout.Width(130f)))
            {
                Rect buttonRect = GUILayoutUtility.GetLastRect();
                ShowModulePicker(
                    buttonRect,
                    asset,
                    modules);
            }
            if (GUILayout.Button(
                    new GUIContent(
                        "Шаблони",
                        "Швидко додає узгоджений набір модулів для типового призначення будівлі."),
                    GUILayout.Width(82f)))
            {
                Rect presetRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(
                    presetRect,
                    new ModulePresetPopup(
                        preset => ApplyPreset(
                            asset,
                            modules,
                            preset)));
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Виправити",
                        "Нормалізує старі/дубльовані модулі у просту canonical схему без зміни основного задуму будівлі."),
                    GUILayout.Width(82f)))
            {
                NormalizeModules(asset, modules);
            }

            _showAdvanced = GUILayout.Toggle(
                _showAdvanced,
                new GUIContent(
                    "Розширені",
                    "Показує compatibility/службові поля. Для звичайного налаштування не потрібні."),
                EditorStyles.miniButton,
                GUILayout.Width(90f));
            SirenixEditorGUI.EndBoxHeader();

            EditorGUILayout.HelpBox(
                "Додавайте лише потрібні можливості. Недоступні або повторні модулі будуть заблоковані з поясненням.",
                MessageType.Info);

            if (modules.Count == 0)
            {
                GUILayout.Space(18f);
                var centered = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                };
                EditorGUILayout.LabelField(
                    "Ця будівля поки без модулів",
                    centered,
                    GUILayout.Height(24f));
                EditorGUILayout.LabelField(
                    "Додайте лише ту поведінку, яка реально потрібна в грі.",
                    EditorStyles.centeredGreyMiniLabel,
                    GUILayout.Height(20f));

                if (GUILayout.Button(
                        "+ Додати перший модуль",
                        GUILayout.Height(42f)))
                {
                    Rect buttonRect = GUILayoutUtility.GetLastRect();
                    ShowModulePicker(buttonRect, asset, modules);
                }

                GUILayout.Space(18f);
                SirenixEditorGUI.EndBox();
                return;
            }

            for (int index = 0; index < modules.Count; index++)
            {
                BuildingModuleDefinition module = modules[index];
                if (module == null)
                {
                    DrawMissingModule(asset, modules, index);
                    continue;
                }

                InspectorProperty elementProperty = index < Property.Children.Count
                    ? Property.Children[index]
                    : null;
                DrawModuleCard(asset, modules, module, elementProperty, index);
            }

            SirenixEditorGUI.EndBox();
        }

        private void ShowModulePicker(
            Rect anchorRect,
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules)
        {
            PopupWindow.Show(
                anchorRect,
                new BuildingModulePickerPopup(
                    asset,
                    modules,
                    module =>
                    {
                        if (module != null)
                            _expanded[module] = true;
                        RefreshPropertyTree();
                    }));
        }

        private void DrawModuleCard(
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules,
            BuildingModuleDefinition module,
            InspectorProperty elementProperty,
            int index)
        {
            BuildingModuleEditorDescriptor descriptor = BuildingModuleEditorCatalog.Find(module.GetType());
            string title = descriptor?.DisplayName ?? module.GetType().Name;
            string category = descriptor?.Category ?? "Інше";
            string description = descriptor?.Description ?? "Опис для цього модуля ще не додано.";
            bool expanded =
                _expanded.TryGetValue(module, out bool stored)
                && stored;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            bool nextExpanded = EditorGUILayout.Foldout(
                expanded,
                new GUIContent(title, description),
                true,
                EditorStyles.foldoutHeader);
            if (nextExpanded != expanded)
                _expanded[module] = nextExpanded;

            GUILayout.Label(
                new GUIContent(category, $"Категорія модуля: {category}."),
                EditorStyles.miniLabel,
                GUILayout.Width(82f));

            EditorGUI.BeginChangeCheck();
            bool enabled = GUILayout.Toggle(
                module.IsEnabled,
                new GUIContent("Активний", "Що робить: Тимчасово вмикає або вимикає модуль без його видалення.\nВплив у грі: Вимкнений модуль повністю ігнорується runtime та валідацією."),
                GUILayout.Width(76f));
            if (EditorGUI.EndChangeCheck())
            {
                RecordChange(asset, "Змінити активність модуля");
                module.IsEnabled = enabled;
                CompleteChange(asset);
            }

            using (new EditorGUI.DisabledScope(index <= 0))
            {
                if (GUILayout.Button(new GUIContent("↑", "Перемістити модуль вище."), GUILayout.Width(24f)))
                {
                    MoveModule(asset, modules, index, index - 1);
                    return;
                }
            }
            using (new EditorGUI.DisabledScope(index >= modules.Count - 1))
            {
                if (GUILayout.Button(new GUIContent("↓", "Перемістити модуль нижче."), GUILayout.Width(24f)))
                {
                    MoveModule(asset, modules, index, index + 1);
                    return;
                }
            }
            if (GUILayout.Button(new GUIContent("×", "Видалити цей модуль із будівлі."), GUILayout.Width(24f)))
            {
                RemoveModule(asset, modules, module, index);
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                description,
                EditorStyles.wordWrappedMiniLabel);

            string runtimeEffect =
                BuildingDefinitionCapabilities
                    .GetModuleRuntimeEffectDescription(module);
            EditorGUILayout.HelpBox(
                $"Runtime: {runtimeEffect}",
                BuildingDefinitionCapabilities
                    .HasRuntimeConsumer(module.GetType())
                    ? MessageType.Info
                    : MessageType.Error);

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(
                    "LIVE: зміни asset доходять до construction runtime; " +
                    "ефект застосовується до наступної перевірки/розміщення.",
                    EditorStyles.wordWrappedMiniLabel);
            }

            if (nextExpanded && elementProperty != null)
            {
                EditorGUI.indentLevel++;
                if (module is TerrainPlacementRuleModule terrainModule)
                {
                    TerrainPlacementRuleModuleEditorGUI.Draw(
                        asset,
                        terrainModule,
                        elementProperty,
                        ApplyModuleChange,
                        RefreshPropertyTree);
                }
                else if (module is FogRevealBuildingModule fogModule)
                {
                    DrawFogRevealModuleFields(
                        asset,
                        fogModule,
                        elementProperty);
                }
                else
                {
                    DrawDefaultModuleFields(
                        module,
                        elementProperty);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private enum FogRevealEditorMode
        {
            Once = 0,
            WhileActive = 1,
        }

        private void DrawFogRevealModuleFields(
            BuildingDefinitionAsset asset,
            FogRevealBuildingModule fog,
            InspectorProperty elementProperty)
        {
            if (_showAdvanced)
            {
                DrawDefaultModuleFields(
                    fog,
                    elementProperty);
                return;
            }

            FogRevealEditorMode current =
                fog.RevealWhileActive
                    ? FogRevealEditorMode.WhileActive
                    : FogRevealEditorMode.Once;

            EditorGUI.BeginChangeCheck();
            FogRevealEditorMode next =
                (FogRevealEditorMode)EditorGUILayout.EnumPopup(
                    new GUIContent(
                        "Поведінка",
                        "Once — відкрити область після побудови один раз. WhileActive — тримати область видимою, доки будівля існує."),
                    current);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyModuleChange(
                    asset,
                    "Змінити режим Fog Reveal",
                    () =>
                    {
                        fog.RevealOnBuilt =
                            next == FogRevealEditorMode.Once;
                        fog.RevealWhileActive =
                            next == FogRevealEditorMode.WhileActive;
                        fog.OnlyAfterConstructionComplete = true;
                    });
            }

            DrawDefaultModuleFields(
                fog,
                elementProperty);
        }

        private void DrawDefaultModuleFields(
            BuildingModuleDefinition module,
            InspectorProperty elementProperty)
        {
            for (int childIndex = 0;
                 childIndex < elementProperty.Children.Count;
                 childIndex++)
            {
                InspectorProperty child =
                    elementProperty.Children[childIndex];

                if (string.Equals(
                        child.Name,
                        nameof(BuildingModuleDefinition.IsEnabled),
                        StringComparison.Ordinal))
                    continue;

                if (!_showAdvanced
                    && IsAdvancedModuleField(module, child.Name))
                    continue;

                child.Draw(child.Label);
            }
        }

        private static bool IsAdvancedModuleField(
            BuildingModuleDefinition module,
            string fieldName)
        {
            if (fieldName == nameof(BuildingModuleDefinition.SingletonScope))
                return true;

            if (module is TownHallBuildingModule
                && fieldName == nameof(TownHallBuildingModule.IsCentral))
                return true;

            if (module is CastleBuildingModule
                && (fieldName == nameof(CastleBuildingModule.IsCapital)
                    || fieldName == nameof(CastleBuildingModule.GarrisonCapacity)))
                return true;

            if (module is HousingBuildingModule
                && fieldName == nameof(HousingBuildingModule.IsGarrisonCapable))
                return true;

            if (module is DefenseBuildingModule
                && fieldName == nameof(DefenseBuildingModule.GarrisonCapacity))
                return true;

            if (module is ProductionBuildingModule
                && (fieldName == nameof(ProductionBuildingModule.WorkersRequired)
                    || fieldName == nameof(ProductionBuildingModule.Priority)))
                return true;

            if (module is FogRevealBuildingModule
                && (fieldName == nameof(FogRevealBuildingModule.RevealOnBuilt)
                    || fieldName == nameof(FogRevealBuildingModule.RevealWhileActive)
                    || fieldName == nameof(FogRevealBuildingModule.OnlyAfterConstructionComplete)))
                return true;

            return false;
        }

        private void NormalizeModules(
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules)
        {
            if (modules == null)
                return;

            RecordChange(asset, "Нормалізувати модулі будівлі");

            modules.RemoveAll(module => module == null);

            // Keep the first module of each concrete type so repair never
            // silently prefers a later accidental duplicate.
            var seenTypes = new HashSet<Type>();
            for (int index = 0; index < modules.Count;)
            {
                Type type = modules[index].GetType();
                if (!seenTypes.Add(type))
                {
                    modules.RemoveAt(index);
                    continue;
                }

                index++;
            }

            StorageBuildingModule storage =
                FindModule<StorageBuildingModule>(modules);
            WarehouseBuildingModule warehouse =
                FindModule<WarehouseBuildingModule>(modules);
            BarnBuildingModule barn =
                FindModule<BarnBuildingModule>(modules);

            if (storage == null && warehouse != null)
            {
                storage = new StorageBuildingModule
                {
                    StorageKind = BuildingStorageKind.Any,
                    Capacity = warehouse.MaxCapacity,
                    AcceptedResourceIds =
                        warehouse.ResourceIds
                        ?? Array.Empty<string>(),
                };
                modules.Add(storage);
            }
            else if (storage == null && barn != null)
            {
                storage = new StorageBuildingModule
                {
                    StorageKind = BuildingStorageKind.Food,
                    Capacity = -1,
                    AcceptedResourceIds =
                        barn.FoodResourceIds
                        ?? Array.Empty<string>(),
                };
                modules.Add(storage);
            }

            if (storage != null)
            {
                RemoveModules<WarehouseBuildingModule>(modules);
                RemoveModules<BarnBuildingModule>(modules);
                if (storage.Capacity < -1)
                    storage.Capacity = -1;
            }

            ProductionBuildingModule production =
                FindModule<ProductionBuildingModule>(modules);
            WorkforceBuildingModule workforce =
                FindModule<WorkforceBuildingModule>(modules);
            bool wasWorkerless =
                FindModule<WorkerlessBuildingModule>(modules) != null;

            if (wasWorkerless)
            {
                if (production?.Recipes != null)
                {
                    for (int index = 0;
                         index < production.Recipes.Count;
                         index++)
                    {
                        if (production.Recipes[index] != null)
                            production.Recipes[index].RequiresWorkers = false;
                    }
                }

                if (workforce == null)
                {
                    workforce = new WorkforceBuildingModule();
                    modules.Add(workforce);
                }

                workforce.WorkersRequired = 0;
                RemoveModules<WorkerlessBuildingModule>(modules);
            }

            if (production != null)
            {
                bool recipeNeedsWorkers =
                    ProductionModuleRequiresWorkers(production);

                if (workforce == null
                    && (production.WorkersRequired > 0
                        || production.Priority != 0
                        || recipeNeedsWorkers))
                {
                    workforce = new WorkforceBuildingModule
                    {
                        WorkersRequired =
                            Mathf.Max(
                                recipeNeedsWorkers ? 1 : 0,
                                production.WorkersRequired),
                        Priority =
                            Mathf.Max(0, production.Priority),
                    };
                    modules.Add(workforce);
                }

                if (workforce != null)
                {
                    production.WorkersRequired = 0;
                    production.Priority = 0;
                }

                NormalizeRecipes(production);
            }

            int legacyGarrisonCapacity = 0;
            CastleBuildingModule castle =
                FindModule<CastleBuildingModule>(modules);
            DefenseBuildingModule defense =
                FindModule<DefenseBuildingModule>(modules);
            HousingBuildingModule housing =
                FindModule<HousingBuildingModule>(modules);

            if (castle != null)
            {
                castle.IsCapital = true;
                legacyGarrisonCapacity = Mathf.Max(
                    legacyGarrisonCapacity,
                    castle.GarrisonCapacity);
            }

            if (defense != null)
            {
                legacyGarrisonCapacity = Mathf.Max(
                    legacyGarrisonCapacity,
                    defense.GarrisonCapacity);
            }

            if (housing?.IsGarrisonCapable == true)
            {
                legacyGarrisonCapacity = Mathf.Max(
                    legacyGarrisonCapacity,
                    Mathf.Max(1, housing.Capacity));
            }

            GarrisonBuildingModule garrison =
                FindModule<GarrisonBuildingModule>(modules);
            if (garrison == null && legacyGarrisonCapacity > 0)
            {
                garrison = new GarrisonBuildingModule
                {
                    Capacity =
                        Mathf.Max(1, legacyGarrisonCapacity),
                };
                modules.Add(garrison);
            }
            else if (garrison != null)
            {
                garrison.Capacity =
                    Mathf.Max(1, garrison.Capacity);
            }

            if (garrison != null)
            {
                if (castle != null)
                    castle.GarrisonCapacity = 0;
                if (defense != null)
                    defense.GarrisonCapacity = 0;
                if (housing != null)
                    housing.IsGarrisonCapable = false;
            }

            TownHallBuildingModule townHall =
                FindModule<TownHallBuildingModule>(modules);
            if (townHall != null)
                townHall.IsCentral = true;

            FogRevealBuildingModule fog =
                FindModule<FogRevealBuildingModule>(modules);
            if (fog != null)
            {
                fog.RevealRadius =
                    Mathf.Max(1, fog.RevealRadius);
                if (fog.RevealWhileActive)
                {
                    fog.RevealOnBuilt = false;
                }
                else if (!fog.RevealOnBuilt)
                {
                    fog.RevealOnBuilt = true;
                }
                fog.OnlyAfterConstructionComplete = true;
            }

            CompleteChange(asset);

            Debug.Log(
                $"[MoyvaConstructionModules] normalize " +
                $"building={(asset != null ? asset.name : "<unknown>")} " +
                $"modules={modules.Count}");
        }

        private static TModule FindModule<TModule>(
            List<BuildingModuleDefinition> modules)
            where TModule : BuildingModuleDefinition
        {
            for (int index = 0;
                 index < (modules?.Count ?? 0);
                 index++)
            {
                if (modules[index] is TModule typed)
                    return typed;
            }

            return null;
        }

        private static void RemoveModules<TModule>(
            List<BuildingModuleDefinition> modules)
            where TModule : BuildingModuleDefinition
        {
            for (int index = modules.Count - 1;
                 index >= 0;
                 index--)
            {
                if (modules[index] is TModule)
                    modules.RemoveAt(index);
            }
        }

        private static bool ProductionModuleRequiresWorkers(
            ProductionBuildingModule production)
        {
            if (production?.Recipes == null
                || production.Recipes.Count == 0)
            {
                return production != null
                    && production.WorkersRequired > 0;
            }

            for (int index = 0;
                 index < production.Recipes.Count;
                 index++)
            {
                if (production.Recipes[index]?.RequiresWorkers == true)
                    return true;
            }

            return false;
        }

        private static void NormalizeRecipes(
            ProductionBuildingModule production)
        {
            if (production?.Recipes == null)
                return;

            var usedIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int index = 0;
                 index < production.Recipes.Count;
                 index++)
            {
                ProductionRecipeDefinition recipe =
                    production.Recipes[index];
                if (recipe == null)
                    continue;

                string baseId =
                    string.IsNullOrWhiteSpace(recipe.RecipeId)
                        ? $"recipe-{index + 1}"
                        : recipe.RecipeId.Trim();
                string id = baseId;
                int suffix = 2;
                while (!usedIds.Add(id))
                {
                    id = $"{baseId}-{suffix}";
                    suffix++;
                }

                recipe.RecipeId = id;
                recipe.TurnsPerCycle =
                    Mathf.Max(1, recipe.TurnsPerCycle);
                NormalizeResourceAmounts(recipe.Inputs);
                NormalizeResourceAmounts(recipe.Outputs);
            }
        }

        private static void NormalizeResourceAmounts(
            List<BuildingResourceAmount> entries)
        {
            if (entries == null)
                return;

            for (int index = entries.Count - 1;
                 index >= 0;
                 index--)
            {
                BuildingResourceAmount entry = entries[index];
                if (entry == null)
                {
                    entries.RemoveAt(index);
                    continue;
                }

                entry.ResourceId = entry.ResourceId?.Trim();
                entry.Amount = Mathf.Max(1, entry.Amount);
            }
        }

        private void ApplyPreset(
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules,
            string preset)
        {
            if (modules == null || string.IsNullOrWhiteSpace(preset))
                return;

            List<BuildingModuleDefinition> candidates =
                BuildPresetModules(preset);
            if (candidates.Count == 0)
                return;

            var simulated =
                new List<BuildingModuleDefinition>(modules);
            for (int index = 0; index < candidates.Count; index++)
            {
                BuildingModuleDefinition candidate = candidates[index];
                Type type = candidate.GetType();
                if (ContainsModuleType(simulated, type))
                    continue;

                string conflict =
                    BuildingModuleEditorCatalog.GetConflictReason(
                        simulated,
                        type);
                if (!string.IsNullOrWhiteSpace(conflict))
                {
                    Debug.LogWarning(
                        $"[MoyvaConstructionModules] preset '{preset}' " +
                        $"не застосовано: {conflict}",
                        asset);
                    return;
                }

                simulated.Add(candidate);
            }

            RecordChange(asset, "Застосувати шаблон модулів");
            for (int index = 0; index < candidates.Count; index++)
            {
                BuildingModuleDefinition candidate = candidates[index];
                if (!ContainsModuleType(modules, candidate.GetType()))
                    modules.Add(candidate);
            }

            CompleteChange(asset);
        }

        private static List<BuildingModuleDefinition>
            BuildPresetModules(string preset)
        {
            var result = new List<BuildingModuleDefinition>();
            switch (preset)
            {
                case "housing":
                    result.Add(
                        new HousingBuildingModule
                        {
                            Capacity = 4,
                        });
                    break;

                case "production":
                    result.Add(new ProductionBuildingModule());
                    result.Add(
                        new WorkforceBuildingModule
                        {
                            WorkersRequired = 1,
                            Priority = 10,
                        });
                    break;

                case "storage":
                    result.Add(
                        new StorageBuildingModule
                        {
                            StorageKind = BuildingStorageKind.Any,
                            Capacity = 100,
                        });
                    break;

                case "defense":
                    result.Add(
                        new DefenseBuildingModule
                        {
                            Armor = 2,
                            AttackRange = 5,
                            AttackDamage = 10,
                        });
                    break;

                case "garrison":
                    result.Add(
                        new GarrisonBuildingModule
                        {
                            Capacity = 4,
                        });
                    break;

                case "townhall":
                    result.Add(
                        new TownHallBuildingModule
                        {
                            BuildRadius = 12,
                            IsCentral = true,
                        });
                    result.Add(
                        new SettlementCenterBuildingModule
                        {
                            InfluenceRadius = 12,
                            MinimumDistanceFromOtherCenters = 8,
                        });
                    break;

                case "castle":
                    result.Add(
                        new CastleBuildingModule
                        {
                            IsCapital = true,
                            ExclusionRadius = 10,
                        });
                    result.Add(
                        new SettlementCenterBuildingModule
                        {
                            InfluenceRadius = 15,
                            MinimumDistanceFromOtherCenters = 12,
                        });
                    result.Add(
                        new GarrisonBuildingModule
                        {
                            Capacity = 8,
                        });
                    result.Add(
                        new BuildingPerPlayerLimitModule
                        {
                            MaxBuildingsPerPlayer = 1,
                            LimitScope = BuildingLimitScope.PerOwner,
                            OverflowPolicy = BuildingLimitOverflowPolicy.Block,
                        });
                    break;
            }

            return result;
        }

        private static bool ContainsModuleType(
            List<BuildingModuleDefinition> modules,
            Type type)
        {
            for (int index = 0; index < (modules?.Count ?? 0); index++)
            {
                if (modules[index]?.GetType() == type)
                    return true;
            }

            return false;
        }

        private sealed class ModulePresetPopup
            : PopupWindowContent
        {
            private readonly Action<string> _apply;

            public ModulePresetPopup(Action<string> apply)
            {
                _apply = apply;
            }

            public override Vector2 GetWindowSize()
                => new Vector2(330f, 340f);

            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.LabelField(
                    "Швидкі шаблони",
                    EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    "Шаблон додає лише відсутні сумісні модулі. Після цього залишиться налаштувати конкретні ресурси/числа.",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(6f);

                DrawPreset("Житло", "housing");
                DrawPreset("Виробництво", "production");
                DrawPreset("Сховище", "storage");
                DrawPreset("Оборонна споруда", "defense");
                DrawPreset("Гарнізон", "garrison");
                DrawPreset("Ратуша / центр", "townhall");
                DrawPreset("Замок", "castle");
            }

            private void DrawPreset(
                string label,
                string key)
            {
                if (!GUILayout.Button(label))
                    return;

                _apply?.Invoke(key);
                editorWindow.Close();
            }
        }

        private void ApplyModuleChange(
            BuildingDefinitionAsset asset,
            string undoName,
            Action mutation)
        {
            if (mutation == null)
                return;

            RecordChange(asset, undoName);
            mutation();
            CompleteChange(asset);
        }

        private void DrawMissingModule(
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules,
            int index)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.HelpBox(
                "Посилання на модуль втрачено. Видаліть порожній запис і додайте потрібний модуль повторно.",
                MessageType.Error);
            if (GUILayout.Button("Видалити", GUILayout.Width(80f)))
            {
                RecordChange(asset, "Видалити втрачений модуль");
                modules.RemoveAt(index);
                CompleteChange(asset);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MoveModule(
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules,
            int from,
            int to)
        {
            RecordChange(asset, "Змінити порядок модулів");
            BuildingModuleDefinition item = modules[from];
            modules.RemoveAt(from);
            modules.Insert(to, item);
            CompleteChange(asset);
            GUIUtility.ExitGUI();
        }

        private void RemoveModule(
            BuildingDefinitionAsset asset,
            List<BuildingModuleDefinition> modules,
            BuildingModuleDefinition module,
            int index)
        {
            BuildingModuleEditorDescriptor descriptor = BuildingModuleEditorCatalog.Find(module.GetType());
            string name = descriptor?.DisplayName ?? module.GetType().Name;
            if (!EditorUtility.DisplayDialog(
                    "Видалення модуля",
                    $"Видалити «{name}» із цієї будівлі?",
                    "Видалити",
                    "Скасувати"))
            {
                return;
            }

            RecordChange(asset, "Видалити модуль будівлі");
            modules.RemoveAt(index);
            _expanded.Remove(module);
            CompleteChange(asset);
            GUIUtility.ExitGUI();
        }

        private BuildingDefinitionAsset GetTargetAsset()
        {
            var targets = Property.Tree.WeakTargets;
            for (int index = 0; index < targets.Count; index++)
            {
                if (targets[index] is BuildingDefinitionAsset asset)
                    return asset;
            }

            return Selection.activeObject as BuildingDefinitionAsset;
        }

        private void RecordChange(BuildingDefinitionAsset asset, string undoName)
        {
            if (asset != null)
                Undo.RecordObject(asset, undoName);
        }

        private void CompleteChange(BuildingDefinitionAsset asset)
        {
            if (asset != null)
            {
                asset.NotifyEditorDataChanged();
                EditorUtility.SetDirty(asset);
            }
            RefreshPropertyTree();
        }

        private void RefreshPropertyTree()
        {
            Property.Tree.UpdateTree();
            GUIHelper.RequestRepaint();
        }

        private sealed class BuildingModulePickerPopup : PopupWindowContent
        {
            private readonly BuildingDefinitionAsset _asset;
            private readonly List<BuildingModuleDefinition> _modules;
            private readonly Action<BuildingModuleDefinition> _onAdded;
            private Vector2 _scroll;
            private string _search = string.Empty;
            private bool _showLegacy;

            public BuildingModulePickerPopup(
                BuildingDefinitionAsset asset,
                List<BuildingModuleDefinition> modules,
                Action<BuildingModuleDefinition> onAdded)
            {
                _asset = asset;
                _modules = modules;
                _onAdded = onAdded;
            }

            public override Vector2 GetWindowSize() => new(560f, 520f);

            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.LabelField("Каталог модулів", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    "Оберіть можливість або правило для цієї будівлі. Недоступні варіанти пояснюють причину блокування.",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(5f);

                GUI.SetNextControlName("BuildingModuleSearch");
                _search = EditorGUILayout.TextField(
                    new GUIContent(
                        "Пошук",
                        "Що робить: Фільтрує модулі за українською назвою, описом або C#-типом.\nВплив у грі: Не змінює дані будівлі."),
                    _search);

                _showLegacy = EditorGUILayout.ToggleLeft(
                    new GUIContent(
                        "Показати застарілі compatibility-модулі",
                        "Warehouse/Barn/Workerless підтримуються для старих asset, але для нових використовуйте Storage/Workforce."),
                    _showLegacy);

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                string previousCategory = null;
                IReadOnlyList<BuildingModuleEditorDescriptor> options = BuildingModuleEditorCatalog.Options;
                for (int index = 0; index < options.Count; index++)
                {
                    BuildingModuleEditorDescriptor option = options[index];
                    if (!option.MatchesSearch(_search))
                        continue;
                    if (!_showLegacy
                        && BuildingModuleEditorCatalog
                            .IsLegacyModule(option.ModuleType))
                    {
                        continue;
                    }

                    if (!string.Equals(previousCategory, option.Category, StringComparison.Ordinal))
                    {
                        previousCategory = option.Category;
                        EditorGUILayout.Space(6f);
                        EditorGUILayout.LabelField(previousCategory, EditorStyles.boldLabel);
                    }

                    DrawOption(option);
                }
                EditorGUILayout.EndScrollView();
            }

            public override void OnOpen()
            {
                EditorApplication.delayCall += () => EditorGUI.FocusTextInControl("BuildingModuleSearch");
            }

            private void DrawOption(BuildingModuleEditorDescriptor option)
            {
                string conflictReason = BuildingModuleEditorCatalog.GetConflictReason(_modules, option.ModuleType);
                bool blocked = !string.IsNullOrWhiteSpace(conflictReason);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (new EditorGUI.DisabledScope(blocked))
                {
                    string title = blocked ? $"⛔ {option.DisplayName}" : option.DisplayName;
                    string tooltip = blocked
                        ? $"{option.Description}\n\nПричина блокування: {conflictReason}"
                        : option.Description;
                    if (GUILayout.Button(new GUIContent(title, tooltip), EditorStyles.miniButton))
                    {
                        if (_asset != null)
                            Undo.RecordObject(_asset, "Додати модуль будівлі");
                        BuildingModuleDefinition created =
                            option.Create();
                        _modules.Add(created);
                        if (_asset != null)
                        {
                            _asset.NotifyEditorDataChanged();
                            EditorUtility.SetDirty(_asset);
                        }
                        _onAdded?.Invoke(created);
                        editorWindow.Close();
                    }
                }

                EditorGUILayout.LabelField(option.Description, EditorStyles.wordWrappedMiniLabel);
                if (blocked)
                    EditorGUILayout.HelpBox(conflictReason, MessageType.Warning);
                EditorGUILayout.EndVertical();
            }
        }
    }
}
