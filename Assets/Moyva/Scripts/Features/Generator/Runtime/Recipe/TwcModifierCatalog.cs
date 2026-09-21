using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Attributes;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Description of one TileWorldCreator modifier type (generator or modifier)
    /// discovered via reflection using the <see cref="ModifierAttribute"/>.
    /// </summary>
    public readonly struct TwcModifierEntry
    {
        public readonly Type Type;
        public readonly ModifierAttribute.Category Category;
        public readonly string DisplayName;
        public readonly string IconPath;

        public TwcModifierEntry(Type type, ModifierAttribute attribute)
        {
            Type = type;
            Category = attribute.category;
            DisplayName = string.IsNullOrEmpty(attribute.name) ? type.Name : attribute.name;
            IconPath = attribute.iconPath;
        }

        public bool IsGenerator => Category == ModifierAttribute.Category.Generators;
    }

    /// <summary>
    /// Registry of all available TileWorldCreator modifiers. Built once via
    /// reflection and used to classify recipe <c>TwcModifierMaskStep</c> entries
    /// and to restore modifier instances from serialized type names.
    /// </summary>
    public static class TwcModifierCatalog
    {
        private static List<TwcModifierEntry> _entries;
        private static Dictionary<string, TwcModifierEntry> _byTypeName;

        public static IReadOnlyList<TwcModifierEntry> Entries
        {
            get
            {
                EnsureBuilt();
                return _entries;
            }
        }

        public static IEnumerable<TwcModifierEntry> Generators =>
            Entries.Where(e => e.IsGenerator);

        public static IEnumerable<TwcModifierEntry> Modifiers =>
            Entries.Where(e => !e.IsGenerator);

        public static bool TryGet(string typeName, out TwcModifierEntry entry)
        {
            EnsureBuilt();
            if (string.IsNullOrEmpty(typeName))
            {
                entry = default;
                return false;
            }
            return _byTypeName.TryGetValue(typeName, out entry);
        }

        public static Type ResolveType(string typeName)
        {
            return TryGet(typeName, out var entry) ? entry.Type : null;
        }

        private static void EnsureBuilt()
        {
            if (_entries != null)
                return;

            _entries = new List<TwcModifierEntry>();
            _byTypeName = new Dictionary<string, TwcModifierEntry>();
            var baseType = typeof(BlueprintModifier);
            IReadOnlyList<Assembly> assemblies;
            try
            {
                assemblies = UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies();
            }
            catch
            {
                assemblies = Array.Empty<Assembly>();
            }

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || !baseType.IsAssignableFrom(type))
                        continue;

                    var attribute = type.GetCustomAttribute<ModifierAttribute>(false);
                    if (attribute == null)
                        continue;

                    var entry = new TwcModifierEntry(type, attribute);
                    _entries.Add(entry);
                    _byTypeName[type.FullName ?? type.Name] = entry;
                }
            }

            _entries.Sort((a, b) =>
            {
                int byCategory = a.Category.CompareTo(b.Category);
                return byCategory != 0
                    ? byCategory
                    : string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });
        }
    }
}
