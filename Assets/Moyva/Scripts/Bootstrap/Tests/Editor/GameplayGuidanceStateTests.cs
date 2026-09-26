using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.UIActions.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>P062: guidance session lifecycle — one popup per goal, blocked
    /// clicks deduplicate the notification, closing keeps the goal resumable,
    /// and the construction focus arms a one-shot page jump with a
    /// time-boxed highlight.</summary>
    [TestFixture]
    internal sealed class GameplayGuidanceStateTests
    {
        private static GuidanceGoal PlacementGoal(string buildingId = "forge",
            int x = 3, int y = 4)
            => new GuidanceGoal
            {
                Kind = GuidanceGoalKind.Placement,
                BuildingId = buildingId,
                Position = new Vector2Int(x, y),
                PlacementCount = 1,
            };

        private static GuidanceGoal RecruitmentGoal(string unitTypeId = "pikeman",
            int x = 8, int y = 8)
            => new GuidanceGoal
            {
                Kind = GuidanceGoalKind.Recruitment,
                UnitTypeId = unitTypeId,
                Position = new Vector2Int(x, y),
            };

        [Test]
        public void Guidance_OpenMinimizeReopenClose_LifecycleKeepsSinglePopup()
        {
            var state = new GameplayHtmlState();
            state.OpenGuidance(PlacementGoal());
            Assert.NotNull(state.Guidance);
            Assert.IsTrue(state.Guidance.Open);

            state.MinimizeGuidance();
            Assert.NotNull(state.Guidance, "Minimize must keep the saved goal.");
            Assert.IsFalse(state.Guidance.Open);

            state.ReopenGuidance(PlacementGoal());
            Assert.IsTrue(state.Guidance.Open);

            state.CloseGuidance();
            Assert.IsNull(state.Guidance, "Dismissing drops the session entirely.");
        }

        [Test]
        public void Guidance_RepeatedOpen_SingleSession_NoDuplicatePopup()
        {
            var state = new GameplayHtmlState();
            state.OpenGuidance(PlacementGoal());
            state.OpenGuidance(PlacementGoal());
            state.OpenGuidance(RecruitmentGoal());
            Assert.NotNull(state.Guidance);
            Assert.IsTrue(state.Guidance.Open);
            Assert.AreEqual(GuidanceGoalKind.Recruitment, state.Guidance.Goal.Kind,
                "The last blocked action owns the single popup.");
        }

        [Test]
        public void Guidance_FocusClick_TogglesCollapse()
        {
            var state = new GameplayHtmlState();
            state.OpenGuidance(PlacementGoal());
            state.SetGuidanceFocus(1);
            Assert.AreEqual(1, state.Guidance.FocusIndex);
            state.SetGuidanceFocus(1);
            Assert.AreEqual(-1, state.Guidance.FocusIndex,
                "Clicking the focused card collapses its option list.");
        }

        [Test]
        public void Guidance_MoveFocus_ClampsToBlockerRange()
        {
            var state = new GameplayHtmlState();
            state.OpenGuidance(PlacementGoal());
            state.MoveGuidanceFocus(-1, 3);
            Assert.AreEqual(0, state.Guidance.FocusIndex);
            state.MoveGuidanceFocus(10, 3);
            Assert.AreEqual(2, state.Guidance.FocusIndex);
        }

        [Test]
        public void Notifications_SameGoal_Deduplicates()
        {
            var state = new GameplayHtmlState();
            var goal = RecruitmentGoal();
            state.AddNotification("Cannot recruit pikeman: wood", "Warning", goal: goal);
            state.AddNotification("Cannot recruit pikeman: wood", "Warning", goal: RecruitmentGoal());
            Assert.AreEqual(1, state.Notifications.Count,
                "A repeated blocked action refreshes the same notification.");
        }

        [Test]
        public void Notifications_DifferentGoals_KeptSeparately()
        {
            var state = new GameplayHtmlState();
            state.AddNotification("A", "Warning", goal: RecruitmentGoal("pikeman"));
            state.AddNotification("B", "Warning", goal: RecruitmentGoal("archer"));
            state.AddNotification("C", "Warning", goal: PlacementGoal("forge"));
            Assert.AreEqual(3, state.Notifications.Count);
        }

        [Test]
        public void Notifications_GoalIsPreservedOnSnapshot()
        {
            var state = new GameplayHtmlState();
            var goal = RecruitmentGoal();
            state.AddNotification("Cannot recruit", "Warning", goal: goal);
            Assert.AreSame(goal, state.Notifications[0].Goal,
                "The notification must keep a live link to the guidance goal.");
        }

        [Test]
        public void ConstructionFocus_ArmsOneShotJump_AndTimeboxedHighlight()
        {
            var state = new GameplayHtmlState();
            state.SetConstructionFocus("sawmill");
            Assert.AreEqual("sawmill", state.ConsumeConstructionFocusJump(),
                "The first render after focus consumes the pending page jump.");
            Assert.IsNull(state.ConsumeConstructionFocusJump(),
                "Later renders must not re-page — the player owns the pager.");
            Assert.AreEqual("sawmill", state.HighlightedConstructionId,
                "The highlight persists past the one-shot jump so it is visible.");
        }

        [Test]
        public void ConstructionFocus_FilterChange_ClearsFocus()
        {
            var state = new GameplayHtmlState();
            state.SetConstructionFocus("sawmill");
            state.SetConstructionCategory("military");
            Assert.IsEmpty(state.HighlightedConstructionId);
            Assert.IsNull(state.ConsumeConstructionFocusJump());
        }

        // ---------- bridge-level: close ordering + goal resume ----------

        private sealed class FakeRouter : IUiActionRouter
        {
            public readonly List<string> Executed = new();
            public UiActionResult Execute(in UiActionRequest request)
                => Execute(request.ActionId, request.Source, request.ContextId, request.TargetId);
            public UiActionResult Execute(UiActionId actionId,
                UiActionSource source = UiActionSource.Programmatic,
                string contextId = null, string targetId = null)
            {
                Executed.Add(actionId.ToString());
                return UiActionResult.Performed();
            }
        }

        private sealed class FakeConstruction : IConstructionSessionCommands
        {
            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) { }
            public string GetActiveOwner() => "player_0";
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => false;
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            { buildingId = null; return false; }
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => new Dictionary<Vector2Int, string>();
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => false;
            public bool TryGetPendingPlacementStatus(Vector2Int position,
                out ConstructionPendingPlacementStatus status)
            { status = default; return false; }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position) => null;
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
            GameplayHtmlState state, FakeRouter router,
            IConstructionSessionCommands construction = null)
        {
            var container = new DiContainer();
            container.Bind<IUiActionRouter>().FromInstance(router);
            var lazy = new LazyInject<IUiActionRouter>(container,
                new InjectContext(container, typeof(IUiActionRouter)));
            return new GameplayHtmlBridge(
                state, null, lazy, construction ?? new FakeConstruction(),
                null, null, null);
        }

        [Test]
        public void ClosePanel_GuidanceOpen_PeelsPopupBeforePanel()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Construction);
            state.OpenGuidance(PlacementGoal());
            var bridge = CreateBridge(state, new FakeRouter());

            bridge.ClosePanel();

            Assert.NotNull(state.Guidance, "Closing peels the popup, keeps the goal.");
            Assert.IsFalse(state.Guidance.Open);
            Assert.AreEqual(GameplayHtmlPanel.Construction, state.OpenPanelId,
                "The underlying panel stays open under the peeled popup.");
        }

        [Test]
        public void GuidanceResumeGoal_PendingStillThere_ReopensConstruction()
        {
            var state = new GameplayHtmlState();
            var goal = PlacementGoal("lumber-camp");
            state.OpenGuidance(goal);
            var router = new FakeRouter();
            var construction = new PendingFakeConstruction(
                new Dictionary<Vector2Int, string> { [goal.Position] = "lumber-camp" });
            var bridge = CreateBridge(state, router, construction);

            bridge.GuidanceResumeGoal();

            Assert.Contains(UiActionIds.Construction.Open.ToString(), router.Executed);
            Assert.IsFalse(state.Guidance.Open);
        }

        [Test]
        public void GuidanceResumeGoal_PendingCleared_ReselectsBuilding()
        {
            var state = new GameplayHtmlState();
            state.OpenGuidance(PlacementGoal("lumber-camp"));
            var router = new FakeRouter();
            var construction = new FakeConstruction(); // no pending placements
            var bridge = CreateBridge(state, router, construction);

            bridge.GuidanceResumeGoal();

            Assert.Contains(UiActionIds.Construction.SelectBuilding.ToString(),
                router.Executed,
                "A cleared pending re-enters placement for the saved building.");
        }

        [Test]
        public void GuidanceBuildProducer_OpensConstruction_AndArmsFocus()
        {
            var state = new GameplayHtmlState();
            state.OpenGuidance(PlacementGoal());
            var router = new FakeRouter();
            var bridge = CreateBridge(state, router);

            bridge.GuidanceBuildProducer("sawmill");

            Assert.Contains(UiActionIds.Construction.Open.ToString(), router.Executed);
            Assert.IsFalse(state.Guidance.Open, "Popup collapses for the map flow.");
            Assert.AreEqual("sawmill", state.ConsumeConstructionFocusJump());
        }

        private sealed class PendingFakeConstruction : IConstructionSessionCommands
        {
            private readonly Dictionary<Vector2Int, string> _pending;
            public PendingFakeConstruction(Dictionary<Vector2Int, string> pending)
                => _pending = pending;
            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) { }
            public string GetActiveOwner() => "player_0";
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => false;
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            { buildingId = null; return false; }
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements() => _pending;
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => false;
            public bool TryGetPendingPlacementStatus(Vector2Int position,
                out ConstructionPendingPlacementStatus status)
            { status = default; return false; }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position) => null;
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
    }
}
