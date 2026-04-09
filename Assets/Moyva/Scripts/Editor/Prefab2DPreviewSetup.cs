using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Налаштовує превью префабів для 2D режиму замість 3D.
    /// Це забезпечує коректне відображення спрайтів у Project вкладці.
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

        [MenuItem("Moyva/Setup/Force 2D on Selected Prefab", priority = 101)]
        public static void Force2DOnSelectedPrefab()
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
            ConfigurePrefabFor2D(prefab);
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);

            EditorUtility.DisplayDialog("Success", "Префаб налаштовано на 2D режим!", "OK");
        }

        private static void ConfigurePrefabFor2D(GameObject prefab)
        {
            var spriteRenderer = prefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 0;
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    spriteRenderer.material = new Material(shader);
                }
            }

            var meshRenderer = prefab.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            var transform = prefab.transform;
            var pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, 0);
            transform.localScale = Vector3.one;
        }
    }
}
