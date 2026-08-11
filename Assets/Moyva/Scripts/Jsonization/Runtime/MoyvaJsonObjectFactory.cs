using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>Replacement for migration-era ScriptableObject.CreateInstance for plain JSON models.</summary>
    public static class MoyvaJsonObjectFactory
    {
        public static T Create<T>() where T : class
        {
            try { return Activator.CreateInstance(typeof(T), true) as T; }
            catch { return FormatterServices.GetUninitializedObject(typeof(T)) as T; }
        }

        public static object Create(Type type)
        {
            if (type == null) return null;
            try { return Activator.CreateInstance(type, true); }
            catch { return FormatterServices.GetUninitializedObject(type); }
        }

        /// <summary>
        /// Migration-safe replacement for test/editor cleanup code.
        /// Plain JSON config objects require no destruction. Unity objects keep
        /// their previous DestroyImmediate semantics.
        /// </summary>
        public static void DestroyImmediate(object value)
        {
            if (value is UnityEngine.Object unityObject)
                UnityEngine.Object.DestroyImmediate(unityObject);
        }

        public static void DestroyImmediate(object value, bool allowDestroyingAssets)
        {
            if (value is UnityEngine.Object unityObject)
                UnityEngine.Object.DestroyImmediate(unityObject, allowDestroyingAssets);
        }

        public static void CopySerialized(object source, object target)
        {
            CopySerializedManagedFieldsOnly(source, target);
        }

        public static void CopySerializedManagedFieldsOnly(object source, object target)
        {
            if (source == null || target == null)
                return;

            Type sourceType = source.GetType();
            Type targetType = target.GetType();
            const BindingFlags Flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            foreach (FieldInfo sourceField in sourceType.GetFields(Flags))
            {
                if (sourceField.IsStatic)
                    continue;

                FieldInfo targetField = targetType.GetField(sourceField.Name, Flags);
                if (targetField == null || targetField.IsStatic || targetField.IsInitOnly)
                    continue;

                if (!targetField.FieldType.IsAssignableFrom(sourceField.FieldType))
                    continue;

                targetField.SetValue(target, sourceField.GetValue(source));
            }
        }
    }
}
