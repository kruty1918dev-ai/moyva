using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GiantGrey.TileWorldCreator.Editor
{
    [CustomPropertyDrawer(typeof(BlueprintLayer))]
    public sealed class BlueprintLayerPropertyDrawer : PropertyDrawer
    {
        private const float VerticalSpacing = 2f;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout
            {
                text = property.displayName,
                value = property.isExpanded
            };

            foldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);

            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                foldout.Add(new PropertyField(iterator.Copy()));
                enterChildren = false;
            }

            return foldout;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
            {
                return height;
            }

            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + VerticalSpacing;
                enterChildren = false;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;

            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();
            bool enterChildren = true;
            float y = foldoutRect.yMax + VerticalSpacing;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                float childHeight = EditorGUI.GetPropertyHeight(iterator, true);
                Rect childRect = new Rect(position.x, y, position.width, childHeight);
                EditorGUI.PropertyField(childRect, iterator, true);
                y += childHeight + VerticalSpacing;
                enterChildren = false;
            }

            EditorGUI.indentLevel--;
        }
    }
}