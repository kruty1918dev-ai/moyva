using System.IO;
using Kruty1918.SaveSystem;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>Save-модуль бот-опонента: персистить ідентичність бота в збереження та відновлює її при завантаженні.</summary>
    [SaveModuleId("Kruty1918.Moyva.Bootstrap.Runtime.BotOpponentSaveModule")]
    internal sealed class BotOpponentSaveModule : ISaveModule
    {
        private const int SaveMagic = unchecked((int)0x53544F42); // "BOTS"
        private const int SaveVersion = 1;
        private const int MaxIdLength = 256;

        /// <summary>Записує дані бот-опонента в контекст збереження.</summary>
        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(SaveMagic);
            context.Writer.Write(SaveVersion);
            context.Writer.Write(GameLaunchContext.HasBotOpponent
                ? GameLaunchContext.BotPlayerId ?? string.Empty
                : string.Empty);
            context.Writer.Write(GameLaunchContext.HasBotOpponent
                ? GameLaunchContext.BotDifficultyId ?? string.Empty
                : string.Empty);
        }

        /// <summary>Відновлює дані бот-опонента з контексту завантаження.</summary>
        public void OnLoad(ISaveContext context)
        {
            if (context.Reader.ReadInt32() != SaveMagic)
                throw new InvalidDataException("Invalid bot opponent save block.");
            int version = context.Reader.ReadInt32();
            if (version != SaveVersion)
                throw new InvalidDataException($"Unsupported bot opponent save version {version}.");
            string playerId = context.Reader.ReadString();
            string difficultyId = context.Reader.ReadString();
            if (playerId.Length > MaxIdLength || difficultyId.Length > MaxIdLength)
                throw new InvalidDataException("Bot opponent identity exceeds the id length bound.");
            GameLaunchContext.RestoreLoadedBotOpponent(playerId, difficultyId);
        }
    }
}
