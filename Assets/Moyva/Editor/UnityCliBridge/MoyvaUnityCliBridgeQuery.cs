#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Editor.UnityCliBridge
{
    internal static class MoyvaUnityCliQuery
    {
        internal static string ProjectStatusJson()
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = NewResult("project-status", scene);
            result.message = "Moyva Unity CLI bridge is active.";
            return ToJson(result);
        }

        internal static string SceneTreeJson()
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = NewResult("scene-tree", scene);

            if (!scene.IsValid() || !scene.isLoaded)
                return ErrorJson(result, "Active scene is invalid or not loaded.");

            foreach (GameObject root in scene.GetRootGameObjects())
                AddHierarchyRecursive(root.transform, result.nodes, includeOnlyUi: false);

            result.count = result.nodes.Count;
            result.message = $"Captured {result.count} scene objects.";
            return ToJson(result);
        }

        internal static string UiAuditJson()
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = NewResult("ui-audit", scene);

            if (!scene.IsValid() || !scene.isLoaded)
                return ErrorJson(result, "Active scene is invalid or not loaded.");

            foreach (GameObject root in scene.GetRootGameObjects())
                AddHierarchyRecursive(root.transform, result.nodes, includeOnlyUi: true);

            result.count = result.nodes.Count;
            result.message = $"Captured {result.count} UI-related objects.";
            return ToJson(result);
        }

        internal static string InspectObjectJson(string hierarchyPath)
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = NewResult("ui-object", scene);

            GameObject target = FindByHierarchyPath(scene, hierarchyPath);
            if (target == null)
                return ErrorJson(result, $"Object not found: {hierarchyPath}");

            result.nodes.Add(Snapshot(target));
            result.count = 1;
            result.message = $"Captured {hierarchyPath}.";
            return ToJson(result);
        }

        internal static GameObject FindByHierarchyPath(Scene scene, string hierarchyPath)
        {
            if (!scene.IsValid() || string.IsNullOrWhiteSpace(hierarchyPath))
                return null;

            string clean = hierarchyPath.Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(clean))
                return null;

            string[] parts = clean.Split('/');
            GameObject current = scene.GetRootGameObjects()
                .FirstOrDefault(go => string.Equals(go.name, parts[0], StringComparison.Ordinal));

            if (current == null)
                return null;

            for (int i = 1; i < parts.Length; i++)
            {
                Transform next = null;
                for (int c = 0; c < current.transform.childCount; c++)
                {
                    Transform child = current.transform.GetChild(c);
                    if (string.Equals(child.name, parts[i], StringComparison.Ordinal))
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                    return null;

                current = next.gameObject;
            }

            return current;
        }

        internal static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            var names = new Stack<string>();
            Transform cursor = transform;
            while (cursor != null)
            {
                names.Push(cursor.name);
                cursor = cursor.parent;
            }

            return string.Join("/", names);
        }

        internal static BridgeResult NewResult(string operation, Scene scene)
        {
            return new BridgeResult
            {
                operation = operation,
                projectPath = System.IO.Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                unityVersion = Application.unityVersion,
                scene = scene.IsValid() ? scene.name : string.Empty,
                scenePath = scene.IsValid() ? scene.path : string.Empty,
                isPlaying = EditorApplication.isPlaying,
                isCompiling = EditorApplication.isCompiling,
                isUpdating = EditorApplication.isUpdating,
                isSceneDirty = scene.IsValid() && scene.isDirty,
            };
        }

        internal static string ToJson(BridgeResult result)
        {
            return JsonUtility.ToJson(result, true);
        }

        internal static string ErrorJson(BridgeResult result, string message)
        {
            result.ok = false;
            result.message = message;
            return ToJson(result);
        }

        private static void AddHierarchyRecursive(
            Transform transform,
            List<UiNodeSnapshot> output,
            bool includeOnlyUi)
        {
            GameObject go = transform.gameObject;
            bool isUi =
                go.GetComponent<RectTransform>() != null ||
                go.GetComponent<Canvas>() != null ||
                go.GetComponent<Graphic>() != null ||
                go.GetComponent<TMP_Text>() != null ||
                go.GetComponent<Selectable>() != null ||
                go.GetComponent<LayoutGroup>() != null ||
                go.GetComponent<ContentSizeFitter>() != null;

            if (!includeOnlyUi || isUi)
                output.Add(Snapshot(go));

            for (int i = 0; i < transform.childCount; i++)
                AddHierarchyRecursive(transform.GetChild(i), output, includeOnlyUi);
        }

        private static UiNodeSnapshot Snapshot(GameObject go)
        {
            var snapshot = new UiNodeSnapshot
            {
                name = go.name,
                path = GetHierarchyPath(go.transform),
                activeSelf = go.activeSelf,
                activeInHierarchy = go.activeInHierarchy,
                tag = go.tag,
                layer = go.layer,
                components = go.GetComponents<Component>()
                    .Where(component => component != null)
                    .Select(component => component.GetType().FullName)
                    .ToArray(),
            };

            try
            {
                snapshot.globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
            }
            catch
            {
                snapshot.globalObjectId = string.Empty;
            }

            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                snapshot.hasRectTransform = true;
                snapshot.anchorMin = rect.anchorMin;
                snapshot.anchorMax = rect.anchorMax;
                snapshot.pivot = rect.pivot;
                snapshot.anchoredPosition = rect.anchoredPosition;
                snapshot.sizeDelta = rect.sizeDelta;
                snapshot.localScale = rect.localScale;
            }

            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas != null)
            {
                snapshot.isCanvas = true;
                snapshot.canvasRenderMode = canvas.renderMode.ToString();
                snapshot.canvasSortingOrder = canvas.sortingOrder;
            }

            Image image = go.GetComponent<Image>();
            if (image != null)
            {
                snapshot.isImage = true;
                snapshot.imageColor = image.color;
                snapshot.imageRaycastTarget = image.raycastTarget;
                if (image.sprite != null)
                {
                    snapshot.imageSprite = image.sprite.name;
                    snapshot.imageSpritePath = AssetDatabase.GetAssetPath(image.sprite);
                }
            }

            TMP_Text tmp = go.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                snapshot.isText = true;
                snapshot.text = tmp.text;
                snapshot.fontSize = tmp.fontSize;
                snapshot.textColor = tmp.color;
                snapshot.fontAsset = tmp.font != null ? AssetDatabase.GetAssetPath(tmp.font) : string.Empty;
            }

            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                snapshot.isButton = true;
                snapshot.buttonInteractable = button.interactable;
                snapshot.buttonPersistentListenerCount = button.onClick.GetPersistentEventCount();
            }

            LayoutGroup layout = go.GetComponent<LayoutGroup>();
            if (layout != null)
            {
                snapshot.isLayoutGroup = true;
                snapshot.layoutGroupType = layout.GetType().Name;
            }

            ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                snapshot.hasContentSizeFitter = true;
                snapshot.contentSizeHorizontal = fitter.horizontalFit.ToString();
                snapshot.contentSizeVertical = fitter.verticalFit.ToString();
            }

            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefabSource != null)
                snapshot.prefabAssetPath = AssetDatabase.GetAssetPath(prefabSource);

            return snapshot;
        }
    }
}

#endif
