using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed partial class UnitDesignerWindow
    {
        private const string SafeEditModePrefsKey = "Moyva.UnitDesigner.SafeEditMode";

        private bool _safeEditMode = true;
        private Vector2 _safeEditPreviewScroll;
        private SafeEditOperation _pendingSafeEdit;

        private sealed class SafeEditOperation
        {
            public string Title;
            public string Summary;
            public List<string> PreviewLines;
            public Action Apply;
        }

        private void InitializeSafeEditMode()
        {
            _safeEditMode = EditorPrefs.GetBool(SafeEditModePrefsKey, true);
        }

        private void DisposeSafeEditMode()
        {
            EditorPrefs.SetBool(SafeEditModePrefsKey, _safeEditMode);
        }

        private void DrawSafeEditPanel()
        {
            if (_pendingSafeEdit == null)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Safe Edit Preview", EditorStyles.boldLabel);
            if (!string.IsNullOrWhiteSpace(_pendingSafeEdit.Title))
                EditorGUILayout.LabelField(_pendingSafeEdit.Title, EditorStyles.miniBoldLabel);
            if (!string.IsNullOrWhiteSpace(_pendingSafeEdit.Summary))
                EditorGUILayout.HelpBox(_pendingSafeEdit.Summary, MessageType.Warning);

            if (_pendingSafeEdit.PreviewLines != null && _pendingSafeEdit.PreviewLines.Count > 0)
            {
                _safeEditPreviewScroll = EditorGUILayout.BeginScrollView(_safeEditPreviewScroll, GUILayout.MinHeight(90f), GUILayout.MaxHeight(220f));
                for (int i = 0; i < _pendingSafeEdit.PreviewLines.Count; i++)
                    EditorGUILayout.LabelField($"- {_pendingSafeEdit.PreviewLines[i]}", EditorStyles.miniLabel);
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Застосувати", "Підтвердити mass-операцію"), GUILayout.Height(24f)))
                ApplyPendingSafeEdit();

            if (GUILayout.Button(new GUIContent("Скасувати", "Скасувати mass-операцію"), GUILayout.Height(24f)))
                CancelPendingSafeEdit();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void QueueSafeEditOperation(string title, string summary, List<string> previewLines, Action apply)
        {
            _pendingSafeEdit = new SafeEditOperation
            {
                Title = title,
                Summary = summary,
                PreviewLines = previewLines ?? new List<string>(),
                Apply = apply
            };
            _safeEditPreviewScroll = Vector2.zero;
        }

        private void CancelPendingSafeEdit()
        {
            _pendingSafeEdit = null;
            _safeEditPreviewScroll = Vector2.zero;
        }

        private void ApplyPendingSafeEdit()
        {
            if (_pendingSafeEdit == null)
                return;

            try
            {
                _registryObject?.Update();
                _pendingSafeEdit.Apply?.Invoke();
                TryCommitRegistryChanges("Safe Edit Apply");
            }
            finally
            {
                CancelPendingSafeEdit();
            }
        }

        private int FindUnitIndexByTypeIdExact(string typeId)
        {
            if (_configs == null || string.IsNullOrWhiteSpace(typeId))
                return -1;

            for (int i = 0; i < _configs.arraySize; i++)
            {
                if (string.Equals(GetString(_configs.GetArrayElementAtIndex(i), "TypeId"), typeId, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private SerializedProperty ResolveTargetUnitForSafeEdit(string typeId, int fallbackIndex)
        {
            int index = FindUnitIndexByTypeIdExact(typeId);
            if (index < 0)
                index = fallbackIndex;

            if (_configs == null || index < 0 || index >= _configs.arraySize)
                return null;

            return _configs.GetArrayElementAtIndex(index);
        }
    }
}
