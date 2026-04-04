using System;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NodeInfoAttribute : Attribute
    {
        public string Title { get; }
        public string Category { get; }

        public NodeInfoAttribute(string title, string category = "General")
        {
            Title = title;
            Category = category;
        }
    }
}
