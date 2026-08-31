using System;
using Kruty1918.Moyva.Bootstrap.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    public static class GameplayAdaptiveCanvasPatch
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string LogTag = "[MOYVA_ADAPTIVE_UI]";
        private static readonly Vector2 ReferenceResolution = new(1280f, 720f);

        [MenuItem("Moyva/Gameplay/Apply Full Adaptive UI", false, 123)]
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

        public static string ApplyAndSave()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Exit Play Mode before applying the adaptive UI scene layout.");

            Scene scene = FindOrOpenGameplayScene();
            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException($"No gameplay Canvas found in '{ScenePath}'.");

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
                throw new InvalidOperationException("Gameplay Canvas has no RectTransform.");

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);

            Undo.RecordObject(scaler, "Configure adaptive gameplay CanvasScaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            canvas.pixelPerfect = true;
            EditorUtility.SetDirty(scaler);

            StretchIfFound(canvas.transform, "GameModeUI");
            StretchIfFound(canvas.transform, "EconomyPlayerSymmary");
            StretchIfFound(canvas.transform, "EconomyPlayerSymmary/Root");
            StretchIfFound(canvas.transform, "ConstructionUI");
            StretchIfFound(canvas.transform, "ConstructionUI/Root");
            StretchIfFound(canvas.transform, "GameplayTurnHud");

            GameplayAdaptiveCanvasLayout adaptive = canvas.GetComponent<GameplayAdaptiveCanvasLayout>();
            if (adaptive == null)
            {
                adaptive = Undo.AddComponent<GameplayAdaptiveCanvasLayout>(canvas.gameObject);
                EditorUtility.SetDirty(canvas.gameObject);
            }

            DisablePreviousHudLayout(canvas.transform);

            Canvas.ForceUpdateCanvases();
            if (!adaptive.ApplyLayoutNow())
                throw new InvalidOperationException("GameplayAdaptiveCanvasLayout could not bind the current gameplay UI hierarchy.");
            Canvas.ForceUpdateCanvases();

            EditorUtility.SetDirty(adaptive);
            EditorUtility.SetDirty(canvas);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Failed to save Gamplay_Scene after adaptive UI apply.");
            AssetDatabase.SaveAssets();

            return "P18_2_APPLY_OK " + adaptive.DescribeCurrentLayout();
        }

        public static string ValidateCurrent()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Gamplay_Scene is not loaded.");

            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("Gameplay Canvas missing.");

            GameplayAdaptiveCanvasLayout adaptive = canvas.GetComponent<GameplayAdaptiveCanvasLayout>();
            if (adaptive == null)
                throw new InvalidOperationException("GameplayAdaptiveCanvasLayout missing from Canvas.");

            if (!adaptive.ApplyLayoutNow())
                throw new InvalidOperationException("Adaptive layout validation failed.");

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                throw new InvalidOperationException("CanvasScaler is not ScaleWithScreenSize.");
            if (scaler.referenceResolution != ReferenceResolution ||
                scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ||
                !Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f) ||
                !Mathf.Approximately(scaler.referencePixelsPerUnit, 100f) ||
                !canvas.pixelPerfect)
            {
                throw new InvalidOperationException(
                    "Gameplay Canvas does not use the canonical 1280x720 UI scale policy.");
            }

            return "P18_2_VALIDATE_OK match=" + scaler.matchWidthOrHeight.ToString("0.00") + " " + adaptive.DescribeCurrentLayout();
        }

        private static void DisablePreviousHudLayout(Transform canvas)
        {
            Transform hud = canvas.Find("GameplayTurnHud");
            if (hud == null)
                return;

            foreach (MonoBehaviour behaviour in hud.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null ||
                    behaviour.GetType().FullName != "Kruty1918.Moyva.Bootstrap.Runtime.GameplayResponsiveHudLayout")
                {
                    continue;
                }

                Undo.RecordObject(behaviour, "Disable superseded HUD responsive layout");
                behaviour.enabled = false;
                EditorUtility.SetDirty(behaviour);
            }
        }

        private static void StretchIfFound(Transform canvas, string path)
        {
            if (canvas.Find(path) is not RectTransform rect)
                return;

            Undo.RecordObject(rect, "Stretch adaptive UI root");
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            EditorUtility.SetDirty(rect);
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

        private static Canvas FindPrimaryCanvas(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas != null && canvas.name == "Canvas")
                        return canvas;
                }
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas canvas = root.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                    return canvas;
            }

            return null;
        }
    }
}
