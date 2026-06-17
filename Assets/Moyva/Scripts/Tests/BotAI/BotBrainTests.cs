using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.BotAI
{
    [TestFixture]
    public class BotBrainTests : ZenjectUnitTestFixture
    {
        // ─── Fakes ───────────────────────────────────────────────────────────

        private sealed class FakeFactionRegistry : IFactionRegistry
        {
            private readonly List<FactionDefinition> _all;

            public FakeFactionRegistry(List<FactionDefinition> all) => _all = all;

            public IReadOnlyList<FactionDefinition> GetAll() => _all;
            public IReadOnlyList<FactionDefinition> GetBotFactions() => _all.FindAll(f => f.FactionType == FactionType.Bot);
            public FactionDefinition LocalPlayerFaction => _all.Find(f => f.FactionType == FactionType.Human);
            public bool TryGet(FactionId id, out FactionDefinition definition)
            {
                definition = _all.Find(f => f.FactionId == id);
                return definition != null;
            }
        }

        private sealed class FakeOwnership : IFactionOwnershipService
        {
            private readonly Dictionary<FactionId, List<string>> _units = new();

            public void AddUnit(FactionId factionId, string unitId)
            {
                if (!_units.ContainsKey(factionId))
                    _units[factionId] = new List<string>();
                _units[factionId].Add(unitId);
            }

            public FactionId GetOwner(string unitId)
            {
                foreach (var kv in _units)
                    if (kv.Value.Contains(unitId)) return kv.Key;
                return FactionId.Empty;
            }

            public IReadOnlyList<string> GetUnitIds(FactionId factionId)
                => _units.TryGetValue(factionId, out var list) ? list : new List<string>();

            public void Register(string unitId, FactionId factionId) { }
            public void Unregister(string unitId) { }
        }

        private sealed class FakeUnitService : IUnitService
        {
            private readonly Dictionary<string, Vector2Int> _positions = new();
            private readonly Dictionary<string, float> _stamina = new();

            public void SetPosition(string unitId, Vector2Int pos) => _positions[unitId] = pos;

            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
                => _positions.TryGetValue(unitId, out position);

            public float GetStamina(string unitId)
                => _stamina.TryGetValue(unitId, out var v) ? v : 1f;

            public void SetStamina(string unitId, float stamina) => _stamina[unitId] = stamina;

            public GameObject GetUnitObject(string unitId) => null;

            public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys;

            public string GetUnitTypeId(string unitId) => null;
        }

        private sealed class FakeUnitFactory : IUnitFactory
        {
            public List<string> SpawnedUnitIds { get; } = new();
            private int _counter;

            public string CreateUnit(string typeId, Vector2Int gridPosition)
            {
                var id = $"{typeId}_{++_counter}";
                SpawnedUnitIds.Add(id);
                return id;
            }

            public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId)
            {
                var id = $"{typeId}_{++_counter}";
                SpawnedUnitIds.Add(id);
                return id;
            }

            public string CreateUnitWithId(string forcedUnitId, string typeId, Vector2Int gridPosition, string ownerId)
            {
                var id = string.IsNullOrWhiteSpace(forcedUnitId)
                    ? $"{typeId}_{++_counter}"
                    : forcedUnitId;
                SpawnedUnitIds.Add(id);
                return id;
            }
        }

        private sealed class FakeMovementService : IUnitMovementService
        {
            public List<(string UnitId, Vector2Int Target)> IssuedOrders { get; } = new();

            public Task MoveUnitAsync(string unitId, Vector2Int targetPosition, CancellationToken token = default)
            {
                IssuedOrders.Add((unitId, targetPosition));
                return Task.CompletedTask;
            }
        }

        private sealed class FakeFogOfWarService : IFogOfWarService
        {
            private readonly HashSet<Vector2Int> _visible = new();

            public void SetVisible(Vector2Int pos) => _visible.Add(pos);

            public void Initialize(int width, int height) { }
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position) => _visible.Contains(position) ? FogStateType.Visible : FogStateType.Unexplored;
            public bool IsVisible(Vector2Int position) => _visible.Contains(position);
            public bool IsExplored(Vector2Int position) => _visible.Contains(position);
            public bool[,] GetExploredSnapshot() => new bool[0, 0];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }

        private sealed class FakeFogRegistry : IFogOfWarServiceRegistry
        {
            private readonly Dictionary<string, IFogOfWarService> _map = new();

            public void Register(string factionId, IFogOfWarService service) => _map[factionId] = service;

            public bool TryGetFor(string factionId, out IFogOfWarService service)
                => _map.TryGetValue(factionId, out service);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static FactionDefinition MakeBotFaction(string id, Vector2Int startPos = default)
            => new FactionDefinition(
                new FactionId(id),
                FactionType.Bot,
                "warrior",
                startPos,
                Color.red);

        private static FactionDefinition MakeHumanFaction(string id)
            => new FactionDefinition(
                new FactionId(id),
                FactionType.Human,
                null,
                Vector2Int.zero,
                Color.blue);

        // ─── Tests ───────────────────────────────────────────────────────────

        /// <summary>
        /// Якщо у бота немає юнітів — повинен бути викликаний IUnitFactory.CreateUnit.
        /// </summary>
        [Test]
        public void Tick_WhenNoUnits_SpawnsUnit()
        {
            var botFaction  = MakeBotFaction("bot1", new Vector2Int(5, 5));
            var fakeFactory = new FakeUnitFactory();
            var fakeOwnership = new FakeOwnership();
            var fakeUnitService = new FakeUnitService();
            var fakeMovement = new FakeMovementService();
            var fakeRegistry = new FakeFactionRegistry(new List<FactionDefinition> { botFaction });

            Container.Bind<FactionDefinition>().FromInstance(botFaction);
            Container.Bind<IFactionRegistry>().FromInstance(fakeRegistry);
            Container.Bind<IUnitFactory>().FromInstance(fakeFactory);
            Container.Bind<IFactionOwnershipService>().FromInstance(fakeOwnership);
            Container.Bind<IUnitService>().FromInstance(fakeUnitService);
            Container.Bind<IUnitMovementService>().FromInstance(fakeMovement);

            var brain = Container.Instantiate<Kruty1918.Moyva.BotAI.Runtime.BotBrain>(
                new object[] { botFaction, fakeRegistry, fakeFactory, fakeOwnership, fakeUnitService, fakeMovement, BotDifficultySettings.Normal() });

            brain.Tick(); // Idle → Expanding
            brain.Tick(); // Expanding: unitCount=0 < AttackThreshold → SpawnStartUnit()

            Assert.AreEqual(1, fakeFactory.SpawnedUnitIds.Count, "Повинен спавнити один юніт якщо їх немає.");
        }

        /// <summary>
        /// Якщо є юніти бота і видимий ворог у межах дальності — видає команду на переміщення.
        /// </summary>
        [Test]
        public void Tick_WhenVisibleEnemyInRange_IssuesMoveOrder()
        {
            var botFaction   = MakeBotFaction("bot1", new Vector2Int(0, 0));
            var humanFaction = MakeHumanFaction("human1");

            // Hard: AttackThreshold=2, DefendThreshold=1
            // Потрібно 2 юніти бота для переходу в Attacking
            var fakeOwnership = new FakeOwnership();
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-1");
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-2");
            fakeOwnership.AddUnit(new FactionId("human1"), "human-unit-1");

            var fakeUnitService = new FakeUnitService();
            fakeUnitService.SetPosition("bot-unit-1", new Vector2Int(0, 0));
            fakeUnitService.SetPosition("bot-unit-2", new Vector2Int(100, 100)); // далеко — поза AttackRange
            fakeUnitService.SetPosition("human-unit-1", new Vector2Int(3, 0));   // відстань 3 ≤ 8

            var fakeMovement = new FakeMovementService();
            var fakeFactory  = new FakeUnitFactory();
            var fakeRegistry = new FakeFactionRegistry(new List<FactionDefinition> { botFaction, humanFaction });

            var brain = Container.Instantiate<Kruty1918.Moyva.BotAI.Runtime.BotBrain>(
                new object[] { botFaction, fakeRegistry, fakeFactory, fakeOwnership, fakeUnitService, fakeMovement, BotDifficultySettings.Hard() });

            brain.Tick(); // Idle → Expanding
            brain.Tick(); // Expanding: unitCount=2 >= AttackThreshold(2) → Attacking
            brain.Tick(); // Attacking: unitCount=2 > DefendThreshold(1) → ExecuteAttack

            Assert.AreEqual(1, fakeMovement.IssuedOrders.Count, "Повинна бути видана команда атаки.");
            Assert.AreEqual("bot-unit-1", fakeMovement.IssuedOrders[0].UnitId);
            Assert.AreEqual(new Vector2Int(3, 0), fakeMovement.IssuedOrders[0].Target);
        }

        /// <summary>
        /// Якщо ворог за межами видимості (туман) — команда не видається.
        /// </summary>
        [Test]
        public void Tick_WhenEnemyInFog_DoesNotAttack()
        {
            var botFaction   = MakeBotFaction("bot1", new Vector2Int(0, 0));
            var humanFaction = MakeHumanFaction("human1");

            // Hard: AttackThreshold=2, DefendThreshold=1
            var fakeOwnership = new FakeOwnership();
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-1");
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-2");
            fakeOwnership.AddUnit(new FactionId("human1"), "human-unit-1");

            var fakeUnitService = new FakeUnitService();
            fakeUnitService.SetPosition("bot-unit-1", new Vector2Int(0, 0));
            fakeUnitService.SetPosition("bot-unit-2", new Vector2Int(1, 0));
            fakeUnitService.SetPosition("human-unit-1", new Vector2Int(3, 0));

            var fakeFog = new FakeFogOfWarService();
            // НЕ додаємо позицію ворога як видиму — він у тумані

            var fakeFogRegistry = new FakeFogRegistry();
            fakeFogRegistry.Register("bot1", fakeFog);

            var fakeMovement = new FakeMovementService();
            var fakeFactory  = new FakeUnitFactory();
            var fakeRegistry = new FakeFactionRegistry(new List<FactionDefinition> { botFaction, humanFaction });

            Container.Bind<IFogOfWarServiceRegistry>().FromInstance(fakeFogRegistry);

            var brain = Container.Instantiate<Kruty1918.Moyva.BotAI.Runtime.BotBrain>(
                new object[] { botFaction, fakeRegistry, fakeFactory, fakeOwnership, fakeUnitService, fakeMovement, BotDifficultySettings.Hard() });

            brain.Tick(); // Idle → Expanding
            brain.Tick(); // Expanding: unitCount=2 >= 2 → Attacking
            brain.Tick(); // Attacking: ExecuteAttack — ворог у тумані, команд не видає

            Assert.AreEqual(0, fakeMovement.IssuedOrders.Count, "Не повинно бути команд — ворог у тумані.");
        }

        /// <summary>
        /// Юніти-охоронці бази не відправляються в атаку.
        /// </summary>
        [Test]
        public void Tick_BaseGuards_AreNotSentToAttack()
        {
            var botFaction   = MakeBotFaction("bot1", new Vector2Int(0, 0));
            var humanFaction = MakeHumanFaction("human1");

            var fakeOwnership = new FakeOwnership();
            // Додаємо 3 юніти бота (> MinBaseGuards=2)
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-1");
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-2");
            fakeOwnership.AddUnit(new FactionId("bot1"), "bot-unit-3");
            fakeOwnership.AddUnit(new FactionId("human1"), "human-unit-1");

            var fakeUnitService = new FakeUnitService();
            // Два юніти знаходяться біля бази (Chebyshev ≤ 3)
            fakeUnitService.SetPosition("bot-unit-1", new Vector2Int(1, 1));   // відстань від (0,0) = 1 ≤ 3
            fakeUnitService.SetPosition("bot-unit-2", new Vector2Int(2, 0));   // відстань = 2 ≤ 3
            fakeUnitService.SetPosition("bot-unit-3", new Vector2Int(10, 10)); // далеко від бази
            fakeUnitService.SetPosition("human-unit-1", new Vector2Int(5, 0)); // ворог у межах 8

            var fakeMovement = new FakeMovementService();
            var fakeFactory  = new FakeUnitFactory();
            var fakeRegistry = new FakeFactionRegistry(new List<FactionDefinition> { botFaction, humanFaction });

            var brain = Container.Instantiate<Kruty1918.Moyva.BotAI.Runtime.BotBrain>(
                new object[] { botFaction, fakeRegistry, fakeFactory, fakeOwnership, fakeUnitService, fakeMovement, BotDifficultySettings.Normal() });

            brain.Tick(); // Idle → Expanding
            brain.Tick(); // Expanding: unitCount=3 >= AttackThreshold(3) → Attacking
            brain.Tick(); // Attacking: ExecuteAttack

            // Лише один не-охоронець (bot-unit-3) може атакувати
            // Але bot-unit-3 далеко (10+10=20 Manhattan) — поза AttackRange=8, тому теж не атакує
            // Очікуємо 0 команд: охоронці захищають базу, третій — поза дальністю
            Assert.AreEqual(0, fakeMovement.IssuedOrders.Count,
                "Охоронці бази не повинні атакувати; bot-unit-3 поза дальністю.");
        }
    }
}
