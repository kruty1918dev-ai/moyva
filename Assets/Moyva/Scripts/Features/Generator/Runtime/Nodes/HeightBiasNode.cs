using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Height Bias", "Noise",
        "Піднімає або опускає значення карти висот у заданому діапазоні. " +
        "Пікселі поза діапазоном [Range Min, Range Max] лишаються без змін.")]
    public sealed class HeightBiasNode : NodeBase
    {
        [Tooltip("Мінімальна висота діапазону (включно).")]
        [SerializeField, Range(0f, 1f)] private float _rangeMin = 0f;

        [Tooltip("Максимальна висота діапазону (включно).")]
        [SerializeField, Range(0f, 1f)] private float _rangeMax = 0.3f;

        [Tooltip("На скільки підняти (позитивне) або опустити (негативне) значення у діапазоні.")]
        [SerializeField, Range(-1f, 1f)] private float _offset = 0.1f;

        public override string Title    => "Height Bias";
        public override string Category => "Noise";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Noise")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Biased Noise")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs[0] is not float[,] source)
                return NodeOutput.Error("Вхід Noise є обов'язковим.");

            float rMin = Mathf.Min(_rangeMin, _rangeMax);
            float rMax = Mathf.Max(_rangeMin, _rangeMax);

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var result = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float v = source[x, y];
                    result[x, y] = (v >= rMin && v <= rMax)
                        ? Mathf.Clamp01(v + _offset)
                        : v;
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
