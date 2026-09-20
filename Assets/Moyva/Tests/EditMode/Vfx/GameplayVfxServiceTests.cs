using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Vfx.API;
using Kruty1918.Moyva.Vfx.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Vfx.Tests
{
    /// <summary>
    /// GameplayVfxService: domain-signal → catalogued spawn pipeline with
    /// fog gating, per-key cooldowns, severity scaling and spawn dedupe.
    /// Stubs replace every gameplay dependency — the service stays read-only.
    /// </summary>
    public sealed class GameplayVfxServiceTests
    {
        private sealed class StubSpawner : IVfxSpawner
        {
            public readonly List<VfxSpawnRequest> Requests = new List<VfxSpawnRequest>();
            public VfxEffectRule LastRule;

            public bool TrySpawn(VfxEffectRule rule, in VfxSpawnRequest request)
            {
                Requests.Add(request);
                LastRule = rule;
                return true;
            }
        }

        private sealed class StubFog : IFogStateReader
        {
            public bool Visible = true;

            public FogStateType GetFogState(Vector2Int position)
                => Visible ? FogStateType.Visible : FogStateType.Unexplored;
            public bool IsVisible(Vector2Int position) => Visible;
            public bool IsExplored(Vector2Int position) => Visible;
        }

        private sealed class StubProjection : IGridProjection
        {
            public GridProjectionMode ProjectionMode => default;
            public GridTopology Topology => default;
            public GridWorldPlane WorldPlane => default;
            public Vector3 GridToWorld(Vector2Int p) => new Vector3(p.x, 0f, p.y);
            public Vector3 GridToWorld(Vector2Int p, float e, float o = 0f)
                => new Vector3(p.x, e + o, p.y);
            public Vector2Int WorldToGrid(Vector3 w)
                => new Vector2Int(Mathf.RoundToInt(w.x), Mathf.RoundToInt(w.z));
            public IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int p)
                => Array.Empty<Vector2Int>();
            public float GetStepDistance(Vector2Int a, Vector2Int b) => 1f;
            public float EstimateDistance(Vector2Int a, Vector2Int b) => 1f;
            public Bounds GetWorldBounds(int w, int h) => new Bounds();
        }

        private sealed class StubCombat : IUnitCombatService
        {
            public event Action<string, string> AttackStarted;
            public event Action<UnitAttackResult> AttackResolved;

            public void RaiseResolved(UnitAttackResult result)
                => AttackResolved?.Invoke(result);

            public bool CanAttack(string a, string t, out UnitAttackRejectReason r)
            { r = default; return true; }
            public IReadOnlyList<string> GetAttackableTargets(string a)
                => Array.Empty<string>();
            public IReadOnlyList<Vector2Int> GetAttackableTiles(string a)
                => Array.Empty<Vector2Int>();
            public bool TryGetHealth(string u, out UnitHealthSnapshot h)
            { h = default; return false; }
            public bool TryPreviewAttack(string a, string d, out UnitCombatBreakdown b)
            { b = default; return false; }
            public bool TryPreviewDuel(string a, string d, out UnitCombatDuel du)
            { du = default; return false; }
            public bool TryAttack(string a, string t, out UnitAttackResult r)
            { r = default; return false; }
        }

        private sealed class StubUnits : IUnitService
        {
            public readonly Dictionary<string, Vector2Int> Positions =
                new Dictionary<string, Vector2Int>();
            public readonly Dictionary<string, string> Types =
                new Dictionary<string, string>();

            public float GetStamina(string unitId) => 0f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
                => Positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => Positions.Keys as IReadOnlyCollection<string> ?? new List<string>(Positions.Keys);
            public string GetUnitTypeId(string unitId)
                => Types.TryGetValue(unitId, out string t) ? t : null;
        }

        private sealed class StubFactions : IFactionRegistry
        {
            public Color Color = new Color(0.2f, 0.4f, 0.9f, 1f);
            public IReadOnlyList<FactionDefinition> GetAll() => Array.Empty<FactionDefinition>();
            public FactionDefinition LocalPlayerFaction => null;
            public bool TryGet(FactionId id, out FactionDefinition definition)
            {
                definition = new FactionDefinition(
                    id, FactionType.Human, "warrior", Vector2Int.zero, Color);
                return true;
            }
        }

        private sealed class StubGraphics : IGraphicsSettingsService
        {
            public GraphicsSettingsData Settings { get; private set; }
                = GraphicsSettingsData.ForProfile(GraphicsQualityProfile.Balanced, false);
            public event Action<GraphicsSettingsData> OnSettingsChanged;
            public void Change(GraphicsSettingsData data)
            { Settings = data; OnSettingsChanged?.Invoke(data); }
            public void SetProfile(GraphicsQualityProfile p) { }
            public void SetTargetFrameRate(int f) { }
            public void SetRenderScale(float s) { }
            public void SetDynamicRenderScale(bool e) { }
            public void SetCloseZoomOptimization(bool e) { }
            public void SetTextureMipmapLimit(int l) { }
            public void SetAntiAliasing(int a) { }
            public void SetVSync(bool e) { }
            public void SetShadows(bool e) { }
            public void SetAnisotropicFiltering(bool e) { }
            public void SetLodBias(float l) { }
            public void ResetToDefaults() { }
            public void ApplyCurrentSettings() { }
        }

        private DiContainer _container;
        private SignalBus _bus;
        private StubSpawner _spawner;
        private StubFog _fog;
        private StubCombat _combat;
        private StubUnits _units;
        private GameplayVfxService _service;
        private float _now;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            SignalBusInstaller.Install(_container);
            _container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitMovedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitCreatedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitRecruitmentDeployedSignal>().OptionalSubscriber();
            _bus = _container.Resolve<SignalBus>();

            _spawner = new StubSpawner();
            _fog = new StubFog();
            _combat = new StubCombat();
            _units = new StubUnits();
            _now = 0f;
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            foreach (GameObject go in _stubPrefabs)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _stubPrefabs.Clear();
        }

        private GameplayVfxService CreateService(VfxCatalogConfig config)
        {
            _service = new GameplayVfxService(
                _bus, config, _spawner,
                projection: new StubProjection(),
                grid: null,
                units: _units,
                combat: _combat,
                fog: _fog,
                factions: new StubFactions(),
                graphics: new StubGraphics(),
                camera: null,
                ownedPool: null,
                clock: () => _now);
            _service.Initialize();
            return _service;
        }

        private readonly List<GameObject> _stubPrefabs = new List<GameObject>();

        private VfxEffectRule Rule(string eventName, int priority = 1)
        {
            var prefab = new GameObject("vfx-test"); // non-null: gates prefab==null check
            _stubPrefabs.Add(prefab);
            return new VfxEffectRule
            {
                eventName = eventName,
                prefab = prefab,
                priority = priority,
            };
        }

        private static VfxCatalogConfig Config(params VfxEffectRule[] rules)
            => new VfxCatalogConfig { effects = rules };

        [Test]
        public void BuildingPlaced_Visible_SpawnsAtProjectedPosition()
        {
            CreateService(Config(Rule(VfxEventIds.BuildingPlaced)));
            _bus.Fire(new BuildingPlacedSignal
            {
                BuildingId = "b1", Position = new Vector2Int(3, 5), OwnerId = "f-a",
            });

            Assert.AreEqual(1, _spawner.Requests.Count);
            Assert.AreEqual(new Vector3(3f, 0f, 5f), _spawner.Requests[0].Position);
        }

        [Test]
        public void BuildingPlaced_HiddenByFog_DoesNotSpawn()
        {
            CreateService(Config(Rule(VfxEventIds.BuildingPlaced)));
            _fog.Visible = false;
            _bus.Fire(new BuildingPlacedSignal { BuildingId = "b1", Position = new Vector2Int(3, 5) });
            Assert.AreEqual(0, _spawner.Requests.Count);
        }

        [Test]
        public void UnitDestroyed_UsesLastKnownPosition_NotLiveQuery()
        {
            CreateService(Config(Rule(VfxEventIds.UnitDestroyed)));
            _bus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1", UnitTypeId = "warrior",
                Position = new Vector2Int(2, 2), OwnerId = "f-a",
            });
            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "u1", NewPosition = new Vector2Int(6, 6),
            });

            // Authoritative removal happened before the signal reaches us.
            _units.Positions.Clear();
            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.AreEqual(1, _spawner.Requests.Count);
            Assert.AreEqual(new Vector3(6f, 0f, 6f), _spawner.Requests[0].Position);
        }

        [Test]
        public void UnitSpawn_DedupesCreatedAndDeployed()
        {
            CreateService(Config(Rule(VfxEventIds.UnitSpawned)));
            var at = new Vector2Int(1, 1);
            _bus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1", UnitTypeId = "warrior", Position = at, OwnerId = "f-a",
            });
            _bus.Fire(new UnitRecruitmentDeployedSignal
            {
                UnitId = "u1", UnitTypeId = "warrior", Position = at, OwnerId = "f-a",
            });

            Assert.AreEqual(1, _spawner.Requests.Count);
        }

        [Test]
        public void UnitMoveDust_CooldownPerUnit_Throttles()
        {
            var rule = Rule(VfxEventIds.UnitMoveDust);
            rule.cooldownPerKey = 0.25f;
            CreateService(Config(rule));

            for (int i = 0; i < 5; i++)
            {
                _now += 0.05f;
                _bus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = new Vector2Int(i, 0) });
            }

            Assert.AreEqual(1, _spawner.Requests.Count);

            _now += 0.3f;
            _bus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = new Vector2Int(9, 0) });
            Assert.AreEqual(2, _spawner.Requests.Count);
        }

        [Test]
        public void CombatResolved_ScalesByDamageSeverity()
        {
            CreateService(Config(Rule(VfxEventIds.CombatImpact)));
            _units.Positions["t1"] = new Vector2Int(4, 4);
            _units.Types["t1"] = "warrior";

            _combat.RaiseResolved(new UnitAttackResult(
                succeeded: true, rejectReason: default,
                attackerUnitId: "a1", targetUnitId: "t1",
                damageApplied: 10, targetHpBefore: 10, targetHpAfter: 0,
                targetDied: true));

            Assert.AreEqual(1, _spawner.Requests.Count);
            Assert.Greater(_spawner.Requests[0].Scale, 1.1f); // lethal hit = max severity
        }

        [Test]
        public void CombatResolved_Rejected_NoSpawn()
        {
            CreateService(Config(Rule(VfxEventIds.CombatImpact)));
            _combat.RaiseResolved(UnitAttackResult.Rejected(
                "a1", "t1", UnitAttackRejectReason.TargetOutOfRange));
            Assert.AreEqual(0, _spawner.Requests.Count);
        }

        [Test]
        public void Quality_Performance_RaisesCountScaleBudget()
        {
            var graphics = new StubGraphics();
            graphics.Change(GraphicsSettingsData.ForProfile(GraphicsQualityProfile.Performance, true));
            _service = new GameplayVfxService(
                _bus, Config(Rule(VfxEventIds.BuildingPlaced)), _spawner,
                null, null, _units, _combat, _fog, null, graphics, null, null, () => _now);
            _service.Initialize();

            Assert.Less(_service.Quality.CountScale, 1f);

            graphics.Change(GraphicsSettingsData.ForProfile(GraphicsQualityProfile.Quality, false));
            Assert.AreEqual(1f, _service.Quality.CountScale);
        }
    }
}
