using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            public string CreateUnitWithId(string forcedUnitId, string typeId, Vector2Int gridPosition, string ownerId)
                => string.IsNullOrWhiteSpace(forcedUnitId) ? "unit_1" : forcedUnitId;
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

        private sealed class FakeFactionRegistry : IFactionRegistry
        {
            public FactionDefinition LocalPlayerFaction => null;
            public IReadOnlyList<FactionDefinition> GetAll() => new List<FactionDefinition>();
            public IReadOnlyList<FactionDefinition> GetBotFactions() => new List<FactionDefinition>();
            public bool TryGet(FactionId id, out FactionDefinition definition)
            {
                definition = default;
                return false;
            }
        }

        private sealed class FakeUnitService : IUnitService
        {
            public float GetStamina(string unitId) => 0f;

            public void SetStamina(string unitId, float stamina)
            {
            }

            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            {
                position = Vector2Int.zero;
                return false;
            }

            public GameObject GetUnitObject(string unitId) => null;

            public IReadOnlyCollection<string> GetAllUnitIds() => new List<string>();

            public string GetUnitTypeId(string unitId) => string.Empty;
        }

        private sealed class FakeMovementService : IUnitMovementService
        {
            public Task MoveUnitAsync(string unitId, Vector2Int target, CancellationToken ct = default)
                => Task.CompletedTask;
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
                new FakeFactionRegistry(),
                new FakeUnitFactory(),
                new FakeOwnershipService(units),
                new FakeUnitService(),
                new FakeMovementService(),
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

            // Видаляємо юнітів нижче DefendThreshold
            units.Clear();

            brain.Tick(); // Attacking → Defending (unitCount <= DefendThreshold)

            Assert.AreEqual(BotState.Defending, brain.CurrentState);
        }
    }
}
