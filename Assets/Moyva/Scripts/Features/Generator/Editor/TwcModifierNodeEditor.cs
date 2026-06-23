using System;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Інспектор вузла-обгортки TileWorldCreator-модифікатора. Малює параметри
    /// вкладеного BlueprintModifier і додає українську документацію для ноди та її параметрів.
    /// </summary>
    [CustomEditor(typeof(TwcModifierNode))]
    public sealed class TwcModifierNodeEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor _modifierEditor;
        private UnityEngine.Object _cachedModifier;

        public override VisualElement CreateInspectorGUI()
        {
            var node = (TwcModifierNode)target;
            var root = new VisualElement();

            if (node == null)
                return root;

            Type modifierType = TwcModifierCatalog.ResolveType(node.ModifierTypeName);
            root.Add(new HelpBox(
                GraphNodeDocumentation.BuildTwcInspectorHeader(node.Title, modifierType, node.IsGenerator),
                HelpBoxMessageType.Info));
            root.Add(BuildPortHelpElement(node));
            root.Add(new Label(node.IsGenerator ? "Тип: Генератор" : "Тип: Модифікатор"));

            if (!node.TryRestoreModifierInEditor())
            {
                root.Add(new HelpBox(
                    "TWC-модифікатор '" + node.ModifierTypeName + "' не ініціалізовано.",
                    HelpBoxMessageType.Warning));
                return root;
            }

            root.Add(BuildModifierDocumentationElement(node));

            VisualElement nativeInspector = null;
            try
            {
                nativeInspector = node.CreateModifierInspectorElement(new Vector2Int(64, 64));
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[TwcModifierNodeEditor] Failed to build native TWC inspector for " + node.Title + ": " + ex.Message);
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
            Type modifierType = node != null ? TwcModifierCatalog.ResolveType(node.ModifierTypeName) : null;

            if (node != null)
            {
                EditorGUILayout.HelpBox(
                    GraphNodeDocumentation.BuildTwcInspectorHeader(node.Title, modifierType, node.IsGenerator),
                    MessageType.Info);
                EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildPortsInspectorText(node), MessageType.None);
            }

            if (modifier == null)
            {
                EditorGUILayout.HelpBox(
                    node == null
                        ? "TWC-модифікатор не ініціалізовано."
                        : "TWC-модифікатор '" + node.ModifierTypeName + "' не ініціалізовано.",
                    MessageType.Warning);

                if (node != null && GUILayout.Button("Відновити модифікатор"))
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

            DrawTwcParameterDocumentation(node);

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
                MarkDirty(node);
        }

        private static VisualElement BuildPortHelpElement(TwcModifierNode node)
        {
            return new HelpBox(GraphNodeDocumentation.BuildPortsInspectorText(node), HelpBoxMessageType.None);
        }

        private static VisualElement BuildModifierDocumentationElement(TwcModifierNode node)
        {
            var foldout = new Foldout { text = "Документація параметрів TWC", value = true };
            if (node == null || node.ModifierAsset == null)
            {
                foldout.Add(new HelpBox("Параметри недоступні, бо TWC-модифікатор ще не ініціалізований.", HelpBoxMessageType.Warning));
                return foldout;
            }

            var serializedModifier = new SerializedObject(node.ModifierAsset);
            serializedModifier.Update();
            var property = serializedModifier.GetIterator();
            bool enterChildren = true;
            bool any = false;
            Type modifierType = TwcModifierCatalog.ResolveType(node.ModifierTypeName);

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath.StartsWith("m_", StringComparison.Ordinal)
                    || property.propertyPath == "asset"
                    || property.propertyPath == "isEnabled")
                    continue;

                any = true;
                string help = GraphNodeDocumentation.GetTwcParameterDescription(
                    node.Title,
                    modifierType,
                    property.propertyPath,
                    property.displayName);
                foldout.Add(new Label(property.displayName) { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                foldout.Add(new HelpBox(help, HelpBoxMessageType.None));
            }

            if (!any)
                foldout.Add(new HelpBox("У цього TWC-модифікатора немає відкритих serialized параметрів або вони малюються native інспектором.", HelpBoxMessageType.None));

            return foldout;
        }

        private static void DrawTwcParameterDocumentation(TwcModifierNode node)
        {
            if (node == null || node.ModifierAsset == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Документація параметрів TWC", EditorStyles.boldLabel);
                var serializedModifier = new SerializedObject(node.ModifierAsset);
                serializedModifier.Update();
                var property = serializedModifier.GetIterator();
                bool enterChildren = true;
                bool any = false;
                Type modifierType = TwcModifierCatalog.ResolveType(node.ModifierTypeName);

                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyPath.StartsWith("m_", StringComparison.Ordinal)
                        || property.propertyPath == "asset"
                        || property.propertyPath == "isEnabled")
                        continue;

                    any = true;
                    string help = GraphNodeDocumentation.GetTwcParameterDescription(
                        node.Title,
                        modifierType,
                        property.propertyPath,
                        property.displayName);
                    EditorGUILayout.LabelField(property.displayName, EditorStyles.miniBoldLabel);
                    EditorGUILayout.HelpBox(help, MessageType.None);
                }

                if (!any)
                    EditorGUILayout.HelpBox("У цього TWC-модифікатора немає відкритих serialized параметрів або вони малюються native інспектором.", MessageType.None);
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
            Type modifierType = TwcModifierCatalog.ResolveType(node.ModifierTypeName);
            while (property.Next(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath.StartsWith("m_", StringComparison.Ordinal)
                    || property.propertyPath == "asset"
                    || property.propertyPath == "isEnabled")
                    continue;

                string help = GraphNodeDocumentation.GetTwcParameterDescription(
                    node.Title,
                    modifierType,
                    property.propertyPath,
                    property.displayName);
                EditorGUILayout.PropertyField(property, new GUIContent(property.displayName, help), true);
                EditorGUILayout.HelpBox(help, MessageType.None);
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