using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class TopologyResolverEditorWindow : EditorWindow
    {
        private readonly TopologyResolverEditorModule _module = new();
        private Object _registryAsset;
        private Vector2 _scroll;

        public static void Open()
        {
            var window = GetWindow<TopologyResolverEditorWindow>("Topology Resolver");
            window.minSize = new Vector2(760f, 620f);
            window.Show();
        }

        private void OnEnable()
        {
            if (_registryAsset == null)
                TryAutoFind();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Спеціалізований редактор резолвера", EditorStyles.boldLabel);
                _registryAsset = EditorGUILayout.ObjectField(
                    new GUIContent("Реєстр", "Будь-який реєстр, для якого є IResolverRegistryAdapter"),
                    _registryAsset,
                    typeof(Object),
                    false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Знайти BuildingRegistrySO", GUILayout.Height(24f)))
                        TryAutoFind();

                    if (GUILayout.Button("Відкрити Wall Registry", GUILayout.Height(24f)))
                        WallRegistryWindow.Open();
                }
            }

            if (_registryAsset == null)
            {
                EditorGUILayout.HelpBox("Призначте реєстр для редагування декларативних типів резолвера.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _module.Draw(_registryAsset);
            EditorGUILayout.EndScrollView();
        }

        private void TryAutoFind()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingRegistrySO");
            if (guids.Length == 0)
                return;

            _registryAsset = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
