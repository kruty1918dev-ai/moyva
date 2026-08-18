#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiAudit
{
    internal static class GameplayUiAuditService
    {
        internal const string TargetScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string AllowedReportRelativeRoot = ".local/share/moyva-cli/reports/gameplay-ui";

        private static readonly string[] SourceAreaRoots =
        {
            "Assets/Moyva/Scripts/Features/Construction/UI",
            "Assets/Moyva/Scripts/Features/Economy/UI",
            "Assets/Moyva/Scripts/Features/InfoPanel/UI",
            "Assets/Moyva/Scripts/Features/GameMode",
            "Assets/Moyva/Scripts/Features/HomeMenu/UI",
            "Assets/Moyva/Scripts/Features/HomeMenu/Runtime",
            "Assets/Moyva/Scripts/Features/WorldCreation/UI",
            "Assets/Moyva/Scripts/Features/Interactions",
            "Assets/Moyva/Scripts/Features/Signals",
            "Assets/Moyva/Scripts/Shared",
        };

        private static readonly string[] ProjectNamespacePrefixes =
        {
            "Kruty1918.Moyva.",
            "Zenject.",
        };

        internal static string PrepareTargetScene()
        {
            var result = new AuditCommandResult
            {
                ok = false,
                targetScene = TargetScenePath,
            };

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                result.message = "Unity is compiling/updating; retry after it becomes idle.";
                return JsonUtility.ToJson(result, true);
            }

            Scene target = FindLoadedTargetScene();
            if (target.IsValid() && target.isLoaded)
            {
                if (SceneManager.GetActiveScene() != target)
                    SceneManager.SetActiveScene(target);

                result.ok = true;
                result.message = "Gameplay scene already loaded and is now active.";
                return JsonUtility.ToJson(result, true);
            }

            var dirtyScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isDirty)
                    dirtyScenes.Add(string.IsNullOrWhiteSpace(scene.path) ? scene.name : scene.path);
            }

            if (dirtyScenes.Count > 0)
            {
                result.message =
                    "Gameplay scene is not loaded. Refusing to replace dirty scene(s): " +
                    string.Join(", ", dirtyScenes);
                return JsonUtility.ToJson(result, true);
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath) == null)
            {
                result.message = "Gameplay scene asset not found: " + TargetScenePath;
                return JsonUtility.ToJson(result, true);
            }

            Scene opened = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            if (!opened.IsValid() || !opened.isLoaded)
            {
                result.message = "Unity failed to open gameplay scene.";
                return JsonUtility.ToJson(result, true);
            }

            result.ok = true;
            result.message = "Gameplay scene opened safely because no loaded scene had unsaved changes.";
            return JsonUtility.ToJson(result, true);
        }

        internal static string Status()
        {
            Scene target = FindLoadedTargetScene();
            var result = new AuditCommandResult
            {
                ok = target.IsValid() && target.isLoaded,
                targetScene = TargetScenePath,
                message = target.IsValid() && target.isLoaded
                    ? $"Loaded. active={SceneManager.GetActiveScene() == target}; dirty={target.isDirty}"
                    : "Target gameplay scene is not loaded.",
            };
            return JsonUtility.ToJson(result, true);
        }

        internal static string RunAudit(string outputDir)
        {
            var commandResult = new AuditCommandResult
            {
                ok = false,
                targetScene = TargetScenePath,
            };

            if (!ValidateOutputDirectory(outputDir, out string normalizedOutput, out string validationError))
            {
                commandResult.message = validationError;
                return JsonUtility.ToJson(commandResult, true);
            }

            Scene target = FindLoadedTargetScene();
            if (!target.IsValid() || !target.isLoaded)
            {
                commandResult.message =
                    "Gameplay scene is not loaded. Run moyva-gameplay-ui-prepare first.";
                return JsonUtility.ToJson(commandResult, true);
            }

            Directory.CreateDirectory(normalizedOutput);

            var report = new GameplayUiAuditReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            };

            FillProject(report, target);
            FillScene(report, target);

            List<GameObject> allObjects = GetAllObjects(target);
            List<GameObject> uiObjects = allObjects
                .Where(IsUiObject)
                .OrderBy(go => HierarchyPath(go.transform), StringComparer.Ordinal)
                .ToList();

            FillCanvases(report, target);
            FillUiNodes(report, uiObjects);
            FillSerializedProjectComponents(report, uiObjects);
            FillEventBindings(report, uiObjects);
            FillReusableUiPrefabs(report);
            FillSpriteAssets(report);
            FillFontAssets(report);
            FillUsedUiMaterials(report, uiObjects);
            FillSourceAreas(report);
            FillIssues(report, uiObjects);
            FillSummary(report);

            string gameViewPath = Path.Combine(normalizedOutput, "gameview.png");
            string sceneViewPath = Path.Combine(normalizedOutput, "sceneview.png");

            report.screenshots.gameViewPath = gameViewPath;
            report.screenshots.sceneViewPath = sceneViewPath;
            report.screenshots.sceneViewCaptured = TryCaptureSceneView(sceneViewPath, out string sceneCaptureNote);
            report.screenshots.gameViewRequested = ScheduleGameViewCapture(gameViewPath, out string gameCaptureNote);
            report.screenshots.note =
                $"GameView: {gameCaptureNote} SceneView: {sceneCaptureNote}";

            string reportPath = Path.Combine(normalizedOutput, "gameplay_ui_audit.json");
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));

            string summaryPath = Path.Combine(normalizedOutput, "audit_summary.txt");
            File.WriteAllText(summaryPath, BuildSummaryText(report));

            commandResult.ok = true;
            commandResult.message =
                "Audit written. GameView screenshot may arrive asynchronously after the CLI command returns.";
            commandResult.reportJson = reportPath;
            commandResult.gameViewScreenshot = gameViewPath;
            commandResult.sceneViewScreenshot = sceneViewPath;
            commandResult.uiNodes = report.summary.uiNodes;
            commandResult.issues = report.issues.Count;

            Debug.Log(
                $"[MoyvaGameplayUiAudit] AUDIT_OK report={reportPath} " +
                $"uiNodes={report.summary.uiNodes} issues={report.issues.Count}");

            return JsonUtility.ToJson(commandResult, true);
        }

        internal static string CaptureOnly(string outputDir)
        {
            var result = new AuditCommandResult
            {
                ok = false,
                targetScene = TargetScenePath,
            };

            if (!ValidateOutputDirectory(outputDir, out string normalizedOutput, out string validationError))
            {
                result.message = validationError;
                return JsonUtility.ToJson(result, true);
            }

            Scene target = FindLoadedTargetScene();
            if (!target.IsValid() || !target.isLoaded)
            {
                result.message = "Gameplay scene is not loaded.";
                return JsonUtility.ToJson(result, true);
            }

            Directory.CreateDirectory(normalizedOutput);
            string gamePath = Path.Combine(normalizedOutput, "gameview.png");
            string scenePath = Path.Combine(normalizedOutput, "sceneview.png");

            bool sceneOk = TryCaptureSceneView(scenePath, out string sceneNote);
            bool gameRequested = ScheduleGameViewCapture(gamePath, out string gameNote);

            result.ok = sceneOk || gameRequested;
            result.message = $"GameView={gameNote}; SceneView={sceneNote}";
            result.gameViewScreenshot = gamePath;
            result.sceneViewScreenshot = scenePath;
            return JsonUtility.ToJson(result, true);
        }

        private static void FillProject(GameplayUiAuditReport report, Scene target)
        {
            RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
            Vector2 gameSize = Vector2.zero;
            try
            {
                gameSize = Handles.GetMainGameViewSize();
            }
            catch
            {
                // Keep zero and report it.
            }

            report.project.projectPath =
                Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            report.project.unityVersion = Application.unityVersion;
            report.project.activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            report.project.colorSpace = QualitySettings.activeColorSpace.ToString();
            report.project.renderPipelineAsset = rp != null ? rp.name : "<Built-in>";
            report.project.renderPipelineAssetPath = rp != null ? AssetDatabase.GetAssetPath(rp) : string.Empty;
            report.project.serializationMode = EditorSettings.serializationMode.ToString();
            report.project.activeScenePath = SceneManager.GetActiveScene().path;
            report.project.isPlaying = EditorApplication.isPlaying;
            report.project.isCompiling = EditorApplication.isCompiling;
            report.project.isUpdating = EditorApplication.isUpdating;
            report.project.gameViewSize = gameSize;
            report.project.assetPathCount = AssetDatabase.GetAllAssetPaths().Length;
        }

        private static void FillScene(GameplayUiAuditReport report, Scene target)
        {
            List<GameObject> objects = GetAllObjects(target);
            report.scene.targetScenePath = TargetScenePath;
            report.scene.name = target.name;
            report.scene.path = target.path;
            report.scene.loaded = target.isLoaded;
            report.scene.active = SceneManager.GetActiveScene() == target;
            report.scene.dirty = target.isDirty;
            report.scene.rootCount = target.rootCount;
            report.scene.totalObjectCount = objects.Count;
            report.scene.totalComponentCount = objects.Sum(go => go.GetComponents<Component>().Length);
            report.scene.rootNames = target.GetRootGameObjects().Select(go => go.name).ToArray();
        }

        private static void FillCanvases(GameplayUiAuditReport report, Scene target)
        {
            foreach (GameObject go in GetAllObjects(target))
            {
                Canvas canvas = go.GetComponent<Canvas>();
                if (canvas == null)
                    continue;

                CanvasScaler scaler = go.GetComponent<CanvasScaler>();
                var entry = new AuditCanvas
                {
                    path = HierarchyPath(go.transform),
                    globalObjectId = SafeGlobalObjectId(go),
                    active = go.activeInHierarchy,
                    renderMode = canvas.renderMode.ToString(),
                    sortingOrder = canvas.sortingOrder,
                    overrideSorting = canvas.overrideSorting,
                    targetDisplay = canvas.targetDisplay,
                    pixelPerfect = canvas.pixelPerfect,
                    worldCamera = canvas.worldCamera != null
                        ? HierarchyPath(canvas.worldCamera.transform)
                        : string.Empty,
                    scaleFactor = canvas.scaleFactor,
                    hasScaler = scaler != null,
                };

                if (scaler != null)
                {
                    entry.scalerMode = scaler.uiScaleMode.ToString();
                    entry.referenceResolution = scaler.referenceResolution;
                    entry.matchWidthOrHeight = scaler.matchWidthOrHeight;
                    entry.referencePixelsPerUnit = scaler.referencePixelsPerUnit;
                }

                report.canvases.Add(entry);
            }
        }

        private static void FillUiNodes(GameplayUiAuditReport report, List<GameObject> uiObjects)
        {
            foreach (GameObject go in uiObjects)
                report.uiNodes.Add(CreateNode(go));
        }

        private static AuditUiNode CreateNode(GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
            Graphic graphic = go.GetComponent<Graphic>();
            Image image = go.GetComponent<Image>();
            RawImage rawImage = go.GetComponent<RawImage>();
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            Selectable selectable = go.GetComponent<Selectable>();
            ScrollRect scroll = go.GetComponent<ScrollRect>();
            LayoutGroup layout = go.GetComponent<LayoutGroup>();
            LayoutElement layoutElement = go.GetComponent<LayoutElement>();
            ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
            AspectRatioFitter aspect = go.GetComponent<AspectRatioFitter>();

            var node = new AuditUiNode
            {
                name = go.name,
                path = HierarchyPath(go.transform),
                parentPath = go.transform.parent != null ? HierarchyPath(go.transform.parent) : string.Empty,
                globalObjectId = SafeGlobalObjectId(go),
                prefabSourcePath = GetPrefabSourcePath(go),
                activeSelf = go.activeSelf,
                activeInHierarchy = go.activeInHierarchy,
                siblingIndex = go.transform.GetSiblingIndex(),
                layer = go.layer,
                tag = go.tag,
                components = go.GetComponents<Component>()
                    .Where(c => c != null)
                    .Select(c => c.GetType().FullName)
                    .ToArray(),
            };

            if (rect != null)
            {
                node.anchorMin = rect.anchorMin;
                node.anchorMax = rect.anchorMax;
                node.pivot = rect.pivot;
                node.anchoredPosition = rect.anchoredPosition;
                node.sizeDelta = rect.sizeDelta;
                node.localScale = rect.localScale;
                node.rectSize = rect.rect.size;

                if (TryGetScreenRect(rect, out Rect screenRect))
                {
                    node.screenRectValid = true;
                    node.screenRect = screenRect;
                }
            }

            if (canvasGroup != null)
            {
                node.hasCanvasGroup = true;
                node.canvasGroupAlpha = canvasGroup.alpha;
                node.canvasGroupInteractable = canvasGroup.interactable;
                node.canvasGroupBlocksRaycasts = canvasGroup.blocksRaycasts;
            }

            if (graphic != null)
            {
                node.hasGraphic = true;
                node.graphicType = graphic.GetType().FullName;
                node.graphicColor = graphic.color;
                node.graphicRaycastTarget = graphic.raycastTarget;
                node.graphicMaterialPath =
                    graphic.material != null ? AssetDatabase.GetAssetPath(graphic.material) : string.Empty;
            }

            if (image != null)
            {
                node.hasImage = true;
                node.imageSpriteName = image.sprite != null ? image.sprite.name : string.Empty;
                node.imageSpritePath = image.sprite != null ? AssetDatabase.GetAssetPath(image.sprite) : string.Empty;
                node.imageType = image.type.ToString();
                node.imagePreserveAspect = image.preserveAspect;
                node.imageFillAmount = image.fillAmount;
            }

            if (rawImage != null)
            {
                node.hasRawImage = true;
                node.rawTextureName = rawImage.texture != null ? rawImage.texture.name : string.Empty;
                node.rawTexturePath = rawImage.texture != null ? AssetDatabase.GetAssetPath(rawImage.texture) : string.Empty;
            }

            if (tmp != null)
            {
                node.hasText = true;
                node.text = tmp.text;
                node.fontSize = tmp.fontSize;
                node.fontAssetName = tmp.font != null ? tmp.font.name : string.Empty;
                node.fontAssetPath = tmp.font != null ? AssetDatabase.GetAssetPath(tmp.font) : string.Empty;
                node.textAlignment = tmp.alignment.ToString();
                node.overflowMode = tmp.overflowMode.ToString();
                node.textColor = tmp.color;
            }

            if (selectable != null)
            {
                node.hasSelectable = true;
                node.selectableType = selectable.GetType().FullName;
                node.interactable = selectable.interactable;
                node.transition = selectable.transition.ToString();
                node.navigationMode = selectable.navigation.mode.ToString();
                node.targetGraphicPath = selectable.targetGraphic != null
                    ? HierarchyPath(selectable.targetGraphic.transform)
                    : string.Empty;
            }

            if (scroll != null)
            {
                node.hasScrollRect = true;
                node.scrollHorizontal = scroll.horizontal;
                node.scrollVertical = scroll.vertical;
                node.scrollViewportPath = scroll.viewport != null ? HierarchyPath(scroll.viewport) : string.Empty;
                node.scrollContentPath = scroll.content != null ? HierarchyPath(scroll.content) : string.Empty;
            }

            if (layout != null)
            {
                node.hasLayoutGroup = true;
                node.layoutGroupType = layout.GetType().FullName;
                node.layoutDetails = BuildLayoutDetails(layout);
            }

            if (layoutElement != null)
            {
                node.hasLayoutElement = true;
                node.layoutElementDetails =
                    $"ignore={layoutElement.ignoreLayout}; min=({layoutElement.minWidth:0.##},{layoutElement.minHeight:0.##}); " +
                    $"preferred=({layoutElement.preferredWidth:0.##},{layoutElement.preferredHeight:0.##}); " +
                    $"flexible=({layoutElement.flexibleWidth:0.##},{layoutElement.flexibleHeight:0.##})";
            }

            if (fitter != null)
            {
                node.hasContentSizeFitter = true;
                node.contentSizeFitterDetails = $"{fitter.horizontalFit}/{fitter.verticalFit}";
            }

            if (aspect != null)
            {
                node.hasAspectRatioFitter = true;
                node.aspectRatioFitterDetails = $"{aspect.aspectMode}; ratio={aspect.aspectRatio:0.###}";
            }

            return node;
        }

        private static void FillSerializedProjectComponents(
            GameplayUiAuditReport report,
            List<GameObject> uiObjects)
        {
            foreach (GameObject go in uiObjects)
            {
                foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
                {
                    if (behaviour == null)
                        continue;

                    Type type = behaviour.GetType();
                    string fullName = type.FullName ?? type.Name;

                    if (!IsProjectRelevantType(fullName))
                        continue;

                    var info = new AuditSerializedComponent
                    {
                        objectPath = HierarchyPath(go.transform),
                        componentType = fullName,
                        scriptPath = GetScriptPath(behaviour),
                    };

                    try
                    {
                        var serialized = new SerializedObject(behaviour);
                        SerializedProperty property = serialized.GetIterator();
                        bool enterChildren = true;
                        int guard = 0;

                        while (property.NextVisible(enterChildren) && guard++ < 1000)
                        {
                            enterChildren = false;
                            if (property.propertyPath == "m_Script")
                                continue;

                            AuditSerializedProperty item = SerializeProperty(property);
                            if (item != null)
                                info.properties.Add(item);
                        }
                    }
                    catch (Exception exception)
                    {
                        info.properties.Add(new AuditSerializedProperty
                        {
                            propertyPath = "<serialization-error>",
                            displayName = "serialization error",
                            propertyType = "Exception",
                            value = exception.Message,
                        });
                    }

                    report.serializedComponents.Add(info);
                }
            }
        }

        private static AuditSerializedProperty SerializeProperty(SerializedProperty property)
        {
            var item = new AuditSerializedProperty
            {
                propertyPath = property.propertyPath,
                displayName = property.displayName,
                propertyType = property.propertyType.ToString(),
            };

            try
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                    {
                        UnityEngine.Object value = property.objectReferenceValue;
                        item.value = value != null ? value.name : "<null>";
                        if (value != null)
                        {
                            item.referenceType = value.GetType().FullName;
                            item.referencePath = AssetDatabase.GetAssetPath(value);

                            if (value is GameObject go)
                                item.referenceHierarchyPath = go.scene.IsValid() ? HierarchyPath(go.transform) : string.Empty;
                            else if (value is Component component)
                                item.referenceHierarchyPath =
                                    component.gameObject.scene.IsValid() ? HierarchyPath(component.transform) : string.Empty;
                        }
                        break;
                    }
                    case SerializedPropertyType.String:
                        item.value = property.stringValue;
                        break;
                    case SerializedPropertyType.Boolean:
                        item.value = property.boolValue.ToString();
                        break;
                    case SerializedPropertyType.Integer:
                        item.value = property.longValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    case SerializedPropertyType.Float:
                        item.value = property.doubleValue.ToString("0.#####", CultureInfo.InvariantCulture);
                        break;
                    case SerializedPropertyType.Enum:
                        item.value =
                            property.enumValueIndex >= 0 &&
                            property.enumValueIndex < property.enumDisplayNames.Length
                                ? property.enumDisplayNames[property.enumValueIndex]
                                : property.enumValueIndex.ToString();
                        break;
                    case SerializedPropertyType.Vector2:
                        item.value = property.vector2Value.ToString();
                        break;
                    case SerializedPropertyType.Vector3:
                        item.value = property.vector3Value.ToString();
                        break;
                    case SerializedPropertyType.Color:
                        item.value = property.colorValue.ToString();
                        break;
                    default:
                        if (property.isArray && property.propertyType != SerializedPropertyType.String)
                            item.value = $"arraySize={property.arraySize}";
                        else
                            item.value = string.Empty;
                        break;
                }
            }
            catch (Exception exception)
            {
                item.value = "<read-error: " + exception.Message + ">";
            }

            return item;
        }

        private static void FillEventBindings(GameplayUiAuditReport report, List<GameObject> uiObjects)
        {
            foreach (GameObject go in uiObjects)
            {
                string objectPath = HierarchyPath(go.transform);

                Button button = go.GetComponent<Button>();
                if (button != null)
                    AddEvent(report, objectPath, button, "onClick", button.onClick);

                Toggle toggle = go.GetComponent<Toggle>();
                if (toggle != null)
                    AddEvent(report, objectPath, toggle, "onValueChanged", toggle.onValueChanged);

                Slider slider = go.GetComponent<Slider>();
                if (slider != null)
                    AddEvent(report, objectPath, slider, "onValueChanged", slider.onValueChanged);

                Scrollbar scrollbar = go.GetComponent<Scrollbar>();
                if (scrollbar != null)
                    AddEvent(report, objectPath, scrollbar, "onValueChanged", scrollbar.onValueChanged);

                Dropdown dropdown = go.GetComponent<Dropdown>();
                if (dropdown != null)
                    AddEvent(report, objectPath, dropdown, "onValueChanged", dropdown.onValueChanged);

                TMP_Dropdown tmpDropdown = go.GetComponent<TMP_Dropdown>();
                if (tmpDropdown != null)
                    AddEvent(report, objectPath, tmpDropdown, "onValueChanged", tmpDropdown.onValueChanged);

                InputField input = go.GetComponent<InputField>();
                if (input != null)
                {
                    AddEvent(report, objectPath, input, "onValueChanged", input.onValueChanged);
                    AddEvent(report, objectPath, input, "onEndEdit", input.onEndEdit);
                }

                TMP_InputField tmpInput = go.GetComponent<TMP_InputField>();
                if (tmpInput != null)
                {
                    AddEvent(report, objectPath, tmpInput, "onValueChanged", tmpInput.onValueChanged);
                    AddEvent(report, objectPath, tmpInput, "onEndEdit", tmpInput.onEndEdit);
                }
            }
        }

        private static void AddEvent(
            GameplayUiAuditReport report,
            string objectPath,
            Component owner,
            string eventName,
            UnityEngine.Events.UnityEventBase evt)
        {
            if (evt == null)
                return;

            for (int i = 0; i < evt.GetPersistentEventCount(); i++)
            {
                UnityEngine.Object target = evt.GetPersistentTarget(i);
                var binding = new AuditEventBinding
                {
                    objectPath = objectPath,
                    componentType = owner.GetType().FullName,
                    eventName = eventName,
                    listenerIndex = i,
                    targetName = target != null ? target.name : "<null>",
                    targetType = target != null ? target.GetType().FullName : string.Empty,
                    methodName = evt.GetPersistentMethodName(i),
                };

                if (target != null)
                {
                    binding.targetPath = AssetDatabase.GetAssetPath(target);
                    if (target is GameObject go)
                        binding.targetHierarchyPath = go.scene.IsValid() ? HierarchyPath(go.transform) : string.Empty;
                    else if (target is Component component)
                        binding.targetHierarchyPath =
                            component.gameObject.scene.IsValid() ? HierarchyPath(component.transform) : string.Empty;
                }

                report.events.Add(binding);
            }
        }

        private static void FillReusableUiPrefabs(GameplayUiAuditReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Moyva" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                RectTransform[] rects = prefab.GetComponentsInChildren<RectTransform>(true);
                if (rects.Length == 0)
                    continue;

                Graphic[] graphics = prefab.GetComponentsInChildren<Graphic>(true);
                Selectable[] selectables = prefab.GetComponentsInChildren<Selectable>(true);
                TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);

                string[] projectTypes = prefab.GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(component => component != null)
                    .Select(component => component.GetType().FullName)
                    .Where(IsProjectRelevantType)
                    .Distinct()
                    .OrderBy(value => value)
                    .ToArray();

                report.uiPrefabs.Add(new AuditUiPrefab
                {
                    name = prefab.name,
                    path = path,
                    rectTransformCount = rects.Length,
                    graphicCount = graphics.Length,
                    selectableCount = selectables.Length,
                    tmpTextCount = texts.Length,
                    projectComponentTypes = projectTypes,
                });
            }

            report.uiPrefabs.Sort((a, b) =>
                string.Compare(a.path, b.path, StringComparison.OrdinalIgnoreCase));
        }

        private static void FillSpriteAssets(GameplayUiAuditReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Moyva" });
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!seen.Add(path))
                    continue;

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                    continue;

                report.sprites.Add(new AuditAsset
                {
                    name = sprite.name,
                    path = path,
                    type = sprite.GetType().FullName,
                    detail = $"pixelsPerUnit={sprite.pixelsPerUnit:0.##}; packed={sprite.packed}",
                    size = sprite.rect.size,
                });

                if (report.sprites.Count >= 1500)
                    break;
            }
        }

        private static void FillFontAssets(GameplayUiAuditReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/Moyva" });
            foreach (string guid in guids.Take(300))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font == null)
                    continue;

                report.fonts.Add(new AuditAsset
                {
                    name = font.name,
                    path = path,
                    type = font.GetType().FullName,
                    detail = font.atlasPopulationMode.ToString(),
                });
            }
        }

        private static void FillUsedUiMaterials(
            GameplayUiAuditReport report,
            List<GameObject> uiObjects)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (GameObject go in uiObjects)
            {
                foreach (Graphic graphic in go.GetComponents<Graphic>())
                {
                    if (graphic == null || graphic.material == null)
                        continue;

                    string path = AssetDatabase.GetAssetPath(graphic.material);
                    string key = string.IsNullOrWhiteSpace(path)
                        ? "instance:" + graphic.material.GetInstanceID()
                        : path;

                    if (!seen.Add(key))
                        continue;

                    report.materials.Add(new AuditAsset
                    {
                        name = graphic.material.name,
                        path = path,
                        type = graphic.material.GetType().FullName,
                        detail = graphic.material.shader != null ? graphic.material.shader.name : string.Empty,
                    });
                }
            }
        }

        private static void FillSourceAreas(GameplayUiAuditReport report)
        {
            foreach (string root in SourceAreaRoots)
            {
                string[] scripts = AssetDatabase.GetAllAssetPaths()
                    .Where(path => path.StartsWith(root.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
                    .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                report.sourceAreas.Add(new AuditSourceArea
                {
                    label = Path.GetFileName(root),
                    root = root,
                    scriptCount = scripts.Length,
                    scripts = scripts,
                });
            }
        }

        private static void FillIssues(GameplayUiAuditReport report, List<GameObject> uiObjects)
        {
            Vector2 gameSize = report.project.gameViewSize;

            var nameGroups = uiObjects
                .GroupBy(go => go.name)
                .Where(group => group.Count() > 1);

            foreach (var group in nameGroups)
            {
                report.issues.Add(new AuditIssue
                {
                    severity = "info",
                    code = "duplicate-ui-name",
                    objectPath = string.Empty,
                    message = $"UI name '{group.Key}' occurs {group.Count()} times.",
                    detail = string.Join(" | ", group.Select(go => HierarchyPath(go.transform)).Take(20)),
                });
            }

            var selectableRects = new List<(GameObject go, Rect rect)>();

            foreach (GameObject go in uiObjects)
            {
                string path = HierarchyPath(go.transform);
                RectTransform rect = go.GetComponent<RectTransform>();
                Graphic graphic = go.GetComponent<Graphic>();
                TMP_Text text = go.GetComponent<TMP_Text>();
                Selectable selectable = go.GetComponent<Selectable>();
                CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
                ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
                LayoutGroup parentLayout = go.transform.parent != null
                    ? go.transform.parent.GetComponent<LayoutGroup>()
                    : null;

                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
                {
                    report.issues.Add(new AuditIssue
                    {
                        severity = "error",
                        code = "missing-script",
                        objectPath = path,
                        message = "GameObject contains one or more missing MonoBehaviour scripts.",
                    });
                }

                if (rect != null && (rect.rect.width <= 1f || rect.rect.height <= 1f)
                    && (graphic != null || selectable != null || text != null))
                {
                    report.issues.Add(new AuditIssue
                    {
                        severity = "warning",
                        code = "zero-or-tiny-rect",
                        objectPath = path,
                        message = $"Visible/control UI rect is tiny: {rect.rect.width:0.##}x{rect.rect.height:0.##}.",
                    });
                }

                if (text != null && go.activeInHierarchy && text.fontSize > 0f && text.fontSize < 12f)
                {
                    report.issues.Add(new AuditIssue
                    {
                        severity = "info",
                        code = "small-font",
                        objectPath = path,
                        message = $"TMP font size is {text.fontSize:0.##}; review readability on mobile.",
                    });
                }

                if (graphic != null && graphic.raycastTarget && graphic.color.a <= 0.03f)
                {
                    report.issues.Add(new AuditIssue
                    {
                        severity = "warning",
                        code = "transparent-raycast-blocker",
                        objectPath = path,
                        message = "Almost-transparent Graphic still blocks UI raycasts.",
                    });
                }

                if (canvasGroup != null && canvasGroup.alpha <= 0.01f && canvasGroup.blocksRaycasts)
                {
                    report.issues.Add(new AuditIssue
                    {
                        severity = "warning",
                        code = "invisible-canvasgroup-blocker",
                        objectPath = path,
                        message = "CanvasGroup is invisible but blocks raycasts.",
                    });
                }

                if (parentLayout != null && fitter != null)
                {
                    report.issues.Add(new AuditIssue
                    {
                        severity = "info",
                        code = "layout-control-review",
                        objectPath = path,
                        message = "Child has ContentSizeFitter while parent is controlled by a LayoutGroup; verify layout ownership.",
                        detail = parentLayout.GetType().Name,
                    });
                }

                if (rect != null && go.activeInHierarchy && TryGetScreenRect(rect, out Rect screenRect))
                {
                    if (selectable != null && selectable.interactable)
                    {
                        selectableRects.Add((go, screenRect));

                        if (screenRect.width > 0f && screenRect.height > 0f
                            && (screenRect.width < 44f || screenRect.height < 44f))
                        {
                            report.issues.Add(new AuditIssue
                            {
                                severity = "info",
                                code = "small-interactive-target",
                                objectPath = path,
                                message =
                                    $"Interactable screen rect is about {screenRect.width:0.#}x{screenRect.height:0.#}; review mobile touch size.",
                            });
                        }
                    }

                    if (gameSize.x > 1f && gameSize.y > 1f
                        && (screenRect.xMax < 0f || screenRect.yMax < 0f
                            || screenRect.xMin > gameSize.x || screenRect.yMin > gameSize.y))
                    {
                        report.issues.Add(new AuditIssue
                        {
                            severity = "info",
                            code = "offscreen-ui",
                            objectPath = path,
                            message = "Active UI rect is fully outside the current Game View bounds.",
                        });
                    }
                }
            }

            int overlapGuard = 0;
            for (int i = 0; i < selectableRects.Count && overlapGuard < 200; i++)
            {
                for (int j = i + 1; j < selectableRects.Count && overlapGuard < 200; j++)
                {
                    Rect a = selectableRects[i].rect;
                    Rect b = selectableRects[j].rect;
                    Rect intersection = Intersect(a, b);
                    if (intersection.width <= 0f || intersection.height <= 0f)
                        continue;

                    float smallerArea = Mathf.Min(a.width * a.height, b.width * b.height);
                    float intersectionArea = intersection.width * intersection.height;
                    if (smallerArea <= 0f || intersectionArea / smallerArea < 0.35f)
                        continue;

                    report.issues.Add(new AuditIssue
                    {
                        severity = "info",
                        code = "interactive-overlap",
                        objectPath = HierarchyPath(selectableRects[i].go.transform),
                        message = "Two interactable controls overlap substantially in the current Game View.",
                        detail = HierarchyPath(selectableRects[j].go.transform),
                    });
                    overlapGuard++;
                }
            }
        }

        private static void FillSummary(GameplayUiAuditReport report)
        {
            report.summary.canvases = report.canvases.Count;
            report.summary.uiNodes = report.uiNodes.Count;
            report.summary.buttons = report.uiNodes.Count(node => node.selectableType == typeof(Button).FullName);
            report.summary.texts = report.uiNodes.Count(node => node.hasText);
            report.summary.images = report.uiNodes.Count(node => node.hasImage);
            report.summary.scrollRects = report.uiNodes.Count(node => node.hasScrollRect);
            report.summary.serializedProjectComponents = report.serializedComponents.Count;
            report.summary.serializedProperties =
                report.serializedComponents.Sum(component => component.properties.Count);
            report.summary.eventBindings = report.events.Count;
            report.summary.uiPrefabs = report.uiPrefabs.Count;
            report.summary.sprites = report.sprites.Count;
            report.summary.fonts = report.fonts.Count;
            report.summary.issuesInfo = report.issues.Count(issue => issue.severity == "info");
            report.summary.issuesWarning = report.issues.Count(issue => issue.severity == "warning");
            report.summary.issuesError = report.issues.Count(issue => issue.severity == "error");
        }

        private static string BuildSummaryText(GameplayUiAuditReport report)
        {
            return
                "Moyva Gameplay UI Audit\n" +
                "=======================\n" +
                $"Scene: {report.scene.path}\n" +
                $"Unity: {report.project.unityVersion}\n" +
                $"GameView: {report.project.gameViewSize.x:0}x{report.project.gameViewSize.y:0}\n" +
                $"Canvases: {report.summary.canvases}\n" +
                $"UI nodes: {report.summary.uiNodes}\n" +
                $"Buttons: {report.summary.buttons}\n" +
                $"TMP texts: {report.summary.texts}\n" +
                $"Images: {report.summary.images}\n" +
                $"ScrollRects: {report.summary.scrollRects}\n" +
                $"Serialized project components: {report.summary.serializedProjectComponents}\n" +
                $"Serialized properties: {report.summary.serializedProperties}\n" +
                $"Persistent event bindings: {report.summary.eventBindings}\n" +
                $"Reusable UI prefabs: {report.summary.uiPrefabs}\n" +
                $"Sprites indexed: {report.summary.sprites}\n" +
                $"Fonts indexed: {report.summary.fonts}\n" +
                $"Issues: info={report.summary.issuesInfo}, warning={report.summary.issuesWarning}, error={report.summary.issuesError}\n" +
                $"GameView screenshot requested: {report.screenshots.gameViewRequested}\n" +
                $"SceneView screenshot captured: {report.screenshots.sceneViewCaptured}\n" +
                $"Screenshot note: {report.screenshots.note}\n";
        }

        private static bool ScheduleGameViewCapture(string absolutePath, out string note)
        {
            try
            {
                string directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                if (gameViewType == null)
                {
                    note = "UnityEditor.GameView type not found.";
                    return false;
                }

                EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
                gameView.Show();
                gameView.Focus();
                gameView.Repaint();

                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        ScreenCapture.CaptureScreenshot(absolutePath, 1);
                        Debug.Log("[MoyvaGameplayUiAudit] GameView screenshot requested: " + absolutePath);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning("[MoyvaGameplayUiAudit] GameView capture failed: " + exception.Message);
                    }
                };

                note = "Capture scheduled after GameView focus; installer waits for PNG.";
                return true;
            }
            catch (Exception exception)
            {
                note = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private static bool TryCaptureSceneView(string absolutePath, out string note)
        {
            try
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null || sceneView.camera == null)
                {
                    note = "No active SceneView camera.";
                    return false;
                }

                UnityEngine.Camera camera = sceneView.camera;
                int width = Mathf.Clamp(Mathf.RoundToInt(sceneView.position.width), 320, 1920);
                int height = Mathf.Clamp(Mathf.RoundToInt(sceneView.position.height), 240, 1080);

                RenderTexture previousTarget = camera.targetTexture;
                RenderTexture previousActive = RenderTexture.active;
                RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);

                try
                {
                    camera.targetTexture = rt;
                    camera.Render();
                    RenderTexture.active = rt;

                    var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    texture.Apply();

                    string directory = Path.GetDirectoryName(absolutePath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(texture);
                }
                finally
                {
                    camera.targetTexture = previousTarget;
                    RenderTexture.active = previousActive;
                    RenderTexture.ReleaseTemporary(rt);
                }

                note = $"Captured SceneView camera at {width}x{height}.";
                return File.Exists(absolutePath);
            }
            catch (Exception exception)
            {
                note = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private static string BuildLayoutDetails(LayoutGroup layout)
        {
            if (layout is HorizontalOrVerticalLayoutGroup linear)
            {
                return
                    $"padding={linear.padding}; spacing={linear.spacing:0.##}; alignment={linear.childAlignment}; " +
                    $"controlW={linear.childControlWidth}; controlH={linear.childControlHeight}; " +
                    $"expandW={linear.childForceExpandWidth}; expandH={linear.childForceExpandHeight}";
            }

            if (layout is GridLayoutGroup grid)
            {
                return
                    $"padding={grid.padding}; spacing={grid.spacing}; cell={grid.cellSize}; " +
                    $"alignment={grid.childAlignment}; constraint={grid.constraint}; count={grid.constraintCount}";
            }

            return $"padding={layout.padding}; alignment={layout.childAlignment}";
        }

        private static bool IsUiObject(GameObject go)
        {
            return go.GetComponent<RectTransform>() != null
                   || go.GetComponent<Canvas>() != null
                   || go.GetComponent<EventSystem>() != null
                   || go.GetComponent<Graphic>() != null
                   || go.GetComponent<Selectable>() != null;
        }

        private static bool IsProjectRelevantType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            return ProjectNamespacePrefixes.Any(prefix =>
                fullName.StartsWith(prefix, StringComparison.Ordinal));
        }

        private static string GetScriptPath(MonoBehaviour behaviour)
        {
            MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
            return script != null ? AssetDatabase.GetAssetPath(script) : string.Empty;
        }

        private static string GetPrefabSourcePath(GameObject go)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            return source != null ? AssetDatabase.GetAssetPath(source) : string.Empty;
        }

        private static string SafeGlobalObjectId(UnityEngine.Object obj)
        {
            try
            {
                return GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Scene FindLoadedTargetScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid()
                    && string.Equals(scene.path, TargetScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return scene;
                }
            }

            return default;
        }

        private static List<GameObject> GetAllObjects(Scene scene)
        {
            var result = new List<GameObject>(1024);
            if (!scene.IsValid() || !scene.isLoaded)
                return result;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                var stack = new Stack<Transform>();
                stack.Push(root.transform);
                while (stack.Count > 0)
                {
                    Transform current = stack.Pop();
                    result.Add(current.gameObject);

                    for (int i = current.childCount - 1; i >= 0; i--)
                        stack.Push(current.GetChild(i));
                }
            }

            return result;
        }

        private static string HierarchyPath(Transform transform)
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

        private static bool TryGetScreenRect(RectTransform rect, out Rect screenRect)
        {
            screenRect = default;
            if (rect == null)
                return false;

            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            Canvas canvas = rect.GetComponentInParent<Canvas>();
            UnityEngine.Camera camera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                camera = canvas.worldCamera != null ? canvas.worldCamera : UnityEngine.Camera.main;

            Vector2 p0 = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            Vector2 p1 = RectTransformUtility.WorldToScreenPoint(camera, corners[1]);
            Vector2 p2 = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            Vector2 p3 = RectTransformUtility.WorldToScreenPoint(camera, corners[3]);

            float minX = Mathf.Min(p0.x, p1.x, p2.x, p3.x);
            float maxX = Mathf.Max(p0.x, p1.x, p2.x, p3.x);
            float minY = Mathf.Min(p0.y, p1.y, p2.y, p3.y);
            float maxY = Mathf.Max(p0.y, p1.y, p2.y, p3.y);

            screenRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        private static Rect Intersect(Rect a, Rect b)
        {
            float xMin = Mathf.Max(a.xMin, b.xMin);
            float yMin = Mathf.Max(a.yMin, b.yMin);
            float xMax = Mathf.Min(a.xMax, b.xMax);
            float yMax = Mathf.Min(a.yMax, b.yMax);

            if (xMax <= xMin || yMax <= yMin)
                return new Rect(0f, 0f, 0f, 0f);

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static bool ValidateOutputDirectory(
            string outputDir,
            out string normalized,
            out string error)
        {
            normalized = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(outputDir))
            {
                error = "output-dir is required.";
                return false;
            }

            try
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string allowedRoot = Path.GetFullPath(Path.Combine(home, AllowedReportRelativeRoot));
                normalized = Path.GetFullPath(outputDir);

                string rootWithSeparator = allowedRoot.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

                if (!normalized.StartsWith(rootWithSeparator, StringComparison.Ordinal)
                    && !string.Equals(normalized, allowedRoot, StringComparison.Ordinal))
                {
                    error = "Audit output is restricted to " + allowedRoot;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }
    }
}

#endif
