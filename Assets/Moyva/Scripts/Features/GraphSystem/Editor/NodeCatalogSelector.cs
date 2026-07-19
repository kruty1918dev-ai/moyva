using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Odin-вікно вибору вузлів. Каталог є єдиним джерелом назв, описів і контрактів.
    /// </summary>
    internal sealed class NodeCatalogSelector : OdinMenuEditorWindow
    {
        private GraphAsset _graph;
        private Action<NodeCatalogEntry> _selectionConfirmed;
        internal static void Open(
            GraphAsset graph,
            Vector2 screenPosition,
            Action<NodeCatalogEntry> selectionConfirmed)
        {
            var window = CreateInstance<NodeCatalogSelector>();
            window.titleContent = new GUIContent("Create Node");
            window.minSize = new Vector2(760f, 640f);
            window.position = new Rect(screenPosition.x, screenPosition.y, 760f, 640f);
            window._graph = graph;
            window._selectionConfirmed = selectionConfirmed;
            window.ShowUtility();
            window.Focus();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree
            {
                Config =
                {
                    DrawSearchToolbar = true,
                    AutoHandleKeyboardNavigation = true,
                    ConfirmSelectionOnDoubleClick = true,
                }
            };

            foreach (var entry in GraphNodeCatalog.Entries)
            {
                var descriptor = entry?.Descriptor;
                if (descriptor == null
                    || descriptor.Lifecycle != NodeLifecycle.Active
                    || !string.IsNullOrEmpty(descriptor.UnavailableReason))
                {
                    continue;
                }

                var item = new NodeCatalogMenuItem(
                    entry,
                    !IsUnavailableUniqueNode(descriptor.NodeType));
                tree.Add(entry.Path, item);
            }

            if (tree.Selection != null)
                tree.Selection.SupportsMultiSelect = false;

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            DrawToolbar();
            GUILayout.Space(6f);
            DrawSelectedNodeDescription(MenuTree?.Selection?.SelectedValue as NodeCatalogMenuItem);
            GUILayout.Space(6f);
            base.OnBeginDrawEditors();
        }

        private void DrawToolbar()
        {
            var selectedItem = MenuTree?.Selection?.SelectedValue as NodeCatalogMenuItem;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(!CanCreate(selectedItem)))
                {
                    if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(96f)))
                    {
                        ConfirmSelection(selectedItem);
                    }
                }
            }
        }

        private void ConfirmSelection(NodeCatalogMenuItem selectedItem)
        {
            if (!CanCreate(selectedItem))
                return;

            _selectionConfirmed?.Invoke(selectedItem.Entry);
            Close();
            GUIUtility.ExitGUI();
        }

        private static bool CanCreate(NodeCatalogMenuItem item)
        {
            return item?.Entry?.Descriptor?.IsCreatable == true && item.IsEnabled;
        }

        private void DrawSelectedNodeDescription(NodeCatalogMenuItem item)
        {
            var entry = item?.Entry;
            var descriptor = entry?.Descriptor;

            GUILayout.Space(6f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (descriptor == null)
            {
                EditorGUILayout.LabelField(
                    "Оберіть вузол, щоб побачити його призначення, порти та обмеження.",
                    EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField(descriptor.Title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"{descriptor.Category}  •  {descriptor.StableId}",
                EditorStyles.miniLabel);
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Призначення", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(descriptor.Description)
                    ? "Опис відсутній."
                    : descriptor.Description,
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(4f);
            DrawPorts("Входи", descriptor.Inputs);
            DrawPorts("Виходи", descriptor.Outputs);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Обмеження", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                BuildConstraints(item),
                EditorStyles.wordWrappedLabel);

            if (item != null && !item.IsEnabled)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.HelpBox(
                    "Цей вузол уже присутній у графі як унікальний, тому створення тимчасово недоступне.",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawPorts(
            string heading,
            IReadOnlyList<PortDefinition> ports)
        {
            EditorGUILayout.LabelField(heading, EditorStyles.boldLabel);
            if (ports == null || ports.Count == 0)
            {
                EditorGUILayout.LabelField("— немає", EditorStyles.miniLabel);
                return;
            }

            for (int i = 0; i < ports.Count; i++)
            {
                var port = ports[i];
                string requirement = port.Direction == PortDirection.Input
                    ? port.IsRequired ? "обов'язковий" : "необов'язковий"
                    : "результат";
                EditorGUILayout.LabelField(
                    $"• {port.Name}: {FriendlyTypeName(port.ValueType)} ({requirement})",
                    EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static string BuildConstraints(NodeCatalogMenuItem item)
        {
            var descriptor = item?.Entry?.Descriptor;
            if (descriptor == null)
                return "Спеціальні обмеження відсутні.";

            if (!string.IsNullOrEmpty(descriptor.UnavailableReason))
                return descriptor.UnavailableReason;

            var parts = new List<string>();
            if ((descriptor.Capabilities & NodeCapabilities.Deterministic) != 0)
                parts.Add("детермінований");
            if ((descriptor.Capabilities & NodeCapabilities.RectangularMaps) != 0)
                parts.Add("підтримує прямокутні карти");
            if ((descriptor.Capabilities & NodeCapabilities.LogicalPreview) != 0)
                parts.Add("логічне прев'ю 1 px = 1 tile");
            if ((descriptor.Capabilities & NodeCapabilities.ExternalDependency) != 0)
                parts.Add("залежить від TileWorldCreator");
            if (item != null && !item.IsEnabled)
                parts.Add("недоступний, бо вже існує унікальний вузол цього типу");

            return parts.Count == 0
                ? "Спеціальні обмеження відсутні."
                : string.Join("; ", parts) + ".";
        }

        private bool IsUnavailableUniqueNode(Type nodeType)
        {
            if (_graph?.Nodes == null
                || nodeType == null
                || !Attribute.IsDefined(nodeType, typeof(UniqueNodeAttribute)))
            {
                return false;
            }

            return _graph.Nodes.Any(node =>
                node != null && node.GetType() == nodeType);
        }

        private static string FriendlyTypeName(Type type)
        {
            if (type == typeof(float[,])) return "Float Map";
            if (type == typeof(bool[,])) return "Mask";
            if (type == typeof(int[,])) return "Integer Map";
            if (type == typeof(string[,])) return "Tile Map";
            if (type == typeof(object)) return "Any";
            return type?.Name ?? "Unknown";
        }

        [Serializable]
        private sealed class NodeCatalogMenuItem
        {
            internal NodeCatalogMenuItem(NodeCatalogEntry entry, bool isEnabled)
            {
                Entry = entry;
                IsEnabled = isEnabled;
            }

            internal NodeCatalogEntry Entry { get; }
            internal bool IsEnabled { get; }

            public override string ToString() =>
                Entry?.Descriptor?.Title ?? "<Invalid Node>";
        }
    }
}
