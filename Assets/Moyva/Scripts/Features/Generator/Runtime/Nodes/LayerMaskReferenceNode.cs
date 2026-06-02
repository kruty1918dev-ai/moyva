using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Layer Ref", "Layers", "Повертає bool-маску іншого шару за його Layer ID, щоб далі комбінувати її в поточному шарі.")]
    public sealed class LayerMaskReferenceNode : NodeBase, IPreviewableNode
    {
        [SerializeField, HideInInspector] private string _sourceLayerId;

        public override string Title => "Layer Ref";
        public override string Category => "Layers";

        public string SourceLayerId => _sourceLayerId;

        public void SetSourceLayerId(string layerId)
        {
            _sourceLayerId = layerId;
        }

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask"),
            PortDefinition.Output<Texture2D>("Texture")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (context == null)
                return NodeOutput.Error("NodeContext відсутній.");

            if (string.IsNullOrEmpty(_sourceLayerId))
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("Для Layer Ref не вибрано шар-джерело.", empty, BuildTexture(empty));
            }

            if (!context.TryGetService<LayerMaskRegistry>(out var registry) || registry == null)
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("LayerMaskRegistry недоступний у NodeContext.", empty, BuildTexture(empty));
            }

            if (!registry.TryGetLatestMask(_sourceLayerId, out var mask) || mask == null)
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("Для вибраного шару ще немає згенерованої маски.", empty, BuildTexture(empty));
            }

            return NodeOutput.Success(mask, BuildTexture(mask));
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            return null;
        }

        private static bool[,] CreateEmptyMask(NodeContext context)
        {
            int w = Mathf.Max(1, context.MapSize.x);
            int h = Mathf.Max(1, context.MapSize.y);
            return new bool[w, h];
        }

        private static Texture2D BuildTexture(bool[,] mask)
        {
            int w = mask.GetLength(0);
            int h = mask.GetLength(1);

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    pixels[y * w + x] = mask[x, y] ? Color.white : Color.black;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
