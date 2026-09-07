using System.IO;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
        /// <summary>Відмова хоста — показати причину автору запиту.</summary>
        Rejected = 2,
    }

    /// <summary>Payload для розміщення будівлі (BuildingPlace).</summary>
    public readonly struct BuildingPlacePayload
    {
        private const byte IntentExtensionMarker = 0xA7;
        private const byte IntentExtensionVersion = 2;

        public readonly GameActionMessageKind Kind;
        public readonly string BuildingId;
        public readonly Vector2Int Position;
        public readonly string OwnerId;
        public readonly string SourceFactionId;
        public readonly bool HasRelocationSource;
        public readonly Vector2Int RelocationSourcePosition;
        public readonly string SatisfiedReplacementBuildingId;
        public readonly ConstructionRotation Rotation;

        public BuildingPlacePayload(GameActionMessageKind kind, string buildingId, Vector2Int position,
                                    string ownerId, string sourceFactionId,
                                    bool hasRelocationSource = false,
                                    Vector2Int relocationSourcePosition = default,
                                    string satisfiedReplacementBuildingId = null,
                                    ConstructionRotation rotation =
                                        ConstructionRotation.Degrees0)
        {
            Kind            = kind;
            BuildingId      = buildingId;
            Position        = position;
            OwnerId         = ownerId;
            SourceFactionId = sourceFactionId;
            HasRelocationSource = hasRelocationSource;
            RelocationSourcePosition = relocationSourcePosition;
            SatisfiedReplacementBuildingId =
                satisfiedReplacementBuildingId;
            Rotation = ConstructionRotationUtility.Normalize(
                (int)rotation);
        }

        public ConstructionPlacementCommitIntent ToCommitIntent()
            => new ConstructionPlacementCommitIntent(
                HasRelocationSource
                    ? RelocationSourcePosition
                    : (Vector2Int?)null,
                SatisfiedReplacementBuildingId,
                Rotation);

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
            // Appended extension keeps existing readers compatible: the legacy
            // prefix is unchanged and old payloads simply end before this marker.
            w.Write(IntentExtensionMarker);
            w.Write(IntentExtensionVersion);
            w.Write(HasRelocationSource);
            w.Write(RelocationSourcePosition.x);
            w.Write(RelocationSourcePosition.y);
            w.Write(SatisfiedReplacementBuildingId ?? "");
            w.Write((byte)Rotation);
            return ms.ToArray();
        }

        public static BuildingPlacePayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            GameActionMessageKind kind =
                (GameActionMessageKind)r.ReadByte();
            string buildingId = r.ReadString();
            var position =
                new Vector2Int(r.ReadInt32(), r.ReadInt32());
            string ownerId = r.ReadString();
            string sourceFactionId = r.ReadString();

            bool hasRelocationSource = false;
            Vector2Int relocationSourcePosition = default;
            string satisfiedReplacementBuildingId = null;
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0;
            if (ms.Position < ms.Length)
            {
                byte marker = r.ReadByte();
                if (marker != IntentExtensionMarker)
                {
                    throw new InvalidDataException(
                        $"Unknown BuildingPlace payload extension marker 0x{marker:X2}.");
                }

                byte version = r.ReadByte();
                if (version != 1
                    && version != IntentExtensionVersion)
                {
                    throw new InvalidDataException(
                        $"Unsupported BuildingPlace payload extension version {version}.");
                }

                hasRelocationSource = r.ReadBoolean();
                relocationSourcePosition =
                    new Vector2Int(r.ReadInt32(), r.ReadInt32());
                satisfiedReplacementBuildingId = r.ReadString();
                if (version >= 2)
                {
                    rotation = ConstructionRotationUtility.Normalize(
                        r.ReadByte());
                }
            }

            return new BuildingPlacePayload(
                kind,
                buildingId,
                position,
                ownerId,
                sourceFactionId,
                hasRelocationSource,
                relocationSourcePosition,
                satisfiedReplacementBuildingId,
                rotation);
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

    public enum CaravanCommandAction : byte
    {
        Transfer = 0,
        StartRoute = 1,
        StopRoute = 2,
        FoundSettlement = 3,
    }

    public readonly struct CaravanCommandPayload
    {
        public readonly GameActionMessageKind Kind;
        public readonly CaravanCommandAction Action;
        public readonly string OwnerId;
        public readonly string UnitId;
        public readonly byte CargoOperation;
        public readonly string SettlementId;
        public readonly string WarehouseKey;
        public readonly string TargetSettlementId;
        public readonly string TargetWarehouseKey;
        public readonly bool Repeat;
        public readonly string BuildingId;
        public readonly IReadOnlyDictionary<string, float> Resources;
        public readonly string RequestId;
        public readonly byte RoutePhase;
        public readonly string RejectionReason;

        public CaravanCommandPayload(GameActionMessageKind kind, CaravanCommandAction action,
            string ownerId, string unitId, byte cargoOperation, string settlementId, string warehouseKey,
            string targetSettlementId, string targetWarehouseKey, bool repeat, string buildingId,
            IReadOnlyDictionary<string, float> resources, string requestId = null, byte routePhase = byte.MaxValue,
            string rejectionReason = null)
        {
            Kind = kind;
            Action = action;
            OwnerId = ownerId ?? string.Empty;
            UnitId = unitId ?? string.Empty;
            CargoOperation = cargoOperation;
            SettlementId = settlementId ?? string.Empty;
            WarehouseKey = warehouseKey ?? string.Empty;
            TargetSettlementId = targetSettlementId ?? string.Empty;
            TargetWarehouseKey = targetWarehouseKey ?? string.Empty;
            Repeat = repeat;
            BuildingId = buildingId ?? string.Empty;
            Resources = resources ?? new Dictionary<string, float>();
            RequestId = requestId ?? string.Empty;
            RoutePhase = routePhase;
            RejectionReason = rejectionReason ?? string.Empty;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write((byte)Action);
            w.Write(OwnerId);
            w.Write(UnitId);
            w.Write(CargoOperation);
            w.Write(SettlementId);
            w.Write(WarehouseKey);
            w.Write(TargetSettlementId);
            w.Write(TargetWarehouseKey);
            w.Write(Repeat);
            w.Write(BuildingId);
            w.Write(RequestId);
            w.Write(Resources.Count);
            foreach (var pair in Resources)
            {
                w.Write(pair.Key ?? string.Empty);
                w.Write(pair.Value);
            }
            w.Write(RoutePhase);
            w.Write(RejectionReason);
            return ms.ToArray();
        }

        public static CaravanCommandPayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data ?? System.Array.Empty<byte>());
            using var r = new BinaryReader(ms);
            var kind = (GameActionMessageKind)r.ReadByte();
            var action = (CaravanCommandAction)r.ReadByte();
            string ownerId = r.ReadString();
            string unitId = r.ReadString();
            byte operation = r.ReadByte();
            string settlementId = r.ReadString();
            string warehouseKey = r.ReadString();
            string targetSettlementId = r.ReadString();
            string targetWarehouseKey = r.ReadString();
            bool repeat = r.ReadBoolean();
            string buildingId = r.ReadString();
            string requestId = r.ReadString();
            int count = r.ReadInt32();
            if (count < 0 || count > 256)
                throw new InvalidDataException("Invalid caravan resource count.");
            var resources = new Dictionary<string, float>(System.StringComparer.Ordinal);
            for (int index = 0; index < count; index++)
            {
                string id = r.ReadString();
                float amount = r.ReadSingle();
                if (string.IsNullOrWhiteSpace(id) || resources.ContainsKey(id)
                    || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                    throw new InvalidDataException("Invalid caravan resource entry.");
                resources.Add(id, amount);
            }
            byte routePhase = byte.MaxValue;
            if (ms.Position < ms.Length)
                routePhase = r.ReadByte();
            string rejectionReason = string.Empty;
            if (ms.Position < ms.Length)
                rejectionReason = r.ReadString();
            if (ms.Position != ms.Length)
                throw new InvalidDataException("Unexpected trailing caravan payload data.");
            return new CaravanCommandPayload(kind, action, ownerId, unitId, operation,
                settlementId, warehouseKey, targetSettlementId, targetWarehouseKey, repeat, buildingId, resources,
                requestId, routePhase, rejectionReason);
        }
    }

    public readonly struct CombatCommandPayload
    {
        public readonly GameActionMessageKind Kind;
        public readonly string RequesterOwnerId;
        public readonly string AttackerEntityId;
        public readonly string TargetEntityId;
        public readonly int DamageApplied;
        public readonly bool TargetDied;
        public readonly string RequestId;
        public readonly string RejectionReason;

        public CombatCommandPayload(
            GameActionMessageKind kind,
            string requesterOwnerId,
            string attackerEntityId,
            string targetEntityId,
            int damageApplied,
            bool targetDied,
            string requestId = null,
            string rejectionReason = null)
        {
            Kind = kind;
            RequesterOwnerId = requesterOwnerId ?? string.Empty;
            AttackerEntityId = attackerEntityId ?? string.Empty;
            TargetEntityId = targetEntityId ?? string.Empty;
            DamageApplied = damageApplied < 0 ? 0 : damageApplied;
            TargetDied = targetDied;
            RequestId = requestId ?? string.Empty;
            RejectionReason = rejectionReason ?? string.Empty;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write(RequesterOwnerId);
            w.Write(AttackerEntityId);
            w.Write(TargetEntityId);
            w.Write(DamageApplied);
            w.Write(TargetDied);
            w.Write(RequestId);
            w.Write(RejectionReason);
            return ms.ToArray();
        }

        public static CombatCommandPayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data ?? System.Array.Empty<byte>());
            using var r = new BinaryReader(ms);
            var payload = new CombatCommandPayload(
                (GameActionMessageKind)r.ReadByte(),
                r.ReadString(),
                r.ReadString(),
                r.ReadString(),
                r.ReadInt32(),
                r.ReadBoolean(),
                r.ReadString());
            string rejectionReason = ms.Position < ms.Length ? r.ReadString() : string.Empty;
            if (ms.Position != ms.Length)
                throw new InvalidDataException("Unexpected trailing combat payload data.");
            return new CombatCommandPayload(
                payload.Kind,
                payload.RequesterOwnerId,
                payload.AttackerEntityId,
                payload.TargetEntityId,
                payload.DamageApplied,
                payload.TargetDied,
                payload.RequestId,
                rejectionReason);
        }
    }

    public readonly struct SettlementCaptureCommandPayload
    {
        public readonly GameActionMessageKind Kind;
        public readonly string RequesterOwnerId;
        public readonly string UnitId;
        public readonly string TargetEntityId;
        public readonly Vector2Int TargetPosition;
        public readonly string PreviousOwnerId;
        public readonly string NewOwnerId;
        public readonly string RequestId;
        public readonly string RejectionReason;

        public SettlementCaptureCommandPayload(
            GameActionMessageKind kind,
            string requesterOwnerId,
            string unitId,
            string targetEntityId,
            Vector2Int targetPosition,
            string previousOwnerId,
            string newOwnerId,
            string requestId = null,
            string rejectionReason = null)
        {
            Kind = kind;
            RequesterOwnerId = requesterOwnerId ?? string.Empty;
            UnitId = unitId ?? string.Empty;
            TargetEntityId = targetEntityId ?? string.Empty;
            TargetPosition = targetPosition;
            PreviousOwnerId = previousOwnerId ?? string.Empty;
            NewOwnerId = newOwnerId ?? string.Empty;
            RequestId = requestId ?? string.Empty;
            RejectionReason = rejectionReason ?? string.Empty;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write((byte)Kind);
            w.Write(RequesterOwnerId);
            w.Write(UnitId);
            w.Write(TargetEntityId);
            w.Write(TargetPosition.x);
            w.Write(TargetPosition.y);
            w.Write(PreviousOwnerId);
            w.Write(NewOwnerId);
            w.Write(RequestId);
            w.Write(RejectionReason);
            return ms.ToArray();
        }

        public static SettlementCaptureCommandPayload FromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data ?? System.Array.Empty<byte>());
            using var r = new BinaryReader(ms);
            var payload = new SettlementCaptureCommandPayload(
                (GameActionMessageKind)r.ReadByte(),
                r.ReadString(),
                r.ReadString(),
                r.ReadString(),
                new Vector2Int(r.ReadInt32(), r.ReadInt32()),
                r.ReadString(),
                r.ReadString(),
                r.ReadString());
            string rejectionReason = ms.Position < ms.Length ? r.ReadString() : string.Empty;
            if (ms.Position != ms.Length)
                throw new InvalidDataException("Unexpected trailing settlement capture payload data.");
            payload = new SettlementCaptureCommandPayload(
                payload.Kind,
                payload.RequesterOwnerId,
                payload.UnitId,
                payload.TargetEntityId,
                payload.TargetPosition,
                payload.PreviousOwnerId,
                payload.NewOwnerId,
                payload.RequestId,
                rejectionReason);
            return payload;
        }
    }
}
