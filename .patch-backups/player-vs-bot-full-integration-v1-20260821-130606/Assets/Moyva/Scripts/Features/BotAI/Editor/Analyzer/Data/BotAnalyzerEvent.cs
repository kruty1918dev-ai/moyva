using System;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    public enum BotAnalyzerEventType
    {
        System = 0, TurnStarted, TurnEnded, Observation, ObservationLost,
        StrategyChanged, GoalChanged, CandidateSetChanged, TopCandidateChanged,
        UnitMoved, UnitSpawned, UnitRemoved, UnitStateChanged, CombatObserved,
        BuildingStarted, BuildingProgress, BuildingPlaced, BuildingRemoved,
        RecruitmentStarted, RecruitmentProgress, RecruitmentReady, RecruitmentDeployed,
        ResourceChanged, FogVisible, FogExplored, MemoryChanged, Warning, Error,
    }

    public enum BotAnalyzerEventSeverity
    {
        Trace = 0, Info, Decision, Action, Warning, Error,
    }

    public enum BotAnalyzerTimelineFilter
    {
        All = 0, Decision, Action, Movement, Combat, Build, Economy, Fog, Observation, Warning,
    }

    [Serializable]
    public sealed class BotAnalyzerEvent
    {
        public long Sequence;
        public double EditorTime;
        public string UtcTimestamp = string.Empty;
        public long GlobalTurn;
        public string OwnerId = string.Empty;
        public BotAnalyzerEventType Type;
        public BotAnalyzerEventSeverity Severity;
        public string ActorId = string.Empty;
        public string TargetId = string.Empty;
        public bool HasFromCell;
        public Vector2Int FromCell;
        public bool HasToCell;
        public Vector2Int ToCell;
        public string Title = string.Empty;
        public string Detail = string.Empty;
        public bool HasScore;
        public int Score;
        public override string ToString() => $"[{GlobalTurn}] {Type}: {Title}";
    }
}
