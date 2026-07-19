using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Єдиний незмінний опис типу вузла для каталогу, фабрики, документації та тестів.
    /// </summary>
    public sealed class NodeDescriptor
    {
        public NodeDescriptor(
            string stableId,
            Type nodeType,
            string title,
            string category,
            int order,
            string description,
            NodeLifecycle lifecycle,
            string previewOutput,
            NodeCapabilities capabilities,
            IReadOnlyList<PortDefinition> inputs,
            IReadOnlyList<PortDefinition> outputs,
            string unavailableReason = null)
        {
            StableId = stableId;
            NodeType = nodeType;
            Title = title;
            Category = category;
            Order = order;
            Description = description;
            Lifecycle = lifecycle;
            PreviewOutput = previewOutput;
            Capabilities = capabilities;
            Inputs = inputs ?? Array.Empty<PortDefinition>();
            Outputs = outputs ?? Array.Empty<PortDefinition>();
            UnavailableReason = unavailableReason;
        }

        public string StableId { get; }
        public Type NodeType { get; }
        public string Title { get; }
        public string Category { get; }
        public int Order { get; }
        public string Description { get; }
        public NodeLifecycle Lifecycle { get; }
        public string PreviewOutput { get; }
        public NodeCapabilities Capabilities { get; }
        public IReadOnlyList<PortDefinition> Inputs { get; }
        public IReadOnlyList<PortDefinition> Outputs { get; }
        public string UnavailableReason { get; }
        public bool IsCreatable =>
            Lifecycle == NodeLifecycle.Active
            && NodeType != null
            && string.IsNullOrEmpty(UnavailableReason);
    }

    public static class NodeCategoryOrder
    {
        private static readonly string[] OrderedCategories =
        {
            "Core",
            "Generators",
            "Masks",
            "Height",
            "Math",
            "Modifiers",
            "Layers",
            "Tiles",
            "Objects",
            "Values",
            "Subgraphs",
            "Advanced/TileWorldCreator"
        };

        public static int Get(string category)
        {
            string value = category ?? string.Empty;
            for (int i = 0; i < OrderedCategories.Length; i++)
            {
                string known = OrderedCategories[i];
                if (string.Equals(value, known, StringComparison.Ordinal)
                    || value.StartsWith(known + "/", StringComparison.Ordinal))
                    return i;
            }

            return OrderedCategories.Length;
        }
    }
}
