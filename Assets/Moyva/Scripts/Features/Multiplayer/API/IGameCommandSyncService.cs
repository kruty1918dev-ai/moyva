using System;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Серіалізує та надсилає ігрові команди іншим учасникам.
    /// Також приймає вхідні команди і направляє їх до відповідних ігрових підсистем.
    /// </summary>
    public interface IGameCommandSyncService
    {
        void SendCommand(GameCommandType type, byte[] payload);

        /// <summary>
        /// Sends a single command to the specified peer (unicast).
        /// Used for one-shot late-join catch-up payloads such as world snapshots.
        /// </summary>
        void SendCommandToPeer(string peerId, GameCommandType type, byte[] payload);

        void RegisterHandler(GameCommandType type, Action<string, byte[]> handler);
    }
}
