using System;
using System.IO;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.GameMode.Runtime.GameModeSaveModule")]
    internal sealed class GameModeSaveModule : IStagedSaveModule, ISaveModuleExecutionOrder
    {
        private const int SchemaVersion = 1;

        private readonly IGameResultStateStore _resultState;

        public GameModeSaveModule(
            IGameResultStateStore resultState)
        {
            _resultState = resultState ?? throw new ArgumentNullException(nameof(resultState));
        }

        // Restore the terminal state before turns resume or evaluate a winner.
        public int SaveLoadOrder => 850;

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(SchemaVersion);
            context.Writer.Write(_resultState.IsGameOver);
            context.Writer.Write(_resultState.WinnerId);
        }

        public void OnLoad(ISaveContext context)
            => PrepareLoad(context)();

        public Action PrepareLoad(ISaveContext context)
        {
            int version = context.Reader.ReadInt32();
            if (version != SchemaVersion)
                throw new InvalidDataException("Unsupported game mode save version.");

            bool isGameOver = context.Reader.ReadBoolean();
            string winnerId = context.Reader.ReadString();
            if (winnerId.Length > 256 || winnerId != winnerId.Trim()
                || (!isGameOver && winnerId.Length != 0))
                throw new InvalidDataException("Invalid saved match winner.");

            if (context.Reader.BaseStream.Position != context.Reader.BaseStream.Length)
                throw new InvalidDataException("Unexpected trailing game mode save data.");

            return () => _resultState.RestoreResult(isGameOver, winnerId);
        }

        public Action PrepareMissingData()
            => () => _resultState.RestoreResult(false, string.Empty);
    }
}
