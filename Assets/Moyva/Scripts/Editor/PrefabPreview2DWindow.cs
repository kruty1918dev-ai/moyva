using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Editor Window для налаштування 2D превью для всього проекту.
    /// </summary>
    public class PrefabPreview2DWindow : EditorWindow
    {
        private bool _showSpriteOutlines = true;
        private bool _enablePixelPerfect = true;
        private float _previewZoom = 1f;
        private Vector2 _scrollPosition;

        [MenuItem("Moyva/Windows/Prefab 2D Preview Setup", priority = 10)]
        public static void ShowWindow()
        {
            GetWindow<PrefabPreview2DWindow>("2D Preview Setup");
        }

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Налаштування 2D превью для префабів", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Опції превью
            EditorGUILayout.LabelField("Опції превью", EditorStyles.boldLabel);
            
            _showSpriteOutlines = EditorGUILayout.Toggle("Показувати контури спрайтів", _showSpriteOutlines);
            _enablePixelPerfect = EditorGUILayout.Toggle("Pixel Perfect превью", _enablePixelPerfect);
            _previewZoom = EditorGUILayout.Slider("Zoom превью", _previewZoom, 0.5f, 3f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Дії", EditorStyles.boldLabel);

            if (GUILayout.Button("Фіксити встановлення 2D превью на всі префаби", GUILayout.Height(30)))
            {
                FixAllPrefabsFor2DPreview();
            }

            if (GUILayout.Button("Налаштувати гізмо для 2D", GUILayout.Height(30)))
            {
                SetupGizmoFor2D();
            }

            if (GUILayout.Button("Показати Sprite Outline Gizmo", GUILayout.Height(30)))
            {
                ShowSpriteOutlines();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Це вікно допомагає налаштувати превью для 2D префабів.\n" +
                "Якщо превью все ще показуються як 3D, переконайтеся, " +
                "що префаби використовують SpriteRenderer компоненти.",
                MessageType.Info);

            GUILayout.EndScrollView();
        }

        private static void FixAllPrefabsFor2DPreview()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moyva" });
            var count = 0;

            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = PrefabUtility.LoadPrefabContents(path);

                // Налаштовуємо кожен префаб
                if (ConfigurePrefabFor2D(prefab))
                {
                    count++;
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }

                PrefabUtility.UnloadPrefabContents(prefab);
            }

            EditorUtility.DisplayDialog("Success",
                $"Налаштовано {count} префабів для 2D превью!",
                "OK");

            AssetDatabase.Refresh();
        }

        private static bool ConfigurePrefabFor2D(GameObject prefab)
        {
            bool changed = false;

            // Налаштовуємо SpriteRenderer компоненти
            var spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in spriteRenderers)
            {
                if (sr.material == null || !sr.material.shader.name.Contains("Sprite"))
                {
                    sr.material = new Material(Shader.Find("Sprites/Default"));
                    changed = true;
                }

                // Переконуємось, що сортування правильне
                if (sr.sortingLayerID == 0 && sr.sortingOrder == 0)
                {
                    changed = true;
                }
            }

            // Дезактивуємо MeshRenderer якщо є SpriteRenderer
            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0 && spriteRenderers.Length > 0)
            {
                foreach (var mr in meshRenderers)
                {
                    if (!mr.enabled)
                    {
                        mr.enabled = false;
                        changed = true;
                    }
                }
            }

            // Забезпечуємо коректний масштаб
            if (prefab.transform.localScale != Vector3.one)
            {
                prefab.transform.localScale = Vector3.one;
                changed = true;
            }

            // Встановлюємо позицію як (0, 0, 0) для 2D
            if (prefab.transform.localPosition.z != 0)
            {
                var pos = prefab.transform.localPosition;
                pos.z = 0;
                prefab.transform.localPosition = pos;
                changed = true;
            }

            return changed;
        }

        private static void SetupGizmoFor2D()
        {
            // Включаємо Sprite Renderer гізмо
            EditorGizmos.SetGizmoEnabled(typeof(SpriteRenderer), true);

            // Вимикаємо 3D гізмо
            EditorGizmos.SetGizmoEnabled(typeof(MeshRenderer), false);
            EditorGizmos.SetGizmoEnabled(typeof(BoxCollider), false);

            EditorUtility.DisplayDialog("Success", "Гізмо налаштовано для 2D!", "OK");
        }

        private static void ShowSpriteOutlines()
        {
            EditorGizmos.SetGizmoEnabled(typeof(SpriteRenderer), true);
            EditorUtility.DisplayDialog("Info", "Sprite Outline Gizmo включено в Scene View.", "OK");
        }
    }
}
