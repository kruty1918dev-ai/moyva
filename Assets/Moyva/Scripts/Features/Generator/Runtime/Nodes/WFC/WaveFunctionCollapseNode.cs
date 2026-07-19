using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.WFC
{
    [NodeInfo(
        "Wave Function Collapse",
        "Generators",
        "Генерує карту точного розміру графа за локальними патернами вхідного зразка.",
        StableId = "moyva.generators.wave-function-collapse",
        Order = 30,
        PreviewOutput = "out.generated_map")]
    public sealed class WaveFunctionCollapseNode : NodeBase,
        IAsyncNode, ICustomEditorNode
    {
        [Header("WFC Settings")]
        [Tooltip("Розмір патерна, який WFC аналізує у вхідному зразку. Більший розмір краще зберігає складні мотиви, але значно збільшує складність генерації.")]
        [SerializeField, Range(2, 5)] private int _patternSize = 2;
        [Tooltip("Чи вважати вхідний зразок циклічним по краях. Корисно, якщо sample може повторюватися без швів і ти хочеш врахувати сусідство через межі прикладу.")]
        [SerializeField] private bool _periodicInput;
        [Tooltip("Чи має вихідна карта циклічно зшиватися по краях. Це важливо для безшовних текстур або топологічно замкнених карт.")]
        [SerializeField] private bool _periodicOutput;
        [Tooltip("Максимальна кількість спроб побудувати валідний результат до визнання генерації невдалою. Більше значення підвищує шанс успіху на складних патернах.")]
        [SerializeField, Range(1, 50)] private int _maxAttempts = 10;

        // Legacy serialized fields are retained so existing assets deserialize safely.
        // WFC output now always follows NodeContext.MapSize.
        [HideInInspector, SerializeField] private int _outputWidth;
        [HideInInspector, SerializeField] private int _outputHeight;

        public override string Title => "Wave Function Collapse";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>(
                "Sample",
                "in.sample",
                PortMapSizePolicy.Variable)
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("Generated Map", "out.generated_map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return ExecuteAsync(inputs, context).GetAwaiter().GetResult();
        }

        public Task<NodeOutput> ExecuteAsync(object[] inputs, NodeContext context)
        {
            var sample = inputs[0] as string[,];
            if (sample == null)
                return Task.FromResult(NodeOutput.Error("Sample input is required."));

            int outW = Mathf.Max(1, context?.MapSize.x ?? 0);
            int outH = Mathf.Max(1, context?.MapSize.y ?? 0);
            int seed = context.Seed;

            var settings = new WFCAlgorithm.WFCSettings
            {
                PatternSize = _patternSize,
                PeriodicInput = _periodicInput,
                PeriodicOutput = _periodicOutput,
                OutputWidth = outW,
                OutputHeight = outH,
                Seed = seed,
                MaxAttempts = _maxAttempts
            };

            var wfc = new WFCAlgorithm();
            var result = wfc.Run(sample, settings, context.Cancellation, context.Progress);

            if (result == null)
                return Task.FromResult(
                    NodeOutput.Error("WFC failed to produce a valid result."));

            return Task.FromResult(NodeOutput.Success(result));
        }

#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            // WFC Editor window will be created in Phase 5.3
            Debug.Log("[WFC] Editor window not yet implemented.");
        }
#endif
    }
}
