using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// М'яко налаштовує preview prefab-ів під глобальний режим Moyva Project Settings.
    /// </summary>
    [InitializeOnLoad]
    public static class Prefab2DPreviewSetup
    {
        static Prefab2DPreviewSetup()
        {
            EditorApplication.update += Initialize2DPreview;
        }

        private static void Initialize2DPreview()
        {
            // Виконується один раз при завантаженні
            EditorApplication.update -= Initialize2DPreview;

            // 2D режим налаштовується автоматично при редаганні префабу через Prefab Mode
        }

        [MenuItem("Moyva/Setup/Apply Adaptive Preview Defaults to Selected Prefab", priority = 101)]
        public static void ApplyAdaptivePreviewDefaultsToSelectedPrefab()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Будь ласка, виберіть префаб!", "OK");
                return;
            }

            var path = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
            {
                EditorUtility.DisplayDialog("Error", "Вибраний об'єкт не є префабом!", "OK");
                return;
            }

            var prefab = PrefabUtility.LoadPrefabContents(path);
            bool changed = ConfigurePrefabForAdaptivePreview(prefab);
            if (changed)
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);

            EditorUtility.DisplayDialog("Adaptive Preview",
                changed
                    ? $"Prefab оновлено для режиму {AdaptivePrefabPreviewUtility.DescribeCurrentMode()}."
                    : $"Змін не потрібно для режиму {AdaptivePrefabPreviewUtility.DescribeCurrentMode()}.",
                "OK");
        }

        private static bool ConfigurePrefabForAdaptivePreview(GameObject prefab)
        {
            if (AdaptivePrefabPreviewUtility.Uses3DPreview())
                return false;

            bool changed = false;
            var spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            var shader = Shader.Find("Sprites/Default");
            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer == null || shader == null)
                    continue;

                if (spriteRenderer.material == null
                    || spriteRenderer.material.shader == null
                    || !spriteRenderer.material.shader.name.Contains("Sprite"))
                {
                    spriteRenderer.material = new Material(shader);
                    changed = true;
                }
            }

            return changed;
        }
    }
}
