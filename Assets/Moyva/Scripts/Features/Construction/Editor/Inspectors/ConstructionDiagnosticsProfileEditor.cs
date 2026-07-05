#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionDiagnosticsProfileSO))]
    internal sealed class ConstructionDiagnosticsProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls verbose logs and scene-level debug visualization toggles.", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
}
#endif
