using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Signals
{
    [TestFixture]
    public sealed class WorldGenerationSignalStateTests
    {
        [Test]
        public void StoreWorldGeneratedData_ReturnsClonedPayload()
        {
            var state = new WorldGenerationSignalState();
            long sequence = state.BeginWorldSnapshotCycle("test-session");
            var tileMap = new[,] { { "grass" } };
            var objectMap = new[,] { { "tree" } };
            var heightMap = new[,] { { 2.5f } };
            var terrainMap = new[,] { { 3 } };

            var stored = state.StoreWorldGeneratedData(new WorldGeneratedDataSignal
            {
                StartupSequence = sequence,
                StartupSessionId = "test-session",
                Source = WorldGeneratedDataSource.GeneratedHost,
                Width = 1,
                Height = 1,
                TileMap = tileMap,
                ObjectMap = objectMap,
                HeightMap = heightMap,
                TerrainLevelMap = terrainMap,
                HasMapWorldBounds = true,
                MapWorldBoundsCenter = Vector3.one,
                MapWorldBoundsSize = Vector3.one * 2f,
            });

            tileMap[0, 0] = "water";
            objectMap[0, 0] = "rock";
            heightMap[0, 0] = 7.5f;
            terrainMap[0, 0] = 9;

            Assert.IsTrue(state.TryGetWorldGeneratedData(out var cached));
            Assert.AreEqual(sequence, stored.StartupSequence);
            Assert.Greater(stored.SnapshotRevision, 0);
            Assert.AreEqual("grass", cached.TileMap[0, 0]);
            Assert.AreEqual("tree", cached.ObjectMap[0, 0]);
            Assert.AreEqual(2.5f, cached.HeightMap[0, 0], 0.001f);
            Assert.AreEqual(3, cached.TerrainLevelMap[0, 0]);
            Assert.AreEqual(Vector3.one, cached.MapWorldBoundsCenter);
        }

        [Test]
        public void StoreWorldSpawnPositions_ReturnsClonedAssignments()
        {
            var state = new WorldGenerationSignalState();
            long sequence = state.BeginWorldSnapshotCycle("spawn-session");
            var assignments = new[]
            {
                new SpawnPositionAssignment
                {
                    SlotIndex = 0,
                    ParticipantId = "player_0",
                    Position = new Vector2Int(4, 6),
                }
            };

            Assert.IsTrue(state.TryStoreWorldSpawnPositions(new WorldSpawnPositionsSignal
            {
                StartupSequence = sequence,
                StartupSessionId = "spawn-session",
                Source = WorldSpawnPositionsSource.GeneratedHost,
                Assignments = assignments,
            }, out var stored));

            assignments[0] = new SpawnPositionAssignment
            {
                SlotIndex = 1,
                ParticipantId = "player_1",
                Position = new Vector2Int(8, 9),
            };

            Assert.IsTrue(state.TryGetWorldSpawnPositions(out var cached));
            Assert.AreEqual(sequence, stored.StartupSequence);
            Assert.Greater(stored.SnapshotRevision, 0);
            Assert.AreEqual(1, cached.Assignments.Length);
            Assert.AreEqual(0, cached.Assignments[0].SlotIndex);
            Assert.AreEqual("player_0", cached.Assignments[0].ParticipantId);
            Assert.AreEqual(new Vector2Int(4, 6), cached.Assignments[0].Position);
        }

        [Test]
        public void BeginWorldSnapshotCycle_ClearsOldSpawnSnapshot()
        {
            var state = new WorldGenerationSignalState();
            long oldSequence = state.BeginWorldSnapshotCycle("world-a");
            Assert.IsTrue(state.TryStoreWorldSpawnPositions(new WorldSpawnPositionsSignal
            {
                StartupSequence = oldSequence,
                StartupSessionId = "world-a",
                Source = WorldSpawnPositionsSource.GeneratedHost,
                Assignments = new[] { new SpawnPositionAssignment { SlotIndex = 0, Position = new Vector2Int(1, 1) } },
            }, out _));

            long newSequence = state.BeginWorldSnapshotCycle("world-b");

            Assert.AreNotEqual(oldSequence, newSequence);
            Assert.IsFalse(state.TryGetWorldSpawnPositions(out _));
            Assert.IsTrue(state.TryGetCurrentWorldIdentity(out var currentSequence, out var sessionId));
            Assert.AreEqual(newSequence, currentSequence);
            Assert.AreEqual("world-b", sessionId);
        }

        [Test]
        public void TryStoreWorldSpawnPositions_PrefersHigherPrioritySource()
        {
            var state = new WorldGenerationSignalState();
            long sequence = state.BeginWorldSnapshotCycle("priority-session");

            Assert.IsTrue(state.TryStoreWorldSpawnPositions(new WorldSpawnPositionsSignal
            {
                StartupSequence = sequence,
                StartupSessionId = "priority-session",
                Source = WorldSpawnPositionsSource.GeneratedHost,
                Assignments = new[] { new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "local", Position = new Vector2Int(2, 2) } },
            }, out var generated));

            Assert.IsTrue(state.TryStoreWorldSpawnPositions(new WorldSpawnPositionsSignal
            {
                StartupSequence = sequence,
                StartupSessionId = "priority-session",
                Source = WorldSpawnPositionsSource.MultiplayerSync,
                Assignments = new[] { new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "remote", Position = new Vector2Int(5, 5) } },
            }, out var synced));

            Assert.IsFalse(state.TryStoreWorldSpawnPositions(new WorldSpawnPositionsSignal
            {
                StartupSequence = sequence,
                StartupSessionId = "priority-session",
                Source = WorldSpawnPositionsSource.DirectGameplayTest,
                Assignments = new[] { new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "test", Position = new Vector2Int(9, 9) } },
            }, out _));

            Assert.AreNotEqual(generated.SnapshotRevision, synced.SnapshotRevision);
            Assert.IsTrue(state.TryGetWorldSpawnPositions(out var cached));
            Assert.AreEqual(WorldSpawnPositionsSource.MultiplayerSync, cached.Source);
            Assert.AreEqual("remote", cached.Assignments[0].ParticipantId);
            Assert.AreEqual(new Vector2Int(5, 5), cached.Assignments[0].Position);
        }
    }
}
