using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum MaskComparison
    {
        GreaterThan,
        LessThan,
        Between
    }

    [NodeInfo("Mask", "Utility", "Перетворює числову карту в булеву маску за заданим правилом порівняння. Це базовий інструмент для відсікання областей під воду, ліси, гори, спавн об'єктів та інші фічі.")]
    public sealed class MaskNode : NodeBase
    {
        [Header("Mask Settings")]
        [Tooltip("Тип порівняння, за яким значення вхідної карти буде переведене в true або false. Дозволяє вибирати області вище порогу, нижче порогу або в межах діапазону.")]
        [SerializeField] private MaskComparison _comparison = MaskComparison.GreaterThan;
        [Tooltip("Основний поріг для порівняння. Використовується для режимів GreaterThan і LessThan, а також як нижня межа в режимі Between.")]
        [SerializeField] private float _threshold = 0.5f;
        [Tooltip("Верхня межа діапазону для режиму Between. Дає змогу вибирати лише значення всередині певного коридору.")]
        [SerializeField] private float _upperThreshold = 0.8f;

        public override string Title => "Mask";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var mask = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float v = heightMap[x, y];
                    mask[x, y] = _comparison switch
                    {
                        MaskComparison.GreaterThan => v > _threshold,
                        MaskComparison.LessThan => v < _threshold,
                        MaskComparison.Between => v >= _threshold && v <= _upperThreshold,
                        _ => false
                    };
                }
            }

            return NodeOutput.Success(mask);
        }
    }
}
