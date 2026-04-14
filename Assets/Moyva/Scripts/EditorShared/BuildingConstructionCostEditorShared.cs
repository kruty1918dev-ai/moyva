using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class BuildingConstructionCostEditorShared
    {
        public static void DrawCostList(SerializedProperty costListProperty, string addButtonLabel = "Додати ресурс")
        {
            if (costListProperty == null || !costListProperty.isArray)
            {
                EditorGUILayout.HelpBox("Не вдалося знайти список вартості будівництва.", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (costListProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Будівництво безкоштовне. Додайте хоча б один ресурс, якщо споруда має ціну.", MessageType.Info);
            }

            int removeIndex = -1;
            for (int i = 0; i < costListProperty.arraySize; i++)
            {
                var entry = costListProperty.GetArrayElementAtIndex(i);
                if (entry == null)
                    continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Ресурс {i + 1}", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();

                Color previousColor = GUI.color;
                GUI.color = new Color(0.9f, 0.35f, 0.35f);
                if (GUILayout.Button("Видалити", GUILayout.Width(82f)))
                    removeIndex = i;
                GUI.color = previousColor;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(entry.FindPropertyRelative("ResourceId"), new GUIContent("Ресурс"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("Amount"), new GUIContent("Кількість"));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(4f);
            if (GUILayout.Button(addButtonLabel))
                AddCostEntry(costListProperty);

            if (removeIndex >= 0)
                costListProperty.DeleteArrayElementAtIndex(removeIndex);

            EditorGUILayout.EndVertical();
        }

        private static void AddCostEntry(SerializedProperty costListProperty)
        {
            costListProperty.arraySize++;
            var entry = costListProperty.GetArrayElementAtIndex(costListProperty.arraySize - 1);
            if (entry == null)
                return;

            entry.FindPropertyRelative("Amount").intValue = 1;
            entry.FindPropertyRelative("ResourceId").stringValue = FindFirstResourceId();
        }

        private static string FindFirstResourceId()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(EconomyResourceDefinition)}");
            var resourceIds = new List<string>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var resource = AssetDatabase.LoadAssetAtPath<EconomyResourceDefinition>(path);
                if (resource == null || string.IsNullOrWhiteSpace(resource.Id))
                    continue;

                resourceIds.Add(resource.Id.Trim());
            }

            resourceIds.Sort(StringComparer.Ordinal);
            return resourceIds.Count > 0 ? resourceIds[0] : string.Empty;
        }
    }
}
