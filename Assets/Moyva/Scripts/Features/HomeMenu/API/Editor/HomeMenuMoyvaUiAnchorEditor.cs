using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kruty1918.Moyva.HomeMenu.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.Editor
{
    [CustomEditor(typeof(HomeMenuMoyvaUiAnchor), true)]
    internal sealed class HomeMenuMoyvaUiAnchorEditor : UnityEditor.Editor
    {
        private const string AutoSyncKey = "Moyva.HomeMenu.UnityHTML.EditorAutoSyncHierarchyToHtml";
        private const double HierarchyDebounceSeconds = 0.35d;

        private static readonly HashSet<int> DirtyAnchorIds = new();
        private static double _nextHierarchySyncTime;
        private static bool _queuedHierarchySync;
        private static bool _syncInProgress;

        [InitializeOnLoadMethod]
        private static void RegisterEditorHooks()
        {
            EditorApplication.hierarchyChanged -= HandleHierarchyChanged;
            EditorApplication.hierarchyChanged += HandleHierarchyChanged;
            EditorApplication.update -= FlushQueuedHierarchySync;
            EditorApplication.update += FlushQueuedHierarchySync;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("UnityHTML authoring sync is editor-only and disabled in Play Mode.", MessageType.Info);
                return;
            }

            var anchor = (HomeMenuMoyvaUiAnchor)target;
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("UnityHTML Authoring", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("HTML -> Preview"))
                    RefreshPreview(anchor);

                if (GUILayout.Button("Hierarchy -> HTML"))
                    WriteHierarchyToHtml(anchor);
            }

            var autoSync = EditorPrefs.GetBool(AutoSyncKey, false);
            var nextAutoSync = EditorGUILayout.ToggleLeft("Auto write selected preview hierarchy to HTML", autoSync);
            if (nextAutoSync != autoSync)
                EditorPrefs.SetBool(AutoSyncKey, nextAutoSync);

            EditorGUILayout.HelpBox(
                "HTML -> Preview remounts the TextAsset into the edit-mode hierarchy. Hierarchy -> HTML serializes the current mount root back into the HTML file. Auto sync only watches selected anchors with live preview enabled.",
                MessageType.None);
        }

        private static void RefreshPreview(HomeMenuMoyvaUiAnchor anchor)
        {
            if (anchor == null || Application.isPlaying)
                return;

            _syncInProgress = true;
            try
            {
                anchor.EditorRefreshPreview();
                EditorSceneManager.MarkSceneDirty(anchor.gameObject.scene);
            }
            finally
            {
                _syncInProgress = false;
            }
        }

        private static void WriteHierarchyToHtml(HomeMenuMoyvaUiAnchor anchor)
        {
            if (anchor == null || Application.isPlaying)
                return;

            if (!anchor.TryGetEditorAuthoringTargets(out var mountRoot, out var htmlAsset, out _))
            {
                Debug.LogWarning("[HomeMenuUnityHTML] Cannot write hierarchy to HTML: mount root or HTML asset is missing.", anchor);
                return;
            }

            var path = AssetDatabase.GetAssetPath(htmlAsset);
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogWarning("[HomeMenuUnityHTML] Cannot write hierarchy to HTML: HTML asset path is missing.", anchor);
                return;
            }

            var absolutePath = Path.GetFullPath(path);
            var html = HomeMenuHtmlHierarchySerializer.Serialize(mountRoot);

            _syncInProgress = true;
            try
            {
                File.WriteAllText(absolutePath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                EditorUtility.SetDirty(htmlAsset);
                EditorSceneManager.MarkSceneDirty(anchor.gameObject.scene);
            }
            finally
            {
                _syncInProgress = false;
            }
        }

        private static void HandleHierarchyChanged()
        {
            if (_syncInProgress || Application.isPlaying || !EditorPrefs.GetBool(AutoSyncKey, false))
                return;

            var active = Selection.activeGameObject;
            if (active == null)
                return;

            var anchor = active.GetComponentInParent<HomeMenuMoyvaUiAnchor>();
            if (anchor == null || !anchor.EditorLivePreview)
                return;

            DirtyAnchorIds.Add(anchor.GetInstanceID());
            _nextHierarchySyncTime = EditorApplication.timeSinceStartup + HierarchyDebounceSeconds;
            _queuedHierarchySync = true;
        }

        private static void FlushQueuedHierarchySync()
        {
            if (!_queuedHierarchySync || Application.isPlaying)
                return;
            if (EditorApplication.timeSinceStartup < _nextHierarchySyncTime)
                return;

            _queuedHierarchySync = false;
            foreach (var id in DirtyAnchorIds)
            {
#pragma warning disable CS0618 // EntityIdToObject is not available in all supported Unity editor versions.
                if (EditorUtility.InstanceIDToObject(id) is HomeMenuMoyvaUiAnchor anchor &&
#pragma warning restore CS0618
                    anchor != null &&
                    anchor.EditorLivePreview)
                    WriteHierarchyToHtml(anchor);
            }

            DirtyAnchorIds.Clear();
        }
    }

    internal sealed class HomeMenuHtmlAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (Application.isPlaying)
                return;

            var changed = new HashSet<string>(StringComparer.Ordinal);
            AddAll(changed, importedAssets);
            AddAll(changed, movedAssets);
            AddAll(changed, deletedAssets);
            AddAll(changed, movedFromAssetPaths);
            if (changed.Count == 0)
                return;

            var anchors = UnityEngine.Object.FindObjectsByType<HomeMenuMoyvaUiAnchor>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (var i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                if (anchor == null || !anchor.EditorLivePreview)
                    continue;

                if (!anchor.TryGetEditorAuthoringTargets(out _, out var htmlAsset, out var cssAsset))
                    continue;

                var htmlPath = AssetDatabase.GetAssetPath(htmlAsset);
                var cssPath = cssAsset != null ? AssetDatabase.GetAssetPath(cssAsset) : string.Empty;
                if (changed.Contains(htmlPath) || (!string.IsNullOrWhiteSpace(cssPath) && changed.Contains(cssPath)))
                    anchor.EditorRefreshPreview();
            }
        }

        private static void AddAll(HashSet<string> target, string[] values)
        {
            if (values == null)
                return;

            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    target.Add(values[i]);
            }
        }
    }

    internal static class HomeMenuHtmlHierarchySerializer
    {
        public static string Serialize(RectTransform mountRoot)
        {
            var sb = new StringBuilder(4096);
            for (var i = 0; i < mountRoot.childCount; i++)
                AppendTransform(sb, mountRoot.GetChild(i), 0);

            return sb.ToString().TrimEnd() + Environment.NewLine;
        }

        private static void AppendTransform(StringBuilder sb, Transform transform, int depth)
        {
            if (transform == null)
                return;

            var text = transform.GetComponent<TMP_Text>();
            var selectable = transform.GetComponent<Selectable>();
            var tag = selectable is Button ? "button" : text != null ? "text" : "view";
            var className = NormalizeClassName(transform.name);
            Indent(sb, depth).Append('<').Append(tag);
            if (!string.IsNullOrWhiteSpace(className))
                sb.Append(" className=\"").Append(EscapeAttribute(className)).Append('"');
            sb.Append('>');

            if (text != null)
            {
                sb.Append(EscapeText(text.text));
                sb.Append("</").Append(tag).AppendLine(">");
                return;
            }

            if (transform.childCount == 0)
            {
                sb.Append("</").Append(tag).AppendLine(">");
                return;
            }

            sb.AppendLine();
            for (var i = 0; i < transform.childCount; i++)
                AppendTransform(sb, transform.GetChild(i), depth + 1);
            Indent(sb, depth).Append("</").Append(tag).AppendLine(">");
        }

        private static string NormalizeClassName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return string.Empty;

            var builder = new StringBuilder(objectName.Length);
            var previousWasDash = false;
            for (var i = 0; i < objectName.Length; i++)
            {
                var c = objectName[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToLowerInvariant(c));
                    previousWasDash = false;
                }
                else if (!previousWasDash)
                {
                    builder.Append('-');
                    previousWasDash = true;
                }
            }

            return builder.ToString().Trim('-');
        }

        private static StringBuilder Indent(StringBuilder sb, int depth)
        {
            for (var i = 0; i < depth; i++)
                sb.Append("  ");
            return sb;
        }

        private static string EscapeText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private static string EscapeAttribute(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return EscapeText(value).Replace("\"", "&quot;");
        }
    }
}
