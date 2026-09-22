using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Interactions.Runtime;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Interactions
{
    /// <summary>
    /// Fog-of-war leakage gate: clicking a tile whose occupants are hidden
    /// from the local player must not open building/unit info panels or
    /// select the occupant. Movement commands into fog stay untouched.
    /// </summary>
    [TestFixture]
    public sealed class TileInteractionFogGateTests
    {
        private static readonly Vector2Int Tile = new Vector2Int(9, 9);

        private DiContainer _container;
        private SignalBus _signals;
        private FakeObjectsMap _objects;
        private StubFog _fog;
        private TileInteractionService _service;
        private List<BuildingInfoPanelRequestedSignal> _buildingPanels;
        private List<UnitInfoPanelRequestedSignal> _unitPanels;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<BuildingInfoPanelRequestedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitInfoPanelRequestedSignal>().OptionalSubscriber();
            _container.DeclareSignal<MapObjectInfoPanelRequestedSignal>().OptionalSubscriber();
            _container.DeclareSignal<WorldInfoPanelClosedSignal>().OptionalSubscriber();
            _container.DeclareSignal<LocalUnitSelectionChangedSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            _buildingPanels = new List<BuildingInfoPanelRequestedSignal>();
            _unitPanels = new List<UnitInfoPanelRequestedSignal>();
            _signals.Subscribe<BuildingInfoPanelRequestedSignal>(
                signal => _buildingPanels.Add(signal));
            _signals.Subscribe<UnitInfoPanelRequestedSignal>(
                signal => _unitPanels.Add(signal));

            _objects = new FakeObjectsMap();
            _fog = new StubFog();
            _service = new TileInteractionService(
                new FakeGridService(),
                _objects,
                new FakeBuildingRegistry(),
                mapObjectRegistryService: null,
                mapObjectEconomyService: null,
                unitMovementService: null,
                unitMovementQuery: null,
                unitOwnershipQuery: null,
                unitCombatService: null,
                unitGroupService: null,
                remoteGroups: null,
                combatCommandService: null,
                remoteCombat: null,
                roleResolver: null,
                fogState: _fog,
                constructionService: null,
                turns: null,
                constructionLifecycle: null,
                notifications: null,
                _signals);
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
        }

        [Test]
        public void ClickFoggedTile_WithEnemyBuilding_DoesNotOpenInfoPanel()
        {
            _objects.Register(Tile, "watchtower");
            _fog.Visible = false;

            _service.HandleTileClick(Tile);

            Assert.AreEqual(0, _buildingPanels.Count,
                "Fog-hidden building must not open an info panel.");
        }

        [Test]
        public void ClickVisibleTile_WithBuilding_OpensInfoPanel()
        {
            _objects.Register(Tile, "watchtower");
            _fog.Visible = true;

            _service.HandleTileClick(Tile);

            Assert.AreEqual(1, _buildingPanels.Count);
            Assert.AreEqual("watchtower", _buildingPanels[0].BuildingId);
            Assert.AreEqual(Tile, _buildingPanels[0].Position);
        }

        [Test]
        public void ClickFoggedTile_WithEnemyUnit_DoesNotOpenUnitPanel()
        {
            _objects.Register(Tile, "enemy-unit");
            _fog.Visible = false;

            _service.HandleTileClick(Tile);

            Assert.AreEqual(0, _unitPanels.Count,
                "Fog-hidden unit presence must not leak through selection.");
        }

        [Test]
        public void ClickVisibleTile_WithUnit_OpensUnitPanel()
        {
            _objects.Register(Tile, "enemy-unit");
            _fog.Visible = true;

            _service.HandleTileClick(Tile);

            Assert.AreEqual(1, _unitPanels.Count);
            Assert.AreEqual("enemy-unit", _unitPanels[0].UnitId);
        }

        [Test]
        public void ClickFoggedEmptyTile_FiresNothing()
        {
            _fog.Visible = false;

            _service.HandleTileClick(Tile);

            Assert.AreEqual(0, _buildingPanels.Count);
            Assert.AreEqual(0, _unitPanels.Count);
        }

        private sealed class StubFog : IFogStateReader
        {
            public bool Visible;
            public FogStateType GetFogState(Vector2Int position)
                => Visible ? FogStateType.Visible : FogStateType.Unexplored;
            public bool IsVisible(Vector2Int position) => Visible;
            public bool IsExplored(Vector2Int position) => Visible;
        }

        private sealed class FakeGridService : IGridService
        {
            public string GetTileData(Vector2Int position) => "grass";
            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = "grass";
                return true;
            }
            public void SetTileData(Vector2Int position, string tileTypeId) { }
            public int GridWidth => 100;
            public int GridHeight => 100;
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();

            public bool IsOccupied(Vector2Int position)
                => _occupants.ContainsKey(position);
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);
            public void Register(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;
            public void Move(Vector2Int from, Vector2Int to) { }
            public void Unregister(Vector2Int position)
                => _occupants.Remove(position);
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                position = default;
                return false;
            }
        }

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition _tower = new BuildingDefinition
            {
                Id = "watchtower",
                DisplayName = "Watchtower",
            };

            public BuildingDefinition[] GetAll() => new[] { _tower };
            public BuildingDefinition GetById(string id)
                => id == _tower.Id ? _tower : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(
                string buildingId) => null;
        }
    }
}
