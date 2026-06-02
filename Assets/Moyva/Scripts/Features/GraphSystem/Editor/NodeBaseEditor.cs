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
        private readonly Dictionary<string, bool> _foldouts = new();
        private readonly Dictionary<string, UnityEditor.Editor> _nestedEditors = new();

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

            var twcNode = target as TwcModifierNode;

            if (target is NodeBase node)
            {
                var info = System.Attribute.GetCustomAttribute(node.GetType(), typeof(NodeInfoAttribute)) as NodeInfoAttribute;
                if (info != null && !string.IsNullOrWhiteSpace(info.Description))
                {
                    EditorGUILayout.HelpBox(info.Description, MessageType.Info);
                    EditorGUILayout.Space(4);
                }
            }

            var property = serializedObject.GetIterator();
            property.NextVisible(true); // skip m_Script

            while (property.NextVisible(false))
            {
                if (property.name is "_nodeId" or "_editorPosition")
                    continue;

                if (twcNode != null && property.name is "_modifierTypeName")
                    continue;

                if (twcNode != null && property.name is "_modifier")
                {
                    DrawTwcModifierInspector(twcNode);
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);

                if (property.propertyType == SerializedPropertyType.ObjectReference
                    && property.objectReferenceValue is ScriptableObject so)
                {
                    DrawNestedEditor(property.propertyPath, property.displayName, so);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTwcModifierInspector(TwcModifierNode node)
        {
            var modifier = node?.ModifierAsset;

            EditorGUILayout.LabelField("TileWorldCreator", EditorStyles.boldLabel);

            if (modifier == null)
            {
                EditorGUILayout.HelpBox(
                    $"TWC-модифікатор '{node.ModifierTypeName}' не ініціалізовано.",
                    MessageType.Warning);

                if (GUILayout.Button("Відновити модифікатор"))
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

            DrawNestedEditor($"twc:{node.GetInstanceID()}", modifier);
        }

        private void DrawNestedEditor(string key, string displayName, ScriptableObject so)
        {
            _foldouts.TryGetValue(key, out var expanded);

            expanded = EditorGUILayout.Foldout(expanded, $"    \u25B6 {displayName}: {so.name}", true);
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
            _foldouts.TryGetValue(key, out var expanded);

            expanded = EditorGUILayout.Foldout(expanded, "    \u25B6 Parameters", true);
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
