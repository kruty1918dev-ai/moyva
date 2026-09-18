using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    [TestFixture]
    public sealed class FogOfWarSaveModuleTests
    {
        [Test]
        public void RoundTrip_Explored_OwnerExplored_AndIntel_Restored()
        {
            var sourceFog = new FakeFogStore
            {
                Explored = new bool[4, 4]
                {
                    { true, false, false, false },
                    { false, true, false, false },
                    { false, false, true, false },
                    { false, false, false, true },
                },
            };
            sourceFog.OwnerExplored["owner-a"] = new bool[4, 4]
            {
                { true, true, false, false },
                { false, false, false, false },
                { false, false, false, false },
                { false, false, false, false },
            };

            var sourceIntel = new FakeIntelStore();
            sourceIntel.Snapshots["owner-a"] = new FogIntelSnapshot
            {
                Units =
                {
                    new FogIntelUnitRecord
                    {
                        UnitId = "u1",
                        TypeId = "warrior",
                        OwnerId = "owner-b",
                        LastKnownPosition = new Vector2Int(3, 2),
                        LastSeenSequence = 42,
                    },
                },
                Buildings =
                {
                    new FogIntelBuildingRecord
                    {
                        BuildingId = "b1",
                        OwnerId = "owner-b",
                        Position = new Vector2Int(1, 1),
                        RotationQuarterTurns = 2,
                        LastSeenSequence = 7,
                    },
                },
            };

            var stream = new MemoryStream();
            var module = new FogOfWarSaveModule(sourceFog, sourceIntel);
            module.OnSave(new FakeSaveContext(stream));

            stream.Position = 0;
            var targetFog = new FakeFogStore();
            var targetIntel = new FakeIntelStore();
            var module2 = new FogOfWarSaveModule(targetFog, targetIntel);
            module2.OnLoad(new FakeSaveContext(stream));

            Assert.IsTrue(targetFog.Explored[0, 0]);
            Assert.IsTrue(targetFog.Explored[3, 3]);
            Assert.IsFalse(targetFog.Explored[0, 1]);

            Assert.IsTrue(targetFog.OwnerExplored["owner-a"][0, 0]);
            Assert.IsTrue(targetFog.OwnerExplored["owner-a"][0, 1]);
            Assert.IsFalse(targetFog.OwnerExplored["owner-a"][1, 0]);

            FogIntelSnapshot intel = targetIntel.Snapshots["owner-a"];
            Assert.AreEqual(1, intel.Units.Count);
            Assert.AreEqual("u1", intel.Units[0].UnitId);
            Assert.AreEqual(new Vector2Int(3, 2), intel.Units[0].LastKnownPosition);
            Assert.AreEqual(42, intel.Units[0].LastSeenSequence);
            Assert.AreEqual(1, intel.Buildings.Count);
            Assert.AreEqual("b1", intel.Buildings[0].BuildingId);
            Assert.AreEqual(2, intel.Buildings[0].RotationQuarterTurns);
        }

        [Test]
        public void Load_LegacyExploredOnly_StillWorks()
        {
            // Pre-versioned save layout: width, height, then the bool matrix.
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(2);
            writer.Write(2);
            writer.Write(true);
            writer.Write(false);
            writer.Write(false);
            writer.Write(true);

            stream.Position = 0;
            var targetFog = new FakeFogStore();
            new FogOfWarSaveModule(targetFog).OnLoad(new FakeSaveContext(stream));

            Assert.IsTrue(targetFog.Explored[0, 0]);
            Assert.IsFalse(targetFog.Explored[0, 1]);
            Assert.IsTrue(targetFog.Explored[1, 1]);
        }

        [Test]
        public void Load_OlderVersioned_WithoutIntel_DoesNotThrow()
        {
            // Format -2: version, width, height, matrix, fixedAreaCount.
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(-2);
            writer.Write(2);
            writer.Write(2);
            writer.Write(true);
            writer.Write(true);
            writer.Write(true);
            writer.Write(true);
            writer.Write(0);

            stream.Position = 0;
            var targetFog = new FakeFogStore();
            var targetIntel = new FakeIntelStore();
            var module = new FogOfWarSaveModule(targetFog, targetIntel);

            Assert.DoesNotThrow(() => module.OnLoad(new FakeSaveContext(stream)));
            Assert.IsTrue(targetFog.Explored[1, 1]);
            Assert.AreEqual(0, targetIntel.Snapshots.Count);
        }

        // ── Fakes ───────────────────────────────────────────────────────────

        private sealed class FakeSaveContext : ISaveContext
        {
            public FakeSaveContext(MemoryStream stream)
            {
                Writer = new BinaryWriter(stream);
                Reader = new BinaryReader(stream);
            }

            public BinaryWriter Writer { get; }
            public BinaryReader Reader { get; }
        }

        private sealed class FakeFogStore
            : IFogExplorationSnapshotStore
            , IFogOwnerExplorationSnapshotStore
        {
            public bool[,] Explored;
            public readonly Dictionary<string, bool[,]> OwnerExplored =
                new Dictionary<string, bool[,]>();

            public bool[,] GetExploredSnapshot() => Explored;
            public void LoadFromSnapshot(bool[,] explored) => Explored = explored;
            public IReadOnlyCollection<string> GetKnownFogOwnerIds()
                => OwnerExplored.Keys.ToList();
            public bool[,] GetExploredSnapshot(string ownerId)
                => OwnerExplored.TryGetValue(ownerId, out var snapshot) ? snapshot : null;
            public void LoadFromSnapshot(string ownerId, bool[,] explored)
                => OwnerExplored[ownerId] = explored;
        }

        private sealed class FakeIntelStore : IFogIntelSnapshotStore
        {
            public readonly Dictionary<string, FogIntelSnapshot> Snapshots =
                new Dictionary<string, FogIntelSnapshot>();

            public IReadOnlyCollection<string> GetIntelOwnerIds()
                => Snapshots.Keys.ToList();
            public FogIntelSnapshot CaptureSnapshot(string ownerId)
                => Snapshots[ownerId];
            public void LoadSnapshot(string ownerId, FogIntelSnapshot snapshot)
                => Snapshots[ownerId] = snapshot;
        }
    }
}
