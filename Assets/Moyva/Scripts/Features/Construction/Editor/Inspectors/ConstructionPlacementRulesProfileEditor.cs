#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
#if UNITY_EDITOR
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionPlacementRulesProfileSO))]
    internal sealed class ConstructionPlacementRulesProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Це глобальні правила для всіх будівель у режимі «Успадкувати». Для звичайної гри залиште «Перевіряти terrain» увімкненим, а «Дозволити будівництво на воді» — вимкненим.",
                MessageType.Info);

            TerrainLayerProfileSO terrainProfiles =
                TerrainRuleEditorContext.ResolveTerrainLayerProfile(out string status);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(terrainProfiles == null))
                {
                    if (GUILayout.Button(new GUIContent(
                            "Відкрити профілі terrain",
                            "Тут шарам мапи призначаються теги water, land та інші.")))
                    {
                        Selection.activeObject = terrainProfiles;
                        EditorGUIUtility.PingObject(terrainProfiles);
                    }
                }

                if (GUILayout.Button(new GUIContent(
                        "Відкрити PDF-інструкцію",
                        "Покрокова українська інструкція з прикладами.")))
                {
                    string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                    string guidePath = Path.Combine(
                        projectRoot ?? string.Empty,
                        "docs/systems/construction/building-placement-rules-guide.pdf");
                    if (File.Exists(guidePath))
                        EditorUtility.OpenWithDefaultApp(guidePath);
                    else
                        EditorUtility.DisplayDialog(
                            "Інструкцію не знайдено",
                            $"Очікуваний файл:\n{guidePath}",
                            "Гаразд");
                }

            }

            if (terrainProfiles == null)
                EditorGUILayout.HelpBox(status, MessageType.Warning);

            EditorGUILayout.Space(4f);
            base.OnInspectorGUI();
        }
    }
}
#endif

#endif
