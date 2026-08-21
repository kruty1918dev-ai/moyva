using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerDiffEngineTests
    {
        [Test]
        public void Diff_ReportsStrategyGoalMovementAndResourceChanges()
        {
            var before = Frame(10);
            before.Strategy = new BotAnalyzerStrategyState { Available = true, Posture = "Search", Score = 480, Reason = "No contact." };
            before.Goal = new BotAnalyzerGoalState { Available = true, Kind = "SearchEnemy", Priority = 550, Reason = "Search." };
            before.OwnUnits.Add(new BotAnalyzerUnitState { UnitId = "u1", OwnerId = "bot", TypeId = "warrior", Cell = new Vector2Int(1, 1), Stamina = 10 });
            before.Resources.Add(new BotAnalyzerResourceState { ResourceId = "wood", DisplayName = "Wood", Amount = 100 });

            var after = Frame(10);
            after.Strategy = new BotAnalyzerStrategyState { Available = true, Posture = "Pressure", Score = 650, Reason = "Visible contact." };
            after.Goal = new BotAnalyzerGoalState { Available = true, Kind = "PressureEnemy", Priority = 650, Reason = "Contact." };
            after.OwnUnits.Add(new BotAnalyzerUnitState { UnitId = "u1", OwnerId = "bot", TypeId = "warrior", Cell = new Vector2Int(2, 1), Stamina = 10 });
            after.Resources.Add(new BotAnalyzerResourceState { ResourceId = "wood", DisplayName = "Wood", Amount = 70 });

            var events = new BotAnalyzerDiffEngine().Diff(before, after);

            Assert.That(events.Any(x => x.Type == BotAnalyzerEventType.StrategyChanged), Is.True);
            Assert.That(events.Any(x => x.Type == BotAnalyzerEventType.GoalChanged), Is.True);
            Assert.That(events.Any(x => x.Type == BotAnalyzerEventType.UnitMoved && x.FromCell == new Vector2Int(1, 1) && x.ToCell == new Vector2Int(2, 1)), Is.True);
            Assert.That(events.Any(x => x.Type == BotAnalyzerEventType.ResourceChanged && x.Detail.Contains("-30")), Is.True);
        }

        private static BotAnalyzerFrame Frame(long turn) => new()
        {
            OwnerId = "bot",
            GlobalTurn = turn,
            Round = 2,
            Phase = "AwaitingInput",
            EditorTime = 1,
            UtcTimestamp = "2026-08-21T08:00:00.0000000Z",
            SelectedBotActive = true,
        };
    }
}
