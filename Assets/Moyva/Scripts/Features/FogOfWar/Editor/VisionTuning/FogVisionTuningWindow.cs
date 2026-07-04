using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    /// <summary>
    /// Thin Odin host for the fog vision tuning tool.
    /// </summary>
    public sealed class FogVisionTuningWindow : OdinEditorWindow
    {
        private FogVisionTuningTool _tool;

        [MenuItem("Moyva/Fog Of War/Vision Tuner")]
        private static void Open()
        {
            var window = GetWindow<FogVisionTuningWindow>();
            window.titleContent = new GUIContent("Fog Vision Tuner");
            window.minSize = new Vector2(700f, 460f);
            window.Show();
        }

        private void OnEnable()
        {
            _tool ??= new FogVisionTuningTool();
            _tool.Initialize();
        }

        private void OnGUI()
        {
            _tool ??= new FogVisionTuningTool();
            _tool.Draw();
        }
    }
}
