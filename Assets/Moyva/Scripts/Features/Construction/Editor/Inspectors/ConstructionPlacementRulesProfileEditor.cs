#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionPlacementRulesProfileSO))]
    internal sealed class ConstructionPlacementRulesProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls global placement restrictions, terrain/fog behavior, and influence rules.", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
}
#endif
