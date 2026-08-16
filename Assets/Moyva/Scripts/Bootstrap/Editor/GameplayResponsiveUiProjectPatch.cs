using System;
using System.Linq;
using Kruty1918.Moyva.Bootstrap.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    /// <summary>
    /// Scene-side installer for the responsive gameplay HUD layer.
    /// Designed to be executed from the already-running Unity Editor.
    /// </summary>
    public static class GameplayResponsiveUiProjectPatch
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string HudRootName = "GameplayTurnHud";
        private const string LogTag = "[MOYVA_RESPONSIVE_UI]";
        private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        [MenuItem("Moyva/Gameplay/Apply Responsive UI", false, 121)]
        public static void ApplyFromMenu()
        {
            try
            {
                string result = ApplyAndSave();
                Debug.Log($"{LogTag} {result}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError($"{LogTag} FAILED {exception.Message}");
                throw;
            }
        }

        [MenuItem("Moyva/Gameplay/Validate Responsive UI", false, 122)]
        public static void ValidateFromMenu()
        {
            Scene scene = FindOrOpenGameplayScene();
            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException($"No gameplay Canvas found in '{ScenePath}'.");

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                throw new InvalidOperationException("Gameplay Canvas is not configured for Scale With Screen Size.");

            Transform root = FindDirectChild(canvas.transform, HudRootName);
            if (root == null)
                throw new InvalidOperationException($"HUD root '{HudRootName}' is missing.");

            GameplayResponsiveHudLayout responsive = root.GetComponent<GameplayResponsiveHudLayout>();
            if (responsive == null)
                throw new InvalidOperationException("GameplayResponsiveHudLayout is missing from GameplayTurnHud.");
            if (!responsive.ApplyLayoutNow())
                throw new InvalidOperationException("GameplayResponsiveHudLayout could not bind the authored HUD hierarchy.");

            Debug.Log($"{LogTag} VALIDATION_OK scene={ScenePath} reference={scaler.referenceResolution} match={scaler.matchWidthOrHeight:0.00}");
        }

        public static string ApplyAndSave()
        {
            Scene scene = FindOrOpenGameplayScene();
            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException($"No gameplay Canvas found in '{ScenePath}'.");

            ConfigureCanvasScaler(canvas);

            Transform root = FindDirectChild(canvas.transform, HudRootName);
            if (root == null)
            {
                throw new InvalidOperationException(
                    $"HUD root '{HudRootName}' is missing. Run 'Moyva/Gameplay/Author Scene Turn HUD' once, then re-run this command.");
            }

            RectTransform rootRect = root as RectTransform;
            if (rootRect == null)
                throw new InvalidOperationException("GameplayTurnHud is not a RectTransform.");
            Stretch(rootRect);

            NormalizeResourceBar(canvas, root);
            NormalizeBuildButton(canvas, root);

            GameplayResponsiveHudLayout responsive = root.GetComponent<GameplayResponsiveHudLayout>();
            if (responsive == null)
            {
                responsive = Undo.AddComponent<GameplayResponsiveHudLayout>(root.gameObject);
                EditorUtility.SetDirty(root.gameObject);
            }

            Canvas.ForceUpdateCanvases();
            if (!responsive.ApplyLayoutNow())
                throw new InvalidOperationException("Responsive layout could not bind the current GameplayTurnHud hierarchy.");

            root.SetAsLastSibling();
            EditorUtility.SetDirty(responsive);
            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            return $"APPLY_OK scene={ScenePath} reference={scaler.referenceResolution} match={scaler.matchWidthOrHeight:0.00} root={HudRootName}";
        }

        private static Scene FindOrOpenGameplayScene()
        {
            Scene loaded = SceneManager.GetSceneByPath(ScenePath);
            if (loaded.IsValid() && loaded.isLoaded)
            {
                SceneManager.SetActiveScene(loaded);
                return loaded;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                throw new OperationCanceledException("Scene switch cancelled by user.");

            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static void ConfigureCanvasScaler(Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);

            Undo.RecordObject(scaler, "Configure responsive gameplay CanvasScaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            EditorUtility.SetDirty(scaler);
        }

        private static void NormalizeResourceBar(Canvas canvas, Transform hudRoot)
        {
            TMP_Text resourceText = canvas.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(text => text != null &&
                                        !IsUnder(text.transform, hudRoot) &&
                                        ContainsAny(text.text,
                                            "Materials", "Матеріали",
                                            "Food", "Їжа",
                                            "Money", "Гроші"));
            RectTransform panel = FindWideImageAncestor(resourceText != null ? resourceText.transform : null, canvas.transform);
            if (panel == null)
                return;

            Canvas.ForceUpdateCanvases();
            Vector2 currentSize = panel.rect.size;
            float width = Mathf.Clamp(currentSize.x > 1f ? currentSize.x : 440f, 340f, 620f);
            float height = Mathf.Clamp(currentSize.y > 1f ? currentSize.y : 42f, 34f, 86f);

            Undo.RecordObject(panel, "Anchor gameplay resource bar");
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0f, -14f);
            panel.sizeDelta = new Vector2(width, height);
            EditorUtility.SetDirty(panel);
        }

        private static void NormalizeBuildButton(Canvas canvas, Transform hudRoot)
        {
            TMP_Text buildText = canvas.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(text => text != null &&
                                        !IsUnder(text.transform, hudRoot) &&
                                        ContainsAny(text.text, "Будувати", "Build"));
            Button button = FindButtonAncestor(buildText != null ? buildText.transform : null);
            if (button == null || button.transform is not RectTransform rect)
                return;

            Canvas.ForceUpdateCanvases();
            Vector2 currentSize = rect.rect.size;
            float width = Mathf.Clamp(currentSize.x > 1f ? currentSize.x : 150f, 120f, 220f);
            float height = Mathf.Clamp(currentSize.y > 1f ? currentSize.y : 54f, 42f, 80f);

            Undo.RecordObject(rect, "Anchor gameplay build button");
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-18f, 18f);
            rect.sizeDelta = new Vector2(width, height);
            EditorUtility.SetDirty(rect);
        }

        private static Canvas FindPrimaryCanvas(Scene scene)
        {
            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in sceneRoot.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas != null && canvas.name == "Canvas")
                        return canvas;
                }
            }

            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            {
                Canvas canvas = sceneRoot.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                    return canvas;
            }
            return null;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == childName)
                    return child;
            }
            return null;
        }

        private static RectTransform FindWideImageAncestor(Transform transform, Transform canvas)
        {
            RectTransform fallback = null;
            Transform current = transform;
            while (current != null && current != canvas)
            {
                if (current is RectTransform rect && current.GetComponent<Image>() != null)
                {
                    fallback = rect;
                    if (rect.rect.width >= 240f)
                        return rect;
                }
                current = current.parent;
            }
            return fallback;
        }

        private static Button FindButtonAncestor(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                Button button = current.GetComponent<Button>();
                if (button != null)
                    return button;
                current = current.parent;
            }
            return null;
        }

        private static bool IsUnder(Transform target, Transform possibleAncestor)
        {
            Transform current = target;
            while (current != null)
            {
                if (current == possibleAncestor)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            foreach (string needle in needles)
            {
                if (value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static void Stretch(RectTransform rect)
        {
            Undo.RecordObject(rect, "Stretch gameplay HUD root");
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            EditorUtility.SetDirty(rect);
        }
    }
}
