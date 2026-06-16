using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// Extended tests for ConstructionService — edge-cases, undo/redo, demolish, fog, wall gates.
    /// </summary>
    [TestFixture]
    public class ConstructionServiceExtendedTests : ZenjectUnitTestFixture
    {
        private sealed class FakeFogOfWarService : IFogOfWarService
        {
            private readonly Dictionary<Vector2Int, FogStateType> _states = new();
            public void SetState(Vector2Int p, FogStateType s) => _states[p] = s;
            public void Initialize(int w, int h) { }
            public void RegisterUnit(string uid, Vector2Int p, int v) { }
            public void UpdateUnitVisionRange(string uid, int visionRange) { }
            public void RegisterFixedVisionArea(string uid, Vector2Int p, int v, FogRevealShape shape) { }
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            public void UpdateUnitPosition(string uid, Vector2Int p) { }
            public void UnregisterUnit(string uid) { }
            public FogStateType GetFogState(Vector2Int p) => _states.TryGetValue(p, out var s) ? s : FogStateType.Visible;
            public bool IsVisible(Vector2Int p) => GetFogState(p) == FogStateType.Visible;
            public bool IsExplored(Vector2Int p) => GetFogState(p) != FogStateType.Unexplored;
            public bool[,] GetExploredSnapshot() => new bool[0, 0];
            public void LoadFromSnapshot(bool[,] e) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }

        private sealed class FakeWallPlacementService : IWallPlacementService
        {
            private readonly IObjectsMapService _map;
            public FakeWallPlacementService(IObjectsMapService map) { _map = map; }
            public void ShowWallHandles(Vector2Int p) { }
            public void DragWall(Vector2Int s, Vector2 t) { }
            public IReadOnlyList<Vector2Int> BuildPath(Vector2Int s, Vector2Int e) => new[] { s, e };
            public bool IsWallOrGate(string id) => id == "wall" || id == "gate";
            public bool IsWall(string id) => id == "wall";
            public bool IsGate(string id) => id == "gate";
            public bool CanReplaceWallWithGate(Vector2Int p, string gateId, out string replaced)
            {
                replaced = null;
                if (!IsGate(gateId)) return false;
                if (!_map.TryGetOccupant(p, out var occ) || occ != "wall") return false;
                replaced = occ;
                return true;
            }
            public bool TryResolvePlacedVisual(Vector2Int p, string occ, out GameObject pf, out Quaternion r)
            { pf = null; r = Quaternion.identity; return false; }
            public bool TryResolvePreviewVisual(Vector2Int p, string bid, out GameObject pf)
            { pf = null; return false; }
            public void EndDrag() { }
        }

        private sealed class FakeEconomyInfoMediator : IEconomyInfoMediator
        {
            public EconomySettlementContext? SettlementContext;
            public bool HasAnyWarehouse;
            public int OwnerPoolConsumeCalls;
            public int SettlementConsumeCalls;

            private readonly Dictionary<string, float> _ownerPoolResources = new(StringComparer.Ordinal);
            private readonly Dictionary<string, float> _settlementResources = new(StringComparer.Ordinal);

            public void SetOwnerPoolResource(string resourceId, float amount)
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                    return;

                if (amount <= 0f)
                    _ownerPoolResources.Remove(resourceId);
                else
                    _ownerPoolResources[resourceId.Trim()] = amount;
            }

            public float GetOwnerPoolResource(string resourceId)
            {
                return !string.IsNullOrWhiteSpace(resourceId) && _ownerPoolResources.TryGetValue(resourceId.Trim(), out var amount)
                    ? amount
                    : 0f;
            }

            public bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context)
            {
                if (SettlementContext.HasValue)
                {
                    context = SettlementContext.Value;
                    return true;
                }

                context = default;
                return false;
            }

            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementContext context)
            {
                if (SettlementContext.HasValue && string.Equals(NormalizeOwnerId(ownerId), NormalizeOwnerId(SettlementContext.Value.OwnerId), System.StringComparison.Ordinal))
                {
                    context = SettlementContext.Value;
                    return true;
                }

                context = default;
                return false;
            }

            public bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId)
            {
                buildingId = null;
                ownerId = null;
                return false;
            }

            public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                SettlementConsumeCalls++;
                return TryConsume(_settlementResources, resourceCosts, out errorMessage, "поселенні");
            }

            public bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                OwnerPoolConsumeCalls++;
                return TryConsume(_ownerPoolResources, resourceCosts, out errorMessage, "стартовому запасі");
            }

            public bool OwnerHasAnyWarehouse(string ownerId)
                => HasAnyWarehouse;

            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
                => new Dictionary<string, float>(StringComparer.Ordinal);

            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
                => new Dictionary<string, float>(StringComparer.Ordinal);

            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => new Dictionary<string, float>(_settlementResources, StringComparer.Ordinal);

            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
                => new Dictionary<string, float>(_ownerPoolResources, StringComparer.Ordinal);

            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => new Dictionary<string, float>(_ownerPoolResources, StringComparer.Ordinal);

            public string GetResourceDisplayName(string resourceId)
                => resourceId;

            private static bool TryConsume(
                Dictionary<string, float> pool,
                IReadOnlyDictionary<string, float> resourceCosts,
                out string errorMessage,
                string sourceLabel)
            {
                errorMessage = string.Empty;
                if (resourceCosts == null || resourceCosts.Count == 0)
                    return true;

                foreach (var pair in resourceCosts)
                {
                    if (pair.Value <= 0f)
                        continue;

                    float current = pool.TryGetValue(pair.Key, out var value) ? value : 0f;
                    if (current + 0.0001f < pair.Value)
                    {
                        errorMessage = $"Недостатньо '{pair.Key}' у {sourceLabel}.";
                        return false;
                    }
                }

                foreach (var pair in resourceCosts)
                {
                    if (pair.Value <= 0f)
                        continue;

                    float current = pool.TryGetValue(pair.Key, out var value) ? value : 0f;
                    float next = current - pair.Value;
                    if (next <= 0.0001f)
                        pool.Remove(pair.Key);
                    else
                        pool[pair.Key] = next;
                }

                return true;
            }

            private static string NormalizeOwnerId(string ownerId)
            {
                return string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
            }
        }

        private IConstructionService _service;
        private IInitializable _init;
        private System.IDisposable _disposable;
        private SignalBus _signalBus;
        private IObjectsMapService _objectsMap;
        private FakeFogOfWarService _fog;
        private FakeEconomyInfoMediator _economyInfoMediator;
        private BuildingRegistrySO _buildingRegistry;
        private int _placedCount;
        private int _cancelledCount;
        private int _demolishedCount;
        private int _previewChangedCount;
        private BuildingPlacedSignal _lastPlacedSignal;
        private BuildingPreviewChangedSignal _lastPreview;

        public override void Setup()
        {
            base.Setup();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingCancelledSignal>();
            Container.DeclareSignal<BuildingDemolishedSignal>();
            Container.DeclareSignal<BuildingPreviewChangedSignal>();
            Container.DeclareSignal<ShowWallHandlesSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>();
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();

            Container.BindInterfacesAndSelfTo<Kruty1918.Moyva.ObjectsMap.Runtime.ObjectsMapService>().AsSingle();
            _objectsMap = Container.Resolve<IObjectsMapService>();

            _fog = new FakeFogOfWarService();
            Container.Bind<IFogOfWarService>().FromInstance(_fog).AsSingle();
            Container.Bind<IWallPlacementService>().To<FakeWallPlacementService>().AsSingle();
            _economyInfoMediator = new FakeEconomyInfoMediator();
            Container.Bind<IEconomyInfoMediator>().FromInstance(_economyInfoMediator).AsSingle();

            _buildingRegistry = ScriptableObject.CreateInstance<BuildingRegistrySO>();
            Container.Bind<IBuildingRegistry>().FromInstance(_buildingRegistry).AsSingle();
            Container.BindInstance(0).WithId("minSpacing");
            Container.BindInstance(0).WithId("townHallBuildRadius");

            var type = typeof(IConstructionService).Assembly
                .GetType("Kruty1918.Moyva.Construction.Runtime.ConstructionService");
            Container.BindInterfacesAndSelfTo(type).AsSingle().NonLazy();
            Container.ResolveRoots();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<IConstructionService>();
            _init = _service as IInitializable;
            _disposable = _service as System.IDisposable;
            _init?.Initialize();

            // Активуємо режим будівництва
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });

            _placedCount = 0;
            _cancelledCount = 0;
            _demolishedCount = 0;
            _previewChangedCount = 0;
            _signalBus.Subscribe<BuildingPlacedSignal>(s =>
            {
                _placedCount++;
                _lastPlacedSignal = s;
            });
            _signalBus.Subscribe<BuildingCancelledSignal>(_ => _cancelledCount++);
            _signalBus.Subscribe<BuildingDemolishedSignal>(_ => _demolishedCount++);
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(s =>
            {
                _previewChangedCount++;
                _lastPreview = s;
            });
        }

        public override void Teardown()
        {
            _disposable?.Dispose();
            if (_buildingRegistry != null)
                UnityEngine.Object.DestroyImmediate(_buildingRegistry);
            base.Teardown();
        }

        // --- SelectBuilding ---
        [Test]
        public void SelectBuilding_NullId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.SelectBuilding(null));
        }

        [Test]
        public void SelectBuilding_EmptyId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.SelectBuilding(""));
        }

        [Test]
        public void SelectBuilding_ValidId_SetsSelectedBuilding()
        {
            _service.SelectBuilding("house");
            Assert.AreEqual("house", _service.GetSelectedBuildingId());
        }

        // --- Cancel ---
        [Test]
        public void Cancel_WithoutSelect_FiresCancelledSignal()
        {
            _service.Cancel();
            Assert.AreEqual(1, _cancelledCount);
        }

        [Test]
        public void Cancel_AfterSelect_ClearsSelection()
        {
            _service.SelectBuilding("house");
            _service.Cancel();
            Assert.AreEqual(1, _cancelledCount);
        }

        [Test]
        public void Cancel_Twice_FiresTwice()
        {
            _service.Cancel();
            _service.Cancel();
            Assert.AreEqual(2, _cancelledCount);
        }

        // --- ToggleDemolishMode ---
        [Test]
        public void ToggleDemolishMode_TogglesState()
        {
            bool before = _service.IsDemolishMode;
            _service.ToggleDemolishMode();
            Assert.AreNotEqual(before, _service.IsDemolishMode);
        }

        [Test]
        public void ToggleDemolishMode_TwiceReturnsToOriginal()
        {
            bool before = _service.IsDemolishMode;
            _service.ToggleDemolishMode();
            _service.ToggleDemolishMode();
            Assert.AreEqual(before, _service.IsDemolishMode);
        }

        // --- TryPreviewAt ---
        [Test]
        public void TryPreviewAt_WithoutSelectingBuilding_ReturnsFalse()
        {
            Assert.IsFalse(_service.TryPreviewAt(Vector2Int.zero));
        }

        [Test]
        public void TryPreviewAt_ConsumesOwnerPoolBeforeAnyWarehouseExists()
        {
            _buildingRegistry.Buildings = new[]
            {
                CreateBuilding("castle", new CastleBuildingModule { ExclusionRadius = 3 }, ("wood", 30)),
            };
            _economyInfoMediator.SetOwnerPoolResource("wood", 30f);
            _economyInfoMediator.HasAnyWarehouse = false;

            _service.SelectBuilding("castle");

            Assert.IsTrue(_service.TryPreviewAt(new Vector2Int(5, 5)));

            _service.Confirm();

            Assert.AreEqual(1, _economyInfoMediator.OwnerPoolConsumeCalls);
            Assert.AreEqual(0, _economyInfoMediator.SettlementConsumeCalls);
            Assert.AreEqual(0f, _economyInfoMediator.GetOwnerPoolResource("wood"), 0.01f);
        }

        [Test]
        public void TryPreviewAt_ConsumesOwnerPoolEvenWhenSettlementExistsUntilWarehouseAppears()
        {
            _buildingRegistry.Buildings = new[]
            {
                CreateBuilding("castle", new CastleBuildingModule { ExclusionRadius = 3 }, ("wood", 20)),
            };
            _economyInfoMediator.SettlementContext = new EconomySettlementContext("settlement-1", "Settlement 1", "player_0");
            _economyInfoMediator.SetOwnerPoolResource("wood", 20f);
            _economyInfoMediator.HasAnyWarehouse = false;

            _service.SelectBuilding("castle");

            Assert.IsTrue(_service.TryPreviewAt(new Vector2Int(8, 8)));

            _service.Confirm();

            Assert.AreEqual(1, _economyInfoMediator.OwnerPoolConsumeCalls);
            Assert.AreEqual(0, _economyInfoMediator.SettlementConsumeCalls);
            Assert.AreEqual(0f, _economyInfoMediator.GetOwnerPoolResource("wood"), 0.01f);
        }

        // --- TryDemolishAt ---
        [Test]
        public void TryDemolishAt_EmptyTile_ReturnsFalse()
        {
            _service.ToggleDemolishMode();
            Assert.IsFalse(_service.TryDemolishAt(Vector2Int.zero));
        }

        // --- Fog integration ---
        [Test]
        public void TryPreviewAt_UnexploredFog_ReturnsFalse()
        {
            _fog.SetState(Vector2Int.zero, FogStateType.Unexplored);
            _service.SelectBuilding("house");
            Assert.IsFalse(_service.TryPreviewAt(Vector2Int.zero));
        }

        // --- Undo/Redo ---
        [Test]
        public void UndoLast_EmptyHistory_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.UndoLast());
        }

        [Test]
        public void RedoLast_EmptyHistory_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.RedoLast());
        }

        // --- Occupied tile ---
        [Test]
        public void TryPreviewAt_OccupiedTile_ReturnsFalse()
        {
            _objectsMap.Register(Vector2Int.zero, "blocker");
            _service.SelectBuilding("house");
            Assert.IsFalse(_service.TryPreviewAt(Vector2Int.zero));
        }

        // --- DirectPlace on occupied ---
        [Test]
        public void TryDirectPlace_OccupiedTile_ReturnsFalse()
        {
            _objectsMap.Register(Vector2Int.zero, "blocker");
            Assert.IsFalse(_service.TryDirectPlace("house", Vector2Int.zero, null));
        }

        [Test]
        public void TryDirectPlace_FiresOwnerIdFromFaction()
        {
            Assert.IsTrue(_service.TryDirectPlace("castle", Vector2Int.zero, " local-player "));

            Assert.AreEqual(1, _placedCount);
            Assert.AreEqual("local-player", _lastPlacedSignal.OwnerId);
            Assert.AreEqual("local-player", _lastPlacedSignal.SourceFactionId);
        }

        // --- TryDemolishByFaction ---
        [Test]
        public void TryDemolishByFaction_EmptyPos_ReturnsFalse()
        {
            Assert.IsFalse(_service.TryDemolishByFaction(Vector2Int.zero, "faction1"));
        }

        // --- RemovePendingAt ---
        [Test]
        public void RemovePendingAt_NoPending_ReturnsFalse()
        {
            Assert.IsFalse(_service.RemovePendingAt(Vector2Int.zero));
        }

        // --- TryMovePendingPlacement ---
        [Test]
        public void TryMovePendingPlacement_NoPending_ReturnsFalse()
        {
            Assert.IsFalse(_service.TryMovePendingPlacement(Vector2Int.zero, Vector2Int.one));
        }

        // --- Confirm without preview ---
        [Test]
        public void Confirm_WithoutPreview_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.Confirm());
            Assert.AreEqual(0, _placedCount);
        }

        [Test]
        public void GetPendingPlacements_ReturnsPreviewBeforeConfirm()
        {
            var position = new Vector2Int(3, 8);

            _service.SelectBuilding("castle");
            Assert.IsTrue(_service.TryPreviewAt(position));

            var pending = _service.GetPendingPlacements();

            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual("castle", pending[position]);
            Assert.AreEqual(0, _service.GetPlayerPlacedBuildings().Count);
        }

        // --- GetPendingCount ---
        [Test]
        public void GetPlayerPlacedBuildings_Initially_Empty()
        {
            Assert.AreEqual(0, _service.GetPlayerPlacedBuildings().Count);
        }

        // --- SelectBuilding twice: override ---
        [Test]
        public void SelectBuilding_Twice_OverridesFirst()
        {
            _service.SelectBuilding("house");
            _service.SelectBuilding("barracks");
            Assert.AreEqual("barracks", _service.GetSelectedBuildingId());
        }

        private static BuildingDefinition CreateBuilding(
            string id,
            BuildingModuleDefinition module,
            params (string ResourceId, int Amount)[] costs)
        {
            var definition = new BuildingDefinition
            {
                Id = id,
                Modules = module == null
                    ? new List<BuildingModuleDefinition>()
                    : new List<BuildingModuleDefinition> { module },
                ConstructionCost = new List<BuildingDefinition.BuildingConstructionCostEntry>(),
            };

            if (costs == null)
                return definition;

            for (int index = 0; index < costs.Length; index++)
            {
                var cost = costs[index];
                definition.ConstructionCost.Add(new BuildingDefinition.BuildingConstructionCostEntry
                {
                    ResourceId = cost.ResourceId,
                    Amount = cost.Amount,
                });
            }

            return definition;
        }
    }
}
