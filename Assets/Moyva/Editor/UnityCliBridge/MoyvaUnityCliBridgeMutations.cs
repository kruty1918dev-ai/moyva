#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Editor.UnityCliBridge
{
    internal static class MoyvaUnityCliMutations
    {
        internal static string Select(string hierarchyPath)
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-select", scene);
            GameObject target = MoyvaUnityCliQuery.FindByHierarchyPath(scene, hierarchyPath);
            if (target == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Object not found: {hierarchyPath}");

            Selection.activeGameObject = target;
            EditorGUIUtility.PingObject(target);
            result.message = $"Selected {hierarchyPath}.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        internal static string SetActive(string hierarchyPath, bool active)
        {
            return Mutate("ui-set-active", hierarchyPath, target =>
            {
                Undo.RecordObject(target, "Moyva CLI Set Active");
                target.SetActive(active);
            });
        }

        internal static string SetText(string hierarchyPath, string text)
        {
            return Mutate("ui-set-text", hierarchyPath, target =>
            {
                TMP_Text tmp = target.GetComponent<TMP_Text>();
                if (tmp == null)
                    throw new InvalidOperationException("Target does not have TMP_Text.");

                Undo.RecordObject(tmp, "Moyva CLI Set Text");
                tmp.text = text ?? string.Empty;
                EditorUtility.SetDirty(tmp);
            });
        }

        internal static string SetSprite(string hierarchyPath, string assetPath)
        {
            return Mutate("ui-set-sprite", hierarchyPath, target =>
            {
                Image image = target.GetComponent<Image>();
                if (image == null)
                    throw new InvalidOperationException("Target does not have UnityEngine.UI.Image.");

                Sprite sprite = string.IsNullOrWhiteSpace(assetPath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                if (!string.IsNullOrWhiteSpace(assetPath) && sprite == null)
                    throw new InvalidOperationException($"Sprite not found at asset path: {assetPath}");

                Undo.RecordObject(image, "Moyva CLI Set Sprite");
                image.sprite = sprite;
                EditorUtility.SetDirty(image);
            });
        }

        internal static string SetRect(
            string hierarchyPath,
            float x,
            float y,
            float width,
            float height)
        {
            return Mutate("ui-set-rect", hierarchyPath, target =>
            {
                RectTransform rect = target.GetComponent<RectTransform>();
                if (rect == null)
                    throw new InvalidOperationException("Target does not have RectTransform.");

                Undo.RecordObject(rect, "Moyva CLI Set Rect");
                rect.anchoredPosition = new Vector2(x, y);
                rect.sizeDelta = new Vector2(width, height);
                EditorUtility.SetDirty(rect);
            });
        }

        internal static string SetAnchors(
            string hierarchyPath,
            float minX,
            float minY,
            float maxX,
            float maxY,
            float pivotX,
            float pivotY)
        {
            return Mutate("ui-set-anchors", hierarchyPath, target =>
            {
                RectTransform rect = target.GetComponent<RectTransform>();
                if (rect == null)
                    throw new InvalidOperationException("Target does not have RectTransform.");

                Undo.RecordObject(rect, "Moyva CLI Set Anchors");
                rect.anchorMin = new Vector2(minX, minY);
                rect.anchorMax = new Vector2(maxX, maxY);
                rect.pivot = new Vector2(pivotX, pivotY);
                EditorUtility.SetDirty(rect);
            });
        }

        internal static string Reparent(string hierarchyPath, string newParentPath)
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-reparent", scene);

            GameObject target = MoyvaUnityCliQuery.FindByHierarchyPath(scene, hierarchyPath);
            GameObject parent = MoyvaUnityCliQuery.FindByHierarchyPath(scene, newParentPath);
            if (target == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Object not found: {hierarchyPath}");
            if (parent == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Parent not found: {newParentPath}");
            if (target.transform == parent.transform || parent.transform.IsChildOf(target.transform))
                return MoyvaUnityCliQuery.ErrorJson(result, "Invalid hierarchy cycle.");

            Undo.SetTransformParent(target.transform, parent.transform, "Moyva CLI Reparent");
            EditorSceneManager.MarkSceneDirty(scene);
            result.message = $"Reparented {hierarchyPath} under {newParentPath}. Scene is dirty but not saved.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        internal static string CreatePanel(string parentPath, string name)
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-create-panel", scene);
            GameObject parent = MoyvaUnityCliQuery.FindByHierarchyPath(scene, parentPath);
            if (parent == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Parent not found: {parentPath}");

            var go = new GameObject(
                string.IsNullOrWhiteSpace(name) ? "Panel" : name.Trim(),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            Undo.RegisterCreatedObjectUndo(go, "Moyva CLI Create Panel");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 180f);

            Image image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.65f);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(scene);
            result.message = $"Created panel {MoyvaUnityCliQuery.GetHierarchyPath(go.transform)}. Scene is dirty but not saved.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        internal static string CreateText(string parentPath, string name, string text)
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-create-text", scene);
            GameObject parent = MoyvaUnityCliQuery.FindByHierarchyPath(scene, parentPath);
            if (parent == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Parent not found: {parentPath}");

            var go = new GameObject(
                string.IsNullOrWhiteSpace(name) ? "Text" : name.Trim(),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            Undo.RegisterCreatedObjectUndo(go, "Moyva CLI Create Text");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 60f);

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text ?? string.Empty;
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.Center;

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(scene);
            result.message = $"Created text {MoyvaUnityCliQuery.GetHierarchyPath(go.transform)}. Scene is dirty but not saved.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        internal static string Delete(string hierarchyPath, string confirmation)
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-delete", scene);

            if (!string.Equals(confirmation, "DELETE", StringComparison.Ordinal))
                return MoyvaUnityCliQuery.ErrorJson(result, "Deletion requires --confirm DELETE.");

            GameObject target = MoyvaUnityCliQuery.FindByHierarchyPath(scene, hierarchyPath);
            if (target == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Object not found: {hierarchyPath}");

            string oldPath = MoyvaUnityCliQuery.GetHierarchyPath(target.transform);
            Undo.DestroyObjectImmediate(target);
            EditorSceneManager.MarkSceneDirty(scene);
            result.message = $"Deleted {oldPath}. Scene is dirty but not saved; Undo is available.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        internal static string SaveActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-save-scene", scene);

            if (!scene.IsValid() || string.IsNullOrWhiteSpace(scene.path))
                return MoyvaUnityCliQuery.ErrorJson(result, "Active scene has no saved asset path.");

            string backup = BackupSceneFile(scene.path);
            if (string.IsNullOrWhiteSpace(backup))
                return MoyvaUnityCliQuery.ErrorJson(result, "Could not create external scene backup; save aborted.");

            bool saved = EditorSceneManager.SaveScene(scene);
            if (!saved)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Unity failed to save {scene.path}.");

            result.backupPath = backup;
            result.message = $"Saved {scene.path}; previous disk version backed up externally.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        internal static string UndoLast()
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult("ui-undo", scene);
            Undo.PerformUndo();
            result.message = "Unity Undo.PerformUndo executed.";
            return MoyvaUnityCliQuery.ToJson(result);
        }

        private static string Mutate(string operation, string hierarchyPath, Action<GameObject> mutation)
        {
            Scene scene = SceneManager.GetActiveScene();
            BridgeResult result = MoyvaUnityCliQuery.NewResult(operation, scene);
            GameObject target = MoyvaUnityCliQuery.FindByHierarchyPath(scene, hierarchyPath);
            if (target == null)
                return MoyvaUnityCliQuery.ErrorJson(result, $"Object not found: {hierarchyPath}");

            try
            {
                mutation(target);
                EditorSceneManager.MarkSceneDirty(scene);
                result.message = $"{operation} applied to {hierarchyPath}. Scene is dirty but not saved.";
                return MoyvaUnityCliQuery.ToJson(result);
            }
            catch (Exception exception)
            {
                return MoyvaUnityCliQuery.ErrorJson(result, exception.Message);
            }
        }

        private static string BackupSceneFile(string sceneAssetPath)
        {
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                if (string.IsNullOrWhiteSpace(projectRoot))
                    return string.Empty;

                string source = Path.Combine(
                    projectRoot,
                    sceneAssetPath.Replace('/', Path.DirectorySeparatorChar));

                if (!File.Exists(source))
                    return string.Empty;

                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupRoot = Path.Combine(
                    home,
                    ".local",
                    "share",
                    "moyva-cli",
                    "backups",
                    "scenes",
                    stamp);

                string destination = Path.Combine(
                    backupRoot,
                    sceneAssetPath.Replace('/', Path.DirectorySeparatorChar));

                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, true);

                string meta = source + ".meta";
                if (File.Exists(meta))
                    File.Copy(meta, destination + ".meta", true);

                return destination;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MoyvaUnityCliBridge] Scene backup failed: {exception}");
                return string.Empty;
            }
        }
    }
}

#endif
