using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    [CustomEditor(typeof(NodeBase), true)]
    public sealed class NodeBaseEditor : OdinEditor
    {
        private readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();
        private readonly Dictionary<string, PropertyTree> _nestedTrees = new Dictionary<string, PropertyTree>();
        private readonly Dictionary<string, ScriptableObject> _nestedTreeTargets = new Dictionary<string, ScriptableObject>();
        private bool _showPorts = true;

        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (var tree in _nestedTrees.Values)
            {
                if (tree is IDisposable disposable)
                    disposable.Dispose();
            }
            _nestedTrees.Clear();
            _nestedTreeTargets.Clear();
        }

        public override void OnInspectorGUI()
        {
            var node = target as NodeBase;
            var twcNode = target as TwcModifierNode;

            if (node != null)
                DrawNodeDocumentation(node);

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool odinChanged = EditorGUI.EndChangeCheck();

            if (twcNode != null)
                DrawTwcModifierInspector(twcNode);

            if (odinChanged && target != null)
                EditorUtility.SetDirty(target);
        }

        private void DrawNodeDocumentation(NodeBase node)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Документація ноди", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildInspectorHeader(node), MessageType.Info);

                _showPorts = EditorGUILayout.Foldout(_showPorts, "Порти / типи даних", true);
                if (_showPorts)
                {
                    EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildPortsInspectorText(node), MessageType.None);
                }
            }

            EditorGUILayout.Space(4);
        }

        private void DrawTwcModifierInspector(TwcModifierNode node)
        {
            var modifier = node?.ModifierAsset;

            EditorGUILayout.LabelField("TileWorldCreator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                GraphNodeDocumentation.BuildTwcInspectorHeader(node?.Title, node != null ? TwcModifierCatalog.ResolveType(node.ModifierTypeName) : null, node != null && node.IsGenerator),
                MessageType.Info);

            if (modifier == null)
            {
                EditorGUILayout.HelpBox(
                    node == null
                        ? "TWC-модифікатор не ініціалізовано."
                        : $"TWC-модифікатор '{node.ModifierTypeName}' не ініціалізовано.",
                    MessageType.Warning);

                if (node != null && GUILayout.Button("Відновити модифікатор"))
                {
                    Undo.RecordObject(node, "Restore TWC modifier");
                    if (node.TryRestoreModifierInEditor())
                    {
                        EditorUtility.SetDirty(node);
                        GUI.changed = true;
                        Repaint();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Не вдалося відновити модифікатор за TypeName.", MessageType.Error);
                    }
                }

                return;
            }

            EditorGUILayout.LabelField(
                node.IsGenerator ? "Тип: Генератор" : "Тип: Модифікатор",
                EditorStyles.miniLabel);

            DrawTwcParameterDocumentation(node, modifier);
            DrawNestedEditor($"twc:{node.GetInstanceID()}", modifier);
        }

        private static void DrawTwcParameterDocumentation(TwcModifierNode node, ScriptableObject modifier)
        {
            if (modifier == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Параметри TWC-модифікатора", EditorStyles.boldLabel);
                var serializedModifier = new SerializedObject(modifier);
                serializedModifier.Update();

                var property = serializedModifier.GetIterator();
                bool enterChildren = true;
                bool any = false;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyPath.StartsWith("m_", StringComparison.Ordinal)
                        || property.propertyPath == "asset"
                        || property.propertyPath == "isEnabled")
                        continue;

                    any = true;
                    string help = GraphNodeDocumentation.GetTwcParameterDescription(
                        node != null ? node.Title : null,
                        node != null ? TwcModifierCatalog.ResolveType(node.ModifierTypeName) : null,
                        property.propertyPath,
                        property.displayName);

                    EditorGUILayout.LabelField(property.displayName, EditorStyles.miniBoldLabel);
                    EditorGUILayout.HelpBox(help, MessageType.None);
                }

                if (!any)
                    EditorGUILayout.HelpBox("У цього TWC-модифікатора немає відкритих serialized параметрів або вони малюються native інспектором.", MessageType.None);
            }
        }

        private void DrawNestedEditor(string key, ScriptableObject so)
        {
            bool expanded;
            _foldouts.TryGetValue(key, out expanded);

            expanded = EditorGUILayout.Foldout(expanded, "    ▶ Parameters", true);
            _foldouts[key] = expanded;

            if (!expanded)
                return;

            if (!_nestedTrees.TryGetValue(key, out var tree)
                || tree == null
                || !_nestedTreeTargets.TryGetValue(key, out var target)
                || target != so)
            {
                if (tree is IDisposable disposable)
                    disposable.Dispose();

                tree = PropertyTree.Create(new SerializedObject(so));
                tree.DrawMonoScriptObjectField = false;
                _nestedTrees[key] = tree;
                _nestedTreeTargets[key] = so;
            }

            EditorGUI.indentLevel++;
            tree.UpdateTree();
            EditorGUI.BeginChangeCheck();
            tree.Draw(false);
            bool changed = EditorGUI.EndChangeCheck();
            tree.ApplyChanges();
            if (changed)
                EditorUtility.SetDirty(so);
            EditorGUI.indentLevel--;
        }
    }
}
