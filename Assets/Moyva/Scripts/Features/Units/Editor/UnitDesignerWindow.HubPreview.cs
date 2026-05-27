using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed partial class UnitDesignerWindow : IMoyvaHubPreviewProvider, IMoyvaHubSettingsOpener
    {
        public string HubToolMenuPath => "Moyva/Tools/Unit Designer";

        public string GetHubPreviewSummary()
        {
            int total = _configs != null ? _configs.arraySize : 0;
            string selected = _selectedIndex >= 0 && _configs != null && _selectedIndex < _configs.arraySize
                ? _configs.GetArrayElementAtIndex(_selectedIndex).FindPropertyRelative("TypeId")?.stringValue
                : "none";
            return $"Units: {total}, Selected: {selected}";
        }

        public void DrawHubPreview(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.10f, 0.16f, 0.20f));

            int total = _configs != null ? _configs.arraySize : 0;
            float health = total > 0 ? 1f : 0f;
            Rect barRect = new Rect(rect.x + 10f, rect.y + rect.height - 18f, rect.width - 20f, 8f);
            EditorGUI.DrawRect(barRect, new Color(0f, 0f, 0f, 0.28f));
            EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * health, barRect.height), total > 0 ? new Color(0.15f, 0.72f, 0.42f) : new Color(0.82f, 0.24f, 0.24f));

            if (_selectedIndex >= 0 && _configs != null && _selectedIndex < _configs.arraySize)
            {
                var cfg = _configs.GetArrayElementAtIndex(_selectedIndex);
                var sprite = cfg.FindPropertyRelative("Sprite")?.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    Rect iconRect = new Rect(rect.x + 10f, rect.y + 10f, 48f, 48f);
                    AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iconRect, null, sprite);
                }
            }

            GUIStyle label = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = Color.white } };
            GUI.Label(new Rect(rect.x + 66f, rect.y + 12f, rect.width - 76f, 16f), "Unit Designer", label);
            GUI.Label(new Rect(rect.x + 66f, rect.y + 30f, rect.width - 76f, 16f), GetHubPreviewSummary(), EditorStyles.miniLabel);
        }

        public bool OpenHubSettingsFromPreview()
        {
            Open();
            return true;
        }
    }
}
