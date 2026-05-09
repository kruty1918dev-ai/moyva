using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GeneratedWorldSaveModule : ISaveModule
    {
        private readonly MapVisualInstantiator _mapVisualInstantiator;

        public GeneratedWorldSaveModule(MapVisualInstantiator mapVisualInstantiator)
        {
            _mapVisualInstantiator = mapVisualInstantiator;
        }

        public void OnSave(ISaveContext context)
        {
            if (!_mapVisualInstantiator.TryGetCurrentWorldData(out var data) || data == null)
                return;

            context.Writer.Write(data.Width);
            context.Writer.Write(data.Height);

            WriteStringMap(context, data.BiomeMap, data.Width, data.Height);
            WriteStringMap(context, data.ObjectMap, data.Width, data.Height);
            WriteFloatMap(context, data.HeightMap, data.Width, data.Height);
            WriteStringMap(context, data.BuildingMap, data.Width, data.Height);

            context.Writer.Write(data.WorldName ?? string.Empty);
            context.Writer.Write(data.Seed);
            context.Writer.Write(data.Size);
            context.Writer.Write(data.MapType);
            context.Writer.Write(data.Difficulty);

            WriteSpawnPositions(context, data.SpawnPositions);
        }

        public void OnLoad(ISaveContext context)
        {
            int width = context.Reader.ReadInt32();
            int height = context.Reader.ReadInt32();

            if (width <= 0 || height <= 0)
                return;

            var data = new GeneratedWorldData
            {
                Width = width,
                Height = height,
                BiomeMap = ReadStringMap(context, width, height),
                ObjectMap = ReadStringMap(context, width, height),
                HeightMap = ReadFloatMap(context, width, height),
                BuildingMap = ReadStringMap(context, width, height),
            };

            if (context.Reader.BaseStream.Position < context.Reader.BaseStream.Length)
            {
                data.WorldName = context.Reader.ReadString();
                data.Seed = context.Reader.ReadInt32();
                data.Size = context.Reader.ReadInt32();
                data.MapType = context.Reader.ReadInt32();
                data.Difficulty = context.Reader.ReadInt32();

                if (context.Reader.BaseStream.Position < context.Reader.BaseStream.Length)
                    data.SpawnPositions = ReadSpawnPositions(context);
            }

            _mapVisualInstantiator.SetPendingWorldData(data);
        }

        private static void WriteStringMap(ISaveContext context, string[,] map, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(map?[x, y] ?? string.Empty);
        }

        private static string[,] ReadStringMap(ISaveContext context, int width, int height)
        {
            var map = new string[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = context.Reader.ReadString();

            return map;
        }

        private static void WriteFloatMap(ISaveContext context, float[,] map, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(map?[x, y] ?? 0f);
        }

        private static float[,] ReadFloatMap(ISaveContext context, int width, int height)
        {
            var map = new float[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = context.Reader.ReadSingle();

            return map;
        }

        private static void WriteSpawnPositions(ISaveContext context, SpawnPositionAssignment[] assignments)
        {
            int count = assignments?.Length ?? 0;
            context.Writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                context.Writer.Write(assignments[i].SlotIndex);
                context.Writer.Write(assignments[i].ParticipantId ?? string.Empty);
                context.Writer.Write(assignments[i].IsBot);
                context.Writer.Write(assignments[i].Position.x);
                context.Writer.Write(assignments[i].Position.y);
            }
        }

        private static SpawnPositionAssignment[] ReadSpawnPositions(ISaveContext context)
        {
            int count = context.Reader.ReadInt32();
            if (count <= 0)
                return null;

            var assignments = new SpawnPositionAssignment[count];
            for (int i = 0; i < count; i++)
            {
                assignments[i] = new SpawnPositionAssignment
                {
                    SlotIndex = context.Reader.ReadInt32(),
                    ParticipantId = context.Reader.ReadString(),
                    IsBot = context.Reader.ReadBoolean(),
                    Position = new UnityEngine.Vector2Int(context.Reader.ReadInt32(), context.Reader.ReadInt32()),
                };
            }

            return assignments;
        }
    }
}