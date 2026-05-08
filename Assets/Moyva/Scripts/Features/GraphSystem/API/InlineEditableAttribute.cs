using System;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class InlineEditableAttribute : Attribute
    {
        public string Label { get; }

        public InlineEditableAttribute(string label = null)
        {
            Label = label;
        }
    }
}