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
        void RegisterHandler(GameCommandType type, Action<string, byte[]> handler);
    }
}
