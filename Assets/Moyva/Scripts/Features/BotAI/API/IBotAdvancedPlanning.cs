using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotUnitRoleResolver
    {
        BotUnitTacticalRole Resolve(UnitClassConfig config);
    }

    public interface IBotInfluenceMapService
    {
        BotInfluenceScore Evaluate(BotWorldSnapshot snapshot, Vector2Int position);
    }

    public interface IBotTacticalSequencer
    {
        IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy);
    }

    public interface IBotRecruitmentPlanner
    {
        IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy);
    }

    public interface IBotScoutingPlanner
    {
        IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy);
    }

    public interface IBotGoalStore
    {
        BotGoalSnapshot Observe(BotWorldSnapshot snapshot, BotStrategicContext strategy);
        bool TryGet(string ownerId, out BotGoalSnapshot goal);
        IReadOnlyList<BotGoalSnapshot> Capture();
        void Restore(IReadOnlyList<BotGoalSnapshot> goals);
    }

    public interface IBotDecisionTrace
    {
        void Record(string ownerId, long globalTurn, BotStrategicContext strategy, IReadOnlyList<BotActionCandidate> candidates);
        IReadOnlyList<BotDecisionTraceEntry> GetLast(string ownerId);
    }
}
