#if UNITY_EDITOR
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomEditor(typeof(ConstructionWallProfileSO))]
    internal sealed class ConstructionWallProfileEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls gate replacement, wall handle visibility, and high-level wall path behavior defaults.", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
}
#endif
