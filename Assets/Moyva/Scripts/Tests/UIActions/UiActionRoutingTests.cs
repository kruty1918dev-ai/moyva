using System;
using System.Collections.Generic;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.UIActions.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.UIActions
{
    [TestFixture]
    public sealed class UiActionRoutingTests
    {
        private sealed class RecordingHandler : IUiActionHandler
        {
            private readonly UiActionResult _result;

            public RecordingHandler(
                IEnumerable<UiActionId> actionIds,
                UiActionResult? result = null)
            {
                ActionIds = new List<UiActionId>(actionIds);
                _result = result ?? UiActionResult.Performed();
            }

            public IReadOnlyCollection<UiActionId> ActionIds { get; }
            public readonly List<UiActionRequest> Requests = new();

            public UiActionResult Execute(in UiActionRequest request)
            {
                Requests.Add(request);
                return _result;
            }
        }

        [Test]
        public void UiActionId_ValidId_IsAccepted()
        {
            var id = new UiActionId("ui.construction.open");

            Assert.AreEqual("ui.construction.open", id.Value);
        }

        [Test]
        public void UiActionId_InvalidId_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => _ = new UiActionId("Build"));
            Assert.Throws<ArgumentException>(() => _ = new UiActionId("ui_build_open"));
        }

        [Test]
        public void UiActionId_Equality_IsOrdinalAndStable()
        {
            Assert.AreEqual(
                new UiActionId("ui.construction.open"),
                UiActionIds.Construction.Open);
            Assert.AreNotEqual(
                new UiActionId("ui.construction.open"),
                new UiActionId("ui.construction.close"));
        }

        [Test]
        public void UiActionRouter_RegisteredAction_ExecutesHandler()
        {
            var journal = new UiActionJournal();
            var handler = new RecordingHandler(new[] { UiActionIds.Construction.Open });
            var router = new UiActionRouter(new List<IUiActionHandler> { handler }, journal);

            UiActionResult result = router.Execute(
                UiActionIds.Construction.Open,
                UiActionSource.Button);

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(1, handler.Requests.Count);
        }

        [Test]
        public void UiActionRouter_UnknownAction_ReturnsUnavailable()
        {
            var journal = new UiActionJournal();
            var router = new UiActionRouter(new List<IUiActionHandler>(), journal);

            UiActionResult result = router.Execute(
                UiActionIds.Construction.Open,
                UiActionSource.Programmatic);

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(UiActionReason.ActionUnavailable, result.Reason);
            Assert.IsFalse(result.Consumed);
            Assert.AreEqual(1, journal.Count);
        }

        [Test]
        public void UiActionRouter_DuplicateHandlerRegistration_IsRejected()
        {
            var left = new RecordingHandler(new[] { UiActionIds.Construction.Open });
            var right = new RecordingHandler(new[] { UiActionIds.Construction.Open });

            Assert.Throws<InvalidOperationException>(
                () => _ = new UiActionRouter(
                    new List<IUiActionHandler> { left, right },
                    new UiActionJournal()));
        }

        [Test]
        public void UiActionRouter_ReturnsHandlerResult()
        {
            var expected = UiActionResult.Rejected(
                UiActionReason.InsufficientResources,
                consumed: true);
            var handler = new RecordingHandler(
                new[] { UiActionIds.Recruitment.Enqueue },
                expected);
            var router = new UiActionRouter(
                new List<IUiActionHandler> { handler },
                new UiActionJournal());

            UiActionResult result = router.Execute(
                UiActionIds.Recruitment.Enqueue,
                UiActionSource.Button);

            Assert.AreEqual(expected.Status, result.Status);
            Assert.AreEqual(expected.Reason, result.Reason);
            Assert.AreEqual(expected.Consumed, result.Consumed);
        }

        [Test]
        public void UiActionRouter_RecordsEveryExecutionInJournal()
        {
            var journal = new UiActionJournal();
            var handler = new RecordingHandler(new[] { UiActionIds.Construction.Open });
            var router = new UiActionRouter(new List<IUiActionHandler> { handler }, journal);

            router.Execute(UiActionIds.Construction.Open, UiActionSource.Button);
            router.Execute(UiActionIds.Construction.Open, UiActionSource.Hotkey);

            Assert.AreEqual(2, journal.Count);
        }

        [Test]
        public void UiAction_SourceAndTargetArePassedToHandler()
        {
            var handler = new RecordingHandler(new[] { UiActionIds.Construction.SelectBuilding });
            var router = new UiActionRouter(
                new List<IUiActionHandler> { handler },
                new UiActionJournal());

            router.Execute(new UiActionRequest(
                UiActionIds.Construction.SelectBuilding,
                UiActionSource.Button,
                "ConstructionPanel",
                "barracks"));

            Assert.AreEqual(UiActionSource.Button, handler.Requests[0].Source);
            Assert.AreEqual("barracks", handler.Requests[0].TargetId);
        }
    }
}
