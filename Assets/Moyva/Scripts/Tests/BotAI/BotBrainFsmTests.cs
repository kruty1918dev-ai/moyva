using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.BotAI
{
    /// <summary>
    /// Юніт-тести FSM логіки BotBrain.
    /// Перевіряє: початковий стан, переходи між станами.
    /// </summary>
    [TestFixture]
    public class BotBrainFsmTests : ZenjectUnitTestFixture
    {
        private sealed class FakeUnitFactory : IUnitFactory
        {
            public string CreateUnit(string typeId, Vector2Int gridPosition) => "unit_1";
            public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId) => "unit_1";
        }

        private sealed class FakeOwnershipService : IFactionOwnershipService
        {
            private readonly List<string> _units;

            public FakeOwnershipService(List<string> units) => _units = units;

            public FactionId GetOwner(string unitId) => FactionId.Empty;
            public IReadOnlyList<string> GetUnitIds(FactionId factionId) => _units;
            public void Register(string unitId, FactionId factionId) { }
            public void Unregister(string unitId) { }
        }

        private FactionDefinition MakeFaction() =>
            new FactionDefinition(
                new FactionId("bot_1"),
                FactionType.Bot,
                "warrior",
                Vector2Int.zero,
                Color.red);

        private BotBrain CreateBrain(List<string> units, IBotDifficultySettings settings = null)
        {
            settings ??= BotDifficultySettings.Normal();

            return new BotBrain(
                MakeFaction(),
                new FakeUnitFactory(),
                new FakeOwnershipService(units),
                settings);
        }

        [Test]
        public void WhenNoUnits_StartsInIdle()
        {
            var brain = CreateBrain(new List<string>());
            Assert.AreEqual(BotState.Idle, brain.CurrentState);
        }

        [Test]
        public void WhenNoUnits_TickTransitionsToExpanding()
        {
            var brain = CreateBrain(new List<string>());
            brain.Tick();
            Assert.AreEqual(BotState.Expanding, brain.CurrentState);
        }

        [Test]
        public void WhenEnoughUnits_TransitionsToAttacking()
        {
            var settings = BotDifficultySettings.Normal(); // AttackThreshold = 3
            var units = new List<string> { "u1", "u2", "u3" };
            var brain = CreateBrain(units, settings);

            brain.Tick(); // Idle → Expanding
            brain.Tick(); // Expanding → Attacking (unitCount >= AttackThreshold)

            Assert.AreEqual(BotState.Attacking, brain.CurrentState);
        }

        [Test]
        public void WhenFewUnits_TransitionsToDefending()
        {
            var settings = BotDifficultySettings.Normal(); // AttackThreshold=3, DefendThreshold=1
            var units = new List<string> { "u1", "u2", "u3" };
            var brain = CreateBrain(units, settings);

            brain.Tick(); // Idle → Expanding
            brain.Tick(); // Expanding → Attacking

            // Тепер видаляємо юнітів нижче DefendThreshold
            units.Clear();

            brain.Tick(); // Attacking → Defending (unitCount <= DefendThreshold)

            Assert.AreEqual(BotState.Defending, brain.CurrentState);
        }
    }
}
