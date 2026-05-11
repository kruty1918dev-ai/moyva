using System.Collections.Generic;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed partial class UnitDesignerWindow
    {
        private void DrawUnitPresetSection()
        {
            BeginSection("Пресети", "d_Preset.Context", "Шаблони для швидкого застосування повторюваних налаштувань юнітів.");

            EditorGUI.BeginChangeCheck();
            _designerPresetLibrary = (DesignerPresetLibrarySO)EditorGUILayout.ObjectField(
                new GUIContent("Preset Library", "Спільна бібліотека шаблонів для дизайнерів."),
                _designerPresetLibrary,
                typeof(DesignerPresetLibrarySO),
                false);
            if (EditorGUI.EndChangeCheck())
                MoyvaProjectEditorContext.Set(_designerPresetLibrary);

            if (_designerPresetLibrary == null)
            {
                EditorGUILayout.HelpBox("Призначте DesignerPresetLibrarySO, щоб застосовувати шаблони юнітів.", MessageType.Info);
                EndSection();
                return;
            }

            var presets = _designerPresetLibrary.UnitPresets;
            if (presets == null || presets.Count == 0)
            {
                EditorGUILayout.HelpBox("У бібліотеці немає Unit preset-ів.", MessageType.Warning);
                EndSection();
                return;
            }

            var names = new string[presets.Count];
            for (int i = 0; i < presets.Count; i++)
            {
                string name = presets[i] != null ? presets[i].Name : string.Empty;
                names[i] = string.IsNullOrWhiteSpace(name) ? $"Unit Preset {i + 1}" : name;
            }

            _selectedUnitPresetIndex = Mathf.Clamp(_selectedUnitPresetIndex, 0, presets.Count - 1);
            _selectedUnitPresetIndex = EditorGUILayout.Popup(new GUIContent("Unit Preset"), _selectedUnitPresetIndex, names);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Preset", GUILayout.Height(22f)))
                    RequestApplySelectedUnitPreset();

                if (GUILayout.Button("Ping Library", GUILayout.Height(22f)))
                    EditorGUIUtility.PingObject(_designerPresetLibrary);
            }

            EndSection();
        }

        private void RequestApplySelectedUnitPreset()
        {
            if (_designerPresetLibrary == null || _designerPresetLibrary.UnitPresets == null || _designerPresetLibrary.UnitPresets.Count == 0)
                return;

            if (_registry == null || _registry.Configs == null || _selectedIndex < 0 || _selectedIndex >= _registry.Configs.Count)
                return;

            var preset = _designerPresetLibrary.UnitPresets[Mathf.Clamp(_selectedUnitPresetIndex, 0, _designerPresetLibrary.UnitPresets.Count - 1)];
            if (preset == null || preset.Template == null)
            {
                EditorUtility.DisplayDialog("Unit Preset", "Обраний preset порожній.", "OK");
                return;
            }

            string typeId = _registry.Configs[_selectedIndex]?.TypeId;
            string presetName = string.IsNullOrWhiteSpace(preset.Name) ? "Unit Preset" : preset.Name;

            void Apply()
            {
                if (_registry == null || _registry.Configs == null || _selectedIndex < 0 || _selectedIndex >= _registry.Configs.Count)
                    return;

                Undo.RecordObject(_registry, $"Unit: apply preset {presetName}");
                if (!DesignerPresetApplier.ApplyUnitPreset(preset, _registry.Configs[_selectedIndex]))
                    return;

                RefreshSerializedObject();
                SelectByTypeId(typeId);
                EditorUtility.SetDirty(_registry);
            }

            if (_safeEditMode)
            {
                QueueSafeEditOperation(
                    "Apply Unit Preset",
                    $"Preset '{presetName}' буде застосовано до '{typeId}'.",
                    new List<string>
                    {
                        $"Ціль: {typeId}",
                        $"Preset: {presetName}",
                        "Змінюються серіалізовані поля UnitClassConfig (TypeId зберігається)."
                    },
                    Apply);
                return;
            }

            Apply();
            TryCommitRegistryChanges($"Apply Unit Preset: {presetName}");
        }
    }
}
