using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum TextureMaskChannel
    {
        Luminance,
        Red,
        Green,
        Blue,
        Alpha
    }

    public enum TextureMaskSampling
    {
        Nearest,
        Bilinear
    }

    [NodeInfo(
        "Texture Mask",
        "Masks",
        "Явно перетворює читабельну Texture2D на карту й маску точного розміру контексту з обраним способом семплінгу.",
        StableId = "moyva.masks.texture",
        Order = 30,
        PreviewOutput = "out.mask")]
    public sealed class TextureMaskNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Текстура за замовчуванням, якщо вхід Texture не підключено.")]
        private Texture2D _texture;

        [SerializeField]
        [Tooltip("Канал текстури, який використовується як значення 0..1.")]
        private TextureMaskChannel _channel = TextureMaskChannel.Luminance;

        [SerializeField]
        [Tooltip("Явний алгоритм зміни розміру текстури до сітки карти.")]
        private TextureMaskSampling _sampling = TextureMaskSampling.Nearest;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Порогове значення для булевої маски.")]
        private float _threshold = 0.5f;

        [SerializeField]
        [Tooltip("Інвертувати булеву маску після застосування порогу.")]
        private bool _invert;

        public override string Title => "Texture Mask";
        public override string Category => "Masks";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.OptionalInput<Texture2D>("Texture", "in.texture")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask"),
            PortDefinition.Output<float[,]>("Map", "out.map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var texture = inputs != null && inputs.Length > 0
                ? inputs[0] as Texture2D ?? _texture
                : _texture;
            if (texture == null)
                return NodeOutput.Error("Texture не призначено і не підключено.");

            MapNodeUtility.ResolveSize(context, out int width, out int height);
            var values = new float[width, height];
            var mask = new bool[width, height];

            try
            {
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    float u = (x + 0.5f) / width;
                    float v = (y + 0.5f) / height;
                    Color color = Sample(texture, u, v);
                    float value = ResolveChannel(color);
                    values[x, y] = value;
                    bool enabled = value >= _threshold;
                    mask[x, y] = _invert ? !enabled : enabled;
                }
            }
            catch (UnityException exception)
            {
                return NodeOutput.Error(
                    $"Texture '{texture.name}' недоступна для CPU-читання: {exception.Message}");
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(mask, values);
        }

        private Color Sample(Texture2D texture, float u, float v)
        {
            if (_sampling == TextureMaskSampling.Bilinear)
                return texture.GetPixelBilinear(u, v);

            int x = Mathf.Clamp(Mathf.FloorToInt(u * texture.width), 0, texture.width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(v * texture.height), 0, texture.height - 1);
            return texture.GetPixel(x, y);
        }

        private float ResolveChannel(Color color)
        {
            return _channel switch
            {
                TextureMaskChannel.Red => color.r,
                TextureMaskChannel.Green => color.g,
                TextureMaskChannel.Blue => color.b,
                TextureMaskChannel.Alpha => color.a,
                _ => color.grayscale
            };
        }
    }
}
