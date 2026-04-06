using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("River Generator", "Features", "Генерує річки від випадкових точок на краю мапи до найнижчої точки карти.")]
    public sealed class RiverNode : NodeBase
    {
        [Tooltip("ID об'єкта річки в ObjectMap.")]
        [SerializeField, MapObjectId] private string _baseObjectId = "river";

        [Tooltip("Кількість річок для генерації.")]
        [SerializeField, Min(1)] private int _riversCount = 1;

        [Header("Path Diversity")]
        [Tooltip("Штраф за клітинки, де вже проходить інша річка. Більше значення -> менше перетинів.")]
        [SerializeField, Range(0f, 500f)] private float _usedCellPenalty = 120f;

        [Tooltip("Штраф за прокладання близько до існуючого русла.")]
        [SerializeField, Range(0f, 200f)] private float _nearRiverPenalty = 35f;

        [Tooltip("Радіус зони близькості для штрафу (0 вимикає цей штраф).")]
        [SerializeField, Range(0, 4)] private int _nearRiverRadius = 1;

        public override string Title => "River Generator";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            var heightMap = inputs[1] as float[,];
            if (biomeMap == null || heightMap == null)
                return NodeOutput.Error("BiomeMap and HeightMap inputs are required.");

            var result = (string[,])biomeMap.Clone();
            int width = result.GetLength(0);
            int height = result.GetLength(1);
            var objectMap = new string[width, height];

            var pathfinder = context.GetService<IRiverPathfinder>();
            var riverGen = new RiverFeatureGenerator(
                _baseObjectId,
                _riversCount,
                _usedCellPenalty,
                _nearRiverPenalty,
                _nearRiverRadius,
                pathfinder);
            riverGen.ApplyFeatures(result, objectMap, heightMap, width, height);

            return NodeOutput.Success(result, objectMap);
        }
    }
}
