#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionInputProfileSO))]
    internal sealed class ConstructionInputProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls tap thresholds, drag support, multi-touch cancellation, and UI blocking behavior.", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
}
#endif
