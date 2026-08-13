using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class TurnSaveModule : ISaveModule
    {
        private const int Magic = unchecked((int)0x5455524E);
        private const int Version = 1;
        private readonly ITurnService _turns;
        private readonly ITurnStateRestorer _restorer;

        public TurnSaveModule(ITurnService turns, ITurnStateRestorer restorer)
        {
            _turns = turns;
            _restorer = restorer;
        }

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(Magic);
            context.Writer.Write(Version);
            context.Writer.Write(_turns.Round);
            context.Writer.Write(_turns.GlobalTurn);
            context.Writer.Write(_turns.ActiveOwnerId ?? string.Empty);
            context.Writer.Write(_turns.ActionsThisTurn);
        }

        public void OnLoad(ISaveContext context)
        {
            if (context.Reader.ReadInt32() != Magic || context.Reader.ReadInt32() != Version)
                throw new System.IO.InvalidDataException("Unsupported turn-state save block.");

            int round = context.Reader.ReadInt32();
            long globalTurn = context.Reader.ReadInt64();
            string activeOwner = context.Reader.ReadString();
            int actions = context.Reader.ReadInt32();

            // TurnService owns restore ordering. The request is safe even when the world or
            // faction registry is not ready yet; it will be resumed only after owner resolution.
            _restorer.Restore(round, globalTurn, activeOwner, actions);
        }
    }
}
