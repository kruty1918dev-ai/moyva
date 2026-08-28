using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    public sealed class StartingPositionPolicyTests
    {
        [SetUp]
        public void SetUp()
        {
            GameLaunchContext.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameLaunchContext.Reset();
        }

        [Test]
        public void CanRunStartLogic_AllowsLaunchHost_WhenSessionHasParticipantsButNoLocalRole()
        {
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                "New World",
                12345,
                0,
                0,
                0,
                4,
                false,
                64,
                64,
                isLocalPlayerHost: true,
                localPlayerId: "host-1");
            var session = new SessionManagerStub(
                new[] { new Participant(new ParticipantIdentity("host-1", "Host"), isHost: false) },
                localPlayerId: string.Empty,
                isLocalPlayerHost: false);
            var policy = CreatePolicy(session);

            Assert.That(policy.CanRunStartLogic(), Is.True);
            Assert.That(policy.ShouldComputeHostStartPositions(), Is.True);
        }

        [Test]
        public void ResolveStartPositionCount_UsesActualParticipants_WhenSessionCapacityIsHigher()
        {
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                "New World",
                12345,
                0,
                0,
                0,
                4,
                false,
                64,
                64,
                isLocalPlayerHost: true,
                localPlayerId: "host-1");
            var session = new SessionManagerStub(
                new[] { new Participant(new ParticipantIdentity("host-1", "Host"), isHost: true) },
                localPlayerId: "host-1",
                isLocalPlayerHost: true);
            var policy = CreatePolicy(session);

            Assert.That(policy.ResolveStartPositionCount(), Is.EqualTo(1));
        }

        [Test]
        public void BuildSpawnAssignments_PrefersLaunchLocalHost_WhenSessionParticipantHasAlias()
        {
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                "New World",
                12345,
                0,
                0,
                0,
                4,
                false,
                64,
                64,
                isLocalPlayerHost: true,
                localPlayerId: "session-host");
            var factory = new StartingPositionAssignmentFactory();
            var positions = new[] { new Vector2Int(4, 5) };
            var participants = new[]
            {
                new Participant(
                    new ParticipantIdentity("lobby-host-alias", "Host"),
                    isHost: true),
            };

            SpawnPositionAssignment[] assignments =
                factory.BuildSpawnAssignments(
                    positions,
                    participants,
                    "session-host",
                    hasWorldSettings: true,
                    maxPlayers: 4);

            Assert.That(assignments, Has.Length.EqualTo(1));
            Assert.That(assignments[0].ParticipantId, Is.EqualTo("session-host"));
        }

        [Test]
        public void CanRunStartLogic_DoesNotUseLaunchHost_WhenSessionLocalPlayerDiffers()
        {
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                "New World",
                12345,
                0,
                0,
                0,
                4,
                false,
                64,
                64,
                isLocalPlayerHost: true,
                localPlayerId: "host-1");
            var session = new SessionManagerStub(
                new[] { new Participant(new ParticipantIdentity("client-1", "Client"), isHost: false) },
                localPlayerId: "client-1",
                isLocalPlayerHost: false);
            var policy = CreatePolicy(session);

            Assert.That(policy.CanRunStartLogic(), Is.False);
            Assert.That(policy.ShouldComputeHostStartPositions(), Is.False);
        }

        [Test]
        public void PickStartingPositions_UsesBestEffortFallback_ForGeneratedHost()
        {
            var settings = new StartingPositionInitializerSettings
            {
                minMarginFromBorder = 0,
                relativeMarginFactor = 0f,
                requireHeightMapForStart = true,
                startCandidateAttempts = 1,
                startTerrainSampleRadius = 1,
                minimumLandRatioAroundStart = 1f,
            };
            var selector = new StartingPositionSelector(settings, pathfinder: null);
            var signal = new WorldGeneratedDataSignal
            {
                Source = WorldGeneratedDataSource.GeneratedHost,
                Width = 8,
                Height = 8,
                HeightMap = CreateHeightMap(8, 8, 0.5f),
                TileMap = CreateTileMap(8, 8, "water"),
            };

            var positions = selector.PickStartingPositions(signal, 1);

            Assert.That(positions, Has.Count.EqualTo(1));
            Assert.That(positions[0].x, Is.InRange(0, 7));
            Assert.That(positions[0].y, Is.InRange(0, 7));
            LogAssert.NoUnexpectedReceived();
        }

        private static StartingPositionPolicy CreatePolicy(ISessionManager session)
        {
            return new StartingPositionPolicy(
                new StartingPositionInitializerSettings(),
                session,
                new BootstrapStartingPositionState());
        }

        private static float[,] CreateHeightMap(int width, int height, float value)
        {
            var heightMap = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    heightMap[x, y] = value;
            }

            return heightMap;
        }

        private static string[,] CreateTileMap(int width, int height, string value)
        {
            var tileMap = new string[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    tileMap[x, y] = value;
            }

            return tileMap;
        }

        private sealed class SessionManagerStub : ISessionManager
        {
            public SessionManagerStub(
                IReadOnlyList<Participant> participants,
                string localPlayerId,
                bool isLocalPlayerHost)
            {
                Participants = participants;
                LocalPlayerId = localPlayerId;
                IsLocalPlayerHost = isLocalPlayerHost;
            }

            public IReadOnlyList<Participant> Participants { get; }
            public string LocalPlayerId { get; }
            public bool IsLocalPlayerHost { get; }

            public Task<bool> CreateOrJoinSessionAsync(
                SessionConnectOptions options,
                CancellationToken ct = default)
            {
                return Task.FromResult(false);
            }

            public Task LeaveSessionAsync(CancellationToken ct = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
