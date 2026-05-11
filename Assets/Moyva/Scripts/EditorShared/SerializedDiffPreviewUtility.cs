using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class SerializedDiffPreviewUtility
    {
        public static string CaptureSnapshot(UnityEngine.Object target)
        {
            if (target == null)
                return string.Empty;

            return EditorJsonUtility.ToJson(target, true);
        }

        public static List<string> BuildDiff(SerializedObject current, string baselineSnapshot, int maxItems = 200)
        {
            var changes = new List<string>();
            if (current == null || current.targetObject == null || string.IsNullOrWhiteSpace(baselineSnapshot))
                return changes;

            var baselineObject = ScriptableObject.CreateInstance(current.targetObject.GetType());
            if (baselineObject == null)
                return changes;

            try
            {
                EditorJsonUtility.FromJsonOverwrite(baselineSnapshot, baselineObject);
                var baselineSerialized = new SerializedObject(baselineObject);

                var iterator = current.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.propertyPath == "m_Script")
                        continue;

                    if (iterator.propertyType == SerializedPropertyType.Generic && iterator.hasVisibleChildren)
                        continue;

                    var baselineProp = baselineSerialized.FindProperty(iterator.propertyPath);
                    if (baselineProp == null)
                    {
                        changes.Add($"{iterator.propertyPath}: <missing> -> {ValueToString(iterator)}");
                    }
                    else if (!AreEqual(iterator, baselineProp))
                    {
                        changes.Add($"{iterator.propertyPath}: {ValueToString(baselineProp)} -> {ValueToString(iterator)}");
                    }

                    if (changes.Count >= maxItems)
                        break;
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(baselineObject);
            }

            return changes;
        }

        public static List<string> BuildDiff(UnityEngine.Object currentTarget, string baselineSnapshot, int maxItems = 200)
        {
            if (currentTarget == null)
                return new List<string>();

            var serialized = new SerializedObject(currentTarget);
            return BuildDiff(serialized, baselineSnapshot, maxItems);
        }

        private static bool AreEqual(SerializedProperty current, SerializedProperty baseline)
        {
            if (current.propertyType != baseline.propertyType)
                return false;

            switch (current.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return current.intValue == baseline.intValue;
                case SerializedPropertyType.Boolean:
                    return current.boolValue == baseline.boolValue;
                case SerializedPropertyType.Float:
                    return Mathf.Approximately(current.floatValue, baseline.floatValue);
                case SerializedPropertyType.String:
                    return string.Equals(current.stringValue, baseline.stringValue, StringComparison.Ordinal);
                case SerializedPropertyType.Color:
                    return current.colorValue == baseline.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return current.objectReferenceInstanceIDValue == baseline.objectReferenceInstanceIDValue;
                case SerializedPropertyType.Enum:
                    return current.enumValueIndex == baseline.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return current.vector2Value == baseline.vector2Value;
                case SerializedPropertyType.Vector3:
                    return current.vector3Value == baseline.vector3Value;
                case SerializedPropertyType.Vector4:
                    return current.vector4Value == baseline.vector4Value;
                case SerializedPropertyType.Rect:
                    return current.rectValue == baseline.rectValue;
                case SerializedPropertyType.Bounds:
                    return current.boundsValue == baseline.boundsValue;
                case SerializedPropertyType.Quaternion:
                    return current.quaternionValue == baseline.quaternionValue;
                case SerializedPropertyType.Vector2Int:
                    return current.vector2IntValue == baseline.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return current.vector3IntValue == baseline.vector3IntValue;
                case SerializedPropertyType.ArraySize:
                    return current.intValue == baseline.intValue;
                default:
                    return string.Equals(ValueToString(current), ValueToString(baseline), StringComparison.Ordinal);
            }
        }

        private static string ValueToString(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "true" : "false";
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("0.###", CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(property.stringValue) ? "\"\"" : property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
                case SerializedPropertyType.Enum:
                    return property.enumDisplayNames != null && property.enumDisplayNames.Length > property.enumValueIndex && property.enumValueIndex >= 0
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.enumValueIndex.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString();
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString();
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.eulerAngles.ToString();
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);
                default:
                    return property.displayName;
            }
        }
    }
}
