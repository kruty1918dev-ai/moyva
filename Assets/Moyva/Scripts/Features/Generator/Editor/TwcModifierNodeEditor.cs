using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Інспектор вузла-обгортки TileWorldCreator-модифікатора. Малює параметри
    /// вкладеного <see cref="GiantGrey.TileWorldCreator.BlueprintModifier"/>,
    /// щоб їх можна було редагувати прямо з графа.
    /// </summary>
    [CustomEditor(typeof(TwcModifierNode))]
    public sealed class TwcModifierNodeEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor _modifierEditor;
        private Object _cachedModifier;

        public override VisualElement CreateInspectorGUI()
        {
            var node = (TwcModifierNode)target;
            var root = new VisualElement();

            if (node == null)
                return root;

            root.Add(new Label(node.IsGenerator ? "Тип: Генератор" : "Тип: Модифікатор"));

            if (!node.TryRestoreModifierInEditor())
            {
                root.Add(new HelpBox(
                    $"TWC-модифікатор '{node.ModifierTypeName}' не ініціалізовано.",
                    HelpBoxMessageType.Warning));
                return root;
            }

            VisualElement nativeInspector = null;
            try
            {
                nativeInspector = node.CreateModifierInspectorElement(new Vector2Int(64, 64));
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TwcModifierNodeEditor] Failed to build native TWC inspector for {node.Title}: {ex.Message}");
            }

            if (nativeInspector != null && nativeInspector.childCount > 0)
            {
                nativeInspector.RegisterCallback<SerializedPropertyChangeEvent>(_ => MarkDirty(node));
                root.Add(nativeInspector);
            }
            else
            {
                root.Add(new IMGUIContainer(() => DrawSerializedFallback(node)));
            }

            return root;
        }

        public override void OnInspectorGUI()
        {
            var node = (TwcModifierNode)target;
            if (node != null)
                node.TryRestoreModifierInEditor();

            var modifier = node != null ? node.ModifierAsset : null;

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
            EditorGUILayout.Space(2);

            if (_modifierEditor == null || _cachedModifier != modifier)
            {
                if (_modifierEditor != null)
                    DestroyImmediate(_modifierEditor);
                _modifierEditor = CreateEditor(modifier);
                _cachedModifier = modifier;
            }

            EditorGUI.BeginChangeCheck();
            _modifierEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty(node);
            }
        }

        private static void DrawSerializedFallback(TwcModifierNode node)
        {
            if (node == null || node.ModifierAsset == null)
                return;

            var serializedModifier = new SerializedObject(node.ModifierAsset);
            serializedModifier.Update();

            EditorGUI.BeginChangeCheck();
            var property = serializedModifier.GetIterator();
            bool enterChildren = true;
            while (property.Next(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath.StartsWith("m_", System.StringComparison.Ordinal)
                    || property.propertyPath is "asset" or "isEnabled")
                    continue;

                EditorGUILayout.PropertyField(property, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedModifier.ApplyModifiedProperties();
                MarkDirty(node);
            }
            else
            {
                serializedModifier.ApplyModifiedProperties();
            }
        }

        private static void MarkDirty(TwcModifierNode node)
        {
            if (node == null)
                return;

            if (node.ModifierAsset != null)
                EditorUtility.SetDirty(node.ModifierAsset);
            EditorUtility.SetDirty(node);
        }

        private void OnDisable()
        {
            if (_modifierEditor != null)
                DestroyImmediate(_modifierEditor);
        }
    }
}
