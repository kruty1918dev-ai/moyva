using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Налаштовує гізмо та превью рендеринг для 2D режиму.
    /// </summary>
    [InitializeOnLoad]
    public static class Gizmo2DSetup
    {
        static Gizmo2DSetup()
        {
            EditorApplication.update += Initialize;
        }

        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            // Гізмо налаштування виконуються через меню
        }

        /// <summary>
        /// Показує інформацію про 2D гізмо
        /// </summary>
        [MenuItem("Moyva/Gizmo/Show Sprite Bounds", priority = 200)]
        public static void ShowSpriteBounds()
        {
            EditorUtility.DisplayDialog("Info", 
                "SpriteRenderer гізмо видні за замовчуванням у Scene View.\n\n" +
                "Для управління видимістю:\n" +
                "1. Відкрийте Scene View\n" +
                "2. Натисніть на іконку 'Gizmos' (верхній праву кут)\n" +
                "3. Виберіть компоненти які хочете бачити",
                "OK");
        }

        /// <summary>
        /// Вимикає 3D гізмо для очищення перегляду
        /// </summary>
        [MenuItem("Moyva/Gizmo/Hide 3D Bounds", priority = 201)]
        public static void Hide3DBounds()
        {
            EditorUtility.DisplayDialog("Info", 
                "Для вимикання 3D гізмо:\n\n" +
                "1. У Scene View знайдіть меню 'Gizmos'\n" +
                "2. Вимкніть: 3D Icons, Collider Bounds\n" +
                "3. Залишіть SpriteRenderer включеним",
                "OK");
        }
    }
}
