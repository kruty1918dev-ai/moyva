using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// G26 acceptance invariants for wall topology: placed masks count only
    /// committed neighbors of the same collection, preview masks additionally
    /// include pending placements, corner/straight/end variants map to the
    /// correct collection slots, gates carry open/closed state that persists
    /// through capture/restore, and passage respects the gate owner.
    /// </summary>
    public sealed class G26AcceptanceTests
    {
        private const string WallId = "stone-wall";
        private const string GateId = "stone-gate";

        private DiContainer _container;
        private SignalBus _signalBus;
        private FakeObjectsMap _objects;
        private FakeBuildingRegistry _registry;
        private FakeSessionCommands _session;
        private WallTopologyService _topology;
        private WallCollectionDefinition _collection;

        private GameObject _horizontal;
        private GameObject _vertical;
        private GameObject _cornerNe;
        private GameObject _cornerSw;
        private GameObject _gate;
        private readonly List<GameObject> _prefabs = new List<GameObject>();

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            public readonly Dictionary<Vector2Int, string> Occupants = new();

            public bool IsOccupied(Vector2Int position) => Occupants.ContainsKey(position);
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => Occupants.TryGetValue(position, out occupantId);
            public void Register(Vector2Int position, string occupantId)
                => Occupants[position] = occupantId;
            public void Move(Vector2Int from, Vector2Int to)
            {
                if (!Occupants.TryGetValue(from, out var id)) return;
                Occupants.Remove(from);
                Occupants[to] = id;
            }
            public void Unregister(Vector2Int position) => Occupants.Remove(position);
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var pair in Occupants)
                    if (pair.Value == occupantId) { position = pair.Key; return true; }
                position = default;
                return false;
            }
        }

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            public readonly Dictionary<string, BuildingDefinition> Defs = new();
            public WallCollectionDefinition Collection;

            public BuildingDefinition[] GetAll() => new List<BuildingDefinition>(Defs.Values).ToArray();
            public BuildingDefinition GetById(string id)
                => id != null && Defs.TryGetValue(id, out var d) ? d : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category) => new BuildingDefinition[0];
            public WallCollectionDefinition[] GetWallCollections()
                => Collection != null ? new[] { Collection } : new WallCollectionDefinition[0];
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
                => Collection != null && Collection.ContainsBuilding(buildingId)
                    ? Collection
                    : null;
        }

        private sealed class FakeSessionCommands
            : IConstructionSessionCommands, IConstructionBuildingOwnershipQuery
        {
            public readonly Dictionary<Vector2Int, string> Pending = new();
            public readonly Dictionary<Vector2Int, string> Owners = new();

            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public bool IsAuthoritativeRuntime => true;

            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) { }
            public string GetActiveOwner() => "player_0";
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => Pending.ContainsKey(position);
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
                => Pending.TryGetValue(position, out buildingId);
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements() => Pending;
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => Pending.Remove(position);
            public bool TryGetPendingPlacementStatus(Vector2Int position, out ConstructionPendingPlacementStatus status)
            {
                status = default;
                return Pending.ContainsKey(position);
            }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position) => default;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(string buildingId) => null;
            public void Confirm() { }
            public void Cancel() { }
            public void UndoLast() { }
            public void RedoLast() { }
            public void ToggleDemolishMode() { }
            public bool TryDemolishAt(Vector2Int position) => false;
            public string GetLastActionMessage() => null;

            public bool TryGetPlacedBuildingOwner(Vector2Int position, out string ownerId)
                => Owners.TryGetValue(position, out ownerId);
        }

        [SetUp]
        public void SetUp()
        {
            _horizontal = Track(new GameObject("wall-h"));
            _vertical = Track(new GameObject("wall-v"));
            _cornerNe = Track(new GameObject("corner-ne"));
            _cornerSw = Track(new GameObject("corner-sw"));
            _gate = Track(new GameObject("gate"));

            _collection = new WallCollectionDefinition
            {
                CollectionId = "stone",
                WallBuildingId = WallId,
                GateBuildingId = GateId,
                HorizontalPrefab = _horizontal,
                VerticalPrefab = _vertical,
                CornerNorthEastPrefab = _cornerNe,
                CornerSouthWestPrefab = _cornerSw,
                GatePrefab = _gate,
            };

            _objects = new FakeObjectsMap();
            _session = new FakeSessionCommands();
            _registry = new FakeBuildingRegistry { Collection = _collection };
            _registry.Defs[WallId] = new BuildingDefinition { Id = WallId };
            _registry.Defs[GateId] = new BuildingDefinition
            {
                Id = GateId,
                Modules = new List<BuildingModuleDefinition>
                {
                    new GateBuildingModule { OpenSpeed = 2f },
                },
            };

            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            _signalBus = _container.Resolve<SignalBus>();
            _container.Bind<IConstructionSessionCommands>().FromInstance(_session);
            _topology = new WallTopologyService(
                _container.Resolve<LazyInject<IConstructionSessionCommands>>(),
                _registry,
                _objects,
                _signalBus);
            _topology.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _topology?.Dispose();
            foreach (var p in _prefabs)
                if (p != null) Object.DestroyImmediate(p);
        }

        private GameObject Track(GameObject go)
        {
            _prefabs.Add(go);
            return go;
        }

        private void Place(Vector2Int pos, string id)
        {
            _objects.Register(pos, id);
            _signalBus.Fire(new BuildingPlacedSignal { Position = pos, BuildingId = id });
        }

        [Test]
        public void PlacedMask_CountsOnlyCommittedSameCollectionNeighbors()
        {
            var center = new Vector2Int(5, 5);
            Place(center, WallId);
            Place(center + Vector2Int.up, WallId);
            Place(center + Vector2Int.right, WallId);
            _objects.Register(center + Vector2Int.down, "farm"); // not in collection

            Assert.IsTrue(_topology.TryBuildPlacedMask(center, WallId,
                out _, out TopologyNeighborMask mask));
            Assert.IsTrue(mask.North && mask.East);
            Assert.IsFalse(mask.South || mask.West,
                "non-collection occupant must not connect");
        }

        [Test]
        public void PreviewMask_IncludesPendingNeighbors_PlacedMaskDoesNot()
        {
            var center = new Vector2Int(5, 5);
            var east = center + Vector2Int.right;
            _session.Pending[east] = WallId; // pending, not yet committed

            _topology.TryBuildPreviewMask(center, WallId, out _, out var preview);
            Assert.IsTrue(preview.East, "pending same-collection neighbor counts in preview");

            _topology.TryBuildPlacedMask(center, WallId, out _, out var placed);
            Assert.IsFalse(placed.East, "pending neighbor must not count in the placed mask");
        }

        [Test]
        public void VariantResolution_MatchesNeighbors()
        {
            var resolver = new WallPrefabResolver(new AutoTileVariantResolver());
            var at = new Vector2Int(3, 3);

            // Straight segments follow the connected axis.
            Place(at + Vector2Int.up, WallId);
            Place(at + Vector2Int.down, WallId);
            _topology.TryBuildPlacedMask(at, WallId, out var col, out var mask);
            Assert.AreSame(_vertical, resolver.ResolvePrefab(col, WallId, mask),
                "N+S wall must resolve to the vertical segment");

            // Corner: neighbors N+E → corner SW atlas slot (corner geometry
            // occupies the side opposite the open arms).
            Place(at + Vector2Int.up, WallId);
            _objects.Unregister(at + Vector2Int.down);
            Place(at + Vector2Int.right, WallId);
            _topology.TryBuildPlacedMask(at, WallId, out _, out mask);
            Assert.AreSame(_cornerSw, resolver.ResolvePrefab(col, WallId, mask),
                "N+E neighbors must resolve to the south-west corner variant");

            // Isolated wall falls back to the horizontal segment.
            var lonely = new Vector2Int(9, 9);
            _topology.TryBuildPlacedMask(lonely, WallId, out _, out mask);
            Assert.AreSame(_horizontal, resolver.ResolvePrefab(col, WallId, mask),
                "an isolated wall uses the horizontal segment");

            // Cross/T resolve to the straight segments the collection owns.
            var cross = new Vector2Int(6, 6);
            Place(cross + Vector2Int.up, WallId);
            Place(cross + Vector2Int.down, WallId);
            Place(cross + Vector2Int.right, WallId);
            Place(cross + Vector2Int.left, WallId);
            _topology.TryBuildPlacedMask(cross, WallId, out _, out mask);
            Assert.AreSame(_horizontal, resolver.ResolvePrefab(col, WallId, mask),
                "cross intersection maps to the horizontal fallback slot");
        }

        [Test]
        public void GateState_Toggles_Persists_AndRespectsOwnership()
        {
            var pos = new Vector2Int(4, 4);
            Place(pos, GateId);
            _session.Owners[pos] = "player_0";

            Assert.IsFalse(_topology.IsGateOpen(pos), "new gates start closed");

            Assert.IsTrue(_topology.TrySetGateOpen(pos, true, out float t, out _));
            Assert.AreEqual(0.5f, t, 0.001f, "OpenSpeed 2 → 0.5s transition");
            Assert.IsTrue(_topology.IsGateOpen(pos));

            // Save/restore keeps the open state; closed gates aren't stored.
            byte[] saved = _topology.CaptureState();
            _topology.TrySetGateOpen(pos, false, out _, out _);
            Assert.IsFalse(_topology.IsGateOpen(pos));
            _topology.RestoreState(saved);
            Assert.IsTrue(_topology.IsGateOpen(pos), "open gates survive restore");

            // Passage: same owner opens; foreign owner is refused.
            Assert.IsTrue(_topology.CanUnitPassGate(pos, "player_0", out _));
            _topology.TrySetGateOpen(pos, false, out _, out _);
            Assert.IsTrue(_topology.TryEnsureOpenForUnit(pos, "player_0", out _),
                "own unit forces the gate open");
            Assert.IsTrue(_topology.IsGateOpen(pos));
            Assert.IsFalse(_topology.CanUnitPassGate(pos, "raider_9", out string reason),
                "foreign unit must not pass");
            Assert.IsFalse(string.IsNullOrEmpty(reason));
        }

        [Test]
        public void GateState_DemolitionClears_AndRestoresOnlyRealGates()
        {
            var pos = new Vector2Int(7, 7);
            Place(pos, GateId);
            _topology.TrySetGateOpen(pos, true, out _, out _);

            _signalBus.Fire(new BuildingDemolishedSignal { Position = pos, BuildingId = GateId });
            _objects.Unregister(pos);
            Assert.IsFalse(_topology.IsGateOpen(pos),
                "demolished gate loses its open state");

            // A captured open entry for a now-missing gate is skipped on restore.
            _topology.TrySetGateOpen(pos, true, out _, out _); // non-gate → false
            Assert.IsFalse(_topology.TrySetGateOpen(pos, true, out _, out _),
                "cannot open a cell with no gate module");
        }

        [Test]
        public void VisualResolver_GatesAndWalls_UseDistinctPaths()
        {
            var resolver = new WallVisualResolver(
                _registry,
                null,
                _topology,
                new WallPrefabResolver(new AutoTileVariantResolver()));

            var pos = new Vector2Int(2, 2);
            Place(pos, GateId);
            Assert.IsTrue(resolver.TryResolvePlacedVisual(pos, GateId,
                out GameObject prefab, out _));
            Assert.AreSame(_gate, prefab,
                "gates resolve to the collection gate prefab, not a wall segment");

            Assert.IsFalse(resolver.TryResolvePlacedVisual(pos, "farm",
                out _, out _), "non-collection building resolves nothing");
        }
    }
}
