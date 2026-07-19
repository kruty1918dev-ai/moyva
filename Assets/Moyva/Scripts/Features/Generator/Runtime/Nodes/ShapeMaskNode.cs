using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum ShapeMaskKind
    {
        Rectangle,
        Ellipse,
        Diamond
    }

    [NodeInfo(
        "Shape Mask",
        "Masks",
        "Створює прямокутну, еліптичну або ромбоподібну маску у нормалізованих координатах карти.",
        StableId = "moyva.masks.shape",
        Order = 20,
        PreviewOutput = "out.mask")]
    public sealed class ShapeMaskNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Геометрична форма маски.")]
        private ShapeMaskKind _shape = ShapeMaskKind.Ellipse;

        [SerializeField]
        [Tooltip("Центр форми у діапазоні 0..1 по ширині та висоті.")]
        private Vector2 _center = new(0.5f, 0.5f);

        [SerializeField]
        [Tooltip("Розмір форми у частках ширини та висоти карти.")]
        private Vector2 _size = new(0.75f, 0.75f);

        [SerializeField]
        [Tooltip("Інвертувати результат маски.")]
        private bool _invert;

        public override string Title => "Shape Mask";
        public override string Category => "Masks";
        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            MapNodeUtility.ResolveSize(context, out int width, out int height);
            float halfWidth = Mathf.Max(0.0001f, Mathf.Abs(_size.x) * 0.5f);
            float halfHeight = Mathf.Max(0.0001f, Mathf.Abs(_size.y) * 0.5f);
            var result = new bool[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float normalizedX = (x + 0.5f) / width;
                float normalizedY = (y + 0.5f) / height;
                float dx = Mathf.Abs(normalizedX - _center.x) / halfWidth;
                float dy = Mathf.Abs(normalizedY - _center.y) / halfHeight;
                bool inside = _shape switch
                {
                    ShapeMaskKind.Rectangle => dx <= 1f && dy <= 1f,
                    ShapeMaskKind.Diamond => dx + dy <= 1f,
                    _ => dx * dx + dy * dy <= 1f
                };
                result[x, y] = _invert ? !inside : inside;
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
