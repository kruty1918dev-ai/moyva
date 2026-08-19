using System.Collections.Generic;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.UIActions.Runtime;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace Kruty1918.Moyva.Tests.UIActions
{
    [TestFixture]
    public sealed class UiActionRoutingTests
    {
        private sealed class RecordingHandler : IUiActionHandler
        {
            private readonly HashSet<string> _actionIds;
            private readonly UiActionResult _result;

            public RecordingHandler(
                IEnumerable<string> actionIds,
                UiActionResult? result = null)
            {
                _actionIds = new HashSet<string>(actionIds);
                _result = result ?? UiActionResult.Performed();
            }

            public readonly List<UiActionRequest> Requests = new();

            public bool CanHandle(string actionId)
                => _actionIds.Contains(actionId);

            public UiActionResult Handle(UiActionRequest request)
            {
                Requests.Add(request);
                return _result;
            }
        }

        [Test]
        public void Escape_WhenPlacementActive_CancelsPlacement()
        {
            AssertEscapeRoutesTo(
                "BuildingPlacement",
                UiActionId.BuildCancel,
                UiActionId.BuildCancel);
        }

        [Test]
        public void Escape_WhenDeploymentActive_CancelsDeployment()
        {
            AssertEscapeRoutesTo(
                "DeploymentMode",
                UiActionId.DeploymentCancel,
                UiActionId.DeploymentCancel);
        }

        [Test]
        public void Escape_WhenPanelOpen_ClosesTopPanel()
        {
            AssertEscapeRoutesTo(
                "RecruitmentPanel",
                UiActionId.RecruitmentClose,
                UiActionId.RecruitmentClose);
        }

        [Test]
        public void Escape_WhenModalOpen_ClosesOnlyModal()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var modal = new RecordingHandler(new[] { UiActionId.ModalClose });
            var panel = new RecordingHandler(new[] { UiActionId.PanelClose });
            var router = new UiActionRouter(new List<IUiActionHandler> { panel, modal }, contexts, journal);
            var escape = new UiEscapeRouter(contexts, router, journal);

            contexts.Push(new UiContextRegistration(
                "WorldInfoPanel",
                UiContextLayer.Panel,
                10,
                () => true,
                UiActionId.PanelClose));
            contexts.Push(new UiContextRegistration(
                "ConfirmModal",
                UiContextLayer.Modal,
                10,
                () => true,
                UiActionId.ModalClose));

            Assert.IsTrue(escape.TryHandleEscape());
            Assert.AreEqual(1, modal.Requests.Count);
            Assert.AreEqual(0, panel.Requests.Count);
        }

        [Test]
        public void Escape_WhenNothingConsumes_OpensPause()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var pause = new RecordingHandler(new[] { UiActionId.PauseOpen });
            var router = new UiActionRouter(new List<IUiActionHandler> { pause }, contexts, journal);
            var escape = new UiEscapeRouter(contexts, router, journal);

            Assert.IsFalse(escape.TryHandleEscape());
            router.Execute(UiActionId.PauseOpen, UiActionSource.Escape, "Gameplay");

            Assert.AreEqual(1, pause.Requests.Count);
        }

        [Test]
        public void Escape_WhenPauseOpen_ClosesPause()
        {
            AssertEscapeRoutesTo(
                "PauseModal",
                UiActionId.PauseClose,
                UiActionId.PauseClose,
                UiContextLayer.Modal);
        }

        [Test]
        public void Escape_DoesNotCancelAndPauseSameFrame()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var cancel = new RecordingHandler(new[] { UiActionId.BuildCancel });
            var pause = new RecordingHandler(new[] { UiActionId.PauseOpen });
            var router = new UiActionRouter(new List<IUiActionHandler> { cancel, pause }, contexts, journal);
            var escape = new UiEscapeRouter(contexts, router, journal);
            contexts.Push(new UiContextRegistration(
                "BuildingPlacement",
                UiContextLayer.Mode,
                10,
                () => true,
                UiActionId.BuildCancel));

            Assert.IsTrue(escape.TryHandleEscape());

            Assert.AreEqual(1, cancel.Requests.Count);
            Assert.AreEqual(0, pause.Requests.Count);
        }

        [Test]
        public void Escape_OnlyOneHandlerConsumesPerPress()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var top = new RecordingHandler(new[] { UiActionId.DeploymentCancel });
            var lower = new RecordingHandler(new[] { UiActionId.BuildCancel });
            var router = new UiActionRouter(new List<IUiActionHandler> { lower, top }, contexts, journal);
            var escape = new UiEscapeRouter(contexts, router, journal);
            contexts.Push(new UiContextRegistration("BuildingPlacement", UiContextLayer.Mode, 10, () => true, UiActionId.BuildCancel));
            contexts.Push(new UiContextRegistration("DeploymentMode", UiContextLayer.Mode, 40, () => true, UiActionId.DeploymentCancel));

            Assert.IsTrue(escape.TryHandleEscape());

            Assert.AreEqual(1, top.Requests.Count);
            Assert.AreEqual(0, lower.Requests.Count);
        }

        [Test]
        public void Hotkey_AndButton_InvokeSameAction()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var handler = new RecordingHandler(new[] { UiActionId.BuildToggle });
            var router = new UiActionRouter(new List<IUiActionHandler> { handler }, contexts, journal);

            router.Execute(UiActionId.BuildToggle, UiActionSource.Button, "Gameplay");
            router.Execute(UiActionId.BuildToggle, UiActionSource.Hotkey, "Gameplay");

            Assert.AreEqual(2, handler.Requests.Count);
            Assert.AreEqual(UiActionId.BuildToggle, handler.Requests[0].ActionId);
            Assert.AreEqual(UiActionId.BuildToggle, handler.Requests[1].ActionId);
        }

        [Test]
        public void Hotkey_DisabledInWrongContext()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var handler = new RecordingHandler(new[] { UiActionId.BuildToggle });
            var router = new UiActionRouter(new List<IUiActionHandler> { handler }, contexts, journal);
            contexts.Push(new UiContextRegistration(
                "DeploymentMode",
                UiContextLayer.Mode,
                40,
                () => true,
                UiActionId.DeploymentCancel,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[] { UiActionId.DeploymentConfirm }));

            UiActionResult result = router.Execute(UiActionId.BuildToggle, UiActionSource.Hotkey, "DeploymentMode");

            Assert.AreEqual(UiActionResultState.Rejected, result.State);
            Assert.AreEqual(UiActionReasonCode.WrongContext, result.Reason);
            Assert.AreEqual(0, handler.Requests.Count);
        }

        [Test]
        public void GameplayHotkey_NotTriggeredWhileTyping()
        {
            var contexts = new UiContextStack();
            contexts.Push(new UiContextRegistration(
                "TextEditing",
                UiContextLayer.TextEditing,
                100,
                () => true,
                UiActionId.TextUnfocus,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[] { UiActionId.TextUnfocus }));

            Assert.IsFalse(contexts.IsActionAllowedByContext(UiActionId.BuildToggle));
            Assert.IsTrue(contexts.IsActionAllowedByContext(UiActionId.TextUnfocus));
        }

        [Test]
        public void ConflictingBindings_AreDetected()
        {
            var service = new UiHotkeyService(
                new UiActionRouter(new List<IUiActionHandler>(), new UiContextStack(), new UiActionJournal()),
                new UiContextStack());
            service.SetBinding(new UiHotkeyBinding("test.one", Key.B, allowedContexts: new[] { "Gameplay" }));
            service.SetBinding(new UiHotkeyBinding("test.two", Key.B, allowedContexts: new[] { "Gameplay" }));

            Assert.That(service.DetectConflicts(), Has.Count.GreaterThan(0));
        }

        [Test]
        public void Hotkey_ActionIsJournaled()
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var handler = new RecordingHandler(new[] { UiActionId.BuildToggle });
            var router = new UiActionRouter(new List<IUiActionHandler> { handler }, contexts, journal);

            router.Execute(UiActionId.BuildToggle, UiActionSource.Hotkey, "Gameplay");

            Assert.AreEqual(1, journal.Count);
            Assert.AreEqual(UiActionSource.Hotkey, journal.GetRecent(1)[0].Source);
        }

        private static void AssertEscapeRoutesTo(
            string contextId,
            string escapeActionId,
            string expectedActionId,
            UiContextLayer layer = UiContextLayer.Mode)
        {
            var contexts = new UiContextStack();
            var journal = new UiActionJournal();
            var handler = new RecordingHandler(new[] { expectedActionId });
            var router = new UiActionRouter(new List<IUiActionHandler> { handler }, contexts, journal);
            var escape = new UiEscapeRouter(contexts, router, journal);
            contexts.Push(new UiContextRegistration(
                contextId,
                layer,
                10,
                () => true,
                escapeActionId));

            Assert.IsTrue(escape.TryHandleEscape());
            Assert.AreEqual(1, handler.Requests.Count);
            Assert.AreEqual(expectedActionId, handler.Requests[0].ActionId);
            Assert.AreEqual(UiActionSource.Escape, handler.Requests[0].Source);
        }
    }
}
