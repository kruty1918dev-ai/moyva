using System;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Notifications
{
    [TestFixture]
    public sealed class UnderConstructionNotificationTests : ZenjectUnitTestFixture
    {
        private sealed class FakeGridService : IGridService
        {
            public int GridWidth => 10;
            public int GridHeight => 10;

            public string GetTileData(Vector2Int position) => "grass";

            public bool TryGetTileData(
                Vector2Int position,
                out string tileTypeId)
            {
                tileTypeId = "grass";
                return true;
            }

            public void SetTileData(
                Vector2Int position,
                string tileTypeId)
            {
            }
        }

        private sealed class FakeObjectsMapService : IObjectsMapService
        {
            private readonly Vector2Int _position;
            private readonly string _occupantId;

            public FakeObjectsMapService(
                Vector2Int position,
                string occupantId)
            {
                _position = position;
                _occupantId = occupantId;
            }

            public bool IsOccupied(Vector2Int position)
                => position == _position;

            public bool TryGetOccupant(
                Vector2Int position,
                out string occupantId)
            {
                occupantId = position == _position ? _occupantId : null;
                return occupantId != null;
            }

            public void Register(
                Vector2Int position,
                string occupantId)
            {
            }

            public void Move(
                Vector2Int from,
                Vector2Int to)
            {
            }

            public void Unregister(Vector2Int position)
            {
            }

            public bool TryGetPosition(
                string occupantId,
                out Vector2Int position)
            {
                position = _position;
                return string.Equals(occupantId, _occupantId, StringComparison.Ordinal);
            }
        }

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition _definition = new() { Id = "barrack" };

            public BuildingDefinition[] GetAll()
                => new[] { _definition };

            public BuildingDefinition GetById(string id)
                => string.Equals(id, _definition.Id, StringComparison.Ordinal)
                    ? _definition
                    : null;

            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();

            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();

            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
                => null;
        }

        private sealed class FixedLifecycle : IConstructionLifecycle
        {
            private readonly bool _isOperational;

            public FixedLifecycle(bool isOperational)
            {
                _isOperational = isOperational;
            }

            public bool IsOperational(Vector2Int position)
                => _isOperational;

            public bool TryGetProgress(
                Vector2Int position,
                out int completedTurns,
                out int requiredTurns)
            {
                completedTurns = _isOperational ? 1 : 0;
                requiredTurns = 1;
                return !_isOperational;
            }
        }

        private sealed class RecordingNotificationService : IGameplayNotificationService
        {
            public readonly List<GameplayNotificationRequest> Requests = new();

            public void Show(
                string message,
                GameplayNotificationKind kind = GameplayNotificationKind.Info,
                float? holdDuration = null,
                string dedupKey = null)
            {
                Requests.Add(new GameplayNotificationRequest(
                    message,
                    kind,
                    holdDuration,
                    dedupKey));
            }

            public void Show(GameplayNotificationRequest request)
            {
                Requests.Add(request);
            }

            public void Clear()
            {
                Requests.Clear();
            }
        }

        [Test]
        public void UnderConstructionBuilding_ShowsNotification()
        {
            var notifications = new RecordingNotificationService();
            object service = CreateTileInteractionService(
                isOperational: false,
                notifications,
                out _);

            InvokeHandleTileClick(service, new Vector2Int(2, 3));

            Assert.AreEqual(1, notifications.Requests.Count);
            Assert.AreEqual("Будівля ще будується", notifications.Requests[0].Message);
            Assert.AreEqual(GameplayNotificationKind.Warning, notifications.Requests[0].Kind);
            Assert.AreEqual("building-under-construction", notifications.Requests[0].DedupKey);
        }

        [Test]
        public void UnderConstructionBuilding_DoesNotOpenFunctionalUI()
        {
            var notifications = new RecordingNotificationService();
            object service = CreateTileInteractionService(
                isOperational: false,
                notifications,
                out List<BuildingInfoPanelRequestedSignal> requests);

            InvokeHandleTileClick(service, new Vector2Int(2, 3));

            Assert.AreEqual(0, requests.Count);
        }

        private object CreateTileInteractionService(
            bool isOperational,
            IGameplayNotificationService notifications,
            out List<BuildingInfoPanelRequestedSignal> buildingInfoRequests)
        {
            SetupSignals();
            SignalBus signalBus = Container.Resolve<SignalBus>();
            var capturedRequests = new List<BuildingInfoPanelRequestedSignal>();
            signalBus.Subscribe<BuildingInfoPanelRequestedSignal>(
                signal => capturedRequests.Add(signal));
            buildingInfoRequests = capturedRequests;

            Type serviceType = Type.GetType(
                "Kruty1918.Moyva.Interactions.Runtime.TileInteractionService, Kruty1918.Moyva.Interactions.API");
            Assert.NotNull(serviceType);

            object service = Activator.CreateInstance(
                serviceType,
                new FakeGridService(),
                new FakeObjectsMapService(new Vector2Int(2, 3), "barrack"),
                new FakeBuildingRegistry(),
                null,
                null,
                null,
                null,
                null,
                null,
                new FixedLifecycle(isOperational),
                notifications,
                signalBus);

            Assert.NotNull(service);
            return service;
        }

        private void SetupSignals()
        {
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>();
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>();
            Container.DeclareSignal<MapObjectInfoPanelRequestedSignal>();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>();
            Container.DeclareSignal<WorldInfoSelectionChangedSignal>();
            Container.DeclareSignal<LocalUnitSelectionChangedSignal>();
            Container.DeclareSignal<MoveUnitRequestSignal>();
            Container.DeclareSignal<InterruptMovementSignal>();
        }

        private static void InvokeHandleTileClick(
            object service,
            Vector2Int position)
        {
            MethodInfo method = service.GetType().GetMethod("HandleTileClick");
            Assert.NotNull(method);
            method.Invoke(service, new object[] { position });
        }
    }
}
