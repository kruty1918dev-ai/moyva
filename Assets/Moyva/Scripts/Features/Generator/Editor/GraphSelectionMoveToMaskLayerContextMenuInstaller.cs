#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Registers the "Move selected nodes to Mask Layer" action without requiring direct edits
    /// inside GeneratorGraphView.cs.
    ///
    /// This fixes the case where GraphSelectionMoveToMaskLayerUtility.cs exists,
    /// but the right-click context menu item is not visible.
    /// </summary>
    [InitializeOnLoad]
    internal static class GraphSelectionMoveToMaskLayerContextMenuInstaller
    {
        private const string GeneratorGraphViewTypeName = "Kruty1918.Moyva.GraphSystem.Editor.GeneratorGraphView";
        private const string MenuPath = "Moyva/Перенести виділене на окремий Mask Layer";

        private static readonly HashSet<int> RegisteredGraphViews = new HashSet<int>();
        private static double _nextScanTime;

        static GraphSelectionMoveToMaskLayerContextMenuInstaller()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup < _nextScanTime)
                return;

            _nextScanTime = EditorApplication.timeSinceStartup + 0.75d;
            RegisterOpenGraphViews();
        }

        private static void RegisterOpenGraphViews()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                if (window == null || window.rootVisualElement == null)
                    continue;

                RegisterGraphViewsRecursive(window.rootVisualElement);
            }
        }

        private static void RegisterGraphViewsRecursive(VisualElement element)
        {
            if (element == null)
                return;

            if (IsGeneratorGraphView(element))
                RegisterContextMenu(element);

            for (int i = 0; i < element.childCount; i++)
                RegisterGraphViewsRecursive(element[i]);
        }

        private static bool IsGeneratorGraphView(VisualElement element)
        {
            if (element == null)
                return false;

            Type type = element.GetType();
            return string.Equals(type.FullName, GeneratorGraphViewTypeName, StringComparison.Ordinal)
                   || string.Equals(type.Name, "GeneratorGraphView", StringComparison.Ordinal);
        }

        private static void RegisterContextMenu(VisualElement graphViewElement)
        {
            int id = graphViewElement.GetHashCode();
            if (RegisteredGraphViews.Contains(id))
                return;

            RegisteredGraphViews.Add(id);

            // TrickleDown catches the event even when a NodeView/Port consumes it later.
            graphViewElement.RegisterCallback<ContextualMenuPopulateEvent>(
                OnContextualMenuPopulate,
                TrickleDown.TrickleDown);
        }

        private static void OnContextualMenuPopulate(ContextualMenuPopulateEvent evt)
        {
            var target = evt.target as VisualElement;
            var graphViewElement = FindGeneratorGraphViewAncestor(target);
            if (graphViewElement == null)
                return;

            evt.menu.AppendSeparator();
            evt.menu.AppendAction(
                MenuPath,
                _ => ExecuteMoveToMaskLayer(graphViewElement),
                _ => CanExecuteMoveToMaskLayer(graphViewElement)
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);
        }

        private static VisualElement FindGeneratorGraphViewAncestor(VisualElement element)
        {
            while (element != null)
            {
                if (IsGeneratorGraphView(element))
                    return element;

                element = element.parent;
            }

            return null;
        }

        private static bool CanExecuteMoveToMaskLayer(VisualElement graphViewElement)
        {
            if (graphViewElement == null)
                return false;

            if (ReadBoolMember(graphViewElement, "_isReadOnly", false))
                return false;

            if (ReadGraphAsset(graphViewElement) == null)
                return false;

            return ReadSelectedNodeBases(graphViewElement).Count > 0;
        }

        private static void ExecuteMoveToMaskLayer(VisualElement graphViewElement)
        {
            if (graphViewElement == null)
                return;

            var graph = ReadGraphAsset(graphViewElement);
            if (graph == null)
            {
                EditorUtility.DisplayDialog("Move to Mask Layer", "GraphAsset не знайдено у GeneratorGraphView.", "OK");
                return;
            }

            if (ReadBoolMember(graphViewElement, "_isReadOnly", false))
            {
                EditorUtility.DisplayDialog("Move to Mask Layer", "Graph view зараз у read-only режимі.", "OK");
                return;
            }

            var selectedNodes = ReadSelectedNodeBases(graphViewElement);
            var result = GraphSelectionMoveToMaskLayerUtility.MoveSelectedNodesToNewMaskLayer(graph, selectedNodes);

            if (!result.Success)
            {
                EditorUtility.DisplayDialog("Move to Mask Layer", result.Message, "OK");
                return;
            }

            EditorUtility.SetDirty(graph);

            TryInvokeInstanceMethod(graphViewElement, "ReconcileAdaptiveAddNodeTypes");
            TryInvokeInstanceMethod(graphViewElement, "ScheduleGraphViewRefresh");
            TryInvokeInstanceMethod(graphViewElement, "RefreshView");
            TryInvokeInstanceMethod(graphViewElement, "Reload");
            TryInvokeInstanceMethod(graphViewElement, "Rebuild");

            var window = graphViewElement.panel != null
                ? graphViewElement.panel.visualTree?.GetFirstAncestorOfType<VisualElement>()
                : null;

            if (EditorWindow.focusedWindow != null)
                EditorWindow.focusedWindow.Repaint();

            Debug.Log("[Graph] " + result.Message);
        }

        private static GraphAsset ReadGraphAsset(object graphViewElement)
        {
            object value;
            if (TryReadMember(graphViewElement, "_graphAsset", out value))
                return value as GraphAsset;

            if (TryReadMember(graphViewElement, "GraphAsset", out value))
                return value as GraphAsset;

            if (TryReadMember(graphViewElement, "graphAsset", out value))
                return value as GraphAsset;

            return null;
        }

        private static IReadOnlyList<NodeBase> ReadSelectedNodeBases(VisualElement graphViewElement)
        {
            var result = new List<NodeBase>();

            var graphView = graphViewElement as GraphView;
            if (graphView != null)
            {
                foreach (var item in graphView.selection)
                {
                    var node = ReadNodeBaseFromGraphElement(item);
                    if (node != null && !result.Contains(node))
                        result.Add(node);
                }
            }

            if (result.Count > 0)
                return result;

            object selectionObject;
            if (TryReadMember(graphViewElement, "selection", out selectionObject)
                && selectionObject is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var node = ReadNodeBaseFromGraphElement(item);
                    if (node != null && !result.Contains(node))
                        result.Add(node);
                }
            }

            return result;
        }

        private static NodeBase ReadNodeBaseFromGraphElement(object element)
        {
            if (element == null)
                return null;

            object value;
            foreach (string name in new[]
                     {
                         "Node",
                         "NodeData",
                         "NodeBase",
                         "RuntimeNode",
                         "Data",
                         "_node",
                         "_nodeData",
                         "_nodeBase",
                         "_runtimeNode"
                     })
            {
                if (TryReadMember(element, name, out value) && value is NodeBase node)
                    return node;
            }

            return null;
        }

        private static bool TryReadMember(object target, string memberName, out object value)
        {
            value = null;
            if (target == null || string.IsNullOrEmpty(memberName))
                return false;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                value = field.GetValue(target);
                return true;
            }

            var property = type.GetProperty(memberName, flags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                value = property.GetValue(target);
                return true;
            }

            return false;
        }

        private static bool ReadBoolMember(object target, string memberName, bool fallback)
        {
            object value;
            if (!TryReadMember(target, memberName, out value))
                return fallback;

            return value is bool boolValue ? boolValue : fallback;
        }

        private static bool TryInvokeInstanceMethod(object target, string methodName)
        {
            if (target == null || string.IsNullOrEmpty(methodName))
                return false;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = target.GetType().GetMethod(methodName, flags, null, Type.EmptyTypes, null);
            if (method == null)
                return false;

            try
            {
                method.Invoke(target, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif