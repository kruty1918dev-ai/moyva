using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    [CustomEditor(typeof(NodeBase), true)]
    public sealed class NodeBaseEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();
        private readonly Dictionary<string, UnityEditor.Editor> _nestedEditors = new Dictionary<string, UnityEditor.Editor>();
        private bool _showPorts = true;

        private void OnDisable()
        {
            foreach (var editor in _nestedEditors.Values)
            {
                if (editor != null)
                    DestroyImmediate(editor);
            }
            _nestedEditors.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as NodeBase;
            var twcNode = target as TwcModifierNode;

            if (node != null)
                DrawNodeDocumentation(node);

            var property = serializedObject.GetIterator();
            property.NextVisible(true); // skip m_Script

            while (property.NextVisible(false))
            {
                if (ShouldHideInternalGraphProperty(property.name))
                    continue;

                if (twcNode != null && property.name == "_modifierTypeName")
                    continue;

                if (twcNode != null && property.name == "_modifier")
                {
                    DrawTwcModifierInspector(twcNode);
                    continue;
                }

                DrawDocumentedProperty(node, property);

                if (property.propertyType == SerializedPropertyType.ObjectReference
                    && property.objectReferenceValue is ScriptableObject so)
                {
                    DrawNestedEditor(property.propertyPath, property.displayName, so);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool ShouldHideInternalGraphProperty(string name)
        {
            return name == "_nodeId" || name == "_editorPosition" || name == "_layerId";
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

        private static void DrawDocumentedProperty(NodeBase node, SerializedProperty property)
        {
            if (property == null)
                return;

            Type nodeType = node != null ? node.GetType() : null;
            string help = GraphNodeDocumentation.GetParameterDescription(nodeType, property.propertyPath, property.displayName);
            var content = new GUIContent(property.displayName, help);

            EditorGUILayout.PropertyField(property, content, true);

            if (!string.IsNullOrWhiteSpace(help))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.HelpBox(help, MessageType.None);
                }
            }
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

        private void DrawNestedEditor(string key, string displayName, ScriptableObject so)
        {
            bool expanded;
            _foldouts.TryGetValue(key, out expanded);

            expanded = EditorGUILayout.Foldout(expanded, "    ▶ " + displayName + ": " + so.name, true);
            _foldouts[key] = expanded;

            if (!expanded) return;

            if (!_nestedEditors.TryGetValue(key, out var editor)
                || editor == null
                || editor.target != so)
            {
                if (editor != null)
                    DestroyImmediate(editor);
                editor = CreateEditor(so);
                _nestedEditors[key] = editor;
            }

            EditorGUI.indentLevel++;
            editor.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }

        private void DrawNestedEditor(string key, ScriptableObject so)
        {
            bool expanded;
            _foldouts.TryGetValue(key, out expanded);

            expanded = EditorGUILayout.Foldout(expanded, "    ▶ Parameters", true);
            _foldouts[key] = expanded;

            if (!expanded)
                return;

            if (!_nestedEditors.TryGetValue(key, out var editor)
                || editor == null
                || editor.target != so)
            {
                if (editor != null)
                    DestroyImmediate(editor);
                editor = CreateEditor(so);
                _nestedEditors[key] = editor;
            }

            EditorGUI.indentLevel++;
            editor.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }
    }
}