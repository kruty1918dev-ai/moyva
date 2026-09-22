using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    /// <summary>
    /// Binary round-trip and corruption guards for the group and recruitment
    /// command payloads added to the authoritative command channel.
    /// </summary>
    [TestFixture]
    public sealed class GameActionPayloadTests
    {
        [Test]
        public void UnitGroupCommandPayload_RoundTripsAllActions()
        {
            foreach (UnitGroupCommandAction action in
                     System.Enum.GetValues(typeof(UnitGroupCommandAction)))
            {
                var payload = new UnitGroupCommandPayload(
                    GameActionMessageKind.Request,
                    action,
                    "p1",
                    "grp-3",
                    new Vector2Int(4, -2),
                    new[] { "u1", "u2" },
                    "ignored-on-request");

                UnitGroupCommandPayload restored =
                    UnitGroupCommandPayload.FromBytes(payload.ToBytes());

                Assert.AreEqual(payload.Kind, restored.Kind);
                Assert.AreEqual(payload.Action, restored.Action);
                Assert.AreEqual("p1", restored.RequesterOwnerId);
                Assert.AreEqual("grp-3", restored.GroupId);
                Assert.AreEqual(new Vector2Int(4, -2), restored.TargetPosition);
                CollectionAssert.AreEqual(new[] { "u1", "u2" }, restored.UnitIds);
                Assert.AreEqual("ignored-on-request", restored.RejectionReason);
            }
        }

        [Test]
        public void UnitGroupCommandPayload_RoundTripsRejection()
        {
            var payload = new UnitGroupCommandPayload(
                GameActionMessageKind.Rejected,
                UnitGroupCommandAction.Move,
                "p2",
                "grp-9",
                Vector2Int.zero,
                null,
                "Group belongs to another owner.");

            UnitGroupCommandPayload restored =
                UnitGroupCommandPayload.FromBytes(payload.ToBytes());

            Assert.AreEqual(GameActionMessageKind.Rejected, restored.Kind);
            Assert.AreEqual("Group belongs to another owner.", restored.RejectionReason);
            Assert.AreEqual(0, restored.UnitIds.Length);
        }

        [Test]
        public void UnitGroupCommandPayload_RejectsTruncatedData()
        {
            var payload = new UnitGroupCommandPayload(
                GameActionMessageKind.Request,
                UnitGroupCommandAction.Create,
                "p1",
                "grp-1",
                Vector2Int.zero,
                new[] { "u1" });
            byte[] bytes = payload.ToBytes();
            var truncated = new byte[bytes.Length - 3];
            System.Array.Copy(bytes, truncated, truncated.Length);

            Assert.Throws<EndOfStreamException>(
                () => UnitGroupCommandPayload.FromBytes(truncated));
        }

        [Test]
        public void UnitGroupCommandPayload_RejectsTrailingData()
        {
            var payload = new UnitGroupCommandPayload(
                GameActionMessageKind.Request,
                UnitGroupCommandAction.Disband,
                "p1",
                "grp-1",
                Vector2Int.zero,
                null);
            byte[] bytes = payload.ToBytes();
            var bloated = new byte[bytes.Length + 1];
            System.Array.Copy(bytes, bloated, bytes.Length);

            Assert.Throws<InvalidDataException>(
                () => UnitGroupCommandPayload.FromBytes(bloated));
        }

        [Test]
        public void UnitGroupSyncPayload_RoundTripsGroupTable()
        {
            var payload = new UnitGroupSyncPayload(
                new[] { "grp-1", "grp-2" },
                new[] { "p1", "p2" },
                new[] { new[] { "u1", "u2" }, new[] { "u3" } });

            UnitGroupSyncPayload restored =
                UnitGroupSyncPayload.FromBytes(payload.ToBytes());

            CollectionAssert.AreEqual(new[] { "grp-1", "grp-2" }, restored.GroupIds);
            CollectionAssert.AreEqual(new[] { "p1", "p2" }, restored.OwnerIds);
            Assert.AreEqual(2, restored.MemberUnitIds.Length);
            CollectionAssert.AreEqual(new[] { "u1", "u2" }, restored.MemberUnitIds[0]);
            CollectionAssert.AreEqual(new[] { "u3" }, restored.MemberUnitIds[1]);
        }

        [Test]
        public void UnitGroupSyncPayload_EmptyTable_RoundTrips()
        {
            var restored = UnitGroupSyncPayload.FromBytes(
                new UnitGroupSyncPayload(null, null, null).ToBytes());
            Assert.AreEqual(0, restored.GroupIds.Length);
        }

        [Test]
        public void UnitRecruitmentCommandPayload_RoundTripsAllActions()
        {
            foreach (UnitRecruitmentCommandAction action in
                     System.Enum.GetValues(typeof(UnitRecruitmentCommandAction)))
            {
                var payload = new UnitRecruitmentCommandPayload(
                    GameActionMessageKind.Request,
                    action,
                    "p1",
                    new Vector2Int(3, 7),
                    42L,
                    "warrior",
                    new Vector2Int(4, 8),
                    "why not");

                UnitRecruitmentCommandPayload restored =
                    UnitRecruitmentCommandPayload.FromBytes(payload.ToBytes());

                Assert.AreEqual(payload.Kind, restored.Kind);
                Assert.AreEqual(payload.Action, restored.Action);
                Assert.AreEqual("p1", restored.RequesterOwnerId);
                Assert.AreEqual(new Vector2Int(3, 7), restored.BuildingPosition);
                Assert.AreEqual(42L, restored.QueueId);
                Assert.AreEqual("warrior", restored.UnitTypeId);
                Assert.AreEqual(new Vector2Int(4, 8), restored.TargetPosition);
                Assert.AreEqual("why not", restored.RejectionReason);
            }
        }

        [Test]
        public void UnitRecruitmentCommandPayload_RejectsTruncatedData()
        {
            byte[] bytes = new UnitRecruitmentCommandPayload(
                GameActionMessageKind.Request,
                UnitRecruitmentCommandAction.Enqueue,
                "p1",
                Vector2Int.one,
                1L,
                "warrior",
                Vector2Int.zero).ToBytes();
            var truncated = new byte[bytes.Length - 2];
            System.Array.Copy(bytes, truncated, truncated.Length);

            Assert.Throws<EndOfStreamException>(
                () => UnitRecruitmentCommandPayload.FromBytes(truncated));
        }

        [Test]
        public void UnitRecruitmentSyncPayload_RoundTripsQueueTable()
        {
            var payload = new UnitRecruitmentSyncPayload(
                new[] { 7L },
                new[] { "p1" },
                new[] { new Vector2Int(2, 3) },
                new[] { "bld-1" },
                new[] { "archer" },
                new[] { 1 },
                new[] { 3 },
                new[] { 100L },
                new[] { 101L },
                new byte[] { 2 },
                new[] { "settlement-1" },
                new[] { 5.5f },
                new[] { 2.25f });

            UnitRecruitmentSyncPayload restored =
                UnitRecruitmentSyncPayload.FromBytes(payload.ToBytes());

            Assert.AreEqual(1, restored.Count);
            Assert.AreEqual(7L, restored.QueueIds[0]);
            Assert.AreEqual("p1", restored.OwnerIds[0]);
            Assert.AreEqual(new Vector2Int(2, 3), restored.BuildingPositions[0]);
            Assert.AreEqual("bld-1", restored.BuildingIds[0]);
            Assert.AreEqual("archer", restored.UnitTypeIds[0]);
            Assert.AreEqual(1, restored.CompletedTurns[0]);
            Assert.AreEqual(3, restored.TrainingTurns[0]);
            Assert.AreEqual(100L, restored.EnqueuedGlobalTurns[0]);
            Assert.AreEqual(101L, restored.LastProgressGlobalTurns[0]);
            Assert.AreEqual(2, restored.Statuses[0]);
            Assert.AreEqual("settlement-1", restored.FundingSettlementIds[0]);
            Assert.AreEqual(5.5f, restored.TrainingSeconds[0]);
            Assert.AreEqual(2.25f, restored.CompletedSeconds[0]);
        }

        [Test]
        public void BuildingPlacePayload_RoundTripsRejectionReason()
        {
            var payload = new BuildingPlacePayload(
                GameActionMessageKind.Rejected,
                "castle",
                new Vector2Int(2, 5),
                "p1",
                "p1",
                hasRelocationSource: true,
                relocationSourcePosition: new Vector2Int(1, 1),
                satisfiedReplacementBuildingId: "gate",
                rotation: ConstructionRotation.Degrees90,
                rejectionReason: "Tile or footprint is occupied.");

            BuildingPlacePayload restored =
                BuildingPlacePayload.FromBytes(payload.ToBytes());

            Assert.AreEqual(GameActionMessageKind.Rejected, restored.Kind);
            Assert.AreEqual("castle", restored.BuildingId);
            Assert.AreEqual(new Vector2Int(2, 5), restored.Position);
            Assert.AreEqual("p1", restored.OwnerId);
            Assert.IsTrue(restored.HasRelocationSource);
            Assert.AreEqual(new Vector2Int(1, 1), restored.RelocationSourcePosition);
            Assert.AreEqual("gate", restored.SatisfiedReplacementBuildingId);
            Assert.AreEqual(ConstructionRotation.Degrees90, restored.Rotation);
            Assert.AreEqual(
                "Tile or footprint is occupied.",
                restored.RejectionReason);
        }

        [Test]
        public void BuildingPlacePayload_ReadsVersion2_WithoutRejectionReason()
        {
            // Extension format v2 (before RejectionReason was added) must stay
            // readable: older peers serialize without the trailing reason.
            byte[] bytes;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write((byte)GameActionMessageKind.Request);
                w.Write("castle");
                w.Write(3);
                w.Write(4);
                w.Write("p1");
                w.Write("p1");
                w.Write((byte)0xA7);
                w.Write((byte)2);
                w.Write(false);
                w.Write(0);
                w.Write(0);
                w.Write("");
                w.Write((byte)1);
                bytes = ms.ToArray();
            }

            BuildingPlacePayload restored =
                BuildingPlacePayload.FromBytes(bytes);

            Assert.AreEqual(GameActionMessageKind.Request, restored.Kind);
            Assert.AreEqual("castle", restored.BuildingId);
            Assert.AreEqual(new Vector2Int(3, 4), restored.Position);
            Assert.AreEqual(ConstructionRotation.Degrees90, restored.Rotation);
            Assert.IsNull(restored.RejectionReason);
        }

        [Test]
        public void BuildingPlacePayload_RequestWithoutReason_RoundTripsEmpty()
        {
            var payload = new BuildingPlacePayload(
                GameActionMessageKind.Request,
                "castle",
                new Vector2Int(7, -3),
                "p1",
                "p1");

            BuildingPlacePayload restored =
                BuildingPlacePayload.FromBytes(payload.ToBytes());

            Assert.AreEqual(GameActionMessageKind.Request, restored.Kind);
            Assert.IsTrue(string.IsNullOrEmpty(restored.RejectionReason));
        }

        [Test]
        public void UnitRecruitmentSyncPayload_EmptyTable_RoundTrips()
        {
            var restored = UnitRecruitmentSyncPayload.FromBytes(
                new UnitRecruitmentSyncPayload(
                    null, null, null, null, null, null, null,
                    null, null, null, null, null, null).ToBytes());
            Assert.AreEqual(0, restored.Count);
        }
    }
}
