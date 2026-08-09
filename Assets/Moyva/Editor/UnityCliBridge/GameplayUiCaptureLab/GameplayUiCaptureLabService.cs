using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiCaptureLab
{
    internal static class GameplayUiCaptureLabService
    {
        private const string TargetScene = "Assets/Moyva/Scenes/Gamplay_Scene.unity";

        private sealed class PendingCapture
        {
            public EditorWindow window;
            public string outputPath;
            public string view;
            public string preset;
            public int frames;
        }

        private static PendingCapture _pending;
        private static bool _sceneViewSaved;
        private static Vector3 _savedScenePivot;
        private static Quaternion _savedSceneRotation;
        private static float _savedSceneSize;
        private static bool _savedSceneOrtho;

        internal static string CaptureWindow(string path, string view, string preset, int delayFrames)
        {
            var result = new CaptureLabResult
            {
                operation = "capture-window",
                path = path,
                view = view,
                preset = preset,
                frameDelay = Mathf.Clamp(delayFrames, 2, 30),
            };

            if (_pending != null)
                return Fail(result, "Another actual-window capture is already pending.");
            if (!TryNormalizeOutput(path, out string output, out string error))
                return Fail(result, error);
            if (!TryTargetScene(out error))
                return Fail(result, error);

            EditorWindow window;
            if (string.Equals(view, "game", StringComparison.OrdinalIgnoreCase))
            {
                window = FindGameView();
                if (window == null)
                    return Fail(result, "GameView window is not currently available.");
            }
            else if (string.Equals(view, "scene", StringComparison.OrdinalIgnoreCase))
            {
                SceneView sv = SceneView.lastActiveSceneView ?? Resources.FindObjectsOfTypeAll<SceneView>().FirstOrDefault();
                if (sv == null)
                    return Fail(result, "SceneView window is not currently available.");
                window = sv;
                if (!ApplyScenePreset(sv, preset, out error))
                    return Fail(result, error);
            }
            else
            {
                return Fail(result, "view must be game or scene.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (File.Exists(output))
                File.Delete(output);

            window.Show();
            window.Focus();
            window.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();

            _pending = new PendingCapture
            {
                window = window,
                outputPath = output,
                view = view,
                preset = preset,
                frames = result.frameDelay,
            };
            EditorApplication.update += TickCapture;

            Rect r = window.position;
            result.width = Mathf.Max(1, Mathf.RoundToInt(r.width * EditorGUIUtility.pixelsPerPoint));
            result.height = Mathf.Max(1, Mathf.RoundToInt(r.height * EditorGUIUtility.pixelsPerPoint));
            result.pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
            result.path = output;
            result.message =
                "Scheduled an actual Editor-window pixel capture. This captures what is visibly drawn in GameView/SceneView, including Screen Space Overlay UI when the Unity window is visible.";
            return JsonUtility.ToJson(result, true);
        }

        internal static string CaptureRuntimeScreenshot(string path, int superSize)
        {
            var result = new CaptureLabResult
            {
                operation = "runtime-screen-capture",
                path = path,
                view = "game",
            };

            if (!Application.isPlaying)
                return Fail(result, "ScreenCapture capture requires Play Mode.");
            if (!TryNormalizeOutput(path, out string output, out string error))
                return Fail(result, error);

            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (File.Exists(output))
                File.Delete(output);

            int ss = Mathf.Clamp(superSize, 1, 4);
            ScreenCapture.CaptureScreenshot(output, ss);
            result.path = output;
            result.width = Screen.width * ss;
            result.height = Screen.height * ss;
            result.message =
                "ScreenCapture scheduled at end of the rendered game frame. This path is intentionally independent from Unity Pipeline's camera-only screenshot command.";
            return JsonUtility.ToJson(result, true);
        }

        internal static string RestoreSceneView()
        {
            var result = new CaptureLabResult
            {
                operation = "sceneview-restore",
                view = "scene",
                preset = "restore",
            };

            SceneView sv = SceneView.lastActiveSceneView ?? Resources.FindObjectsOfTypeAll<SceneView>().FirstOrDefault();
            if (sv == null)
                return Fail(result, "SceneView not found.");
            if (!_sceneViewSaved)
            {
                result.message = "No SceneView state was stored; nothing to restore.";
                return JsonUtility.ToJson(result, true);
            }

            sv.orthographic = _savedSceneOrtho;
            sv.LookAtDirect(_savedScenePivot, _savedSceneRotation, _savedSceneSize);
            sv.Repaint();
            result.message = "SceneView camera restored.";
            return JsonUtility.ToJson(result, true);
        }

        internal static string VisualState()
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = new VisualStateResult
            {
                ok = scene.IsValid() && string.Equals(scene.path, TargetScene, StringComparison.OrdinalIgnoreCase),
                scene = scene.IsValid() ? scene.path : string.Empty,
                isPlaying = Application.isPlaying,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                pixelsPerPoint = EditorGUIUtility.pixelsPerPoint,
                gameView = WindowSnapshot(FindGameView()),
                sceneView = WindowSnapshot(SceneView.lastActiveSceneView ?? Resources.FindObjectsOfTypeAll<SceneView>().FirstOrDefault()),
            };

            if (!scene.IsValid())
                return JsonUtility.ToJson(result, true);

            foreach (Canvas canvas in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (canvas == null || canvas.gameObject.scene != scene)
                    continue;
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                result.canvases.Add(new CanvasState
                {
                    path = PathOf(canvas.transform),
                    active = canvas.gameObject.activeInHierarchy,
                    renderMode = canvas.renderMode.ToString(),
                    sortingOrder = canvas.sortingOrder,
                    scaleFactor = canvas.scaleFactor,
                    worldCamera = canvas.worldCamera != null ? canvas.worldCamera.name : string.Empty,
                    referenceResolution = scaler != null ? scaler.referenceResolution : Vector2.zero,
                    matchWidthOrHeight = scaler != null ? scaler.matchWidthOrHeight : 0f,
                });
            }

            foreach (RectTransform rt in Resources.FindObjectsOfTypeAll<RectTransform>())
            {
                if (rt == null || rt.gameObject.scene != scene)
                    continue;
                if (rt.GetComponentInParent<Canvas>(true) == null)
                    continue;

                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                Canvas canvas = rt.GetComponentInParent<Canvas>(true);
                UnityEngine.Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

                Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                foreach (Vector3 corner in corners)
                {
                    Vector2 p = RectTransformUtility.WorldToScreenPoint(camera, corner);
                    min = Vector2.Min(min, p);
                    max = Vector2.Max(max, p);
                }

                Graphic graphic = rt.GetComponent<Graphic>();
                TMP_Text tmp = rt.GetComponent<TMP_Text>();
                Selectable selectable = rt.GetComponent<Selectable>();
                result.ui.Add(new UiState
                {
                    path = PathOf(rt),
                    activeSelf = rt.gameObject.activeSelf,
                    activeInHierarchy = rt.gameObject.activeInHierarchy,
                    anchorMin = rt.anchorMin,
                    anchorMax = rt.anchorMax,
                    pivot = rt.pivot,
                    anchoredPosition = rt.anchoredPosition,
                    sizeDelta = rt.sizeDelta,
                    screenMin = min,
                    screenMax = max,
                    hasGraphic = graphic != null,
                    graphicColor = graphic != null ? graphic.color : Color.clear,
                    raycastTarget = graphic != null && graphic.raycastTarget,
                    text = tmp != null ? tmp.text : string.Empty,
                    fontSize = tmp != null ? tmp.fontSize : 0f,
                    interactable = selectable != null && selectable.interactable,
                });
            }

            result.ui = result.ui.OrderBy(x => x.path, StringComparer.Ordinal).ToList();
            result.canvases = result.canvases.OrderBy(x => x.path, StringComparer.Ordinal).ToList();
            return JsonUtility.ToJson(result, true);
        }

        internal static string RuntimeSelectBuilding(string buildingId)
        {
            var result = new CaptureLabResult
            {
                operation = "runtime-select-building",
                view = "game",
                preset = buildingId,
            };

            if (!Application.isPlaying)
                return Fail(result, "Building selection capture helper requires Play Mode.");
            Type type = FindType("Kruty1918.Moyva.Construction.UI.ConstructionUIController");
            if (type == null)
                return Fail(result, "ConstructionUIController type not found.");
            MonoBehaviour controller = Resources.FindObjectsOfTypeAll(type)
                .OfType<MonoBehaviour>()
                .FirstOrDefault(x => x != null && x.gameObject.scene == SceneManager.GetActiveScene());
            if (controller == null)
                return Fail(result, "ConstructionUIController instance not found.");

            MethodInfo method = type.GetMethod("OnBuildingSelected", BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
                return Fail(result, "OnBuildingSelected(string) method not found.");
            method.Invoke(controller, new object[] { buildingId });
            result.message = "Requested runtime building selection: " + buildingId;
            return JsonUtility.ToJson(result, true);
        }

        private static void TickCapture()
        {
            PendingCapture request = _pending;
            if (request == null)
            {
                EditorApplication.update -= TickCapture;
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            request.window?.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
            request.frames--;
            if (request.frames > 0)
                return;

            try
            {
                CaptureActualWindowPixels(request.window, request.outputPath, request.view, request.preset);
            }
            catch (Exception e)
            {
                try
                {
                    File.WriteAllText(request.outputPath + ".error.txt", e.ToString());
                }
                catch { }
                Debug.LogError("[MoyvaGameplayUICapture74] Window capture failed: " + e);
            }
            finally
            {
                _pending = null;
                EditorApplication.update -= TickCapture;
            }
        }

        private static void CaptureActualWindowPixels(EditorWindow window, string output, string view, string preset)
        {
            if (window == null)
                throw new InvalidOperationException("Capture window disappeared.");

            Rect logical = window.position;
            float ppp = Mathf.Max(1f, EditorGUIUtility.pixelsPerPoint);
            int width = Mathf.Clamp(Mathf.RoundToInt(logical.width * ppp), 64, 4096);
            int height = Mathf.Clamp(Mathf.RoundToInt(logical.height * ppp), 64, 2160);
            Vector2 origin = new Vector2(logical.x * ppp, logical.y * ppp);

            MethodInfo method = FindReadScreenPixel();
            if (method == null)
                throw new MissingMethodException("UnityEditorInternal.InternalEditorUtility.ReadScreenPixel was not found.");

            ParameterInfo[] parameters = method.GetParameters();
            object first;
            if (parameters[0].ParameterType == typeof(Vector2))
                first = origin;
            else if (parameters[0].ParameterType == typeof(Vector2Int))
                first = new Vector2Int(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y));
            else
                throw new InvalidOperationException("Unsupported ReadScreenPixel first parameter: " + parameters[0].ParameterType);

            object raw = method.Invoke(null, new object[] { first, width, height });
            if (raw is not Color[] pixels || pixels.Length != width * height)
                throw new InvalidOperationException("ReadScreenPixel returned unexpected pixel data.");

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(output, texture.EncodeToPNG());

                var metadata = new CaptureLabResult
                {
                    ok = true,
                    operation = "actual-window-pixels",
                    message = "Captured actual Editor window pixels via ReadScreenPixel reflection.",
                    path = output,
                    view = view,
                    preset = preset,
                    width = width,
                    height = height,
                    pixelsPerPoint = ppp,
                };
                File.WriteAllText(output + ".meta.json", JsonUtility.ToJson(metadata, true));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static MethodInfo FindReadScreenPixel()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("UnityEditorInternal.InternalEditorUtility", false);
                if (type == null)
                    continue;
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!string.Equals(method.Name, "ReadScreenPixel", StringComparison.Ordinal))
                        continue;
                    ParameterInfo[] p = method.GetParameters();
                    if (p.Length != 3 || p[1].ParameterType != typeof(int) || p[2].ParameterType != typeof(int))
                        continue;
                    if (p[0].ParameterType == typeof(Vector2) || p[0].ParameterType == typeof(Vector2Int))
                        return method;
                }
            }
            return null;
        }

        private static EditorWindow FindGameView()
        {
            Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView", false);
            if (type == null)
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("UnityEditor.GameView", false))
                    .FirstOrDefault(t => t != null);
            }
            if (type == null)
                return null;
            return Resources.FindObjectsOfTypeAll(type).OfType<EditorWindow>().FirstOrDefault();
        }

        private static bool ApplyScenePreset(SceneView sv, string preset, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(preset) || string.Equals(preset, "current", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(preset, "restore", StringComparison.OrdinalIgnoreCase))
            {
                if (_sceneViewSaved)
                {
                    sv.orthographic = _savedSceneOrtho;
                    sv.LookAtDirect(_savedScenePivot, _savedSceneRotation, _savedSceneSize);
                }
                return true;
            }

            if (!_sceneViewSaved)
            {
                _sceneViewSaved = true;
                _savedScenePivot = sv.pivot;
                _savedSceneRotation = sv.rotation;
                _savedSceneSize = sv.size;
                _savedSceneOrtho = sv.orthographic;
            }

            if (!TryWorldBounds(out Bounds bounds))
            {
                error = "No non-UI Renderer bounds found for SceneView preset.";
                return false;
            }

            float horizontal = Mathf.Max(bounds.extents.x, bounds.extents.z);
            float vertical = Mathf.Max(bounds.extents.y, 1f);
            float full = Mathf.Max(horizontal * 1.28f, vertical * 2f);

            if (string.Equals(preset, "top", StringComparison.OrdinalIgnoreCase))
            {
                sv.orthographic = true;
                sv.LookAtDirect(bounds.center, Quaternion.Euler(90f, 0f, 0f), full);
            }
            else if (string.Equals(preset, "iso", StringComparison.OrdinalIgnoreCase))
            {
                sv.orthographic = false;
                sv.LookAtDirect(bounds.center, Quaternion.Euler(32f, 45f, 0f), full * 0.92f);
            }
            else if (string.Equals(preset, "low", StringComparison.OrdinalIgnoreCase))
            {
                sv.orthographic = false;
                sv.LookAtDirect(bounds.center, Quaternion.Euler(18f, 45f, 0f), full * 0.72f);
            }
            else if (string.Equals(preset, "close", StringComparison.OrdinalIgnoreCase))
            {
                sv.orthographic = false;
                sv.LookAtDirect(bounds.center, Quaternion.Euler(36f, 35f, 0f), full * 0.42f);
            }
            else
            {
                error = "Unknown SceneView preset. Use current, top, iso, low, close, or restore.";
                return false;
            }

            sv.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
            return true;
        }

        private static bool TryWorldBounds(out Bounds bounds)
        {
            Scene scene = SceneManager.GetActiveScene();
            bool initialized = false;
            bounds = default;
            foreach (Renderer renderer in Resources.FindObjectsOfTypeAll<Renderer>())
            {
                if (renderer == null || renderer.gameObject.scene != scene)
                    continue;
                if (renderer.GetComponentInParent<Canvas>(true) != null)
                    continue;
                if (!renderer.enabled)
                    continue;
                if (!initialized)
                {
                    bounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            return initialized;
        }

        private static WindowState WindowSnapshot(EditorWindow window)
        {
            if (window == null)
                return new WindowState { exists = false };
            Rect r = window.position;
            return new WindowState
            {
                exists = true,
                focused = EditorWindow.focusedWindow == window,
                x = r.x,
                y = r.y,
                width = r.width,
                height = r.height,
                title = window.titleContent != null ? window.titleContent.text : string.Empty,
            };
        }

        private static bool TryTargetScene(out string error)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                error = "No active loaded scene.";
                return false;
            }
            if (!string.Equals(scene.path, TargetScene, StringComparison.OrdinalIgnoreCase))
            {
                error = "Active scene is not " + TargetScene + ": " + scene.path;
                return false;
            }
            error = string.Empty;
            return true;
        }

        private static bool TryNormalizeOutput(string requested, out string output, out string error)
        {
            output = string.Empty;
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(requested))
            {
                error = "Output path is empty.";
                return false;
            }

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string root = Path.GetFullPath(Path.Combine(home, ".local", "share", "moyva-cli", "reports", "gameplay-ui-capture"));
            string full = Path.GetFullPath(requested);
            string rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(rootPrefix, StringComparison.Ordinal) && !string.Equals(full, root, StringComparison.Ordinal))
            {
                error = "Capture path is restricted to " + root;
                return false;
            }
            if (!string.Equals(Path.GetExtension(full), ".png", StringComparison.OrdinalIgnoreCase))
            {
                error = "Capture output must end in .png";
                return false;
            }

            output = full;
            return true;
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static string PathOf(Transform t)
        {
            if (t == null)
                return string.Empty;
            var names = new Stack<string>();
            while (t != null)
            {
                names.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", names);
        }

        private static string Fail(CaptureLabResult result, string message)
        {
            result.ok = false;
            result.message = message;
            return JsonUtility.ToJson(result, true);
        }
    }
}
