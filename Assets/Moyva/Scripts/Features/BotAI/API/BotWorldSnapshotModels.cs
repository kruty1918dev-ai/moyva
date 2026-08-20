using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public readonly struct BotUnitSnapshot
    {
        public BotUnitSnapshot(string unitId, string ownerId, string typeId, Vector2Int position, float stamina)
        {
            UnitId = Normalize(unitId);
            OwnerId = Normalize(ownerId);
            TypeId = Normalize(typeId);
            Position = position;
            Stamina = stamina;
        }

        public string UnitId { get; }
        public string OwnerId { get; }
        public string TypeId { get; }
        public Vector2Int Position { get; }
        public float Stamina { get; }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public readonly struct BotBuildingSnapshot
    {
        public BotBuildingSnapshot(string buildingId, string ownerId, Vector2Int position)
        {
            BuildingId = Normalize(buildingId);
            OwnerId = Normalize(ownerId);
            Position = position;
        }

        public string BuildingId { get; }
        public string OwnerId { get; }
        public Vector2Int Position { get; }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class BotWorldSnapshot
    {
        public BotWorldSnapshot(
            string ownerId,
            int round,
            long globalTurn,
            TurnPhase phase,
            int actionsThisTurn,
            Vector2Int startPosition,
            IReadOnlyList<BotUnitSnapshot> ownUnits,
            IReadOnlyList<BotUnitSnapshot> visibleEnemyUnits,
            IReadOnlyList<BotBuildingSnapshot> ownBuildings,
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyRecruitmentItems,
            IReadOnlyList<BotKnownEntityMemory> memory,
            IReadOnlyList<BotBuildingSnapshot> visibleEnemyBuildings = null)
        {
            OwnerId = string.IsNullOrWhiteSpace(ownerId) ? string.Empty : ownerId.Trim();
            Round = round;
            GlobalTurn = globalTurn;
            Phase = phase;
            ActionsThisTurn = actionsThisTurn;
            StartPosition = startPosition;
            OwnUnits = ownUnits ?? Array.Empty<BotUnitSnapshot>();
            VisibleEnemyUnits = visibleEnemyUnits ?? Array.Empty<BotUnitSnapshot>();
            OwnBuildings = ownBuildings ?? Array.Empty<BotBuildingSnapshot>();
            ReadyRecruitmentItems = readyRecruitmentItems ?? Array.Empty<UnitRecruitmentQueueItemSnapshot>();
            Memory = memory ?? Array.Empty<BotKnownEntityMemory>();
            VisibleEnemyBuildings = visibleEnemyBuildings ?? Array.Empty<BotBuildingSnapshot>();
        }

        public string OwnerId { get; }
        public int Round { get; }
        public long GlobalTurn { get; }
        public TurnPhase Phase { get; }
        public int ActionsThisTurn { get; }
        public Vector2Int StartPosition { get; }
        public IReadOnlyList<BotUnitSnapshot> OwnUnits { get; }
        public IReadOnlyList<BotUnitSnapshot> VisibleEnemyUnits { get; }
        public IReadOnlyList<BotBuildingSnapshot> OwnBuildings { get; }
        public IReadOnlyList<BotBuildingSnapshot> VisibleEnemyBuildings { get; }
        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> ReadyRecruitmentItems { get; }
        public IReadOnlyList<BotKnownEntityMemory> Memory { get; }
    }
}
