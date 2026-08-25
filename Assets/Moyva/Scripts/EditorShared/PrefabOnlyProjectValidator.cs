using System;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Shared;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Editor.Shared
{
    /// <summary>
    /// Перевіряє prefab-only посилання, missing scripts і матеріали production-візуалів Moyva.
    /// </summary>
    public static class PrefabOnlyProjectValidator
    {
        private const string MoyvaAssetsRoot = "Assets/Moyva";

        /// <summary>
        /// Запускає перевірку з меню Unity та виводить підсумок у Console.
        /// </summary>
        [MenuItem("Moyva/Validation/Validate Prefab-Only Visuals", false, 500)]
        public static void ValidateFromMenu()
        {
            IReadOnlyList<string> errors = ValidateProject();
            if (errors.Count == 0)
            {
                Debug.Log("[PrefabOnlyValidation] Validation passed.");
                return;
            }

            foreach (string error in errors)
                Debug.LogError($"[PrefabOnlyValidation] {error}");
        }

        /// <summary>
        /// Виконує batch-перевірку та завершує збірку помилкою за наявності порушень.
        /// </summary>
        public static void ValidateBatch()
        {
            IReadOnlyList<string> errors = ValidateProject();
            if (errors.Count == 0)
            {
                Debug.Log("[PrefabOnlyValidation] Validation passed.");
                return;
            }

            throw new BuildFailedException(
                $"Prefab-only validation failed with {errors.Count} error(s):\n" +
                string.Join("\n", errors));
        }

        /// <summary>
        /// Перевіряє всі Moyva-prefab-и та увімкнені build-сцени без зміни їхнього вмісту.
        /// </summary>
        /// <returns>Незмінний список знайдених порушень.</returns>
        public static IReadOnlyList<string> ValidateProject()
        {
            var errors = new List<string>();
            ValidatePrefabs(errors);
            ValidateBuildScenes(errors);
            return errors;
        }

        private static void ValidatePrefabs(List<string> errors)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { MoyvaAssetsRoot });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root != null)
                    ValidateHierarchy(root, path, errors);
            }
        }

        private static void ValidateBuildScenes(List<string> errors)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled || string.IsNullOrWhiteSpace(buildScene.path))
                    continue;

                Scene scene = SceneManager.GetSceneByPath(buildScene.path);
                bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
                if (openedForValidation)
                    scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Additive);

                try
                {
                    foreach (GameObject root in scene.GetRootGameObjects())
                        ValidateHierarchy(root, buildScene.path, errors);
                }
                finally
                {
                    if (openedForValidation && scene.IsValid())
                        EditorSceneManager.CloseScene(scene, removeScene: true);
                }
            }
        }

        private static void ValidateHierarchy(GameObject root, string assetPath, List<string> errors)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                GameObject gameObject = transform.gameObject;
                int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missingScripts > 0)
                {
                    errors.Add(
                        $"{assetPath}: '{HierarchyPath(transform)}' contains {missingScripts} missing script(s).");
                }

                foreach (MonoBehaviour behaviour in gameObject.GetComponents<MonoBehaviour>())
                {
                    if (behaviour != null)
                        ValidateRequiredFields(behaviour, assetPath, errors);
                }
            }
        }

        private static void ValidateRequiredFields(
            MonoBehaviour behaviour,
            string assetPath,
            List<string> errors)
        {
            for (Type type = behaviour.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (FieldInfo field in fields)
                {
                    RequiredReferenceAttribute requirement =
                        field.GetCustomAttribute<RequiredReferenceAttribute>(inherit: true);
                    if (requirement == null)
                        continue;

                    Object value = field.GetValue(behaviour) as Object;
                    string location = $"{assetPath}: {behaviour.GetType().Name}.{field.Name}";
                    if (value == null)
                    {
                        errors.Add($"{location} is required but not assigned.");
                        continue;
                    }

                    ValidateReferenceOrigin(value, requirement.Kind, location, errors);
                    ValidateVisualAsset(value, location, errors);
                }
            }
        }

        private static void ValidateReferenceOrigin(
            Object value,
            RequiredReferenceKind kind,
            string location,
            List<string> errors)
        {
            if (kind == RequiredReferenceKind.Prefab && !PrefabUtility.IsPartOfPrefabAsset(value))
                errors.Add($"{location} must reference a prefab asset, but points to '{value.name}'.");

            if (kind != RequiredReferenceKind.Scene)
                return;

            GameObject gameObject = value as GameObject;
            if (value is Component component)
                gameObject = component.gameObject;
            if (gameObject == null || !gameObject.scene.IsValid())
                errors.Add($"{location} must reference a scene object, but points to '{value.name}'.");
        }

        private static void ValidateVisualAsset(Object value, string location, List<string> errors)
        {
            if (value is Material material)
            {
                ValidateMaterial(material, location, errors);
                return;
            }

            GameObject root = value as GameObject;
            if (value is Component component)
                root = component.gameObject;
            if (root == null)
                return;

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials.Length == 0)
                    errors.Add($"{location}: renderer '{HierarchyPath(renderer.transform)}' has no material.");
                foreach (Material assignedMaterial in materials)
                    ValidateMaterial(assignedMaterial, location, errors);
            }
        }

        private static void ValidateMaterial(Material material, string location, List<string> errors)
        {
            if (material == null)
            {
                errors.Add($"{location}: renderer has an unassigned material slot.");
                return;
            }

            if (material.shader == null || material.shader.name == "Hidden/InternalErrorShader")
                errors.Add($"{location}: material '{material.name}' has an invalid shader.");
        }

        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = $"{transform.name}/{path}";
            }

            return path;
        }
    }
}
