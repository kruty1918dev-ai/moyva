using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    [Serializable]
    public sealed class MoyvaJsonBindingRecord
    {
        public Component Target;
        public string PropertyPath;
        public string ConfigType;
        public string ConfigId;
    }

    /// <summary>
    /// Machine-generated scene/prefab bridge. It stores only stable JSON IDs and
    /// scene Component references; no gameplay values are serialized here.
    /// </summary>
    [DefaultExecutionOrder(-32000)]
    public sealed class MoyvaJsonBindingMarker : MonoBehaviour
    {
        [SerializeField] private List<MoyvaJsonBindingRecord> _bindings = new();

        public IReadOnlyList<MoyvaJsonBindingRecord> Bindings => _bindings;

        public void ReplaceBindings(IEnumerable<MoyvaJsonBindingRecord> bindings)
        {
            _bindings = bindings != null
                ? new List<MoyvaJsonBindingRecord>(bindings)
                : new List<MoyvaJsonBindingRecord>();
        }

        private void Awake() => ApplyBindings();

        public void ApplyBindings()
        {
            MoyvaJsonRuntime.EnsureLoaded();

            for (int i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];
                if (binding == null || binding.Target == null)
                    continue;

                try
                {
                    object config = MoyvaJsonRuntime.GetByTypeName(
                        binding.ConfigType,
                        binding.ConfigId);

                    if (config == null)
                        throw new InvalidOperationException(
                            $"Config not found: {binding.ConfigType}/{binding.ConfigId}");

                    SetSerializedPath(binding.Target, binding.PropertyPath, config);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        "[MoyvaJson] Binding failed " +
                        $"{binding.Target.GetType().FullName}.{binding.PropertyPath} " +
                        $"id={binding.ConfigId}: {ex.GetType().Name}: {ex.Message}",
                        this);
                    throw;
                }
            }
        }

        private static void SetSerializedPath(object root, string propertyPath, object value)
        {
            if (root == null || string.IsNullOrWhiteSpace(propertyPath))
                throw new ArgumentException("Invalid binding target/path.");

            string normalized = propertyPath.Replace(".Array.data[", "[");
            string[] segments = normalized.Split('.');
            object current = root;

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                int bracket = segment.IndexOf('[');
                string fieldName = bracket >= 0 ? segment.Substring(0, bracket) : segment;
                FieldInfo field = FindField(current.GetType(), fieldName);

                if (field == null)
                    throw new MissingFieldException(current.GetType().FullName, fieldName);

                bool last = i == segments.Length - 1;
                object fieldValue = field.GetValue(current);

                if (bracket < 0)
                {
                    if (last)
                    {
                        if (value != null && !field.FieldType.IsInstanceOfType(value))
                            throw new InvalidOperationException(
                                $"Type mismatch for {fieldName}: expected {field.FieldType.FullName}, " +
                                $"got {value.GetType().FullName}");
                        field.SetValue(current, value);
                        return;
                    }

                    if (fieldValue == null)
                        throw new NullReferenceException($"Null path segment: {fieldName}");

                    current = fieldValue;
                    continue;
                }

                int end = segment.IndexOf(']', bracket + 1);
                if (end < 0 || !int.TryParse(segment.Substring(bracket + 1, end - bracket - 1), out int index))
                    throw new FormatException($"Invalid array segment: {segment}");

                if (!(fieldValue is IList list))
                    throw new InvalidOperationException($"{fieldName} is not IList.");

                if (index < 0 || index >= list.Count)
                    throw new IndexOutOfRangeException($"{fieldName}[{index}] count={list.Count}");

                if (last)
                {
                    list[index] = value;
                    return;
                }

                current = list[index] ?? throw new NullReferenceException($"Null list element: {segment}");
            }
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(
                    name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
                type = type.BaseType;
            }
            return null;
        }
    }
}
