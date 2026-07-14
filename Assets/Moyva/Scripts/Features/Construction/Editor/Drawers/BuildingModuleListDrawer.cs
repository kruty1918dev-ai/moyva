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
                PopupWindow.Show(
                    buttonRect,
                    new BuildingModulePickerPopup(asset, modules, RefreshPropertyTree));
            }
            SirenixEditorGUI.EndBoxHeader();

            EditorGUILayout.HelpBox(
                "Додавайте лише потрібні можливості. Недоступні або повторні модулі будуть заблоковані з поясненням.",
                MessageType.Info);

            if (modules.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Модулі ще не додані. Будівля використовуватиме лише базові параметри.",
                    MessageType.Warning);
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
            bool expanded = !_expanded.TryGetValue(module, out bool stored) || stored;

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

            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);

            if (nextExpanded && elementProperty != null)
            {
                EditorGUI.indentLevel++;
                for (int childIndex = 0; childIndex < elementProperty.Children.Count; childIndex++)
                {
                    InspectorProperty child = elementProperty.Children[childIndex];
                    if (string.Equals(child.Name, nameof(BuildingModuleDefinition.IsEnabled), StringComparison.Ordinal))
                        continue;

                    child.Draw(child.Label);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
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
            private readonly Action _onChanged;
            private Vector2 _scroll;
            private string _search = string.Empty;

            public BuildingModulePickerPopup(
                BuildingDefinitionAsset asset,
                List<BuildingModuleDefinition> modules,
                Action onChanged)
            {
                _asset = asset;
                _modules = modules;
                _onChanged = onChanged;
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

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                string previousCategory = null;
                IReadOnlyList<BuildingModuleEditorDescriptor> options = BuildingModuleEditorCatalog.Options;
                for (int index = 0; index < options.Count; index++)
                {
                    BuildingModuleEditorDescriptor option = options[index];
                    if (!option.MatchesSearch(_search))
                        continue;

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
                        _modules.Add(option.Create());
                        if (_asset != null)
                        {
                            _asset.NotifyEditorDataChanged();
                            EditorUtility.SetDirty(_asset);
                        }
                        _onChanged?.Invoke();
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
