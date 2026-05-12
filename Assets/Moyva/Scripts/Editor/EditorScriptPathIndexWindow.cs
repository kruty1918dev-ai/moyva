using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    internal sealed class EditorScriptPathIndexWindow : EditorWindow
    {
        private const string RootPath = "Assets/Moyva/Scripts";
        private const string EditorMarker = "/Editor/";

        private readonly struct ScriptEntry
        {
            public ScriptEntry(string assetPath, string relativePath)
            {
                AssetPath = assetPath;
                RelativePath = relativePath;
            }

            public string AssetPath { get; }
            public string RelativePath { get; }
        }

        private sealed class TreeNode
        {
            public readonly string Name;
            public readonly string PathFromRoot;
            public readonly SortedDictionary<string, TreeNode> Children = new(StringComparer.OrdinalIgnoreCase);
            public readonly List<ScriptEntry> Scripts = new();
            public bool Expanded = true;

            public TreeNode(string name, string pathFromRoot)
            {
                Name = name;
                PathFromRoot = pathFromRoot;
            }
        }

        private Vector2 _scroll;
        private string _filter = string.Empty;
        private TreeNode _root;
        private int _scriptCount;
        private int _folderCount;

        [MenuItem("Moyva/Tools/Diagnostics/Editor Script Index", priority = 408)]
        public static void Open()
        {
            GetWindow<EditorScriptPathIndexWindow>("Editor Script Index");
        }

        private void OnEnable()
        {
            RefreshIndex();
        }

        private void OnProjectChange()
        {
            RefreshIndex();
            Repaint();
        }

        private void RefreshIndex()
        {
            _root = new TreeNode("Moyva", string.Empty);
            _scriptCount = 0;
            _folderCount = 0;

            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { RootPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!assetPath.Contains(EditorMarker, StringComparison.OrdinalIgnoreCase)
                    && !assetPath.StartsWith("Assets/Moyva/Scripts/Editor/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relativePath = assetPath.StartsWith("Assets/Moyva/Scripts/", StringComparison.OrdinalIgnoreCase)
                    ? assetPath.Substring("Assets/Moyva/Scripts/".Length)
                    : assetPath;

                AddEntry(assetPath, relativePath);
                _scriptCount++;
            }

            _folderCount = CountFolders(_root) - 1;
        }

        private void AddEntry(string assetPath, string relativePath)
        {
            var parts = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            TreeNode node = _root;
            string currentPath = string.Empty;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string segment = parts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? segment : currentPath + "/" + segment;

                if (!node.Children.TryGetValue(segment, out var child))
                {
                    child = new TreeNode(segment, currentPath);
                    node.Children.Add(segment, child);
                }

                node = child;
            }

            node.Scripts.Add(new ScriptEntry(assetPath, relativePath));
        }

        private static int CountFolders(TreeNode node)
        {
            int count = 1;
            foreach (var child in node.Children.Values)
                count += CountFolders(child);
            return count;
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_root == null)
            {
                EditorGUILayout.HelpBox("No editor scripts found.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawNode(_root, 0, isRoot: true);
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshIndex();
                    Repaint();
                }

                GUILayout.Space(8);
                GUILayout.Label($"Editor scripts: {_scriptCount}  |  folders: {_folderCount}", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Filter", EditorStyles.miniLabel, GUILayout.Width(36));
                string nextFilter = GUILayout.TextField(_filter, EditorStyles.toolbarTextField, GUILayout.MinWidth(160));
                if (!string.Equals(nextFilter, _filter, StringComparison.Ordinal))
                {
                    _filter = nextFilter;
                    Repaint();
                }
            }
        }

        private void DrawNode(TreeNode node, int depth, bool isRoot = false)
        {
            if (!isRoot && !NodeMatchesFilter(node))
                return;

            if (isRoot)
            {
                EditorGUILayout.LabelField(node.Name, EditorStyles.boldLabel);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(depth * 16f);
                    node.Expanded = EditorGUILayout.Foldout(node.Expanded, node.Name, true);
                }
            }

            if (!node.Expanded && !isRoot)
                return;

            foreach (var child in node.Children.Values.OrderBy(child => child.Name, StringComparer.OrdinalIgnoreCase))
                DrawNode(child, depth + 1);

            foreach (var script in node.Scripts.OrderBy(entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase))
            {
                if (!MatchesFilter(script.RelativePath))
                    continue;

                DrawScriptEntry(script, depth + 1);
            }
        }

        private void DrawScriptEntry(ScriptEntry entry, int depth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(depth * 16f);

                if (GUILayout.Button(Path.GetFileNameWithoutExtension(entry.RelativePath), EditorStyles.linkLabel))
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);
                    Selection.activeObject = obj;
                    if (obj != null)
                        EditorGUIUtility.PingObject(obj);
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel(entry.RelativePath, EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        private bool MatchesFilter(string path)
        {
            if (string.IsNullOrWhiteSpace(_filter))
                return true;

            return path.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool NodeMatchesFilter(TreeNode node)
        {
            if (MatchesFilter(node.PathFromRoot))
                return true;

            foreach (var child in node.Children.Values)
            {
                if (NodeMatchesFilter(child))
                    return true;
            }

            foreach (var script in node.Scripts)
            {
                if (MatchesFilter(script.RelativePath))
                    return true;
            }

            return false;
        }
    }
}
