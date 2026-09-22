using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.UIActions.API;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;
using Zenject;
using Kruty1918.Notifications.API;
using Kruty1918.UIActions.API;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// P061: every gameplay panel's physical close button must be a real,
    /// raycastable UGUI control that routes through Globals.gameplay.ClosePanel
    /// — and a domain rejection (e.g. initial-castle veto) must keep the panel
    /// open with a visible reason instead of silently hiding it.
    /// </summary>
    [TestFixture]
    internal sealed class GameplayHtmlClosePanelTests
    {
        private static string HudCss => File.ReadAllText(Path.Combine(
            Application.dataPath, "Moyva", "UI", "Gameplay", "GameplayHud.css.txt"));

        private static GameObject CreateRoot()
        {
            var root = new GameObject("P061 Close Test Root", typeof(RectTransform), typeof(Canvas));
            var rect = root.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 720f);
            return root;
        }

        /// <summary>Stands in for the Globals.gameplay script object; the real
        /// bridge exposes the same ClosePanel() entry point.</summary>
        private sealed class FakeGameplayGlobals
        {
            public int CloseCalls;
            public void ClosePanel() => CloseCalls++;
        }

        private static Button FindCloseButton(GameObject root)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            return buttons.FirstOrDefault(button =>
            {
                var label = button.GetComponentInChildren<TMP_Text>(true);
                return label != null
                    && string.Equals(label.text?.Trim(), "X", StringComparison.Ordinal);
            });
        }

        private static void AssertPhysicalCloseButton(
            Action<GameplayHtmlState> open,
            GameplayHtmlSnapshot snapshot = null,
            string viewportClass = "vp-wide")
        {
            var state = new GameplayHtmlState();
            snapshot ??= new GameplayHtmlSnapshot();
            open(state);
            string html = GameplayHtmlMarkup.Build(snapshot, state, viewportClass);

            var root = CreateRoot();
            var globals = new FakeGameplayGlobals();
            using var host = new UnityHtmlHost();
            try
            {
                UnityHtmlMountResult result = host.Mount(
                    root.GetComponent<RectTransform>(),
                    new UnityHtmlDocument(html, HudCss, "P061 CloseButton"),
                    new Dictionary<string, object> { ["gameplay"] = globals });
                Assert.IsTrue(result.Succeeded, result.ErrorMessage);

                Button close = FindCloseButton(root);
                Assert.NotNull(close, "Panel must render a physical close button.");
                Assert.IsTrue(close.interactable, "Close button must be interactable.");
                Assert.NotNull(close.targetGraphic, "Close button needs a raycast graphic.");
                Assert.IsTrue(close.targetGraphic.raycastTarget,
                    "Close button graphic must accept raycasts.");

                var rect = close.GetComponent<RectTransform>();
                Assert.GreaterOrEqual(rect.rect.width, 30f,
                    "Close hit area is too small for reliable clicks.");
                Assert.GreaterOrEqual(rect.rect.height, 30f,
                    "Close hit area is too small for reliable clicks.");

                // Center and near-edge points must land inside the hit rect —
                // a borderline button would fail the "edges" check.
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                Vector3 centerWorld = (corners[0] + corners[2]) * 0.5f;
                Vector3 edgeWorld = Vector3.Lerp(corners[0], corners[2], 0.04f);
                Vector2 center = RectTransformUtility.WorldToScreenPoint(null, centerWorld);
                Vector2 edge = RectTransformUtility.WorldToScreenPoint(null, edgeWorld);
                Assert.IsTrue(RectTransformUtility.RectangleContainsScreenPoint(
                    rect, center, null), "Center of the close button misses its hit area.");
                Assert.IsTrue(RectTransformUtility.RectangleContainsScreenPoint(
                    rect, edge, null), "Edge of the close button misses its hit area.");

                // The close control must stay inside the mounted viewport on
                // every size class — a panel that overflows the screen leaves
                // the X unreachable.
                var rootRect = root.GetComponent<RectTransform>();
                var rootCorners = new Vector3[4];
                rootRect.GetWorldCorners(rootCorners);
                Assert.GreaterOrEqual(corners[0].x, rootCorners[0].x - 0.5f,
                    "Close button overflows the left edge.");
                Assert.LessOrEqual(corners[2].x, rootCorners[2].x + 0.5f,
                    "Close button overflows the right edge.");
                Assert.GreaterOrEqual(corners[0].y, rootCorners[0].y - 0.5f,
                    "Close button overflows the bottom edge.");
                Assert.LessOrEqual(corners[2].y, rootCorners[2].y + 0.5f,
                    "Close button overflows the top edge.");

                close.onClick.Invoke();
                Assert.AreEqual(1, globals.CloseCalls,
                    "Clicking the close button must route to Globals.gameplay.ClosePanel().");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CloseButton_NotificationsPanel_PhysicalClickCloses()
            => AssertPhysicalCloseButton(
                state => state.OpenPanel(GameplayHtmlPanel.Notifications));

        [Test]
        public void CloseButton_SupplyPanel_PhysicalClickCloses()
            => AssertPhysicalCloseButton(
                state => state.OpenSupplyPanel(new Vector2Int(3, 4), "lumber-camp"));

        [Test]
        public void CloseButton_ConstructionPanel_PhysicalClickCloses()
            => AssertPhysicalCloseButton(
                state => state.OpenPanel(GameplayHtmlPanel.Construction));

        [Test]
        public void CloseButton_ConstructionPanel_ShortViewport_KeepsCloseOnScreen()
            => AssertPhysicalCloseButton(
                state => state.OpenPanel(GameplayHtmlPanel.Construction),
                viewportClass: "vp-short");

        [Test]
        public void CloseButton_ConstructionPanel_CompactViewport_KeepsCloseOnScreen()
            => AssertPhysicalCloseButton(
                state => state.OpenPanel(GameplayHtmlPanel.Construction),
                viewportClass: "vp-compact");

        [Test]
        public void CloseButton_KingdomDashboard_ShortViewport_KeepsCloseOnScreen()
            => AssertPhysicalCloseButton(
                state => state.OpenPanel(GameplayHtmlPanel.Kingdom),
                viewportClass: "vp-short");

        [Test]
        public void CloseButton_KingdomDashboard_PhysicalClickCloses()
            => AssertPhysicalCloseButton(
                state => state.OpenPanel(GameplayHtmlPanel.Kingdom));

        [Test]
        public void CloseButton_SelectionPanel_PhysicalClickCloses()
            => AssertPhysicalCloseButton(
                _ => { },
                new GameplayHtmlSnapshot
                {
                    SelectionKind = "Building",
                    SelectionId = "lumber-camp",
                    SelectionTitle = "Lumber camp",
                    SelectionPosition = new Vector2Int(5, 6),
                });

        [Test]
        public void OnboardingPanel_RendersNoCloseButton()
        {
            var state = new GameplayHtmlState();
            var snapshot = new GameplayHtmlSnapshot { RequiresFirstCastle = true };
            string html = GameplayHtmlMarkup.Build(snapshot, state, "vp-wide");

            var root = CreateRoot();
            using var host = new UnityHtmlHost();
            try
            {
                UnityHtmlMountResult result = host.Mount(
                    root.GetComponent<RectTransform>(),
                    new UnityHtmlDocument(html, HudCss, "P061 Onboarding"),
                    new Dictionary<string, object> { ["gameplay"] = new FakeGameplayGlobals() });
                Assert.IsTrue(result.Succeeded, result.ErrorMessage);
                Assert.IsNull(FindCloseButton(root),
                    "The initial-castle onboarding panel must not offer a close button.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        // ---------- bridge-level: rejection keeps panel open with a reason ----------

        private sealed class FakeRouter : IUiActionRouter
        {
            public readonly Dictionary<string, UiActionResult> Results =
                new(StringComparer.Ordinal);
            public readonly List<string> Executed = new();
            public bool ThrowOnExecute;

            public UiActionResult Execute(in UiActionRequest request)
                => Execute(request.ActionId, request.Source, request.ContextId, request.TargetId);

            public UiActionResult Execute(UiActionId actionId,
                UiActionSource source = UiActionSource.Programmatic,
                string contextId = null, string targetId = null)
            {
                Executed.Add(actionId.ToString());
                if (ThrowOnExecute)
                    throw new InvalidOperationException("simulated router failure");
                return Results.TryGetValue(actionId.ToString(), out var result)
                    ? result
                    : UiActionResult.Performed();
            }
        }

        private sealed class FakeConstructionClose : Kruty1918.Moyva.Construction.API.IConstructionSessionCommands
        {
            public Kruty1918.Moyva.Construction.API.BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) { }
            public string GetActiveOwner() => null;
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => false;
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            { buildingId = null; return false; }
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => new Dictionary<Vector2Int, string>();
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => false;
            public bool TryGetPendingPlacementStatus(Vector2Int position,
                out Kruty1918.Moyva.Construction.API.ConstructionPendingPlacementStatus status)
            { status = default; return false; }
            public Kruty1918.Moyva.Construction.API.ConstructionResourceProjection
                GetResourceProjection(Vector2Int position) => null;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(string buildingId)
                => new Dictionary<string, float>();
            public void Confirm() { }
            public void Cancel() { }
            public void UndoLast() { }
            public void RedoLast() { }
            public void ToggleDemolishMode() { }
            public bool TryDemolishAt(Vector2Int position) => false;
            public string GetLastActionMessage() => null;
        }

        private static GameplayHtmlBridge CreateBridge(
            GameplayHtmlState state, FakeRouter router)
        {
            var container = new DiContainer();
            container.Bind<IUiActionRouter>().FromInstance(router);
            var lazy = new LazyInject<IUiActionRouter>(container,
                new InjectContext(container, typeof(IUiActionRouter)));
            return new GameplayHtmlBridge(
                state, null, lazy, new FakeConstructionClose(),
                null, null, null);
        }

        [Test]
        public void ClosePanel_ConstructionRejected_KeepsPanel_AndSurfacesReason()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            var router = new FakeRouter();
            router.Results[UiActionIds.Construction.Close.ToString()] =
                UiActionResult.Rejected(UiActionReason.ModalBlocked,
                    "Place the first castle before leaving construction mode.");
            var bridge = CreateBridge(state, router);

            bridge.ClosePanel();

            Assert.AreEqual(GameplayHtmlPanel.Construction, state.OpenPanelId,
                "A rejected close must keep the panel open.");
            Assert.IsTrue(state.Feedback.Contains("castle"),
                "Rejection reason must reach the feedback line, got: " + state.Feedback);
        }

        [Test]
        public void ClosePanel_ConstructionAllowed_Closes()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            var router = new FakeRouter();
            var bridge = CreateBridge(state, router);

            bridge.ClosePanel();

            Assert.IsTrue(state.PanelClosing,
                "An allowed close enters the closing state first.");
            Assert.AreEqual(GameplayHtmlPanel.Construction, state.OpenPanelId,
                "The panel stays mounted while the exit motion plays.");
            state.AdvancePanelClose(0f);
            Assert.AreEqual(GameplayHtmlPanel.Construction, state.OpenPanelId,
                "The panel must still be mounted before the close deadline.");
            state.AdvancePanelClose(GameplayHtmlState.PanelCloseSeconds + 0.01f);
            Assert.AreEqual(GameplayHtmlPanel.None, state.OpenPanelId,
                "The panel unmounts once the close window elapses.");
            Assert.Contains(UiActionIds.Construction.Close.ToString(), router.Executed,
                "Closing the construction panel must route through the domain action.");
        }

        [Test]
        public void ClosePanel_Kingdom_Closes()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Kingdom);
            var bridge = CreateBridge(state, new FakeRouter());

            bridge.ClosePanel();
            state.AdvancePanelClose(0f);
            state.AdvancePanelClose(GameplayHtmlState.PanelCloseSeconds + 0.01f);

            Assert.AreEqual(GameplayHtmlPanel.None, state.OpenPanelId);
        }

        [Test]
        public void ClosePanel_Supply_ClosesAndClearsContext()
        {
            var state = new GameplayHtmlState();
            state.OpenSupplyPanel(new Vector2Int(3, 4), "lumber-camp");
            var bridge = CreateBridge(state, new FakeRouter());

            bridge.ClosePanel();
            state.AdvancePanelClose(0f);
            state.AdvancePanelClose(GameplayHtmlState.PanelCloseSeconds + 0.01f);

            Assert.AreEqual(GameplayHtmlPanel.None, state.OpenPanelId);
            Assert.IsNull(state.SupplyPosition, "Closing must clear the supply context.");
        }

        // ---------- P063: closing lifecycle ----------

        [Test]
        public void ClosePanel_WhileClosing_IsIgnored_NoDuplicateDomainClose()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            var router = new FakeRouter();
            var bridge = CreateBridge(state, router);

            bridge.ClosePanel();
            bridge.ClosePanel();

            int closes = router.Executed.Count(
                id => id == UiActionIds.Construction.Close.ToString());
            Assert.AreEqual(1, closes,
                "A second close during the exit motion must not re-run the domain action.");
            Assert.IsTrue(state.PanelClosing);
        }

        [Test]
        public void MutatingCommand_WhileClosing_IsRejected()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            var router = new FakeRouter();
            var bridge = CreateBridge(state, router);

            bridge.ClosePanel();
            bridge.SelectBuilding("lumber-camp");

            Assert.IsFalse(router.Executed.Contains(
                    UiActionIds.Construction.SelectBuilding.ToString()),
                "Panel commands must be blocked while the panel is closing.");
        }

        [Test]
        public void OpenPanel_WhileClosing_SettlesThenOpens_LastIntentWins()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            var bridge = CreateBridge(state, new FakeRouter());

            bridge.ClosePanel();
            Assert.IsTrue(state.PanelClosing);

            bridge.Notifications();

            Assert.IsFalse(state.PanelClosing,
                "Reopening during the close window resolves the pending close.");
            Assert.AreEqual(GameplayHtmlPanel.Notifications, state.OpenPanelId,
                "The last open intent wins over the in-flight close.");
        }

        [Test]
        public void Markup_WhileClosing_EmitsExitMotion_ThenUnmounts()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Notifications);

            string open = GameplayHtmlMarkup.Build(
                new GameplayHtmlSnapshot(), state, "vp-wide");
            Assert.IsTrue(open.Contains("id=\"gameplay-side-panel\""),
                "Open panel must render the side panel node.");
            Assert.IsTrue(open.Contains("data-motion-role=\"panel\""),
                "Open panel must carry the shared panel motion role.");

            state.ClosePanel();
            string closing = GameplayHtmlMarkup.Build(
                new GameplayHtmlSnapshot(), state, "vp-wide");
            Assert.IsTrue(closing.Contains("id=\"gameplay-side-panel\""),
                "The panel node must stay mounted during closing.");
            Assert.IsTrue(closing.Contains("data-motion=\"exit\""),
                "Closing must swap the node to its declarative exit motion.");

            state.AdvancePanelClose(0f);
            state.AdvancePanelClose(GameplayHtmlState.PanelCloseSeconds + 0.01f);
            string closed = GameplayHtmlMarkup.Build(
                new GameplayHtmlSnapshot(), state, "vp-wide");
            Assert.IsFalse(closed.Contains("id=\"gameplay-side-panel\""),
                "The panel node must unmount after the close window.");
        }

        [Test]
        public void KingdomDashboard_WhileClosing_EmitsExitMotion()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Kingdom);
            state.ClosePanel();

            string closing = GameplayHtmlMarkup.Build(
                new GameplayHtmlSnapshot(), state, "vp-wide");
            Assert.IsTrue(closing.Contains("id=\"kingdom-scrim\""),
                "The dashboard scrim must stay mounted during closing.");
            Assert.IsTrue(closing.Contains("data-motion=\"exit\""),
                "The dashboard must replay its exit motion while closing.");
        }

        [Test]
        public void ClosePanel_SelectionShown_FiresWorldInfoPanelClosed()
        {
            // The selection details panel has no OpenPanelId — Escape dismisses
            // it via WorldInfoPanelClosedSignal; the X button must do the same
            // instead of silently doing nothing.
            var container = new DiContainer();
            SignalBusInstaller.Install(container);
            container.DeclareSignal<Kruty1918.Moyva.Signals.WorldInfoPanelClosedSignal>()
                .OptionalSubscriber();
            var bus = container.Resolve<SignalBus>();
            int closedFired = 0;
            bus.Subscribe<Kruty1918.Moyva.Signals.WorldInfoPanelClosedSignal>(
                () => closedFired++);

            var router = new FakeRouter();
            container.Bind<IUiActionRouter>().FromInstance(router);
            var lazy = new LazyInject<IUiActionRouter>(container,
                new InjectContext(container, typeof(IUiActionRouter)));

            var readModel = new GameplayHudReadModel(
                null, null, new FakeConstructionClose(), null);
            readModel.SetSelection(new Kruty1918.Moyva.Signals.WorldInfoSelectionChangedSignal
            {
                Kind = Kruty1918.Moyva.Signals.WorldInfoSelectionKind.Building,
                ObjectId = "lumber-camp",
                Position = new Vector2Int(5, 6),
            });
            Assert.IsTrue(readModel.HasSelection);

            var state = new GameplayHtmlState();
            var bridge = new GameplayHtmlBridge(
                state, readModel, lazy, new FakeConstructionClose(),
                null, null, null, notificationSignals: bus);

            bridge.ClosePanel();

            Assert.AreEqual(1, closedFired,
                "Closing a selection panel must fire the same signal as Escape.");
        }

        [Test]
        public void ClosePanel_NothingOpen_DoesNotThrow()
        {
            var state = new GameplayHtmlState();
            var bridge = CreateBridge(state, new FakeRouter());
            Assert.DoesNotThrow(() => bridge.ClosePanel());
            Assert.AreEqual(GameplayHtmlPanel.None, state.OpenPanelId);
        }

        [Test]
        public void Execute_WhenRouterThrows_SurfacesRejection_AndKeepsPanel()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            var router = new FakeRouter { ThrowOnExecute = true };
            var bridge = CreateBridge(state, router);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception,
                "InvalidOperationException: simulated router failure");
            Assert.DoesNotThrow(() => bridge.ClosePanel(),
                "A throwing action must not escape into the UI callback.");
            Assert.AreEqual(GameplayHtmlPanel.Construction, state.OpenPanelId,
                "A failed close leaves the panel open.");
            Assert.IsFalse(state.PanelClosing);
            Assert.IsFalse(string.IsNullOrWhiteSpace(state.Feedback),
                "The failure reason must reach the feedback line.");
        }
    }
}
