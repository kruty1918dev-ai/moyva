using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class NodeSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private GeneratorGraphView _graphView;

        public void Initialize(GeneratorGraphView graphView) =>
            _graphView = graphView;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };

            var nodeTypes = TypeCache.GetTypesDerivedFrom<NodeBase>()
                .Where(t => !t.IsAbstract && !t.IsGenericType)
                .Where(t => !string.Equals(t.Name, "SharedSettingsNode", StringComparison.Ordinal))
                .Where(t => !string.Equals(t.Name, "RoutePointNode", StringComparison.Ordinal))
                .Where(t => !IsUnavailableUniqueNode(t));

            var categories = new SortedDictionary<string, List<(string title, Type type)>>();

            foreach (var type in nodeTypes)
            {
                var attr = type.GetCustomAttribute<NodeInfoAttribute>();
                string category = attr?.Category ?? "Other";
                string title = attr?.Title ?? ObjectNames.NicifyVariableName(type.Name);
                string description = attr?.Description ?? "Опис відсутній.";

                if (!categories.ContainsKey(category))
                    categories[category] = new List<(string, Type)>();

                categories[category].Add(($"{title}:::{description}", type));
            }

            foreach (var kvp in categories)
            {
                entries.Add(new SearchTreeGroupEntry(
                    new GUIContent(kvp.Key), 1));

                foreach (var (packed, type) in kvp.Value.OrderBy(x => x.title))
                {
                    var parts = packed.Split(new[] { ":::" }, StringSplitOptions.None);
                    string title = parts.Length > 0 ? parts[0] : packed;
                    string description = parts.Length > 1 ? parts[1] : "Опис відсутній.";
                    string detailedTooltip = BuildDetailedTooltip(type, title, description);

                    entries.Add(new SearchTreeEntry(new GUIContent(title, detailedTooltip))
                    {
                        userData = type,
                        level = 2
                    });
                }
            }

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry,
            SearchWindowContext context)
        {
            var nodeType = (Type)entry.userData;

            // Convert screen position to local graph position
            var windowPos = context.screenMousePosition
                - _graphView.EditorWindow.position.position;
            var graphPos = ((VisualElement)_graphView).ChangeCoordinatesTo(
                _graphView.contentViewContainer, windowPos);

            _graphView.CreateNode(nodeType, graphPos);
            return true;
        }

        private static string BuildDetailedTooltip(Type nodeType, string title, string description)
        {
            NodeBase instance = null;

            try
            {
                instance = ScriptableObject.CreateInstance(nodeType) as NodeBase;

                var inputs = instance?.Inputs ?? Array.Empty<PortDefinition>();
                var outputs = instance?.Outputs ?? Array.Empty<PortDefinition>();

                string purpose = string.IsNullOrWhiteSpace(description)
                    ? "Опис відсутній."
                    : description;

                string example = BuildExampleLine(inputs, outputs);

                return
                    $"{title}\n\n" +
                    $"Призначення:\n{purpose}\n\n" +
                    $"Приклад використання:\n{example}\n\n" +
                    $"Входи:\n{FormatPorts(inputs)}\n\n" +
                    $"Виходи:\n{FormatPorts(outputs)}";
            }
            catch
            {
                return
                    $"{title}\n\n" +
                    $"Призначення:\n{description}\n\n" +
                    "Деталі портів недоступні для цієї ноди.";
            }
            finally
            {
                if (instance != null)
                    DestroyImmediate(instance);
            }
        }

        private bool IsUnavailableUniqueNode(Type nodeType)
        {
            if (nodeType == null || !Attribute.IsDefined(nodeType, typeof(UniqueNodeAttribute)))
                return false;

            var graph = _graphView?.GraphAsset;
            if (graph?.Nodes == null)
                return false;

            return graph.Nodes.Any(node => node != null && node.GetType() == nodeType);
        }

        private static string BuildExampleLine(PortDefinition[] inputs, PortDefinition[] outputs)
        {
            if (inputs.Length == 0 && outputs.Length == 0)
                return "Нода працює без портів (службова або керуюча логіка).";

            if (inputs.Length == 0)
                return $"Запустіть ноду, щоб сформувати результат у: {JoinPortNames(outputs)}.";

            if (outputs.Length == 0)
                return $"Передайте дані у: {JoinPortNames(inputs)}.";

            return $"Подайте дані у: {JoinPortNames(inputs)} -> отримайте результат з: {JoinPortNames(outputs)}.";
        }

        private static string JoinPortNames(PortDefinition[] ports)
        {
            if (ports == null || ports.Length == 0)
                return "-";

            return string.Join(", ", ports.Select(p => string.IsNullOrWhiteSpace(p?.Name) ? "<без назви>" : p.Name));
        }

        private static string FormatPorts(PortDefinition[] ports)
        {
            if (ports == null || ports.Length == 0)
                return "- немає";

            return string.Join("\n", ports.Select(FormatPort));
        }

        private static string FormatPort(PortDefinition port)
        {
            if (port == null)
                return "- <невідомий порт>";

            string portName = string.IsNullOrWhiteSpace(port.Name) ? "<без назви>" : port.Name;
            return $"- {portName}: {GetFriendlyTypeName(port.ValueType)}";
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == null)
                return "Unknown";

            if (type == typeof(float[,])) return "float[,] (висотна мапа)";
            if (type == typeof(string[,])) return "string[,] (мапа тайлів/біомів/об'єктів)";
            if (type == typeof(bool[,])) return "bool[,] (маска)";
            if (type == typeof(int[,])) return "int[,] (цілочислова мапа)";
            if (type == typeof(object)) return "Any (будь-який тип)";

            return type.Name;
        }
    }
}
