namespace Kruty1918.Moyva.BotAI.API
{
    /// <summary>
    /// Single authoritative entry point for one bot faction turn.
    ///
    /// The turn driver calls this boundary at most once for a GlobalTurn. The
    /// implementation may issue asynchronous gameplay commands; normal turn
    /// blockers remain responsible for preventing End Turn until those commands
    /// have settled.
    /// </summary>
    public interface IBotTurnExecutor
    {
        /// <summary>
        /// Begins the bot's work for the supplied authoritative owner/turn pair.
        /// Returns false only when execution could not be started safely.
        /// </summary>
        bool TryBeginTurn(string ownerId, long globalTurn, out string reason);
    }
}
