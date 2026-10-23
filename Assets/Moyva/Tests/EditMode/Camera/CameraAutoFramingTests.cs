using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Camera.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Camera
{
    /// <summary>
    /// Startup auto-framing on a multiplayer client: the frame targets the
    /// local player's spawn (not the first/host assignment), and re-published
    /// spawn/world signals must not teleport the camera again unless the
    /// resolved focus actually moved.
    /// </summary>
    public sealed class CameraAutoFramingTests
    {
        private GameObject _go;
        private UnityEngine.Camera _cam;
        private DiContainer _container;
        private SignalBus _signals;
        private FakeMovement _movement;
        private FakeZoom _zoom;
        private FakeFocusResolver _focusResolver;
        private CameraAutoFramingService _service;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("framing-cam");
            _cam = _go.AddComponent<UnityEngine.Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = 20f;

            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<WorldGeneratedDataSignal>().OptionalSubscriber();
            _container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            _movement = new FakeMovement();
            _zoom = new FakeZoom();
            _focusResolver = new FakeFocusResolver();

            _service = new CameraAutoFramingService(
                _signals,
                _movement,
                _zoom,
                new CameraSettingsSO(),
                projectSettings: null,
                gridProjection: null,
                camera: _cam,
                worldGenerationSignalState: null,
                startupFocusResolver: _focusResolver);
            _service.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _container?.UnbindAll();
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void FireWorld(int revision = 1, long sequence = 1)
        {
            _signals.Fire(new WorldGeneratedDataSignal
            {
                StartupSequence = sequence,
                SnapshotRevision = revision,
                Source = WorldGeneratedDataSource.GeneratedHost,
                Width = 16,
                Height = 16,
            });
        }

        private void FireSpawn(int revision, long sequence = 1)
        {
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                StartupSequence = sequence,
                SnapshotRevision = revision,
                Source = WorldSpawnPositionsSource.MultiplayerSync,
                Assignments = new[]
                {
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 0,
                        ParticipantId = "host",
                        Position = new Vector2Int(2, 2)
                    },
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 1,
                        ParticipantId = "client",
                        Position = new Vector2Int(9, 9)
                    },
                }
            });
        }

        [Test]
        public void AutoFrame_TargetsResolvedLocalSpawn_NotFirstAssignment()
        {
            _focusResolver.Position = new Vector2Int(9, 9);
            _focusResolver.HasResult = true;

            FireWorld();
            FireSpawn(revision: 1);

            Assert.AreEqual(1, _movement.Teleports.Count);
            Assert.AreEqual(9f, _movement.Teleports[0].x, 0.01f);
            Assert.AreEqual(9f, _movement.Teleports[0].y, 0.01f);
        }

        [Test]
        public void AutoFrame_RepublishedSignals_SameFocus_DoNotTeleport()
        {
            _focusResolver.Position = new Vector2Int(9, 9);
            _focusResolver.HasResult = true;

            FireWorld();
            FireSpawn(revision: 1);
            FireSpawn(revision: 2);   // replication resend, same assignments
            FireWorld(revision: 2);   // world snapshot republish
            FireSpawn(revision: 3);

            Assert.AreEqual(1, _movement.Teleports.Count,
                "Re-published signals with an unchanged focus must not re-teleport.");
        }

        [Test]
        public void AutoFrame_FocusChanged_ReframesOnce()
        {
            _focusResolver.Position = new Vector2Int(9, 9);
            _focusResolver.HasResult = true;

            FireWorld();
            FireSpawn(revision: 1);
            Assert.AreEqual(1, _movement.Teleports.Count);

            _focusResolver.Position = new Vector2Int(5, 5);
            FireSpawn(revision: 2);

            Assert.AreEqual(2, _movement.Teleports.Count,
                "A genuinely moved focus (late local-owner resolution) re-frames once.");
            Assert.AreEqual(5f, _movement.Teleports[1].x, 0.01f);
        }

        [Test]
        public void AutoFrame_NoResolver_FallsBackToFirstAssignment()
        {
            _focusResolver.HasResult = false;

            FireWorld();
            FireSpawn(revision: 1);

            Assert.AreEqual(1, _movement.Teleports.Count);
            Assert.AreEqual(2f, _movement.Teleports[0].x, 0.01f,
                "Without a resolver the first assignment stays the fallback.");
        }

        private sealed class FakeFocusResolver : ICameraStartupFocusResolver
        {
            public bool HasResult;
            public Vector2Int Position;

            public bool TryResolveStartupFocusGridPosition(out Vector2Int position)
            {
                position = Position;
                return HasResult;
            }
        }

        private sealed class FakeMovement : ICameraMovement
        {
            public readonly List<Vector3> Teleports = new();

            public event System.Action ManualControlRequested { add { } remove { } }
            public void MoveCamera(Vector3 direction) { }
            public void MoveCameraKeyboard(Vector2 direction, float unscaledDeltaTime, float speedMultiplier = 1f) { }
            public void MoveCameraImmediate(Vector3 direction, float speedMultiplier) { }
            public void RotateCameraAroundFocusPoint(float angleDegrees) { }
            public void SetCameraOrbitInput(float normalizedInput) { }
            public void BeginPointerOrbit() { }
            public void RotatePointerOrbit(float horizontalScreenDelta) { }
            public void EndPointerOrbit() { }
            public void ShiftCameraWorld(Vector3 worldDelta, bool immediate) { }
            public void MoveCameraFocusToWorldPoint(Vector3 focusPoint, bool immediate) { }
            public void SetCameraDistanceToNavigationPlane(float distance, bool immediate) { }
            public bool TryScreenPointToNavigationPlane(Vector2 screenPoint, out Vector3 worldPoint)
            {
                worldPoint = default;
                return false;
            }
            public void ForceMoveCameraToPosition(Vector3 position) { }
            public void TeleportCamera(Vector3 position) { }
            public void TeleportCameraToFocusPoint(Vector3 focusPoint, float distance)
                => Teleports.Add(focusPoint);
        }

        private sealed class FakeZoom : ICameraZoom
        {
            public event System.Action ManualControlRequested { add { } remove { } }
            public void ZoomCamera(float zoomAmount) { }
            public void ZoomCamera(float zoomAmount, Vector2 screenFocalPoint) { }
            public void ZoomCameraByScale(float scaleFactor, bool immediate) { }
            public void ZoomCameraByScale(float scaleFactor, bool immediate, Vector2 screenFocalPoint) { }
            public void ForceZoomCamera(float zoomLevel) { }
        }
    }
}
