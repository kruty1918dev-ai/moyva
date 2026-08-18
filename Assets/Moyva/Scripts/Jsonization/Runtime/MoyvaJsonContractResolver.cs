using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Serializes exactly the gameplay fields Unity used to serialize: public fields
    /// plus [SerializeField]/[SerializeReference] private fields. Editor caches and
    /// [NonSerialized] values never become JSON authoring data.
    /// </summary>
    public sealed class MoyvaJsonContractResolver : DefaultContractResolver
    {
        public MoyvaJsonContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy(
                processDictionaryKeys: false,
                overrideSpecifiedNames: false);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var result = new List<JsonProperty>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                FieldInfo[] fields = current.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsStatic || field.IsNotSerialized)
                        continue;

                    bool unitySerialized = field.IsPublic ||
                        field.GetCustomAttribute<SerializeField>() != null ||
                        field.GetCustomAttribute<SerializeReference>() != null;

                    if (!unitySerialized)
                        continue;

                    JsonProperty property = base.CreateProperty(field, memberSerialization);
                    string name = field.Name.TrimStart('_');
                    if (string.IsNullOrWhiteSpace(name))
                        name = field.Name;

                    property.PropertyName = ResolvePropertyName(name);
                    property.Readable = true;
                    property.Writable = !field.IsInitOnly;

                    if (seen.Add(property.PropertyName))
                        result.Add(property);
                }
            }

            return result;
        }
    }
}
