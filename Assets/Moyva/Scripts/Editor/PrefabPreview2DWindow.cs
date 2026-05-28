using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Editor Window для адаптивного налаштування preview prefab-ів під глобальний режим проекту.
    /// </summary>
    public class PrefabPreview2DWindow : EditorWindow
    {
        private bool _showSpriteOutlines = true;
        private bool _enablePixelPerfect = true;
        private float _previewZoom = 1f;
        private Vector2 _scrollPosition;

        public static void ShowWindow()
        {
            GetWindow<PrefabPreview2DWindow>("Adaptive Preview Setup");
        }

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Адаптивні prefab preview", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Поточний режим: {AdaptivePrefabPreviewUtility.DescribeCurrentMode()}", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            // Опції превью
            EditorGUILayout.LabelField("Опції превью", EditorStyles.boldLabel);
            
            _showSpriteOutlines = EditorGUILayout.Toggle("Показувати контури спрайтів", _showSpriteOutlines);
            _enablePixelPerfect = EditorGUILayout.Toggle("Pixel Perfect превью", _enablePixelPerfect);
            _previewZoom = EditorGUILayout.Slider("Zoom превью", _previewZoom, 0.5f, 3f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Дії", EditorStyles.boldLabel);

            if (GUILayout.Button("М'яко оновити prefab preview defaults", GUILayout.Height(30)))
            {
                FixAllPrefabsForAdaptivePreview();
            }

            if (GUILayout.Button("Підказка для Scene View", GUILayout.Height(30)))
            {
                SetupSceneViewForProjectMode();
            }

            if (GUILayout.Button("Показати Sprite Outline Gizmo", GUILayout.Height(30)))
            {
                ShowHelp();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Preview тепер читають глобальні Moyva Project Settings. " +
                "У 3D/Mesh режимах ця дія не вимикає MeshRenderer і не скидає transform prefab-ів.",
                MessageType.Info);

            GUILayout.EndScrollView();
        }

        private static void FixAllPrefabsForAdaptivePreview()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moyva" });
            var count = 0;
            var settings = AdaptivePrefabPreviewUtility.ProjectSettings;

            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = PrefabUtility.LoadPrefabContents(path);

                if (ConfigurePrefabForAdaptivePreview(prefab, settings))
                {
                    count++;
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }

                PrefabUtility.UnloadPrefabContents(prefab);
            }

            EditorUtility.DisplayDialog("Adaptive Preview",
                $"Оновлено {count} prefab-ів для режиму {AdaptivePrefabPreviewUtility.DescribeCurrentMode()}.",
                "OK");

            AssetDatabase.Refresh();
        }

        private static bool ConfigurePrefabForAdaptivePreview(GameObject prefab, MoyvaProjectSettingsSO settings)
        {
            bool changed = false;
            bool uses3DPreview = AdaptivePrefabPreviewUtility.Uses3DPreview(settings);

            if (uses3DPreview)
                return false;

            var spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in spriteRenderers)
            {
                if (sr.material == null || sr.material.shader == null || !sr.material.shader.name.Contains("Sprite"))
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        sr.material = new Material(shader);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static void SetupSceneViewForProjectMode()
        {
            EditorUtility.DisplayDialog("Scene View Preview",
                "Preview режим читається з Moyva Project Settings.\n\n" +
                "Для sprite/isometric режимів зручно відкривати Prefab Mode і Scene View 2D.\n" +
                "Для Mesh/3D режимів залишайте Scene View у перспективі або orthographic 3D.",
                "OK");
        }

        private static void ShowHelp()
        {
            EditorUtility.DisplayDialog("2D Preview Tips", 
                "💡 Поради для 2D превью:\\n\\n" +
                "✓ Використовуйте Prefab Mode (двічі клік)\\n" +
                "✓ Натисніть '2' на Numpad для 2D режиму\\n" +
                "✓ Натисніть '0' на Numpad для 3D режиму\\n" +
                "✓ Shift+F = Focus на об'єкт\\n" +
                "✓ Right Click + Drag = Панорамування",
                "OK");
        }
    }
}
