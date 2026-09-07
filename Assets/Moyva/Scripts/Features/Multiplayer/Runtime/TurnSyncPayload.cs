using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal enum TurnSyncMessageKind : byte { RequestState = 1, EndTurn, State, Accepted, Rejected }

    internal sealed class TurnSyncPayload
    {
        private const byte Version = 1;
        private const int MaxPayloadBytes = 32768;
        public TurnSyncMessageKind Kind;
        public string Epoch = string.Empty;
        public long Revision;
        public string RequestId = string.Empty;
        public string OwnerId = string.Empty;
        public long GlobalTurn;
        public string Reason = string.Empty;
        public int Round;
        public int Actions;
        public long CalendarHours;
        public bool IsGameOver;
        public string WinnerId = string.Empty;
        public IReadOnlyList<TurnParticipantHistorySnapshot> History = Array.Empty<TurnParticipantHistorySnapshot>();

        public byte[] ToBytes()
        {
            Validate();
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(Version);
            writer.Write((byte)Kind);
            writer.Write(Epoch);
            writer.Write(Revision);
            writer.Write(RequestId);
            writer.Write(OwnerId);
            writer.Write(GlobalTurn);
            writer.Write(Reason);
            if (Kind == TurnSyncMessageKind.State)
            {
                writer.Write(Round);
                writer.Write(Actions);
                writer.Write(CalendarHours);
                writer.Write(IsGameOver);
                writer.Write(WinnerId);
                writer.Write(History.Count);
                foreach (var item in History)
                {
                    writer.Write(item.OwnerId);
                    writer.Write(item.CompletedTurns);
                    writer.Write(item.IsEliminated);
                }
            }
            writer.Flush();
            if (stream.Length > MaxPayloadBytes)
                throw new InvalidDataException("Turn message is too large.");
            return stream.ToArray();
        }

        public static TurnSyncPayload FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length > MaxPayloadBytes)
                throw new InvalidDataException("Invalid turn message size.");
            using var stream = new MemoryStream(bytes, false);
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            if (reader.ReadByte() != Version)
                throw new InvalidDataException("Unsupported turn protocol version.");
            var message = new TurnSyncPayload
            {
                Kind = (TurnSyncMessageKind)reader.ReadByte(),
                Epoch = reader.ReadString(), Revision = reader.ReadInt64(),
                RequestId = reader.ReadString(), OwnerId = reader.ReadString(),
                GlobalTurn = reader.ReadInt64(), Reason = reader.ReadString(),
            };
            if (message.Kind == TurnSyncMessageKind.State)
            {
                message.Round = reader.ReadInt32();
                message.Actions = reader.ReadInt32();
                message.CalendarHours = reader.ReadInt64();
                message.IsGameOver = reader.ReadBoolean();
                message.WinnerId = reader.ReadString();
                int count = reader.ReadInt32();
                if (count < 1 || count > 16)
                    throw new InvalidDataException("Invalid turn roster size.");
                var history = new TurnParticipantHistorySnapshot[count];
                for (int i = 0; i < count; i++)
                    history[i] = new TurnParticipantHistorySnapshot(reader.ReadString(),
                        reader.ReadInt64(), false, false, reader.ReadBoolean());
                message.History = history;
            }
            if (stream.Position != stream.Length)
                throw new InvalidDataException("Unexpected trailing turn message data.");
            message.Validate();
            return message;
        }

        private void Validate()
        {
            if (Kind < TurnSyncMessageKind.RequestState || Kind > TurnSyncMessageKind.Rejected
                || Reason == null || Reason.Length > 1024 || Revision < 0
                || OwnerId == null || OwnerId.Length > 256 || OwnerId != OwnerId.Trim())
                throw new InvalidDataException("Invalid turn message header.");
            if ((Kind != TurnSyncMessageKind.RequestState && !Guid.TryParseExact(Epoch, "N", out _))
                || (Kind != TurnSyncMessageKind.State && !Guid.TryParseExact(RequestId, "N", out _))
                || (Kind == TurnSyncMessageKind.State && RequestId.Length != 0
                    && !Guid.TryParseExact(RequestId, "N", out _)))
                throw new InvalidDataException("Invalid turn message identity.");
            if (Kind == TurnSyncMessageKind.EndTurn && (OwnerId.Length == 0 || GlobalTurn < 1))
                throw new InvalidDataException("Invalid end-turn request.");
            if (Kind != TurnSyncMessageKind.State)
                return;
            if (Revision < 1 || Round < 1 || GlobalTurn < 1 || Actions < 0 || CalendarHours < 0
                || WinnerId == null || WinnerId.Length > 256 || WinnerId != WinnerId.Trim()
                || (!IsGameOver && WinnerId.Length != 0) || History.Count < 1 || History.Count > 16)
                throw new InvalidDataException("Invalid public turn state.");

            var owners = new HashSet<string>(StringComparer.Ordinal);
            bool activeFound = false;
            bool winnerFound = WinnerId.Length == 0;
            long total = 0;
            foreach (var item in History)
            {
                if (string.IsNullOrWhiteSpace(item.OwnerId) || item.OwnerId.Length > 256
                    || item.OwnerId != item.OwnerId.Trim() || !owners.Add(item.OwnerId)
                    || item.CompletedTurns < 0 || item.CompletedTurns > GlobalTurn - 1 - total)
                    throw new InvalidDataException("Invalid turn participant history.");
                total += item.CompletedTurns;
                if (item.OwnerId == OwnerId) activeFound = IsGameOver || !item.IsEliminated;
                if (item.OwnerId == WinnerId && !item.IsEliminated) winnerFound = true;
            }
            if (!activeFound || !winnerFound || total != GlobalTurn - 1)
                throw new InvalidDataException("Turn state does not match its participant history.");
        }
    }
}
