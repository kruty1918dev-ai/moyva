using System;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum LayerOutputKind
    {
        Tiles,
        Objects,
        Masks,
        InternalData,
        Other
    }

    public sealed class LayerOutputSnapshot : ILayerMaskArtifact
    {
        public LayerOutputSnapshot(
            string[,] biomeMap,
            string[,] objectMap,
            float[,] heightMap,
            string[,] buildingMap,
            bool[,] mask,
            object data,
            LayerOutputKind outputKind)
        {
            BiomeMap = biomeMap;
            ObjectMap = objectMap;
            HeightMap = heightMap;
            BuildingMap = buildingMap;
            Mask = mask;
            Data = data;
            OutputKind = outputKind;
            LayerMask = ResolveFinalMask(mask, biomeMap, objectMap, buildingMap, data);
        }

        public string[,] BiomeMap { get; }
        public string[,] ObjectMap { get; }
        public float[,] HeightMap { get; }
        public string[,] BuildingMap { get; }
        public bool[,] Mask { get; }
        public object Data { get; }
        public LayerOutputKind OutputKind { get; }
        public bool[,] LayerMask { get; }
        public int Width => BiomeMap?.GetLength(0)
                            ?? ObjectMap?.GetLength(0)
                            ?? HeightMap?.GetLength(0)
                            ?? BuildingMap?.GetLength(0)
                            ?? Mask?.GetLength(0)
                            ?? 0;
        public int Height => BiomeMap?.GetLength(1)
                             ?? ObjectMap?.GetLength(1)
                             ?? HeightMap?.GetLength(1)
                             ?? BuildingMap?.GetLength(1)
                             ?? Mask?.GetLength(1)
                             ?? 0;

        private static bool[,] ResolveFinalMask(
            bool[,] explicitMask,
            string[,] biomeMap,
            string[,] objectMap,
            string[,] buildingMap,
            object data)
        {
            if (explicitMask != null)
                return explicitMask;
            if (data is bool[,] dataMask)
                return dataMask;

            var maps = new[] { biomeMap, objectMap, buildingMap };
            string[,] populated = null;
            for (int mapIndex = 0; mapIndex < maps.Length && populated == null; mapIndex++)
            {
                var map = maps[mapIndex];
                if (map == null)
                    continue;

                for (int x = 0; x < map.GetLength(0) && populated == null; x++)
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (!string.IsNullOrEmpty(map[x, y]))
                    {
                        populated = map;
                        break;
                    }
                }
            }

            if (populated == null)
                return null;

            int width = populated.GetLength(0);
            int height = populated.GetLength(1);
            var result = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                result[x, y] = !string.IsNullOrEmpty(populated[x, y]);
            return result;
        }
    }

    [NodeInfo(
        "Output",
        "Core",
        "Фінальний вузол шару, який явно фіксує карти, маску та службові дані для runtime і превью.",
        StableId = "moyva.core.output",
        Order = 10)]
    public sealed class OutputNode : NodeBase, IGraphOutputNode
    {
        public const int BiomeMapInputIndex = 0;
        public const int ObjectMapInputIndex = 1;
        public const int HeightMapInputIndex = 2;
        public const int BuildingMapInputIndex = 3;
        public const int MaskInputIndex = 4;
        public const int DataInputIndex = 5;

        [UnityEngine.SerializeField, InlineEditable("Output Kind")]
        private LayerOutputKind _outputKind = LayerOutputKind.Tiles;

        public override string Title => "Output";
        public override string Category => "Core";
        public LayerOutputKind OutputKind
        {
            get => _outputKind;
            set => _outputKind = value;
        }

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.OptionalInput<string[,]>("Biome Map", "in.biome_map"),
            PortDefinition.OptionalInput<string[,]>("Object Map", "in.object_map"),
            PortDefinition.OptionalInput<float[,]>("Height Map", "in.height_map"),
            PortDefinition.OptionalInput<string[,]>("Building Map", "in.building_map"),
            PortDefinition.OptionalInput<bool[,]>("Mask", "in.mask"),
            PortDefinition.AnyInput("Data", required: false, stableId: "in.data")
        };

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = GetInput<string[,]>(inputs, BiomeMapInputIndex);
            var objectMap = GetInput<string[,]>(inputs, ObjectMapInputIndex);
            var heightMap = GetInput<float[,]>(inputs, HeightMapInputIndex);
            var buildingMap = GetInput<string[,]>(inputs, BuildingMapInputIndex);
            var mask = GetInput<bool[,]>(inputs, MaskInputIndex);
            var data = GetInput<object>(inputs, DataInputIndex);

            ResolveSize(context, out int w, out int h);

            biomeMap ??= new string[w, h];
            objectMap ??= new string[w, h];
            heightMap ??= new float[w, h];
            buildingMap ??= new string[w, h];

            var snapshot = new LayerOutputSnapshot(
                biomeMap,
                objectMap,
                heightMap,
                buildingMap,
                mask,
                data,
                _outputKind);
            return NodeOutput.SuccessWithArtifact(snapshot);
        }

        private static T GetInput<T>(object[] inputs, int index) where T : class
        {
            if (inputs == null || index < 0 || index >= inputs.Length)
                return null;

            return inputs[index] as T;
        }

        private static void ResolveSize(NodeContext context, out int width, out int height)
        {
            width = Math.Max(1, context?.MapSize.x ?? 0);
            height = Math.Max(1, context?.MapSize.y ?? 0);
        }
    }
}
