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
                .Where(t => !t.IsAbstract && !t.IsGenericType);

            var categories = new SortedDictionary<string, List<(string title, Type type)>>();

            foreach (var type in nodeTypes)
            {
                var attr = type.GetCustomAttribute<NodeInfoAttribute>();
                string category = attr?.Category ?? "Other";
                string title = attr?.Title ?? ObjectNames.NicifyVariableName(type.Name);

                if (!categories.ContainsKey(category))
                    categories[category] = new List<(string, Type)>();

                categories[category].Add((title, type));
            }

            foreach (var kvp in categories)
            {
                entries.Add(new SearchTreeGroupEntry(
                    new GUIContent(kvp.Key), 1));

                foreach (var (title, type) in kvp.Value.OrderBy(x => x.title))
                {
                    entries.Add(new SearchTreeEntry(new GUIContent(title))
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
    }
}
