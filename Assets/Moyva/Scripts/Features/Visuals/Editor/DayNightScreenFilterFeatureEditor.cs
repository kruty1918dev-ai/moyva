using Kruty1918.Moyva.Visuals;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Visuals.Editor
{
    [CustomEditor(typeof(DayNightScreenFilterFeature))]
    public sealed class DayNightScreenFilterFeatureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8f);
            DrawPresetContextArea();

            if (GUILayout.Button("Відкрити меню пресетів"))
            {
                ShowPresetMenu();
            }
        }

        private void DrawPresetContextArea()
        {
            var rect = GUILayoutUtility.GetRect(10f, 38f, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "ПКМ тут: Пресети -> Обрати профіль дня/ночі");

            Event evt = Event.current;
            if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                ShowPresetMenu();
                evt.Use();
            }
        }

        private void ShowPresetMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Пресети/1 Neutral Contrast"), false, () => ApplyPreset(DayNightScreenFilterFeature.PresetKind.NeutralContrast));
            menu.AddItem(new GUIContent("Пресети/2 Cinematic Dusk"), false, () => ApplyPreset(DayNightScreenFilterFeature.PresetKind.CinematicDusk));
            menu.AddItem(new GUIContent("Пресети/3 Extreme Night"), false, () => ApplyPreset(DayNightScreenFilterFeature.PresetKind.ExtremeNight));
            menu.ShowAsContext();
        }

        private void ApplyPreset(DayNightScreenFilterFeature.PresetKind preset)
        {
            var feature = (DayNightScreenFilterFeature)target;
            Undo.RecordObject(feature, "Apply Day/Night Preset");
            feature.ApplyPreset(preset);
            EditorUtility.SetDirty(feature);
            serializedObject.Update();
            Repaint();
        }
    }
}
