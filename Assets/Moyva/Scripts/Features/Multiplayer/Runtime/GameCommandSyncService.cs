using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Надсилає ігрові команди всім пірам (broadcast) та диспетчеризує вхідні команди.
    /// Перший байт payload — тип команди (GameCommandType).
    /// </summary>
    internal sealed class GameCommandSyncService : IGameCommandSyncService, IDisposable
    {
        private readonly INetworkProvider _network;
        private readonly IMultiplayerLogger _logger;
        private readonly Dictionary<GameCommandType, Action<string, byte[]>> _handlers = new();
        private readonly IDisposable _subscription;

        public GameCommandSyncService(INetworkProvider network, IMultiplayerLogger logger)
        {
            _network = network;
            _logger  = logger;

            _subscription = _network.Messages.Subscribe(OnMessageReceived);
        }

        public void SendCommand(GameCommandType type, byte[] payload)
        {
            var packet = BuildPacket(type, payload);
            _network.SendMessageAsync("*", packet, CancellationToken.None);
        }

        public void RegisterHandler(GameCommandType type, Action<string, byte[]> handler)
        {
            _handlers[type] = handler;
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        private void OnMessageReceived(NetworkMessage message)
        {
            if (message.Payload == null || message.Payload.Length < 1)
                return;

            var type = (GameCommandType)message.Payload[0];

            if (!_handlers.TryGetValue(type, out var handler))
            {
                _logger.Trace($"GameCommandSyncService: немає обробника для {type}");
                return;
            }

            var body = new byte[message.Payload.Length - 1];
            if (body.Length > 0)
                Buffer.BlockCopy(message.Payload, 1, body, 0, body.Length);

            handler(message.SenderId, body);
        }

        private static byte[] BuildPacket(GameCommandType type, byte[] payload)
        {
            var packet = new byte[1 + (payload?.Length ?? 0)];
            packet[0] = (byte)type;
            if (payload != null && payload.Length > 0)
                Buffer.BlockCopy(payload, 0, packet, 1, payload.Length);
            return packet;
        }
    }
}
