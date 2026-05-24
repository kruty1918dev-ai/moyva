using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    [TestFixture]
    public sealed class BootstrapStarterPackMigrationTests : ZenjectUnitTestFixture
    {
        private static readonly IReadOnlyDictionary<Vector2Int, string> EmptyPlacements =
            new Dictionary<Vector2Int, string>();

        private SignalBus _signalBus;

        public override void Setup()
        {
            base.Setup();

            GameLaunchContext.Reset();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<WorldGeneratedDataSignal>();
            Container.DeclareSignal<WorldSpawnPositionsSignal>();
            Container.DeclareSignal<SettlementCreatedSignal>();
            Container.DeclareSignal<GrantStarterPackResourcesSignal>();
        }

        public override void Teardown()
        {
            GameLaunchContext.Reset();
            base.Teardown();
        }

        [Test]
        public void LoadGame_WithPersistedEconomyBlock_DoesNotGrantStarterPackAgain()
        {
            GameLaunchContext.ConfigureMenuLoadGame(3);

            var constructionService = new FakeConstructionService();
            var saveService = new FakeSaveService(hasSave: true);
            var saveInspectorService = new FakeSaveInspectorService { HasEconomyBlock = true };
            BootstrapStarterPackState starterPackState = BindInitializerDependencies(constructionService, saveService, saveInspectorService);

            _signalBus = Container.Resolve<SignalBus>();
            int grantCount = 0;
            _signalBus.Subscribe<GrantStarterPackResourcesSignal>(_ => grantCount++);

            var initializer = Container.Instantiate<BootstrapGameInitializer>();
            initializer.Initialize();

            _signalBus.Fire(new WorldGeneratedDataSignal());

            initializer.Dispose();

            Assert.AreEqual(0, grantCount);
            Assert.IsTrue(starterPackState.HasGranted("player_0"));
            Assert.AreEqual(0, saveService.SaveCalls);
        }

        private BootstrapStarterPackState BindInitializerDependencies(
            FakeConstructionService constructionService,
            FakeSaveService saveService,
            FakeSaveInspectorService saveInspectorService)
        {
            var settings = new BootstrapGameSettings
            {
                InitialResources = new List<InitialResourceEntry>
                {
                    new InitialResourceEntry("food", 50f),
                    new InitialResourceEntry("wood", 30f),
                },
            };

            var startingPositionState = new BootstrapStartingPositionState();
            var starterPackState = new BootstrapStarterPackState();

            Container.Bind<IConstructionService>().FromInstance(constructionService);
            Container.Bind<ISaveService>().FromInstance(saveService);
            Container.Bind<ISaveInspectorService>().FromInstance(saveInspectorService);
            Container.BindInstance(settings);
            Container.BindInstance(startingPositionState);
            Container.BindInstance(starterPackState);
            Container.Bind<IBootstrapOwnerIdResolver>().To<BootstrapOwnerIdResolver>().AsSingle();
            Container.Bind<IBootstrapStarterPackDecisionService>().To<BootstrapStarterPackDecisionService>().AsSingle();
            Container.Bind<IBootstrapStarterPackPersistenceService>().To<BootstrapStarterPackPersistenceService>().AsSingle();
            Container.Bind<IBootstrapStarterPackGrantService>().To<BootstrapStarterPackGrantService>().AsSingle();

            return starterPackState;
        }

        private sealed class FakeSaveInspectorService : ISaveInspectorService
        {
            public bool HasEconomyBlock;

            public bool HasBlock(int slot, Type moduleType)
                => HasEconomyBlock;

            public bool HasBlock<TModule>(int slot = 0)
                => HasEconomyBlock;

            public bool HasBlock(int slot, string moduleTypeFullName)
                => HasEconomyBlock;

            public bool TryGetBlockPayload(int slot, string moduleTypeFullName, out byte[] payload)
            {
                payload = null;
                return false;
            }

            public bool TryGetFogSnapshot(int slot, out bool[,] snapshot)
            {
                snapshot = null;
                return false;
            }
        }

        private sealed class FakeSaveService : ISaveService
        {
            private readonly bool _hasSave;

            public FakeSaveService(bool hasSave)
            {
                _hasSave = hasSave;
            }

            public int SaveCalls { get; private set; }

            public void Save(int slot = 0)
            {
                SaveCalls++;
            }

            public void Load(int slot = 0)
            {
            }

            public bool HasSave(int slot = 0)
                => _hasSave;

            public void Delete(int slot = 0)
            {
            }

            public SaveSlotInfo GetSlotInfo(int slot = 0)
                => new SaveSlotInfo(slot, _hasSave, 0, DateTime.MinValue);
        }

        private sealed class FakeConstructionService : IConstructionService
        {
            public string ActiveOwner { get; private set; } = "player_0";

            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;

            public void SelectBuilding(string buildingId)
            {
            }

            public string GetSelectedBuildingId()
                => null;

            public void SetActiveOwner(string ownerId)
            {
                ActiveOwner = string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
            }

            public string GetActiveOwner()
                => ActiveOwner;

            public bool TryPreviewAt(Vector2Int position)
                => false;

            public bool HasPendingPlacementAt(Vector2Int position)
                => false;

            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            {
                buildingId = null;
                return false;
            }

            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => EmptyPlacements;

            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition)
                => false;

            public void Confirm()
            {
            }

            public void Cancel()
            {
            }

            public void UndoLast()
            {
            }

            public void RedoLast()
            {
            }

            public void ToggleDemolishMode()
            {
            }

            public bool TryDemolishAt(Vector2Int position)
                => false;

            public IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings()
                => EmptyPlacements;

            public void RestoreFromSave(Vector2Int position, string buildingId)
            {
            }

            public bool RemovePendingAt(Vector2Int position)
                => false;

            public bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId)
                => false;

            public bool TryGetPendingPlacementStatus(Vector2Int position, out ConstructionPendingPlacementStatus status)
            {
                status = default;
                return false;
            }

            public ConstructionResourceProjection GetResourceProjection(Vector2Int position)
                => default;

            public string GetLastActionMessage()
                => string.Empty;

            public bool TryDemolishByFaction(Vector2Int position, string factionId)
                => false;

            public bool HasPlacedBuilding(string buildingId, string ownerId = null)
                => false;
        }
    }
}