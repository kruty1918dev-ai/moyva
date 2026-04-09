using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Налаштовує гізмо та превью рендеринг для 2D режиму.
    /// Забезпечує, що спрайти показуються коректно у Project вкладці.
    /// </summary>
    [InitializeOnLoad]
    public static class Gizmo2DSetup
    {
        static Gizmo2DSetup()
        {
            EditorApplication.update += ConfigureGizmo2D;
        }

        private static void ConfigureGizmo2D()
        {
            EditorApplication.update -= ConfigureGizmo2D;

            // Налаштовуємо гізмо для 2D компонентів
            Gizmos.color = Color.white;
            
            // Вимикаємо 3D гізмо, включаємо 2D
            SetupGizmo2DSettings();
        }

        private static void SetupGizmo2DSettings()
        {
            // Знаходимо Editor preferences та налаштовуємо для 2D
            var prefsKey = "Gizmos/Sprite Renderer";
            
            // Переконуємось, що Sprite Renderer гізмо видимі
            EditorGizmos.SetGizmoEnabled(typeof(SpriteRenderer), true);
        }

        /// <summary>
        /// Показує 2D баундінгкс для спрайтів у сценах
        /// </summary>
        [MenuItem("Moyva/Gizmo/Show Sprite Bounds", priority = 200)]
        public static void ShowSpriteBounds()
        {
            EditorGizmos.SetGizmoEnabled(typeof(SpriteRenderer), true);
            EditorUtility.DisplayDialog("Success", "Sprite Bounds гізмо включено!", "OK");
        }

        /// <summary>
        /// Вимикає 3D гізмо для очищення перегляду
        /// </summary>
        [MenuItem("Moyva/Gizmo/Hide 3D Bounds", priority = 201)]
        public static void Hide3DBounds()
        {
            EditorGizmos.SetGizmoEnabled(typeof(MeshRenderer), false);
            EditorGizmos.SetGizmoEnabled(typeof(BoxCollider), false);
            EditorGizmos.SetGizmoEnabled(typeof(SphereCollider), false);
            EditorUtility.DisplayDialog("Success", "3D гізмо приховано!", "OK");
        }
    }

    /// <summary>
    /// Обробляє превью префабів для коректного відображення в 2D.
    /// </summary>
    public class Prefab2DAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessPrefab(GameObject gameObject)
        {
            // Налаштовуємо префаб для 2D превью
            ConfigurePrefabFor2D(gameObject);
        }

        private static void ConfigurePrefabFor2D(GameObject prefab)
        {
            // Переконуємось, що всі SpriteRenderer компоненти налаштовані правильно
            var spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in spriteRenderers)
            {
                // Встановлюємо сортування orden для коректної глибини
                if (sr.sortingOrder == 0 && sr.sprite != null)
                {
                    sr.sortingOrder = 0;
                }
                
                // Переконуємось, що матеріал використовує 2D шейдер
                if (sr.material != null && sr.material.shader != null)
                {
                    // Перевіряємо чи шейдер підходить для спрайтів
                    var shaderName = sr.material.shader.name;
                    if (!shaderName.Contains("Sprite"))
                    {
                        // Якщо не Sprite шейдер, встановлюємо правильний
                        sr.material = new Material(Shader.Find("Sprites/Default"));
                    }
                }
            }

            // Видаляємо непотрібні 3D компоненти з превью
            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in meshRenderers)
            {
                if (!prefab.GetComponent<SpriteRenderer>())
                {
                    // Тільки якщо немає SpriteRenderer
                    mr.enabled = false;
                }
            }
        }
    }
}
