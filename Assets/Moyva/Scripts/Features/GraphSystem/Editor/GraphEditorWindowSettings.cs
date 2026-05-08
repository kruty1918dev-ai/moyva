using UnityEngine;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    [CreateAssetMenu(fileName = "GraphEditorWindowSettings", menuName = "Moyva/Graph Editor Settings", order = 1)]
    public sealed class GraphEditorWindowSettings : ScriptableObject
    {
        // Backwards-compatible storage: GUIDs (string) and direct references to assets.
        public string graphAssetGuid;
        public string previewSettingsGuid;

        // Direct references (preferred). If an assigned reference is missing (deleted), it will be null.
        public GraphAsset graphAsset;
        public EditorPreviewSettings previewSettings;

        public int previewWidth = 64;
        public int previewHeight = 64;
        public bool showInlinePreviews = true;
        public bool autoRunOnChange = true;
        public int previewResolution = 1; // 0=64,1=128,2=full
        public bool previewHeatmap = false;
        public int inspectorTabIndex = 0;

        public bool isInspectorVisible = true;

        public Vector3 cameraPosition = Vector3.zero;
        public Vector3 cameraScale = Vector3.one;
    }
}
