using System;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NodeInfoAttribute : Attribute
    {
        public string Title { get; }
        public string Category { get; }
        public string Description { get; }

        public NodeInfoAttribute(string title, string category = "General", string description = null)
        {
            Title = title;
            Category = category;
            Description = description;
        }
    }
}
