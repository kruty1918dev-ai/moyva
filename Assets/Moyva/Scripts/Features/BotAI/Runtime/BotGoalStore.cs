using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotGoalStore : IBotGoalStore
    {
        private readonly Dictionary<string, BotGoalSnapshot> _goals = new(StringComparer.Ordinal);
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotGoalStore([InjectOptional] BotPlanningProfile profile = null)
        {
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public BotGoalSnapshot Observe(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.OwnerId))
                return default;

            BotGoalSnapshot proposed = Propose(snapshot, strategy);
            if (_goals.TryGetValue(snapshot.OwnerId, out BotGoalSnapshot previous))
            {
                bool emergency = proposed.Kind == BotGoalKind.DefendBase;
                bool hold = snapshot.GlobalTurn <= previous.MinimumHoldUntilTurn;
                if (!emergency && hold && previous.Kind != BotGoalKind.None)
                    return previous;
            }

            _goals[snapshot.OwnerId] = proposed;
            return proposed;
        }

        public bool TryGet(string ownerId, out BotGoalSnapshot goal)
            => _goals.TryGetValue(string.IsNullOrWhiteSpace(ownerId) ? string.Empty : ownerId.Trim(), out goal);

        public IReadOnlyList<BotGoalSnapshot> Capture()
        {
            var owners = new List<string>(_goals.Keys);
            owners.Sort(StringComparer.Ordinal);
            var result = new List<BotGoalSnapshot>(owners.Count);
            for (int i = 0; i < owners.Count; i++) result.Add(_goals[owners[i]]);
            return result;
        }

        public void Restore(IReadOnlyList<BotGoalSnapshot> goals)
        {
            _goals.Clear();
            if (goals == null) return;
            for (int i = 0; i < goals.Count; i++)
            {
                BotGoalSnapshot goal = goals[i];
                if (!string.IsNullOrWhiteSpace(goal.OwnerId) && goal.Kind != BotGoalKind.None)
                    _goals[goal.OwnerId] = goal;
            }
        }

        private BotGoalSnapshot Propose(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            BotGoalKind kind;
            int priority;
            string reason;
            if (strategy.Posture == BotStrategicPosture.EmergencyDefense)
            { kind = BotGoalKind.DefendBase; priority = 1000; reason = "Emergency posture overrides strategic hysteresis."; }
            else if (snapshot.OwnBuildings.Count == 0)
            { kind = BotGoalKind.StabilizeOpening; priority = 900; reason = "No owned infrastructure."; }
            else if (snapshot.VisibleEnemyUnits.Count > 0)
            { kind = BotGoalKind.PressureEnemy; priority = 650; reason = "Visible enemy contact."; }
            else if (snapshot.OwnUnits.Count < 3)
            { kind = BotGoalKind.BuildArmy; priority = 700; reason = "Army below baseline."; }
            else if (strategy.Posture == BotStrategicPosture.Siege)
            { kind = BotGoalKind.SiegeObjective; priority = 760; reason = "Known objective and siege posture."; }
            else if (strategy.Posture == BotStrategicPosture.Search)
            { kind = BotGoalKind.SearchEnemy; priority = 550; reason = "No visible contact; preserve search intent."; }
            else
            { kind = BotGoalKind.DevelopEconomy; priority = 400; reason = "No urgent tactical condition."; }

            string targetId = snapshot.VisibleEnemyUnits.Count > 0 ? snapshot.VisibleEnemyUnits[0].UnitId : string.Empty;
            return new BotGoalSnapshot(
                snapshot.OwnerId,
                kind,
                snapshot.GlobalTurn,
                snapshot.GlobalTurn + _profile.GoalHysteresisTurns,
                priority,
                targetId,
                null,
                reason);
        }
    }
}
