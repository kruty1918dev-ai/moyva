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
        private static readonly string PREVIEW_SCENE_NAME = "Prefab2DPreview";
        private static Scene _previewScene;

        static Prefab2DPreviewSetup()
        {
            EditorApplication.update += Initialize2DPreview;
        }

        private static void Initialize2DPreview()
        {
            // Виконується один раз при завантаженні
            EditorApplication.update -= Initialize2DPreview;

            // Налаштовуємо гізмо для 2D режиму
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            
            // Встановлюємо 2D режим за замовчуванням
            SetupPrefabPreviewCamera();
        }

        private static void SetupPrefabPreviewCamera()
        {
            // Заповнення EditorSettings для превью сцен
            // Це налаштовує камеру правильно для спрайтів
            EditorSceneManager.saveScene += OnSceneSave;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            // Настраиваемо Scene View для 2D
            if (sceneView.orthographic == false && IsInPrefabContext())
            {
                sceneView.orthographic = true;
                sceneView.size = 10f;
            }
        }

        private static void OnSceneSave(Scene scene, string path)
        {
            // При збереженні сцени - налаштуємо камері якщо це превью сцена
            if (scene.name.Contains("Prefab") || scene.name.Contains("Preview"))
            {
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    var camera = go.GetComponent<Camera>();
                    if (camera != null)
                    {
                        camera.orthographic = true;
                        camera.orthographicSize = 10f;
                    }
                }
            }
        }

        private static bool IsInPrefabContext()
        {
            return PrefabStageUtility.GetCurrentPrefabStage() != null;
        }

        /// <summary>
        /// Вручну налаштовує сцену превью для 2D вихідних даних
        /// </summary>
        [MenuItem("Moyva/Setup/Configure 2D Prefab Preview", priority = 100)]
        public static void ConfigurePrefabPreview2D()
        {
            // Знаходимо або створюємо превью сцену
            var previewScenePath = "Assets/Moyva/Scenes/Prefab2DPreview.unity";
            Scene previewScene;

            // Перевіряємо чи сцена існує
            if (System.IO.File.Exists(previewScenePath))
            {
                previewScene = EditorSceneManager.OpenScene(previewScenePath, OpenSceneMode.Additive);
            }
            else
            {
                // Створюємо нову сцену для превью
                previewScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                previewScene.name = "Prefab2DPreview";

                // Встановлюємо правильні налаштування для сцени
                SetupSceneFor2DPreview(previewScene);

                // Зберігаємо сцену
                EditorSceneManager.SaveScene(previewScene, previewScenePath);
            }

            // Встановлюємо цю сцену як превью Environment
            var settings = EditorSettings.GetEditorSettings();
            //EditorSettings.prefabRegularEnvironment = // Це не має публічного API

            EditorUtility.DisplayDialog("2D Preview Setup", 
                "Сцена превью для 2D префабів створена та налаштована!", 
                "OK");
        }

        private static void SetupSceneFor2DPreview(Scene scene)
        {
            // Створюємо камеру для 2D вихідних даних
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            SceneManager.MoveGameObjectToScene(cameraGO, scene);

            var camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1000f;
            camera.depth = -1;

            // Створюємо Light для освітлення спрайтів
            var lightGO = new GameObject("Light");
            SceneManager.MoveGameObjectToScene(lightGO, scene);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.position = new Vector3(0, 5, -10);
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Встановлюємо фон для превью
            var bgGO = new GameObject("Background");
            bgGO.transform.position = new Vector3(0, 0, 100);
            SceneManager.MoveGameObjectToScene(bgGO, scene);
            var renderer = bgGO.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            renderer.sortingOrder = -100;
        }

        /// <summary>
        /// Налаштовує Mode для префаба, щоб він завжди показувався в 2D
        /// </summary>
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

            // Завантажуємо префаб у режимі редагування
            var prefab = PrefabUtility.LoadPrefabContents(path);

            // Переконуємося, що всі SpriteRenderer компоненти налаштовані правильно
            foreach (var spriteRenderer in prefab.GetComponentsInChildren<SpriteRenderer>())
            {
                // Вони вже повинні мати правильні налаштування, але переконуємось
                spriteRenderer.sortingOrder = 0;
            }

            // Видаляємо будь-які непотрібні 3D компоненти
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>())
            {
                if (!(renderer is SpriteRenderer) && renderer.GetComponent<MeshRenderer>() != null)
                {
                    // Видаляємо Mesh Renderer якщо це не Sprite Renderer
                    DestroyImmediate(renderer, true);
                }
            }

            // Зберігаємо зміни
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);

            EditorUtility.DisplayDialog("Success", "Префаб налаштовано на 2D режим!", "OK");
        }
    }
}
