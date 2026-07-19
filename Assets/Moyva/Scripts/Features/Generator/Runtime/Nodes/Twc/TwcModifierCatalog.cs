using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Attributes;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.Twc
{
    /// <summary>
    /// Опис одного типу TileWorldCreator-модифікатора (генератора чи модифікатора),
    /// знайденого через рефлексію за атрибутом <see cref="ModifierAttribute"/>.
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
    /// TWC-незалежний опис пункту меню для створення вузла. Не містить жодних
    /// типів TileWorldCreator, тому може використовуватись зі складок редактора,
    /// які не посилаються на assembly TileWorldCreator (наприклад GraphSystem.Editor).
    /// </summary>
    public readonly struct TwcModifierMenuItem
    {
        public readonly Type ModifierType;
        public readonly string DisplayName;
        public readonly string MenuCategory;
        public readonly bool IsGenerator;

        public TwcModifierMenuItem(Type modifierType, string displayName, string menuCategory, bool isGenerator)
        {
            ModifierType = modifierType;
            DisplayName = displayName;
            MenuCategory = menuCategory;
            IsGenerator = isGenerator;
        }
    }

    /// <summary>
    /// Реєстр усіх доступних TileWorldCreator-модифікаторів. Заповнюється один раз
    /// через рефлексію та використовується для побудови меню створення вузлів і
    /// для відновлення інстансів модифікаторів за іменем типу.
    /// </summary>
    public static class TwcModifierCatalog
    {
        private static readonly HashSet<string> HiddenMenuNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "add",
            "subtract",
            "boolean",
            "pathfinding",
            "pushtopaintpositions",
            "selectbasedonneighbour",
            "heighttexture"
        };

        private static List<TwcModifierEntry> _entries;
        private static Dictionary<string, TwcModifierEntry> _byTypeName;
        private static List<TwcModifierMenuItem> _menuItems;
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

        /// <summary>
        /// TWC-незалежний список пунктів меню для побудови дерева створення вузлів.
        /// </summary>
        public static IReadOnlyList<TwcModifierMenuItem> MenuItems
        {
            get
            {
                EnsureBuilt();
                if (_menuItems == null)
                {
                    _menuItems = _entries
                        .Where(IsSafeForMoyvaMenu)
                        .Select(e => new TwcModifierMenuItem(
                            e.Type,
                            e.DisplayName,
                            e.IsGenerator
                                ? "Advanced/TileWorldCreator/Generators"
                                : "Advanced/TileWorldCreator/Modifiers",
                            e.IsGenerator))
                        .ToList();
                }
                return _menuItems;
            }
        }

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
            Assembly[] assemblies;
            try
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
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

        private static bool IsSafeForMoyvaMenu(TwcModifierEntry entry)
        {
            string typeName = CanonicalName(entry.Type?.Name);
            string displayName = CanonicalName(entry.DisplayName);
            return !HiddenMenuNames.Contains(typeName) && !HiddenMenuNames.Contains(displayName);
        }

        private static string CanonicalName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var chars = new char[value.Length];
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (char.IsLetterOrDigit(character))
                    chars[count++] = char.ToLowerInvariant(character);
            }

            return new string(chars, 0, count);
        }
    }
}
