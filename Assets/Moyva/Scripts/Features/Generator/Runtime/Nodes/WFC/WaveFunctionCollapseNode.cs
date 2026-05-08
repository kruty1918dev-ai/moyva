using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.WFC
{
    [NodeInfo("Wave Function Collapse", "WFC", "Генерує нову карту за зразком, зберігаючи локальні патерни та сумісність сусідів. Це інструмент для полірування тайлів, створення орнаментів, кварталів або складніших структур із контрольованим хаосом.")]
    public sealed class WaveFunctionCollapseNode : NodeBase,
        IAsyncNode, IPreviewableNode, ICustomEditorNode
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

        [Header("Output Size (0 = same as input)")]
        [Tooltip("Ширина фінальної карти WFC. Значення 0 означає використати розмір карти з контексту або вхідного зразка за замовчуванням.")]
        [SerializeField] private int _outputWidth;
        [Tooltip("Висота фінальної карти WFC. Значення 0 означає використати розмір карти з контексту або зразка без примусового перевизначення.")]
        [SerializeField] private int _outputHeight;

        [HideInInspector, SerializeField] private Texture2D _lastPreview;
        [HideInInspector, SerializeField] private string[,] _lastResult;

        public override string Title => "WFC Generator";
        public override string Category => "WFC";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("Sample")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("GeneratedMap")
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

            int outW = _outputWidth > 0 ? _outputWidth : context.MapSize.x;
            int outH = _outputHeight > 0 ? _outputHeight : context.MapSize.y;
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

            _lastResult = result;
            return Task.FromResult(NodeOutput.Success(result));
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastResult == null) return null;

            int rw = _lastResult.GetLength(0);
            int rh = _lastResult.GetLength(1);
            var tex = new Texture2D(rw, rh, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < rw; x++)
            {
                for (int y = 0; y < rh; y++)
                {
                    string id = _lastResult[x, y] ?? "";
                    tex.SetPixel(x, y, StringToColor(id));
                }
            }
            tex.Apply();

            _lastPreview = tex;
            return tex;
        }

        private static Color StringToColor(string id)
        {
            if (string.IsNullOrEmpty(id)) return Color.black;
            int hash = id.GetHashCode();
            float r = ((hash & 0xFF0000) >> 16) / 255f;
            float g = ((hash & 0x00FF00) >> 8) / 255f;
            float b = (hash & 0x0000FF) / 255f;
            return new Color(r, g, b);
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
