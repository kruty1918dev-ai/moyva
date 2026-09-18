using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    internal static class MultiplayerFrameCodec
    {
        public static void PumpHostConnections(
            ref NetworkDriver driver,
            ref NativeList<NetworkConnection> connections,
            Dictionary<int, string> connectionPlayerIds,
            Action<NetworkConnection, DataStreamReader, bool> handleFrame,
            Action<string> peerDisconnected)
        {
            NetworkConnection incoming;
            while ((incoming = driver.Accept()) != default)
                connections.Add(incoming);

            for (int index = 0; index < connections.Length; index++)
            {
                NetworkConnection connection = connections[index];
                if (!connection.IsCreated)
                {
                    connections.RemoveAtSwapBack(index--);
                    continue;
                }

                NetworkEvent.Type eventType;
                while ((eventType = driver.PopEventForConnection(
                           connection,
                           out DataStreamReader stream))
                       != NetworkEvent.Type.Empty)
                {
                    switch (eventType)
                    {
                        case NetworkEvent.Type.Data:
                            handleFrame(
                                connection,
                                stream,
                                true);
                            break;
                        case NetworkEvent.Type.Disconnect:
                            int key = connection.GetHashCode();
                            if (connectionPlayerIds.TryGetValue(
                                    key,
                                    out string peerId))
                            {
                                connectionPlayerIds.Remove(key);
                                peerDisconnected?.Invoke(peerId);
                            }

                            connections[index] = default;
                            break;
                    }
                }
            }
        }

        public static byte[] BuildUserDataFrame(
            byte frameType,
            string senderId,
            string targetPeerId,
            byte[] payload)
        {
            byte[] targetBytes = Encoding.UTF8.GetBytes(
                string.IsNullOrWhiteSpace(targetPeerId)
                    ? "*"
                    : targetPeerId);
            byte[] senderBytes = Encoding.UTF8.GetBytes(
                senderId ?? string.Empty);
            byte[] safePayload = payload ?? Array.Empty<byte>();

            var body = new byte[
                2 + targetBytes.Length
                + 2 + senderBytes.Length
                + safePayload.Length];
            int offset = 0;
            WriteUShort(
                body,
                ref offset,
                (ushort)targetBytes.Length);
            Buffer.BlockCopy(
                targetBytes,
                0,
                body,
                offset,
                targetBytes.Length);
            offset += targetBytes.Length;
            WriteUShort(
                body,
                ref offset,
                (ushort)senderBytes.Length);
            Buffer.BlockCopy(
                senderBytes,
                0,
                body,
                offset,
                senderBytes.Length);
            offset += senderBytes.Length;
            if (safePayload.Length > 0)
            {
                Buffer.BlockCopy(
                    safePayload,
                    0,
                    body,
                    offset,
                    safePayload.Length);
            }

            return Wrap(frameType, body);
        }

        public static bool TryParseUserData(
            byte[] body,
            out string target,
            out string senderId,
            out byte[] payload)
        {
            target = null;
            senderId = null;
            payload = null;
            if (body == null || body.Length < 4)
                return false;

            int offset = 0;
            ushort targetLength = ReadUShort(body, ref offset);
            if (offset + targetLength > body.Length)
                return false;

            target = Encoding.UTF8.GetString(
                body,
                offset,
                targetLength);
            offset += targetLength;

            if (offset + 2 > body.Length)
                return false;

            ushort senderLength = ReadUShort(body, ref offset);
            if (offset + senderLength > body.Length)
                return false;

            senderId = Encoding.UTF8.GetString(
                body,
                offset,
                senderLength);
            offset += senderLength;

            int payloadLength = body.Length - offset;
            payload = new byte[payloadLength];
            if (payloadLength > 0)
            {
                Buffer.BlockCopy(
                    body,
                    offset,
                    payload,
                    0,
                    payloadLength);
            }

            return true;
        }

        public static byte[] Wrap(byte type, byte[] body)
        {
            body ??= Array.Empty<byte>();
            var frame = new byte[3 + body.Length];
            frame[0] = type;
            frame[1] = (byte)(body.Length & 0xFF);
            frame[2] = (byte)((body.Length >> 8) & 0xFF);
            if (body.Length > 0)
                Buffer.BlockCopy(body, 0, frame, 3, body.Length);
            return frame;
        }

        private static void WriteUShort(
            byte[] buffer,
            ref int offset,
            ushort value)
        {
            buffer[offset++] = (byte)(value & 0xFF);
            buffer[offset++] = (byte)((value >> 8) & 0xFF);
        }

        private static ushort ReadUShort(
            byte[] buffer,
            ref int offset)
        {
            ushort value = (ushort)(
                buffer[offset]
                | (buffer[offset + 1] << 8));
            offset += 2;
            return value;
        }
    }
}
