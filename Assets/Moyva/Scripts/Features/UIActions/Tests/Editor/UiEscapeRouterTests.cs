using System;
using System.Collections.Generic;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.UIActions.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.UIActions
{
    /// <summary>
    /// P062: Escape must close exactly one context per press, in stack
    /// priority order — never several independent states at once.
    /// </summary>
    [TestFixture]
    internal sealed class UiEscapeRouterTests
    {
        private static readonly UiActionId PanelEscape = new("test.panel-close");
        private static readonly UiActionId ModeEscape = new("test.mode-cancel");

        private sealed class FakeRouter : IUiActionRouter
        {
            public readonly List<UiActionId> Executed = new();
            public readonly Dictionary<UiActionId, UiActionResult> Results = new();

            public UiActionResult Execute(in UiActionRequest request)
                => Execute(request.ActionId, request.Source, request.ContextId, request.TargetId);

            public UiActionResult Execute(UiActionId actionId,
                UiActionSource source = UiActionSource.Programmatic,
                string contextId = null, string targetId = null)
            {
                Executed.Add(actionId);
                return Results.TryGetValue(actionId, out var result)
                    ? result
                    : UiActionResult.Performed();
            }
        }

        private static UiEscapeRouter CreateRouter(
            UiContextStack stack, FakeRouter actions)
            => new UiEscapeRouter(stack, actions, null);

        [Test]
        public void Escape_ClosesTopmostContextOnly()
        {
            var stack = new UiContextStack();
            var actions = new FakeRouter();
            stack.Push(new UiContextRegistration(
                "mode", UiContextLayer.Mode, 10, () => true, ModeEscape));
            stack.Push(new UiContextRegistration(
                "panel", UiContextLayer.Panel, 300, () => true, PanelEscape));
            var router = CreateRouter(stack, actions);

            Assert.IsTrue(router.TryHandleEscape());

            Assert.AreEqual(1, actions.Executed.Count);
            Assert.AreEqual(PanelEscape, actions.Executed[0],
                "The topmost (panel) context must handle the first Escape.");
        }

        [Test]
        public void Escape_AfterTopDismissed_ReachesNextLevel()
        {
            var stack = new UiContextStack();
            var actions = new FakeRouter();
            bool panelOpen = true;
            stack.Push(new UiContextRegistration(
                "mode", UiContextLayer.Mode, 10, () => true, ModeEscape));
            stack.Push(new UiContextRegistration(
                "panel", UiContextLayer.Panel, 300, () => panelOpen, PanelEscape));
            var router = CreateRouter(stack, actions);

            Assert.IsTrue(router.TryHandleEscape());
            panelOpen = false; // the close actually dismissed the dialog
            Assert.IsTrue(router.TryHandleEscape());

            Assert.AreEqual(2, actions.Executed.Count);
            Assert.AreEqual(ModeEscape, actions.Executed[1],
                "The second press must reach the underlying mode context.");
        }

        [Test]
        public void Escape_WrongContextRejection_FallsThrough()
        {
            var stack = new UiContextStack();
            var actions = new FakeRouter();
            actions.Results[PanelEscape] =
                UiActionResult.Rejected(UiActionReason.WrongContext);
            stack.Push(new UiContextRegistration(
                "mode", UiContextLayer.Mode, 10, () => true, ModeEscape));
            stack.Push(new UiContextRegistration(
                "panel", UiContextLayer.Panel, 300, () => true, PanelEscape));
            var router = CreateRouter(stack, actions);

            Assert.IsTrue(router.TryHandleEscape());

            Assert.AreEqual(2, actions.Executed.Count);
            Assert.AreEqual(ModeEscape, actions.Executed[1],
                "WrongContext means the context declined — the next level runs.");
        }

        [Test]
        public void Escape_DomainRejection_StopsPropagation()
        {
            // e.g. initial-castle veto: the rejection is meaningful — do NOT
            // fall through and cancel additional state behind the dialog.
            var stack = new UiContextStack();
            var actions = new FakeRouter();
            actions.Results[PanelEscape] =
                UiActionResult.Rejected(UiActionReason.ModalBlocked);
            stack.Push(new UiContextRegistration(
                "mode", UiContextLayer.Mode, 10, () => true, ModeEscape));
            stack.Push(new UiContextRegistration(
                "panel", UiContextLayer.Panel, 300, () => true, PanelEscape));
            var router = CreateRouter(stack, actions);

            Assert.IsTrue(router.TryHandleEscape());

            Assert.AreEqual(1, actions.Executed.Count,
                "A real domain rejection must stop Escape propagation.");
        }

        [Test]
        public void Escape_NoActiveContexts_ReturnsFalse()
        {
            var stack = new UiContextStack();
            var actions = new FakeRouter();
            var router = CreateRouter(stack, actions);

            Assert.IsFalse(router.TryHandleEscape());
            Assert.IsEmpty(actions.Executed);
        }

        [Test]
        public void Escape_InactiveTopContext_SkipsToActiveOne()
        {
            var stack = new UiContextStack();
            var actions = new FakeRouter();
            stack.Push(new UiContextRegistration(
                "mode", UiContextLayer.Mode, 10, () => true, ModeEscape));
            stack.Push(new UiContextRegistration(
                "panel", UiContextLayer.Panel, 300, () => false, PanelEscape));
            var router = CreateRouter(stack, actions);

            Assert.IsTrue(router.TryHandleEscape());
            Assert.AreEqual(1, actions.Executed.Count);
            Assert.AreEqual(ModeEscape, actions.Executed[0]);
        }
    }
}
