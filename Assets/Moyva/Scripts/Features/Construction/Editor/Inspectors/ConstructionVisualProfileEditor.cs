#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionVisualProfileSO))]
    internal sealed class ConstructionVisualProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls preview style, blocked flash timing, root naming, and influence-radius shaders.", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
}
#endif
