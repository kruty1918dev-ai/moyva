using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Тип повідомлення для ігрових дій: запит від клієнта або підтвердження від хоста.
    /// </summary>
    public enum GameActionMessageKind : byte
    {
        /// <summary>Запит клієнта до хоста — обробити і підтвердити.</summary>
        Request   = 0,
        /// <summary>Підтверджена дія від хоста — застосувати локально.</summary>
        Confirmed = 1,
    }

    /// <summary>Payload для розміщення будівлі (BuildingPlace).</summary>
    public readonly struct BuildingPlacePayload
    {
        public readonly GameActionMessageKind Kind;
        public readonly string BuildingId;
        public readonly Vector2Int Position;
        public readonly string OwnerId;
        public readonly string SourceFactionId;

        public BuildingPlacePayload(GameActionMessageKind kind, string buildingId, Vector2Int position,
                                    string ownerId, string sourceFactionId)
        {
            Kind            = kind;
            BuildingId      = buildingId;
            Position        = position;
            OwnerId         = ownerId;
            SourceFactionId = sourceFactionId;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write(BuildingId      ?? "");
            w.Write(Position.x);
            w.Write(Position.y);
            w.Write(OwnerId         ?? "");
            w.Write(SourceFactionId ?? "");
            return ms.ToArray();
        }

        public static BuildingPlacePayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new BuildingPlacePayload(
                (GameActionMessageKind)r.ReadByte(),
                r.ReadString(),
                new Vector2Int(r.ReadInt32(), r.ReadInt32()),
                r.ReadString(),
                r.ReadString());
        }
    }

    /// <summary>Payload для знесення будівлі (BuildingDemolish).</summary>
    public readonly struct BuildingDemolishPayload
    {
        public readonly GameActionMessageKind Kind;
        public readonly Vector2Int Position;
        public readonly string OwnerId;

        public BuildingDemolishPayload(GameActionMessageKind kind, Vector2Int position, string ownerId)
        {
            Kind     = kind;
            Position = position;
            OwnerId  = ownerId;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write(Position.x);
            w.Write(Position.y);
            w.Write(OwnerId ?? "");
            return ms.ToArray();
        }

        public static BuildingDemolishPayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new BuildingDemolishPayload(
                (GameActionMessageKind)r.ReadByte(),
                new Vector2Int(r.ReadInt32(), r.ReadInt32()),
                r.ReadString());
        }
    }

    /// <summary>
    /// Payload для руху юніта (UnitMove).
    /// Request: клієнт запитує рух до TargetPosition.
    /// Confirmed: хост підтверджує — всі клієнти стартують MoveUnitAsync до TargetPosition.
    /// </summary>
    public readonly struct UnitMovePayload
    {
        public readonly GameActionMessageKind Kind;
        public readonly string UnitId;
        public readonly Vector2Int TargetPosition;

        public UnitMovePayload(GameActionMessageKind kind, string unitId, Vector2Int targetPosition)
        {
            Kind           = kind;
            UnitId         = unitId;
            TargetPosition = targetPosition;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write(UnitId ?? "");
            w.Write(TargetPosition.x);
            w.Write(TargetPosition.y);
            return ms.ToArray();
        }

        public static UnitMovePayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new UnitMovePayload(
                (GameActionMessageKind)r.ReadByte(),
                r.ReadString(),
                new Vector2Int(r.ReadInt32(), r.ReadInt32()));
        }
    }

    /// <summary>
    /// Payload для спавну юніта (UnitSpawn).
    /// Confirmed містить AssignedUnitId, згенерований хостом,
    /// щоб всі клієнти мали однакові ID юнітів.
    /// </summary>
    public readonly struct UnitSpawnPayload
    {
        public readonly GameActionMessageKind Kind;
        /// <summary>ID призначений хостом. Порожній у Request.</summary>
        public readonly string AssignedUnitId;
        public readonly string UnitTypeId;
        public readonly Vector2Int Position;
        public readonly string OwnerId;

        public UnitSpawnPayload(GameActionMessageKind kind, string assignedUnitId, string unitTypeId,
                                Vector2Int position, string ownerId)
        {
            Kind           = kind;
            AssignedUnitId = assignedUnitId;
            UnitTypeId     = unitTypeId;
            Position       = position;
            OwnerId        = ownerId;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write(AssignedUnitId ?? "");
            w.Write(UnitTypeId     ?? "");
            w.Write(Position.x);
            w.Write(Position.y);
            w.Write(OwnerId        ?? "");
            return ms.ToArray();
        }

        public static UnitSpawnPayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new UnitSpawnPayload(
                (GameActionMessageKind)r.ReadByte(),
                r.ReadString(),
                r.ReadString(),
                new Vector2Int(r.ReadInt32(), r.ReadInt32()),
                r.ReadString());
        }
    }
}
