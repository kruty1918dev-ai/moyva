using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    [Serializable]
    public sealed class BotAnalyzerFrame
    {
        public long Sequence;
        public double EditorTime;
        public string UtcTimestamp = string.Empty;
        public string OwnerId = string.Empty;
        public int Round;
        public long GlobalTurn;
        public string Phase = string.Empty;
        public int ActionsThisTurn;
        public string ActiveOwnerId = string.Empty;
        public bool SelectedBotActive;
        public BotAnalyzerStrategyState Strategy = new();
        public BotAnalyzerGoalState Goal = new();
        public List<BotAnalyzerResourceState> Resources = new();
        public List<BotAnalyzerUnitState> OwnUnits = new();
        public List<BotAnalyzerUnitState> VisibleEnemyUnits = new();
        public List<BotAnalyzerBuildingState> OwnBuildings = new();
        public List<BotAnalyzerBuildingState> VisibleEnemyBuildings = new();
        public List<BotAnalyzerRecruitmentState> Recruitment = new();
        public BotAnalyzerFogState Fog = new();
        public List<BotAnalyzerCandidateState> Candidates = new();
        public List<BotAnalyzerMemoryState> Memory = new();

        [NonSerialized] public HashSet<string> ExistingUnitIds = new(StringComparer.Ordinal);
        [NonSerialized] public HashSet<string> ExistingBuildingKeys = new(StringComparer.Ordinal);

        public static string BuildingKey(string ownerId, string buildingId, Vector2Int cell)
            => $"{ownerId ?? string.Empty}|{buildingId ?? string.Empty}|{cell.x},{cell.y}";
    }

    [Serializable] public sealed class BotAnalyzerStrategyState
    {
        public bool Available;
        public string Posture = string.Empty;
        public int Score;
        public string Reason = string.Empty;
        public long GlobalTurn;
    }

    [Serializable] public sealed class BotAnalyzerGoalState
    {
        public bool Available;
        public string Kind = string.Empty;
        public int Priority;
        public long CreatedTurn;
        public long HoldUntilTurn;
        public string TargetId = string.Empty;
        public bool HasTargetCell;
        public Vector2Int TargetCell;
        public string Reason = string.Empty;
    }

    [Serializable] public sealed class BotAnalyzerResourceState
    {
        public string ResourceId = string.Empty;
        public string DisplayName = string.Empty;
        public float Amount;
    }

    [Serializable] public sealed class BotAnalyzerUnitState
    {
        public string UnitId = string.Empty;
        public string OwnerId = string.Empty;
        public string TypeId = string.Empty;
        public Vector2Int Cell;
        public float Stamina;
        public string TacticalRole = string.Empty;
    }

    [Serializable] public sealed class BotAnalyzerBuildingState
    {
        public string BuildingId = string.Empty;
        public string OwnerId = string.Empty;
        public Vector2Int Cell;
        public bool Operational;
        public bool HasProgress;
        public int CompletedTurns;
        public int RequiredTurns;
    }

    [Serializable] public sealed class BotAnalyzerRecruitmentState
    {
        public long QueueId;
        public string OwnerId = string.Empty;
        public Vector2Int BuildingCell;
        public string BuildingId = string.Empty;
        public string UnitTypeId = string.Empty;
        public int CompletedTurns;
        public int TrainingTurns;
        public int RemainingTurns;
        public long EnqueuedGlobalTurn;
        public long LastProgressGlobalTurn;
        public string Status = string.Empty;
        public bool Ready;
    }

    [Serializable] public sealed class BotAnalyzerFogState
    {
        public int GridWidth;
        public int GridHeight;
        public List<Vector2Int> VisibleCells = new();
        public List<Vector2Int> ExploredCells = new();

        public int TotalCells => Math.Max(0, GridWidth) * Math.Max(0, GridHeight);
        public int VisibleCount => VisibleCells?.Count ?? 0;
        public int ExploredCount => ExploredCells?.Count ?? 0;
        public int KnownCount => VisibleCount + ExploredCount;
        public float KnownPercent => TotalCells <= 0 ? 0f : KnownCount * 100f / TotalCells;
        public float VisiblePercent => TotalCells <= 0 ? 0f : VisibleCount * 100f / TotalCells;
    }

    [Serializable] public sealed class BotAnalyzerCandidateState
    {
        public int Rank;
        public long GlobalTurn;
        public string Posture = string.Empty;
        public string CandidateId = string.Empty;
        public string Kind = string.Empty;
        public int Score;
        public string Explanation = string.Empty;
        public string ActorId = string.Empty;
        public string TargetId = string.Empty;
        public bool HasTargetCell;
        public Vector2Int TargetCell;
        public string DefinitionId = string.Empty;
        public string Reason = string.Empty;
    }

    [Serializable] public sealed class BotAnalyzerMemoryState
    {
        public string EntityId = string.Empty;
        public string Kind = string.Empty;
        public string OwnerId = string.Empty;
        public string TypeId = string.Empty;
        public Vector2Int LastKnownCell;
        public long LastSeenGlobalTurn;
        public int LastKnownHp;
        public bool ConfirmedDestroyed;
    }
}
