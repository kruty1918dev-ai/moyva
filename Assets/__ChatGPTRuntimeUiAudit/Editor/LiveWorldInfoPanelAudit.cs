#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChatGPT.MoyvaAudit
{
    public static class LiveWorldInfoPanelAudit
    {
        private const string TargetPanelPrefix = "BuildingInfoPanel";

        public static string Capture()
        {
            if (!EditorApplication.isPlaying)
                throw new InvalidOperationException("RUNTIME_UI_AUDIT_REQUIRES_PLAY_MODE");

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("ACTIVE_SCENE_NOT_LOADED");

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Environment.CurrentDirectory;
            string outputRoot = Path.Combine(projectRoot, "Temp", "ChatGPTUiRuntimeAudit");
            if (Directory.Exists(outputRoot))
                Directory.Delete(outputRoot, true);
            Directory.CreateDirectory(outputRoot);
            Directory.CreateDirectory(Path.Combine(outputRoot, "panels"));
            Directory.CreateDirectory(Path.Combine(outputRoot, "ui"));
            Directory.CreateDirectory(Path.Combine(outputRoot, "economy"));
            Directory.CreateDirectory(Path.Combine(outputRoot, "environment"));

            var allSceneObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go != null && go.scene.IsValid() && go.scene.isLoaded)
                .OrderBy(GetHierarchyPath, StringComparer.Ordinal)
                .ToList();

            var panels = allSceneObjects
                .Where(go => go.name.StartsWith(TargetPanelPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var activePanels = panels.Where(go => go.activeInHierarchy).ToList();

            WriteManifest(outputRoot, scene, allSceneObjects.Count, panels, activePanels);
            DumpAllCanvases(outputRoot, allSceneObjects);
            DumpAllScrollRects(outputRoot, allSceneObjects);
            DumpResourceLikeTexts(outputRoot, allSceneObjects);
            DumpEconomyRuntime(outputRoot);

            for (int i = 0; i < panels.Count; i++)
            {
                GameObject panel = panels[i];
                string dir = Path.Combine(outputRoot, "panels", $"{i:D2}_{SafeFileName(panel.name)}_{panel.GetInstanceID()}");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "00_panel_summary.txt"), BuildPanelSummary(panel), Encoding.UTF8);
                File.WriteAllText(Path.Combine(dir, "01_hierarchy.txt"), BuildHierarchy(panel.transform), Encoding.UTF8);
                DumpObjectTree(panel.transform, Path.Combine(dir, "objects"));
                DumpRectGeometry(panel.transform, Path.Combine(dir, "02_rect_geometry.tsv"));
                DumpPanelTexts(panel.transform, Path.Combine(dir, "03_texts.tsv"));
                DumpPanelInteractables(panel.transform, Path.Combine(dir, "04_interactables.tsv"));
                DumpPanelLayoutTopology(panel.transform, Path.Combine(dir, "05_layout_topology.tsv"));
                DumpContainment(panel.transform, Path.Combine(dir, "06_containment.tsv"));
            }

            File.WriteAllText(Path.Combine(outputRoot, "COMPLETE.txt"),
                $"RUNTIME_UI_AUDIT_COMPLETE\nscene={scene.path}\npanels={panels.Count}\nactivePanels={activePanels.Count}\ntimeUtc={DateTime.UtcNow:O}\n",
                Encoding.UTF8);

            if (activePanels.Count == 0)
                throw new InvalidOperationException($"BUILDING_INFO_PANEL_NOT_OPEN; output={outputRoot}; candidates={panels.Count}");

            return $"RUNTIME_UI_AUDIT_CAPTURED|output={outputRoot}|panels={panels.Count}|active={activePanels.Count}";
        }

        private static void WriteManifest(string root, Scene scene, int objectCount, List<GameObject> panels, List<GameObject> activePanels)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MOYVA LIVE WORLD INFO PANEL AUDIT");
            sb.AppendLine($"utc: {DateTime.UtcNow:O}");
            sb.AppendLine($"unityVersion: {Application.unityVersion}");
            sb.AppendLine($"scenePath: {scene.path}");
            sb.AppendLine($"sceneName: {scene.name}");
            sb.AppendLine($"playMode: {EditorApplication.isPlaying}");
            sb.AppendLine($"screen: {Screen.width}x{Screen.height}");
            sb.AppendLine($"safeArea: {Screen.safeArea}");
            sb.AppendLine($"sceneObjectCount: {objectCount}");
            sb.AppendLine($"buildingInfoPanelCandidates: {panels.Count}");
            sb.AppendLine($"activeBuildingInfoPanels: {activePanels.Count}");
            foreach (var panel in panels)
                sb.AppendLine($"panel: active={panel.activeInHierarchy} path={GetHierarchyPath(panel)} instanceId={panel.GetInstanceID()}");
            File.WriteAllText(Path.Combine(root, "00_MANIFEST.txt"), sb.ToString(), Encoding.UTF8);
        }

        private static string BuildPanelSummary(GameObject panel)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"name: {panel.name}");
            sb.AppendLine($"path: {GetHierarchyPath(panel)}");
            sb.AppendLine($"activeSelf: {panel.activeSelf}");
            sb.AppendLine($"activeInHierarchy: {panel.activeInHierarchy}");
            sb.AppendLine($"instanceId: {panel.GetInstanceID()}");
            sb.AppendLine($"scene: {panel.scene.path}");
            sb.AppendLine($"prefabSource: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(panel)}");

            Transform resourceText = FindDeep(panel.transform, "ResourcesText");
            if (resourceText != null)
            {
                TMP_Text tmp = resourceText.GetComponent<TMP_Text>();
                sb.AppendLine("resourcesTextPath: " + GetHierarchyPath(resourceText.gameObject));
                sb.AppendLine("resourcesText: " + Escape(tmp != null ? tmp.text : "<no TMP_Text>"));
                if (tmp != null)
                {
                    sb.AppendLine($"resourcesPreferred: {tmp.preferredWidth:0.###}x{tmp.preferredHeight:0.###}");
                    sb.AppendLine($"resourcesFontSize: {tmp.fontSize:0.###}");
                    sb.AppendLine($"resourcesOverflow: {tmp.overflowMode}");
                }
            }
            else
            {
                sb.AppendLine("resourcesTextPath: <missing>");
            }

            return sb.ToString();
        }

        private static string BuildHierarchy(Transform root)
        {
            var sb = new StringBuilder();
            AppendHierarchy(sb, root, 0);
            return sb.ToString();
        }

        private static void AppendHierarchy(StringBuilder sb, Transform t, int depth)
        {
            sb.Append(' ', depth * 2)
              .Append("- ").Append(t.name)
              .Append(" [activeSelf=").Append(t.gameObject.activeSelf)
              .Append(", activeInHierarchy=").Append(t.gameObject.activeInHierarchy)
              .Append(", components=")
              .Append(string.Join(",", t.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().FullName)))
              .AppendLine("]");
            for (int i = 0; i < t.childCount; i++)
                AppendHierarchy(sb, t.GetChild(i), depth + 1);
        }

        private static void DumpObjectTree(Transform root, string outputRoot)
        {
            Directory.CreateDirectory(outputRoot);
            DumpObjectRecursive(root, outputRoot, 0);
        }

        private static void DumpObjectRecursive(Transform t, string parentDir, int siblingIndex)
        {
            string dir = Path.Combine(parentDir, $"{siblingIndex:D3}_{SafeFileName(t.name)}_{t.gameObject.GetInstanceID()}");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "object.txt"), BuildObjectDump(t.gameObject), Encoding.UTF8);
            for (int i = 0; i < t.childCount; i++)
                DumpObjectRecursive(t.GetChild(i), dir, i);
        }

        private static string BuildObjectDump(GameObject go)
        {
            var sb = new StringBuilder();
            Transform t = go.transform;
            sb.AppendLine("=== GAME OBJECT ===");
            sb.AppendLine($"name: {go.name}");
            sb.AppendLine($"hierarchyPath: {GetHierarchyPath(go)}");
            sb.AppendLine($"parent: {(t.parent != null ? GetHierarchyPath(t.parent.gameObject) : "<root>")}");
            sb.AppendLine($"siblingIndex: {t.GetSiblingIndex()}");
            sb.AppendLine($"childCount: {t.childCount}");
            for (int i = 0; i < t.childCount; i++) sb.AppendLine($"child[{i}]: {GetHierarchyPath(t.GetChild(i).gameObject)}");
            sb.AppendLine($"activeSelf: {go.activeSelf}");
            sb.AppendLine($"activeInHierarchy: {go.activeInHierarchy}");
            sb.AppendLine($"tag: {go.tag}");
            sb.AppendLine($"layer: {go.layer} ({LayerMask.LayerToName(go.layer)})");
            sb.AppendLine($"instanceId: {go.GetInstanceID()}");
            sb.AppendLine($"prefabSource: {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go)}");
            sb.AppendLine();

            sb.AppendLine("=== TRANSFORM ===");
            sb.AppendLine($"localPosition: {Fmt(t.localPosition)}");
            sb.AppendLine($"localRotationEuler: {Fmt(t.localEulerAngles)}");
            sb.AppendLine($"localScale: {Fmt(t.localScale)}");
            sb.AppendLine($"worldPosition: {Fmt(t.position)}");
            sb.AppendLine($"worldRotationEuler: {Fmt(t.eulerAngles)}");
            sb.AppendLine($"lossyScale: {Fmt(t.lossyScale)}");
            if (t is RectTransform rt)
                AppendRectTransform(sb, rt);
            sb.AppendLine();

            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component c = components[i];
                if (c == null)
                {
                    sb.AppendLine($"=== COMPONENT {i}: <MISSING SCRIPT> ===");
                    continue;
                }

                sb.AppendLine($"=== COMPONENT {i}: {c.GetType().FullName} ===");
                AppendKnownComponentData(sb, c);
                AppendSerializedProperties(sb, c);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static void AppendRectTransform(StringBuilder sb, RectTransform rt)
        {
            sb.AppendLine($"anchorMin: {Fmt(rt.anchorMin)}");
            sb.AppendLine($"anchorMax: {Fmt(rt.anchorMax)}");
            sb.AppendLine($"pivot: {Fmt(rt.pivot)}");
            sb.AppendLine($"anchoredPosition: {Fmt(rt.anchoredPosition)}");
            sb.AppendLine($"sizeDelta: {Fmt(rt.sizeDelta)}");
            sb.AppendLine($"offsetMin: {Fmt(rt.offsetMin)}");
            sb.AppendLine($"offsetMax: {Fmt(rt.offsetMax)}");
            sb.AppendLine($"rect: {Fmt(rt.rect)}");
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            sb.AppendLine("worldCorners: " + string.Join(" | ", corners.Select(Fmt)));
        }

        private static void AppendKnownComponentData(StringBuilder sb, Component c)
        {
            if (c is Behaviour behaviour) sb.AppendLine($"enabled: {behaviour.enabled}");
            if (c is TMP_Text tmp)
            {
                sb.AppendLine($"TMP.text: {Escape(tmp.text)}");
                sb.AppendLine($"TMP.fontSize: {tmp.fontSize:0.###}");
                sb.AppendLine($"TMP.enableAutoSizing: {tmp.enableAutoSizing}");
                sb.AppendLine($"TMP.fontSizeMin: {tmp.fontSizeMin:0.###}");
                sb.AppendLine($"TMP.fontSizeMax: {tmp.fontSizeMax:0.###}");
                sb.AppendLine($"TMP.preferredWidth: {tmp.preferredWidth:0.###}");
                sb.AppendLine($"TMP.preferredHeight: {tmp.preferredHeight:0.###}");
                sb.AppendLine($"TMP.overflowMode: {tmp.overflowMode}");
                sb.AppendLine($"TMP.alignment: {tmp.alignment}");
            }
            if (c is ScrollRect scroll)
            {
                sb.AppendLine($"ScrollRect.viewport: {(scroll.viewport != null ? GetHierarchyPath(scroll.viewport.gameObject) : "<null>")}");
                sb.AppendLine($"ScrollRect.content: {(scroll.content != null ? GetHierarchyPath(scroll.content.gameObject) : "<null>")}");
                sb.AppendLine($"ScrollRect.horizontal: {scroll.horizontal}");
                sb.AppendLine($"ScrollRect.vertical: {scroll.vertical}");
                sb.AppendLine($"ScrollRect.movementType: {scroll.movementType}");
                sb.AppendLine($"ScrollRect.inertia: {scroll.inertia}");
                sb.AppendLine($"ScrollRect.scrollSensitivity: {scroll.scrollSensitivity:0.###}");
                sb.AppendLine($"ScrollRect.normalizedPosition: {Fmt(scroll.normalizedPosition)}");
            }
            if (c is LayoutElement le)
            {
                sb.AppendLine($"LayoutElement.min=({le.minWidth:0.###},{le.minHeight:0.###}) preferred=({le.preferredWidth:0.###},{le.preferredHeight:0.###}) flexible=({le.flexibleWidth:0.###},{le.flexibleHeight:0.###}) ignore={le.ignoreLayout}");
            }
            if (c is ContentSizeFitter csf)
                sb.AppendLine($"ContentSizeFitter.horizontalFit={csf.horizontalFit} verticalFit={csf.verticalFit}");
            if (c is HorizontalOrVerticalLayoutGroup lg)
                sb.AppendLine($"LayoutGroup.padding={lg.padding} spacing={lg.spacing:0.###} childAlignment={lg.childAlignment} control=({lg.childControlWidth},{lg.childControlHeight}) forceExpand=({lg.childForceExpandWidth},{lg.childForceExpandHeight})");
            if (c is GridLayoutGroup grid)
                sb.AppendLine($"GridLayout.cellSize={Fmt(grid.cellSize)} spacing={Fmt(grid.spacing)} constraint={grid.constraint} constraintCount={grid.constraintCount}");
            if (c is Button button)
                sb.AppendLine($"Button.interactable={button.interactable} persistentListeners={button.onClick.GetPersistentEventCount()}");
            if (c is Toggle toggle)
                sb.AppendLine($"Toggle.isOn={toggle.isOn} interactable={toggle.interactable} persistentListeners={toggle.onValueChanged.GetPersistentEventCount()}");
            if (c is Dropdown dropdown)
                sb.AppendLine($"Dropdown.value={dropdown.value} options={dropdown.options.Count}");
            if (c is Image image)
                sb.AppendLine($"Image.sprite={(image.sprite != null ? AssetDatabase.GetAssetPath(image.sprite) : "<null>")} type={image.type} color={image.color}");
            if (c is RectMask2D) sb.AppendLine("CLIPPER: RectMask2D");
            if (c is Mask mask) sb.AppendLine($"CLIPPER: Mask showMaskGraphic={mask.showMaskGraphic}");
        }

        private static void AppendSerializedProperties(StringBuilder sb, Component c)
        {
            try
            {
                var so = new SerializedObject(c);
                SerializedProperty p = so.GetIterator();
                bool enterChildren = true;
                while (p.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    string value = SerializedValue(p);
                    sb.AppendLine($"SERIALIZED {p.propertyPath} [{p.propertyType}] = {value}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"SERIALIZED_DUMP_ERROR: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static string SerializedValue(SerializedProperty p)
        {
            try
            {
                switch (p.propertyType)
                {
                    case SerializedPropertyType.Integer: return p.longValue.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                    case SerializedPropertyType.Float: return p.doubleValue.ToString("0.######", CultureInfo.InvariantCulture);
                    case SerializedPropertyType.String: return Escape(p.stringValue);
                    case SerializedPropertyType.Color: return p.colorValue.ToString();
                    case SerializedPropertyType.ObjectReference:
                        if (p.objectReferenceValue == null) return "<null>";
                        Object obj = p.objectReferenceValue;
                        string asset = AssetDatabase.GetAssetPath(obj);
                        if (!string.IsNullOrWhiteSpace(asset)) return $"asset:{asset}::{obj.GetType().FullName}";
                        if (obj is Component component) return $"scene:{GetHierarchyPath(component.gameObject)}::{component.GetType().FullName}";
                        if (obj is GameObject go) return $"scene:{GetHierarchyPath(go)}";
                        return $"runtime:{obj.name}::{obj.GetType().FullName}::{obj.GetInstanceID()}";
                    case SerializedPropertyType.LayerMask: return p.intValue.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Enum: return p.enumDisplayNames != null && p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length ? p.enumDisplayNames[p.enumValueIndex] : p.enumValueIndex.ToString();
                    case SerializedPropertyType.Vector2: return Fmt(p.vector2Value);
                    case SerializedPropertyType.Vector3: return Fmt(p.vector3Value);
                    case SerializedPropertyType.Vector4: return p.vector4Value.ToString();
                    case SerializedPropertyType.Rect: return Fmt(p.rectValue);
                    case SerializedPropertyType.ArraySize: return p.intValue.ToString(CultureInfo.InvariantCulture);
                    case SerializedPropertyType.Character: return ((char)p.intValue).ToString();
                    case SerializedPropertyType.AnimationCurve: return p.animationCurveValue != null ? $"keys={p.animationCurveValue.length}" : "<null>";
                    case SerializedPropertyType.Bounds: return p.boundsValue.ToString();
                    case SerializedPropertyType.Quaternion: return p.quaternionValue.eulerAngles.ToString();
                    default: return $"<{p.propertyType}>";
                }
            }
            catch (Exception ex) { return $"<error:{ex.GetType().Name}>"; }
        }

        private static void DumpRectGeometry(Transform root, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\tactive\tanchorMin\tanchorMax\tpivot\tanchoredPosition\tsizeDelta\trect\tworldCorners\trootCanvas\tinsideCanvas\tnearestClipper\tinsideClipper");
            foreach (RectTransform rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                Canvas canvas = rt.GetComponentInParent<Canvas>();
                Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
                RectTransform clipper = FindNearestClipper(rt);
                sb.Append(GetHierarchyPath(rt.gameObject)).Append('\t')
                  .Append(rt.gameObject.activeInHierarchy).Append('\t')
                  .Append(Fmt(rt.anchorMin)).Append('\t').Append(Fmt(rt.anchorMax)).Append('\t')
                  .Append(Fmt(rt.pivot)).Append('\t').Append(Fmt(rt.anchoredPosition)).Append('\t')
                  .Append(Fmt(rt.sizeDelta)).Append('\t').Append(Fmt(rt.rect)).Append('\t')
                  .Append(WorldCorners(rt)).Append('\t')
                  .Append(rootCanvas != null ? GetHierarchyPath(rootCanvas.gameObject) : "<none>").Append('\t')
                  .Append(rootCanvas == null ? "n/a" : IsInside(rt, rootCanvas.transform as RectTransform).ToString()).Append('\t')
                  .Append(clipper != null ? GetHierarchyPath(clipper.gameObject) : "<none>").Append('\t')
                  .Append(clipper == null ? "n/a" : IsInside(rt, clipper).ToString()).AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void DumpPanelTexts(Transform root, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\tactive\ttext\tfontSize\tautoSize\tpreferredWidth\tpreferredHeight\toverflow\talignment");
            foreach (TMP_Text tmp in root.GetComponentsInChildren<TMP_Text>(true))
            {
                sb.Append(GetHierarchyPath(tmp.gameObject)).Append('\t')
                  .Append(tmp.gameObject.activeInHierarchy).Append('\t')
                  .Append(Escape(tmp.text)).Append('\t')
                  .Append(tmp.fontSize.ToString("0.###", CultureInfo.InvariantCulture)).Append('\t')
                  .Append(tmp.enableAutoSizing).Append('\t')
                  .Append(tmp.preferredWidth.ToString("0.###", CultureInfo.InvariantCulture)).Append('\t')
                  .Append(tmp.preferredHeight.ToString("0.###", CultureInfo.InvariantCulture)).Append('\t')
                  .Append(tmp.overflowMode).Append('\t')
                  .Append(tmp.alignment).AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void DumpPanelInteractables(Transform root, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\ttype\tactive\tinteractable\tstate\tpersistentListeners");
            foreach (Selectable s in root.GetComponentsInChildren<Selectable>(true))
            {
                string state = "";
                int listeners = 0;
                if (s is Button b) listeners = b.onClick.GetPersistentEventCount();
                else if (s is Toggle t) { state = "isOn=" + t.isOn; listeners = t.onValueChanged.GetPersistentEventCount(); }
                else if (s is Dropdown d) { state = "value=" + d.value; listeners = d.onValueChanged.GetPersistentEventCount(); }
                sb.Append(GetHierarchyPath(s.gameObject)).Append('\t').Append(s.GetType().FullName).Append('\t')
                  .Append(s.gameObject.activeInHierarchy).Append('\t').Append(s.interactable).Append('\t')
                  .Append(state).Append('\t').Append(listeners).AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void DumpPanelLayoutTopology(Transform root, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\tcomponent\tdetails");
            foreach (Component c in root.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                string details = null;
                if (c is ScrollRect sr) details = $"viewport={PathOf(sr.viewport)} content={PathOf(sr.content)} h={sr.horizontal} v={sr.vertical} movement={sr.movementType}";
                else if (c is RectMask2D) details = "RectMask2D";
                else if (c is Mask m) details = $"Mask showGraphic={m.showMaskGraphic}";
                else if (c is VerticalLayoutGroup v) details = $"VerticalLayout spacing={v.spacing} padding={v.padding} control=({v.childControlWidth},{v.childControlHeight}) expand=({v.childForceExpandWidth},{v.childForceExpandHeight})";
                else if (c is HorizontalLayoutGroup h) details = $"HorizontalLayout spacing={h.spacing} padding={h.padding} control=({h.childControlWidth},{h.childControlHeight}) expand=({h.childForceExpandWidth},{h.childForceExpandHeight})";
                else if (c is GridLayoutGroup g) details = $"GridLayout cell={Fmt(g.cellSize)} spacing={Fmt(g.spacing)} constraint={g.constraint}/{g.constraintCount}";
                else if (c is ContentSizeFitter f) details = $"ContentSizeFitter h={f.horizontalFit} v={f.verticalFit}";
                else if (c is LayoutElement le) details = $"LayoutElement min=({le.minWidth},{le.minHeight}) pref=({le.preferredWidth},{le.preferredHeight}) flex=({le.flexibleWidth},{le.flexibleHeight}) ignore={le.ignoreLayout}";
                if (details != null)
                    sb.Append(GetHierarchyPath(c.gameObject)).Append('\t').Append(c.GetType().FullName).Append('\t').Append(details).AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void DumpContainment(Transform root, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\tcanvas\tinsideCanvas\tclipper\tinsideClipper");
            foreach (RectTransform rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                Canvas c = rt.GetComponentInParent<Canvas>();
                RectTransform canvasRt = c != null ? c.rootCanvas.transform as RectTransform : null;
                RectTransform clipper = FindNearestClipper(rt);
                sb.Append(GetHierarchyPath(rt.gameObject)).Append('\t')
                  .Append(canvasRt != null ? GetHierarchyPath(canvasRt.gameObject) : "<none>").Append('\t')
                  .Append(canvasRt != null && IsInside(rt, canvasRt)).Append('\t')
                  .Append(clipper != null ? GetHierarchyPath(clipper.gameObject) : "<none>").Append('\t')
                  .Append(clipper != null && IsInside(rt, clipper)).AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void DumpAllCanvases(string root, List<GameObject> allObjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\troot\trenderMode\tsortingLayer\torder\tscaleFactor\tpixelRect\tscaler\treferenceResolution\tmatch");
            foreach (Canvas c in allObjects.Select(go => go.GetComponent<Canvas>()).Where(x => x != null))
            {
                CanvasScaler scaler = c.GetComponent<CanvasScaler>();
                sb.Append(GetHierarchyPath(c.gameObject)).Append('\t').Append(c.isRootCanvas).Append('\t').Append(c.renderMode).Append('\t')
                  .Append(c.sortingLayerName).Append('\t').Append(c.sortingOrder).Append('\t').Append(c.scaleFactor.ToString("0.###", CultureInfo.InvariantCulture)).Append('\t')
                  .Append(Fmt(c.pixelRect)).Append('\t').Append(scaler != null ? scaler.uiScaleMode.ToString() : "<none>").Append('\t')
                  .Append(scaler != null ? Fmt(scaler.referenceResolution) : "").Append('\t')
                  .Append(scaler != null ? scaler.matchWidthOrHeight.ToString("0.###", CultureInfo.InvariantCulture) : "").AppendLine();
            }
            File.WriteAllText(Path.Combine(root, "ui", "all_canvases.tsv"), sb.ToString(), Encoding.UTF8);
        }

        private static void DumpAllScrollRects(string root, List<GameObject> allObjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine("path\tactive\tviewport\tcontent\thorizontal\tvertical\tmovement\tviewportRect\tcontentRect\tbiggerX\tbiggerY");
            foreach (ScrollRect sr in allObjects.Select(go => go.GetComponent<ScrollRect>()).Where(x => x != null))
            {
                Rect vr = sr.viewport != null ? sr.viewport.rect : default;
                Rect cr = sr.content != null ? sr.content.rect : default;
                sb.Append(GetHierarchyPath(sr.gameObject)).Append('\t').Append(sr.gameObject.activeInHierarchy).Append('\t')
                  .Append(PathOf(sr.viewport)).Append('\t').Append(PathOf(sr.content)).Append('\t')
                  .Append(sr.horizontal).Append('\t').Append(sr.vertical).Append('\t').Append(sr.movementType).Append('\t')
                  .Append(Fmt(vr)).Append('\t').Append(Fmt(cr)).Append('\t')
                  .Append(sr.viewport != null && sr.content != null && cr.width > vr.width + 0.1f).Append('\t')
                  .Append(sr.viewport != null && sr.content != null && cr.height > vr.height + 0.1f).AppendLine();
            }
            File.WriteAllText(Path.Combine(root, "ui", "all_scrollrects.tsv"), sb.ToString(), Encoding.UTF8);
        }

        private static void DumpResourceLikeTexts(string root, List<GameObject> allObjects)
        {
            string[] keywords = { "resource", "ресурс", "warehouse", "склад", "storage", "food", "їжа", "material", "матеріал" };
            var sb = new StringBuilder();
            sb.AppendLine("path\tactive\ttext");
            foreach (TMP_Text tmp in allObjects.Select(go => go.GetComponent<TMP_Text>()).Where(x => x != null))
            {
                string haystack = (GetHierarchyPath(tmp.gameObject) + "\n" + (tmp.text ?? "")).ToLowerInvariant();
                if (!keywords.Any(k => haystack.Contains(k))) continue;
                sb.Append(GetHierarchyPath(tmp.gameObject)).Append('\t').Append(tmp.gameObject.activeInHierarchy).Append('\t').Append(Escape(tmp.text)).AppendLine();
            }
            File.WriteAllText(Path.Combine(root, "ui", "resource_like_texts.tsv"), sb.ToString(), Encoding.UTF8);
        }

        private static void DumpEconomyRuntime(string root)
        {
            string economyDir = Path.Combine(root, "economy");
            var report = new StringBuilder();
            try
            {
                object container = ResolveZenjectContainer(report);
                string ownerId = "player_0";
                object turnService = ResolveFromContainerBySimpleOrFullName(container, "ITurnService", "Kruty1918.Moyva.Turns.Runtime.ITurnService", report);
                object localOwner = GetMember(turnService, "LocalOwnerId");
                if (localOwner is string s && !string.IsNullOrWhiteSpace(s)) ownerId = s.Trim();
                report.AppendLine($"ownerId={ownerId}");

                object runtimeApi = ResolveFromContainerBySimpleOrFullName(container, "IEconomyRuntimeApi", "Kruty1918.Moyva.Economy.Runtime.IEconomyRuntimeApi", report);
                object mediator = ResolveFromContainerBySimpleOrFullName(container, "IEconomyInfoMediator", null, report);

                var ownerRows = new List<ResourceRow>();
                object totals = Invoke(runtimeApi, "GetOwnerResourceTotals", ownerId);
                foreach (DictionaryEntryLike entry in EnumerateDictionary(totals))
                {
                    string id = entry.Key?.ToString() ?? "";
                    float amount = ToFloat(entry.Value);
                    string display = mediator != null ? Convert.ToString(Invoke(mediator, "GetResourceDisplayName", id)) : "";
                    ownerRows.Add(new ResourceRow(id, display, "", amount));
                }

                var definitions = ReadLoadedResourceDefinitions(report);
                var definitionById = definitions
                    .Where(r => !string.IsNullOrWhiteSpace(r.Id))
                    .GroupBy(r => r.Id, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

                for (int i = 0; i < ownerRows.Count; i++)
                {
                    ResourceRow row = ownerRows[i];
                    if (definitionById.TryGetValue(row.Id, out ResourceRow def))
                    {
                        if (string.IsNullOrWhiteSpace(row.DisplayName)) row.DisplayName = def.DisplayName;
                        row.Category = def.Category;
                        ownerRows[i] = row;
                    }
                }

                WriteResourceRows(Path.Combine(economyDir, "owner_resources.tsv"), ownerRows);
                WriteResourceRows(Path.Combine(economyDir, "resource_definitions.tsv"), definitions);

                var settlementIds = new List<string>();
                object idsObj = Invoke(runtimeApi, "GetSettlementIdsForOwner", ownerId);
                if (idsObj is IEnumerable ids)
                    foreach (object id in ids) if (id != null) settlementIds.Add(id.ToString());

                var ssb = new StringBuilder();
                ssb.AppendLine("settlementId\tresourceId\tdisplayName\tcategory\tamount");
                foreach (string settlementId in settlementIds)
                {
                    object st = Invoke(runtimeApi, "GetSettlementResourceTotals", settlementId);
                    foreach (DictionaryEntryLike entry in EnumerateDictionary(st))
                    {
                        string id = entry.Key?.ToString() ?? "";
                        string display = mediator != null ? Convert.ToString(Invoke(mediator, "GetResourceDisplayName", id)) : "";
                        string category = definitionById.TryGetValue(id, out ResourceRow def) ? def.Category : "";
                        ssb.Append(settlementId).Append('\t').Append(id).Append('\t').Append(Escape(display)).Append('\t')
                           .Append(category).Append('\t').Append(ToFloat(entry.Value).ToString("0.###", CultureInfo.InvariantCulture)).AppendLine();
                    }
                }
                File.WriteAllText(Path.Combine(economyDir, "settlement_resources.tsv"), ssb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                report.AppendLine($"ECONOMY_RUNTIME_ERROR={ex.GetType().FullName}: {ex.Message}");
                report.AppendLine(ex.StackTrace);
            }
            File.WriteAllText(Path.Combine(economyDir, "runtime_resolution.txt"), report.ToString(), Encoding.UTF8);
        }

        private static object ResolveZenjectContainer(StringBuilder report)
        {
            MonoBehaviour sceneContext = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .FirstOrDefault(m => m != null && m.gameObject.scene.IsValid() && m.gameObject.scene.isLoaded && m.GetType().FullName == "Zenject.SceneContext");
            if (sceneContext == null) throw new InvalidOperationException("Zenject.SceneContext not found");
            object container = GetMember(sceneContext, "Container");
            if (container == null) throw new InvalidOperationException("SceneContext.Container == null");
            report.AppendLine($"sceneContext={GetHierarchyPath(sceneContext.gameObject)} containerType={container.GetType().FullName}");
            return container;
        }

        private static object ResolveFromContainerBySimpleOrFullName(object container, string simpleName, string fullName, StringBuilder report)
        {
            if (container == null) return null;
            Type type = FindType(simpleName, fullName);
            if (type == null)
            {
                report.AppendLine($"TYPE_NOT_FOUND simple={simpleName} full={fullName}");
                return null;
            }
            MethodInfo resolve = container.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "Resolve" && !m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Type));
            if (resolve == null)
            {
                report.AppendLine($"CONTAINER_RESOLVE_TYPE_METHOD_NOT_FOUND for {type.FullName}");
                return null;
            }
            try
            {
                object value = resolve.Invoke(container, new object[] { type });
                report.AppendLine($"resolved {type.FullName} => {(value != null ? value.GetType().FullName : "<null>")}");
                return value;
            }
            catch (TargetInvocationException tie)
            {
                report.AppendLine($"RESOLVE_FAILED {type.FullName}: {tie.InnerException?.Message ?? tie.Message}");
                return null;
            }
        }

        private static Type FindType(string simpleName, string fullName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    Type exact = asm.GetType(fullName, false);
                    if (exact != null) return exact;
                }
                try
                {
                    Type match = asm.GetTypes().FirstOrDefault(t => t.Name == simpleName);
                    if (match != null) return match;
                }
                catch (ReflectionTypeLoadException rtl)
                {
                    Type match = rtl.Types?.FirstOrDefault(t => t != null && t.Name == simpleName);
                    if (match != null) return match;
                }
            }
            return null;
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            if (target == null) return null;
            MethodInfo method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == args.Length);
            if (method == null) return null;
            try { return method.Invoke(target, args); }
            catch (TargetInvocationException) { return null; }
        }

        private static object GetMember(object target, string name)
        {
            if (target == null) return null;
            Type t = target.GetType();
            PropertyInfo p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (p != null && p.GetIndexParameters().Length == 0) { try { return p.GetValue(target); } catch { } }
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (f != null) { try { return f.GetValue(target); } catch { } }
            string fieldName = "_" + char.ToLowerInvariant(name[0]) + name.Substring(1);
            f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (f != null) { try { return f.GetValue(target); } catch { } }
            return null;
        }

        private static List<ResourceRow> ReadLoadedResourceDefinitions(StringBuilder report)
        {
            var rows = new List<ResourceRow>();
            ScriptableObject[] all = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            foreach (ScriptableObject so in all)
            {
                if (so == null || so.GetType().Name != "EconomyDatabaseSO") continue;
                report.AppendLine($"EconomyDatabaseSO loaded name={so.name} asset={AssetDatabase.GetAssetPath(so)}");
                object resources = GetMember(so, "Resources");
                if (!(resources is IEnumerable enumerable)) continue;
                foreach (object res in enumerable)
                {
                    if (res == null) continue;
                    string id = Convert.ToString(GetMember(res, "Id")) ?? "";
                    string display = Convert.ToString(GetMember(res, "DisplayName")) ?? "";
                    string category = Convert.ToString(GetMember(res, "Category")) ?? "";
                    rows.Add(new ResourceRow(id, display, category, 0f));
                }
            }
            return rows;
        }

        private static void WriteResourceRows(string path, List<ResourceRow> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("resourceId\tdisplayName\tcategory\tamount");
            foreach (ResourceRow r in rows.OrderBy(r => r.Category, StringComparer.Ordinal).ThenBy(r => r.DisplayName, StringComparer.Ordinal).ThenBy(r => r.Id, StringComparer.Ordinal))
                sb.Append(r.Id).Append('\t').Append(Escape(r.DisplayName)).Append('\t').Append(r.Category).Append('\t').Append(r.Amount.ToString("0.###", CultureInfo.InvariantCulture)).AppendLine();
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static IEnumerable<DictionaryEntryLike> EnumerateDictionary(object value)
        {
            if (value == null) yield break;
            if (value is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict) yield return new DictionaryEntryLike(entry.Key, entry.Value);
                yield break;
            }
            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item == null) continue;
                    object key = GetMember(item, "Key");
                    object val = GetMember(item, "Value");
                    if (key != null) yield return new DictionaryEntryLike(key, val);
                }
            }
        }

        private static float ToFloat(object value)
        {
            try { return value == null ? 0f : Convert.ToSingle(value, CultureInfo.InvariantCulture); }
            catch { return 0f; }
        }

        private static RectTransform FindNearestClipper(RectTransform rt)
        {
            Transform p = rt.parent;
            while (p != null)
            {
                if (p.GetComponent<RectMask2D>() != null || p.GetComponent<Mask>() != null)
                    return p as RectTransform;
                p = p.parent;
            }
            return null;
        }

        private static bool IsInside(RectTransform child, RectTransform container)
        {
            if (child == null || container == null) return false;
            Vector3[] childCorners = new Vector3[4];
            Vector3[] containerCorners = new Vector3[4];
            child.GetWorldCorners(childCorners);
            container.GetWorldCorners(containerCorners);
            float minX = containerCorners.Min(v => v.x) - 0.5f;
            float maxX = containerCorners.Max(v => v.x) + 0.5f;
            float minY = containerCorners.Min(v => v.y) - 0.5f;
            float maxY = containerCorners.Max(v => v.y) + 0.5f;
            return childCorners.All(v => v.x >= minX && v.x <= maxX && v.y >= minY && v.y <= maxY);
        }

        private static string WorldCorners(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            return string.Join(" | ", corners.Select(Fmt));
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (string.Equals(root.name, name, StringComparison.Ordinal)) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static string GetHierarchyPath(GameObject go) => go != null ? GetHierarchyPath(go.transform) : "<null>";
        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var parts = new Stack<string>();
            Transform cur = t;
            while (cur != null) { parts.Push(cur.name); cur = cur.parent; }
            return string.Join("/", parts);
        }
        private static string PathOf(RectTransform rt) => rt != null ? GetHierarchyPath(rt.gameObject) : "<null>";
        private static string SafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unnamed";
            foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            value = value.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
            return value.Length > 80 ? value.Substring(0, 80) : value;
        }
        private static string Escape(string value) => (value ?? "").Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        private static string Fmt(Vector2 v) => $"({v.x:0.###},{v.y:0.###})";
        private static string Fmt(Vector3 v) => $"({v.x:0.###},{v.y:0.###},{v.z:0.###})";
        private static string Fmt(Rect r) => $"(x={r.x:0.###},y={r.y:0.###},w={r.width:0.###},h={r.height:0.###})";

        private readonly struct DictionaryEntryLike
        {
            public DictionaryEntryLike(object key, object value) { Key = key; Value = value; }
            public object Key { get; }
            public object Value { get; }
        }

        private struct ResourceRow
        {
            public ResourceRow(string id, string displayName, string category, float amount)
            { Id = id ?? ""; DisplayName = displayName ?? ""; Category = category ?? ""; Amount = amount; }
            public string Id;
            public string DisplayName;
            public string Category;
            public float Amount;
        }
    }
}
#endif
