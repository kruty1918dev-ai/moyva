using Kruty1918.Moyva.Generator.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(WFCDataSettings))]
    public class WFCDataSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            WFCDataSettings settings = (WFCDataSettings)target;

            if (GUILayout.Button("Open WFC Rules Editor"))
            {
                WFCRulesEditorWindow.OpenWindow(settings);
            }

        }
    }
}