using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [Serializable]
    public struct BiomePaintEntry
    {
        public Color Color;
        [TileId] public string TileId;
    }

    [NodeInfo("Biome Painter", "Biome", "Застосовує ручну біом-маску з текстури. Художник може намалювати біоми вручну, а граф далі адаптує генерацію під цю карту.")]
    public sealed class BiomePainterNode : NodeBase
    {
        [SerializeField] private Texture2D _paintTexture;
        [SerializeField] private List<BiomePaintEntry> _palette = new();
        [SerializeField, Range(0f, 1f)] private float _alphaThreshold = 0.1f;

        public override string Title => "Biome Painter";
        public override string Category => "Biome";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BaseBiomeMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var baseMap = inputs[0] as string[,];
            int w = baseMap?.GetLength(0) ?? Mathf.Max(1, context.MapSize.x);
            int h = baseMap?.GetLength(1) ?? Mathf.Max(1, context.MapSize.y);

            var result = baseMap != null ? (string[,])baseMap.Clone() : new string[w, h];

            if (_paintTexture == null || _palette == null || _palette.Count == 0)
                return NodeOutput.Warning("Paint texture or palette is empty. Returning base map.", result);

            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w;
                for (int y = 0; y < h; y++)
                {
                    float v = (y + 0.5f) / h;
                    Color px = _paintTexture.GetPixelBilinear(u, v);
                    if (px.a < _alphaThreshold)
                    {
                        context.CountIteration();
                        continue;
                    }

                    string id = ResolveBiomeId(px);
                    if (!string.IsNullOrEmpty(id))
                        result[x, y] = id;

                    context.CountIteration();
                }
            }

            return NodeOutput.Success(result);
        }

        private string ResolveBiomeId(Color c)
        {
            float best = float.MaxValue;
            string bestId = null;

            for (int i = 0; i < _palette.Count; i++)
            {
                var p = _palette[i];
                float dr = p.Color.r - c.r;
                float dg = p.Color.g - c.g;
                float db = p.Color.b - c.b;
                float d = dr * dr + dg * dg + db * db;
                if (d < best)
                {
                    best = d;
                    bestId = p.TileId;
                }
            }

            return bestId;
        }
    }
}
