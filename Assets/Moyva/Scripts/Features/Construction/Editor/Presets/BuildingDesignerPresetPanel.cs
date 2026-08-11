#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.IO;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    public static class BuildingDesignerPresetPanel
    {
        private static bool _expanded = true;

        public static void Draw(BuildingRegistrySO registry, Action refresh)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _expanded = EditorGUILayout.Foldout(
                    _expanded,
                    "JSON Presets · moyva.base-buildings.2026-08",
                    true);
                if (!_expanded)
                    return;

                BuildingPresetPackSpec pack;
                try
                {
                    pack = BuildingJsonPresetSerializer.LoadPack();
                }
                catch (Exception ex)
                {
                    EditorGUILayout.HelpBox("Preset pack unavailable: " + ex.Message, MessageType.Error);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Pack: {pack.PackId} · Presets: {pack.PresetIds.Count}", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Validate pack", GUILayout.Width(120f)))
                        Run(() => BuildingPresetBatchService.ValidatePack("designer-validate"), refresh);
                    if (GUILayout.Button("Apply / Reapply", GUILayout.Width(130f)))
                        Run(() => BuildingPresetBatchService.ApplyAll("designer-pack"), refresh);
                    if (GUILayout.Button("Open folder", GUILayout.Width(100f)))
                    {
                        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                                             ?? Directory.GetCurrentDirectory();
                        EditorUtility.RevealInFinder(
                            Path.GetFullPath(Path.Combine(projectRoot, BuildingPresetPaths.PresetRoot)));
                    }
                }

                EditorGUILayout.LabelField(
                    "Дії для конкретної будівлі перенесені в дерево: ПКМ по будівлі → JSON Presets.",
                    EditorStyles.miniLabel);
            }
        }

        private static void Run(Func<BuildingPresetBatchResult> action, Action refresh)
        {
            try
            {
                BuildingPresetBatchResult result = action();
                Debug.Log($"[MoyvaBuildingPresets] {result.Summary}");
                refresh?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoyvaBuildingPresets] Designer action failed: {ex}");
                EditorUtility.DisplayDialog("Building Presets", ex.Message, "OK");
            }
        }
    }
}

#endif
