using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Save module for Fog of War explored tiles.
    /// Stores explored map snapshot as width/height + bool grid.
    /// </summary>
    internal sealed class FogOfWarSaveModule : ISaveModule
    {
        private readonly IFogOfWarService _fogOfWarService;

        public FogOfWarSaveModule(IFogOfWarService fogOfWarService)
        {
            _fogOfWarService = fogOfWarService;
        }

        public void OnSave(ISaveContext context)
        {
            bool[,] snapshot = _fogOfWarService.GetExploredSnapshot();
            if (snapshot == null)
            {
                context.Writer.Write(0);
                context.Writer.Write(0);
                return;
            }

            int width = snapshot.GetLength(0);
            int height = snapshot.GetLength(1);

            context.Writer.Write(width);
            context.Writer.Write(height);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(snapshot[x, y]);
        }

        public void OnLoad(ISaveContext context)
        {
            int width = context.Reader.ReadInt32();
            int height = context.Reader.ReadInt32();

            if (width <= 0 || height <= 0)
                return;

            var snapshot = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    snapshot[x, y] = context.Reader.ReadBoolean();

            _fogOfWarService.LoadFromSnapshot(snapshot);
        }
    }
}
