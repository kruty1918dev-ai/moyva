using Kruty1918.Moyva.Generator.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    public static class ObjectConnectionRulesContextMenu
    {
        [MenuItem("Assets/Moyva/ObjectConnectionRules/Apply Preset/Empty", true)]
        private static bool ValidateApplyEmpty()
        {
            return Selection.activeObject is ObjectConnectionRulesSO;
        }

        [MenuItem("Assets/Moyva/ObjectConnectionRules/Apply Preset/Empty")]
        private static void ApplyEmpty()
        {
            ApplyToSelection(ObjectAutoTilePreset.Empty);
        }

        [MenuItem("Assets/Moyva/ObjectConnectionRules/Apply Preset/River", true)]
        private static bool ValidateApplyRiver()
        {
            return Selection.activeObject is ObjectConnectionRulesSO;
        }

        [MenuItem("Assets/Moyva/ObjectConnectionRules/Apply Preset/River")]
        private static void ApplyRiver()
        {
            ApplyToSelection(ObjectAutoTilePreset.River);
        }

        [MenuItem("Assets/Moyva/ObjectConnectionRules/Apply Preset/Road", true)]
        private static bool ValidateApplyRoad()
        {
            return Selection.activeObject is ObjectConnectionRulesSO;
        }

        [MenuItem("Assets/Moyva/ObjectConnectionRules/Apply Preset/Road")]
        private static void ApplyRoad()
        {
            ApplyToSelection(ObjectAutoTilePreset.Road);
        }

        private static void ApplyToSelection(ObjectAutoTilePreset preset)
        {
            var rules = Selection.activeObject as ObjectConnectionRulesSO;
            if (rules == null)
                return;

            Undo.RecordObject(rules, "Apply ObjectConnectionRules Preset");
            rules.ApplyPreset(preset);
            EditorUtility.SetDirty(rules);
            AssetDatabase.SaveAssets();

            Debug.Log($"Applied preset '{preset}' to '{rules.name}'.", rules);
        }
    }
}
