#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public static class TerrainPlacementRuleModuleEditorGUI
    {
        private static bool _showExactLayers;
        private static bool _showAdvanced;

        public static void Draw(
            BuildingDefinitionAsset asset,
            TerrainPlacementRuleModule module,
            InspectorProperty elementProperty,
            Action<BuildingDefinitionAsset, string, Action> applyChange,
            Action requestRepaint)
        {
            if (module == null)
                return;

            DrawContextNavigation(requestRepaint);
            EditorGUILayout.Space(4f);
            DrawMode(asset, module, applyChange);

            switch (module.MergeMode)
            {
                case PlacementRuleMergeMode.Inherit:
                    DrawInheritedSummary();
                    return;
                case PlacementRuleMergeMode.Disabled:
                    EditorGUILayout.HelpBox(
                        "Terrain-перевірки для цієї будівлі вимкнені. Вона зможе стояти навіть на воді, якщо її не зупинить технічна перевірка меж або зайнятості.",
                        MessageType.Warning);
                    return;
                case PlacementRuleMergeMode.Override:
                    break;
                default:
                    return;
            }

            EditorGUILayout.HelpBox(
                "«Замінити» повністю замінює глобальну групу terrain-правил лише для цієї будівлі. Готові рецепти нижче встановлюють безпечний повний набір значень.",
                MessageType.Info);

            DrawPresets(asset, module, applyChange);

            TerrainLayerProfileSO terrainProfile =
                TerrainRuleEditorContext.ResolveTerrainLayerProfile(out _);
            TerrainRuleEditorCatalogSnapshot catalog =
                TerrainRuleEditorContext.BuildCatalog(terrainProfile);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("2. Оберіть terrain-теги", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Тег — це коротка мітка на кшталт water або land. Тут нічого не потрібно вписувати вручну.",
                EditorStyles.wordWrappedMiniLabel);

            DrawTagValues(
                asset,
                "Дозволені теги",
                "Якщо список порожній — дозволені всі теги, крім заборонених. Якщо непорожній — достатньо одного збігу.",
                () => module.AllowedTerrainTags,
                values => module.AllowedTerrainTags = values,
                catalog.Tags,
                applyChange);
            DrawTagValues(
                asset,
                "Заборонені теги",
                "Будь-який збіг одразу забороняє клітинку. Для звичайної будівлі додайте water.",
                () => module.BlockedTerrainTags,
                values => module.BlockedTerrainTags = values,
                catalog.Tags,
                applyChange);

            _showExactLayers = EditorGUILayout.Foldout(
                _showExactLayers,
                new GUIContent(
                    "Точні terrain-шари (необов'язково)",
                    "Використовуйте точні ID лише тоді, коли семантичного тегу недостатньо."),
                true);
            if (_showExactLayers)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Зазвичай достатньо тегів. Точний ID прив'язує правило до конкретного шару генератора.",
                    MessageType.None);
                DrawLayerValues(
                    asset,
                    "Дозволені шари",
                    () => module.AllowedTerrainIds,
                    values => module.AllowedTerrainIds = values,
                    catalog.Layers,
                    applyChange);
                DrawLayerValues(
                    asset,
                    "Заборонені шари",
                    () => module.BlockedTerrainIds,
                    values => module.BlockedTerrainIds = values,
                    catalog.Layers,
                    applyChange);
                EditorGUI.indentLevel--;
            }

            _showAdvanced = EditorGUILayout.Foldout(
                _showAdvanced,
                new GUIContent(
                    "Додатково: висота, краї та рівна основа",
                    "Обмеження рівнів terrain, сусідніх позицій, пагорбів, країв перепаду та рівності footprint."),
                true);
            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;
                DrawAdvancedChildren(elementProperty);
                EditorGUI.indentLevel--;
            }
        }

        public static void ApplyBlockWaterPreset(TerrainPlacementRuleModule module)
        {
            ResetOverride(module);
            module.BlockedTerrainTags = new[] { "water" };
        }

        public static void ApplyLandOnlyPreset(TerrainPlacementRuleModule module)
        {
            ResetOverride(module);
            module.AllowedTerrainTags = new[] { "land" };
        }

        public static void ApplyWaterOnlyPreset(TerrainPlacementRuleModule module)
        {
            ResetOverride(module);
            module.AllowedTerrainTags = new[] { "water" };
        }

        public static void ApplyAnyTerrainPreset(TerrainPlacementRuleModule module)
        {
            ResetOverride(module);
        }

        private static void DrawContextNavigation(Action requestRepaint)
        {
            TerrainRuleEditorResolution<ConstructionPlacementRulesProfileSO> globalResolution =
                TerrainRuleEditorContext.ResolvePlacementRulesProfileDetailed();
            TerrainRuleEditorResolution<TerrainLayerProfileSO> terrainResolution =
                TerrainRuleEditorContext.ResolveTerrainLayerProfileDetailed();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Налаштування, які використовує сцена", EditorStyles.boldLabel);
            DrawResolvedAssetRow(
                "Глобальні правила",
                globalResolution.Asset,
                globalResolution.Message,
                "Відкрити глобальні правила",
                selected => MoyvaProjectEditorContext.Set(selected),
                requestRepaint);
            DrawResolvedAssetRow(
                "Профілі terrain",
                terrainResolution.Asset,
                terrainResolution.Message,
                "Відкрити профілі terrain",
                selected => MoyvaProjectEditorContext.Set(selected),
                requestRepaint);
            EditorGUILayout.EndVertical();
        }

        private static void DrawResolvedAssetRow<T>(
            string label,
            T resolvedAsset,
            string status,
            string buttonLabel,
            Action<T> rememberSelection,
            Action requestRepaint) where T : UnityEngine.Object
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(label, status));
            if (resolvedAsset != null)
            {
                EditorGUILayout.LabelField(resolvedAsset.name, EditorStyles.miniLabel);
                if (GUILayout.Button(buttonLabel, GUILayout.Width(190f)))
                    OpenAsset(resolvedAsset);
            }
            else
            {
                T selected = (T)EditorGUILayout.ObjectField(null, typeof(T), false);
                if (selected != null)
                {
                    rememberSelection?.Invoke(selected);
                    requestRepaint?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (resolvedAsset == null)
                EditorGUILayout.HelpBox(status, MessageType.Warning);
        }

        private static void DrawMode(
            BuildingDefinitionAsset asset,
            TerrainPlacementRuleModule module,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            EditorGUILayout.LabelField("1. Звідки брати правило?", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DrawModeButton(
                asset,
                module,
                PlacementRuleMergeMode.Inherit,
                "Успадкувати",
                "Використати глобальні правила сцени. Це рекомендований і найбезпечніший режим.",
                applyChange);
            DrawModeButton(
                asset,
                module,
                PlacementRuleMergeMode.Override,
                "Замінити",
                "Налаштувати окреме terrain-правило лише для цієї будівлі.",
                applyChange);
            DrawModeButton(
                asset,
                module,
                PlacementRuleMergeMode.Disabled,
                "Вимкнути",
                "Не перевіряти terrain для цієї будівлі. Використовуйте лише для спеціальних випадків.",
                applyChange);
            EditorGUILayout.EndHorizontal();

            string explanation = module.MergeMode switch
            {
                PlacementRuleMergeMode.Inherit =>
                    "Успадкувати: будівля поводиться за глобальним профілем. Локальні списки збережені, але зараз не застосовуються.",
                PlacementRuleMergeMode.Override =>
                    "Замінити: усе нижче діє тільки для цієї будівлі та не змінює інші будівлі.",
                PlacementRuleMergeMode.Disabled =>
                    "Вимкнути: глобальні й локальні terrain-обмеження ігноруються для цієї будівлі.",
                _ => string.Empty,
            };
            EditorGUILayout.LabelField(explanation, EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawModeButton(
            BuildingDefinitionAsset asset,
            TerrainPlacementRuleModule module,
            PlacementRuleMergeMode mode,
            string label,
            string tooltip,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            Color previous = GUI.backgroundColor;
            if (module.MergeMode == mode)
                GUI.backgroundColor = new Color(0.55f, 0.85f, 0.62f);

            if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Height(28f))
                && module.MergeMode != mode)
            {
                applyChange(
                    asset,
                    "Змінити режим terrain-правил",
                    () => module.MergeMode = mode);
            }

            GUI.backgroundColor = previous;
        }

        private static void DrawInheritedSummary()
        {
            ConstructionPlacementRulesProfileSO profile =
                TerrainRuleEditorContext.ResolvePlacementRulesProfile(out string status);
            if (profile == null)
            {
                EditorGUILayout.HelpBox(status, MessageType.Warning);
                return;
            }

            if (!profile.EnableTerrainRules)
            {
                EditorGUILayout.HelpBox(
                    "У глобальному профілі terrain-правила вимкнені. Ця будівля успадковує саме цей стан.",
                    MessageType.Warning);
                return;
            }

            string water = profile.AllowBuildingOnWater ? "дозволена" : "заборонена";
            string hills = profile.AllowBuildingOnHills ? "дозволені" : "заборонені";
            string edges = profile.BlockEdgeTerrainTiles ? "блокуються" : "не блокуються";
            EditorGUILayout.HelpBox(
                $"Активне глобальне правило: вода {water}; пагорби {hills}; краї перепаду {edges}.",
                MessageType.Info);
        }

        private static void DrawPresets(
            BuildingDefinitionAsset asset,
            TerrainPlacementRuleModule module,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            EditorGUILayout.LabelField("Швидке налаштування", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Одна кнопка очищає попередній локальний terrain-набір і встановлює готовий рецепт.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(
                    "Заборонити воду",
                    "Дозволяє будь-який terrain, крім terrain із тегом water.")))
            {
                applyChange(asset, "Заборонити воду для будівлі", () => ApplyBlockWaterPreset(module));
            }
            if (GUILayout.Button(new GUIContent(
                    "Лише суша",
                    "Дозволяє тільки terrain із тегом land.")))
            {
                applyChange(asset, "Дозволити будівлю лише на суші", () => ApplyLandOnlyPreset(module));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(
                    "Лише вода",
                    "Дозволяє тільки terrain із тегом water, наприклад для доку.")))
            {
                applyChange(asset, "Дозволити будівлю лише на воді", () => ApplyWaterOnlyPreset(module));
            }
            if (GUILayout.Button(new GUIContent(
                    "Дозволити будь-який terrain",
                    "Прибирає локальні terrain-обмеження. Межі мапи та зайняті клітинки все одно перевіряються.")))
            {
                applyChange(asset, "Дозволити будь-який terrain", () => ApplyAnyTerrainPreset(module));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawTagValues(
            BuildingDefinitionAsset asset,
            string label,
            string explanation,
            Func<string[]> getValues,
            Action<string[]> setValues,
            IReadOnlyList<TerrainRuleTagOption> options,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            DrawValueHeader(label, explanation);
            string[] values = TerrainRuleEditorContext.NormalizeValues(getValues());
            DrawCurrentValues(
                asset,
                values,
                value => IsKnownTag(value, options),
                value => applyChange(
                    asset,
                    $"Видалити terrain-тег {value}",
                    () => setValues(TerrainRuleEditorContext.RemoveValue(getValues(), value))));

            if (GUILayout.Button($"+ Додати до «{label}» зі списку ▾"))
                ShowTagMenu(asset, getValues, setValues, options, applyChange);
        }

        private static void DrawLayerValues(
            BuildingDefinitionAsset asset,
            string label,
            Func<string[]> getValues,
            Action<string[]> setValues,
            IReadOnlyList<TerrainRuleLayerOption> options,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            DrawValueHeader(label, null);
            string[] values = TerrainRuleEditorContext.NormalizeValues(getValues());
            DrawCurrentValues(
                asset,
                values,
                value => IsKnownLayer(value, options),
                value => applyChange(
                    asset,
                    $"Видалити terrain-шар {value}",
                    () => setValues(TerrainRuleEditorContext.RemoveValue(getValues(), value))));

            if (GUILayout.Button($"+ Додати до «{label}» зі списку ▾"))
                ShowLayerMenu(asset, getValues, setValues, options, applyChange);
        }

        private static void DrawValueHeader(string label, string explanation)
        {
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (!string.IsNullOrWhiteSpace(explanation))
                EditorGUILayout.LabelField(explanation, EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawCurrentValues(
            BuildingDefinitionAsset asset,
            IReadOnlyList<string> values,
            Func<string, bool> isKnown,
            Action<string> remove)
        {
            if (values == null || values.Count == 0)
            {
                EditorGUILayout.LabelField("— нічого не вибрано —", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index];
                bool known = isKnown?.Invoke(value) ?? true;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(
                    new GUIContent(
                        known ? $"✓ {value}" : $"⚠ {value}",
                        known
                            ? "Значення знайдено в активному каталозі terrain."
                            : "Значення не знайдено в активному каталозі. Воно збережене для сумісності, але правило може ніколи не збігтися."),
                    known ? EditorStyles.label : EditorStyles.boldLabel);
                if (GUILayout.Button(new GUIContent("×", "Прибрати це значення."), GUILayout.Width(24f)))
                    remove?.Invoke(value);
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void ShowTagMenu(
            BuildingDefinitionAsset asset,
            Func<string[]> getValues,
            Action<string[]> setValues,
            IReadOnlyList<TerrainRuleTagOption> options,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            var menu = new GenericMenu();
            bool addedSelectableItem = false;
            for (int index = 0; index < (options?.Count ?? 0); index++)
            {
                TerrainRuleTagOption option = options[index];
                string value = option.Value;
                string menuLabel = option.MenuLabel;
                if (!option.IsPresentInProfile)
                {
                    menu.AddDisabledItem(new GUIContent($"{menuLabel} — спочатку додайте до профілю terrain"));
                    continue;
                }

                addedSelectableItem = true;
                bool selected = TerrainRuleEditorContext.Contains(getValues(), value);
                menu.AddItem(
                    new GUIContent(menuLabel, option.Description),
                    selected,
                    () => applyChange(
                        asset,
                        $"Змінити terrain-тег {value}",
                        () => setValues(selected
                            ? TerrainRuleEditorContext.RemoveValue(getValues(), value)
                            : TerrainRuleEditorContext.AddValue(getValues(), value))));
            }

            if (!addedSelectableItem)
                menu.AddDisabledItem(new GUIContent("У профілях terrain ще немає тегів"));
            menu.ShowAsContext();
        }

        private static void ShowLayerMenu(
            BuildingDefinitionAsset asset,
            Func<string[]> getValues,
            Action<string[]> setValues,
            IReadOnlyList<TerrainRuleLayerOption> options,
            Action<BuildingDefinitionAsset, string, Action> applyChange)
        {
            var menu = new GenericMenu();
            if (options == null || options.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("У профілі terrain ще немає шарів"));
                menu.ShowAsContext();
                return;
            }

            for (int index = 0; index < options.Count; index++)
            {
                TerrainRuleLayerOption option = options[index];
                string value = option.Value;
                bool selected = TerrainRuleEditorContext.Contains(getValues(), value);
                menu.AddItem(
                    new GUIContent(option.MenuLabel),
                    selected,
                    () => applyChange(
                        asset,
                        $"Змінити terrain-шар {value}",
                        () => setValues(selected
                            ? TerrainRuleEditorContext.RemoveValue(getValues(), value)
                            : TerrainRuleEditorContext.AddValue(getValues(), value))));
            }
            menu.ShowAsContext();
        }

        private static bool IsKnownTag(
            string value,
            IReadOnlyList<TerrainRuleTagOption> options)
        {
            for (int index = 0; index < (options?.Count ?? 0); index++)
            {
                if (options[index].IsPresentInProfile
                    && string.Equals(options[index].Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsKnownLayer(
            string value,
            IReadOnlyList<TerrainRuleLayerOption> options)
        {
            for (int index = 0; index < (options?.Count ?? 0); index++)
            {
                if (string.Equals(options[index].Value, value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void DrawAdvancedChildren(InspectorProperty elementProperty)
        {
            if (elementProperty == null)
                return;

            var advancedNames = new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(TerrainPlacementRuleModule.AllowedTerrainLevels),
                nameof(TerrainPlacementRuleModule.BlockedTerrainLevels),
                nameof(TerrainPlacementRuleModule.RequiredNeighborOffsets),
                nameof(TerrainPlacementRuleModule.AllowHills),
                nameof(TerrainPlacementRuleModule.BlockEdgeTerrainTiles),
                nameof(TerrainPlacementRuleModule.RequiresFlatGround),
            };

            for (int index = 0; index < elementProperty.Children.Count; index++)
            {
                InspectorProperty child = elementProperty.Children[index];
                if (advancedNames.Contains(child.Name))
                    child.Draw(child.Label);
            }
        }

        private static void ResetOverride(TerrainPlacementRuleModule module)
        {
            if (module == null)
                return;

            module.MergeMode = PlacementRuleMergeMode.Override;
            module.AllowedTerrainIds = Array.Empty<string>();
            module.BlockedTerrainIds = Array.Empty<string>();
            module.AllowedTerrainTags = Array.Empty<string>();
            module.BlockedTerrainTags = Array.Empty<string>();
            module.AllowedTerrainLevels = Array.Empty<int>();
            module.BlockedTerrainLevels = Array.Empty<int>();
            module.RequiredNeighborOffsets = Array.Empty<Vector2Int>();
            module.AllowHills = true;
            module.BlockEdgeTerrainTiles = false;
            module.RequiresFlatGround = false;
        }

        private static void OpenAsset(UnityEngine.Object asset)
        {
            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}

#endif
