using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum NoiseMaskClipMode
    {
        Multiply,
        LerpToOutsideValue
    }

    [NodeInfo("Noise Mask Clip", "Noise", "Обрізає базовий шум за картою-маскою. Дозволяє задати поріг, плавність переходу та інверсію маски, щоб підготувати керовану карту для подальшого поєднання з іншими шумами.")]
    public sealed class NoiseMaskClipNode : NodeBase
    {
        [Header("Режим обрізання")]
        [Tooltip("Як застосовувати маску до базового шуму. Multiply множить шум на альфа-маску; LerpToOutsideValue замінює зовнішню область заданим значенням.")]
        [SerializeField] private NoiseMaskClipMode _mode = NoiseMaskClipMode.Multiply;

        [Header("Параметри маски")]
        [Tooltip("Поріг маски. Значення маски вище порогу залишають шум, нижче порогу обрізають його.")]
        [SerializeField, Range(0f, 1f)] private float _threshold = 0.5f;
        [Tooltip("Ширина плавного переходу навколо порогу. 0 = жорсткий край, більше значення = м'який край.")]
        [SerializeField, Range(0f, 0.5f)] private float _feather = 0.1f;
        [Tooltip("Степінь контрасту альфа-маски. >1 робить перехід жорсткішим, <1 робить м'якшим.")]
        [SerializeField, Range(0.25f, 4f)] private float _maskPower = 1f;
        [Tooltip("Інвертує карту маски: біле стає чорним, чорне стає білим.")]
        [SerializeField] private bool _invertMask = false;

        [Header("Зовнішня область")]
        [Tooltip("Значення поза маскою для режиму LerpToOutsideValue.")]
        [SerializeField, Range(0f, 1f)] private float _outsideValue = 0f;

        public override string Title => "Noise Mask Clip";
        public override string Category => "Noise";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Base Noise"),
            PortDefinition.Input<float[,]>("Mask Noise")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Clipped Noise"),
            PortDefinition.Output<float[,]>("Blend Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var baseNoise = inputs[0] as float[,];
            var maskNoise = inputs[1] as float[,];

            if (baseNoise == null)
                return NodeOutput.Error("Вхід Base Noise є обов'язковим.");
            if (maskNoise == null)
                return NodeOutput.Error("Вхід Mask Noise є обов'язковим.");

            int width = baseNoise.GetLength(0);
            int height = baseNoise.GetLength(1);
            int maskWidth = maskNoise.GetLength(0);
            int maskHeight = maskNoise.GetLength(1);

            var clipped = new float[width, height];
            var blendMask = new float[width, height];

            float feather = Mathf.Max(0f, _feather);
            float lower = Mathf.Clamp01(_threshold - feather);
            float upper = Mathf.Clamp01(_threshold + feather);
            float power = Mathf.Max(0.0001f, _maskPower);
            float outsideValue = Mathf.Clamp01(_outsideValue);

            bool hardCut = upper <= lower + 0.000001f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float baseValue = Mathf.Clamp01(baseNoise[x, y]);
                    float rawMask = (x < maskWidth && y < maskHeight)
                        ? Mathf.Clamp01(maskNoise[x, y])
                        : 0f;

                    if (_invertMask)
                        rawMask = 1f - rawMask;

                    float alpha;
                    if (hardCut)
                    {
                        alpha = rawMask >= _threshold ? 1f : 0f;
                    }
                    else
                    {
                        alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(lower, upper, rawMask));
                    }

                    alpha = Mathf.Pow(alpha, power);
                    blendMask[x, y] = alpha;

                    clipped[x, y] = _mode switch
                    {
                        NoiseMaskClipMode.Multiply => baseValue * alpha,
                        NoiseMaskClipMode.LerpToOutsideValue => Mathf.Lerp(outsideValue, baseValue, alpha),
                        _ => baseValue * alpha
                    };
                }
            }

            return NodeOutput.Success(clipped, blendMask);
        }
    }
}
