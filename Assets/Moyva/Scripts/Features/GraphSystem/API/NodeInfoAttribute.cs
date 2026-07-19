using System;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public enum NodeLifecycle
    {
        Active,
        Hidden,
        Deprecated
    }

    [Flags]
    public enum NodeCapabilities
    {
        None = 0,
        Deterministic = 1 << 0,
        RectangularMaps = 1 << 1,
        LogicalPreview = 1 << 2,
        RuntimeSafe = 1 << 3,
        ExternalDependency = 1 << 4
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NodeInfoAttribute : Attribute
    {
        public string Title { get; }
        public string Category { get; }
        public string Description { get; }
        public string StableId { get; set; }
        public int Order { get; set; } = 1000;
        public NodeLifecycle Lifecycle { get; set; } = NodeLifecycle.Active;
        public string PreviewOutput { get; set; }
        public NodeCapabilities Capabilities { get; set; } =
            NodeCapabilities.Deterministic
            | NodeCapabilities.RectangularMaps
            | NodeCapabilities.LogicalPreview
            | NodeCapabilities.RuntimeSafe;

        public NodeInfoAttribute(string title, string category = "General", string description = null)
        {
            Title = title;
            Category = category;
            Description = description;
        }
    }
}
