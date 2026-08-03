using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    /// <summary>
    /// Editor-only catalog used by every terrain-tag picker. The runtime value remains
    /// a plain string, so adding the picker does not change existing serialization.
    /// </summary>
    [InitializeOnLoad]
    internal static class TerrainTagEditorCatalog
    {
        internal readonly struct Suggestion
        {
            public Suggestion(string value, string ukrainianName)
            {
                Value = value;
                UkrainianName = ukrainianName;
            }

            public string Value { get; }
            public string UkrainianName { get; }
            public string MenuLabel => $"{Value} — {UkrainianName}";
        }

        internal static readonly Suggestion[] Suggestions = CreateSuggestions();

        private static string[] _cachedCustomTags;

        static TerrainTagEditorCatalog()
        {
            EditorApplication.projectChanged += Invalidate;
            Undo.undoRedoPerformed += Invalidate;
        }

        internal static void Invalidate()
        {
            _cachedCustomTags = null;
        }

        internal static IReadOnlyList<string> GetCustomTags()
        {
            if (_cachedCustomTags != null)
                return _cachedCustomTags;

            var knownTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < Suggestions.Length; index++)
                knownTags.Add(Suggestions[index].Value);

            var customTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets("t:TerrainLayerProfileSO");
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                var asset = AssetDatabase.LoadAssetAtPath<TerrainLayerProfileSO>(path);
                if (asset == null)
                    continue;

                var serializedAsset = new SerializedObject(asset);
                serializedAsset.UpdateIfRequiredOrScript();
                CollectTags(serializedAsset.FindProperty("_profiles"), customTags, knownTags);
                CollectTags(serializedAsset.FindProperty("_fallback"), customTags, knownTags);
            }

            _cachedCustomTags = new string[customTags.Count];
            customTags.CopyTo(_cachedCustomTags);
            Array.Sort(_cachedCustomTags, StringComparer.OrdinalIgnoreCase);
            return _cachedCustomTags;
        }

        internal static bool IsSuggested(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            for (int index = 0; index < Suggestions.Length; index++)
            {
                if (string.Equals(Suggestions[index].Value, tag.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static Suggestion[] CreateSuggestions()
        {
            IReadOnlyList<TerrainRuleTagOption> source =
                TerrainRuleEditorContext.SuggestedTags;
            var result = new Suggestion[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = new Suggestion(
                    source[index].Value,
                    source[index].DisplayName);
            }

            return result;
        }

        internal static string Normalize(string tag)
        {
            return string.IsNullOrWhiteSpace(tag)
                ? string.Empty
                : tag.Trim().ToLowerInvariant();
        }

        internal static bool IsValidCustomTag(string tag, out string normalized, out string error)
        {
            normalized = Normalize(tag);
            if (string.IsNullOrEmpty(normalized))
            {
                error = "Введіть назву тегу.";
                return false;
            }

            for (int index = 0; index < normalized.Length; index++)
            {
                char character = normalized[index];
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_')
                    continue;

                error = "Використовуйте літери, цифри, '-' або '_'. Пробіли не дозволені.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static void CollectTags(
            SerializedProperty profilesOrProfile,
            ISet<string> customTags,
            ISet<string> knownTags)
        {
            if (profilesOrProfile == null)
                return;

            if (profilesOrProfile.isArray)
            {
                for (int profileIndex = 0; profileIndex < profilesOrProfile.arraySize; profileIndex++)
                    CollectProfileTags(profilesOrProfile.GetArrayElementAtIndex(profileIndex), customTags, knownTags);
                return;
            }

            CollectProfileTags(profilesOrProfile, customTags, knownTags);
        }

        private static void CollectProfileTags(
            SerializedProperty profile,
            ISet<string> customTags,
            ISet<string> knownTags)
        {
            var tags = profile?.FindPropertyRelative("_tags");
            if (tags == null || !tags.isArray)
                return;

            for (int tagIndex = 0; tagIndex < tags.arraySize; tagIndex++)
            {
                string tag = tags.GetArrayElementAtIndex(tagIndex).stringValue?.Trim();
                if (string.IsNullOrEmpty(tag) || knownTags.Contains(tag))
                    continue;

                customTags.Add(tag);
            }
        }
    }

    internal sealed class CreateTerrainTagPopup : PopupWindowContent
    {
        private const string ControlName = "MoyvaCreateTerrainTag";

        private readonly Action<string> _onCreated;
        private string _value = string.Empty;
        private bool _focusField = true;

        internal CreateTerrainTagPopup(Action<string> onCreated)
        {
            _onCreated = onCreated;
        }

        public override Vector2 GetWindowSize() => new Vector2(360f, 126f);

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Новий terrain tag", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Коротко, без пробілів. Наприклад: lava", EditorStyles.miniLabel);

            GUI.SetNextControlName(ControlName);
            _value = EditorGUILayout.TextField(_value);

            bool isValid = TerrainTagEditorCatalog.IsValidCustomTag(_value, out string normalized, out string error);
            if (!isValid && !string.IsNullOrEmpty(_value))
                EditorGUILayout.HelpBox(error, MessageType.Warning);

            using (new EditorGUI.DisabledScope(!isValid))
            {
                if (GUILayout.Button("Створити тег"))
                {
                    _onCreated?.Invoke(normalized);
                    editorWindow.Close();
                    GUIUtility.ExitGUI();
                }
            }

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Return && isValid)
            {
                currentEvent.Use();
                _onCreated?.Invoke(normalized);
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
            {
                currentEvent.Use();
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            if (_focusField)
            {
                _focusField = false;
                EditorGUI.FocusTextInControl(ControlName);
            }
        }
    }
}
