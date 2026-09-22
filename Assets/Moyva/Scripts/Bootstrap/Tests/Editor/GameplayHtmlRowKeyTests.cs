using Kruty1918.Moyva.Bootstrap.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// P074: dynamic rows (buildings, units, resources, orders) must carry
    /// reconciliation keys derived from stable identity — never list indices —
    /// so a reorder/removal cannot migrate focus, tooltips, or motion to the
    /// wrong row.
    /// </summary>
    [TestFixture]
    internal sealed class GameplayHtmlRowKeyTests
    {
        [Test]
        public void SupplyPanel_RowKeysDeriveFromStableIdentity()
        {
            var state = new GameplayHtmlState();
            state.OpenSupplyPanel(new Vector2Int(3, 4), "sawmill");
            var snapshot = new GameplayHtmlSnapshot
            {
                Supply = new GameplaySupplySnapshot
                {
                    Resolved = true,
                    BuildingName = "Sawmill",
                    Resources = new[]
                    {
                        new GameplaySupplyResourceSnapshot { ResourceId = "wood", Deficit = 4f },
                    },
                    Sources = new[]
                    {
                        new GameplaySupplySourceSnapshot { SettlementId = "set-a", WarehouseKey = "wh-1", SettlementName = "Alpha" },
                        new GameplaySupplySourceSnapshot { SettlementId = "set-b", WarehouseKey = "wh-2", SettlementName = "Beta" },
                    },
                    Wagons = new[]
                    {
                        new GameplaySupplyWagonSnapshot { UnitId = "wagon-7", FreeCapacity = 12f },
                    },
                },
            };

            string html = GameplayHtmlMarkup.Build(snapshot, state, "vp-wide");

            StringAssert.Contains("data-key=\"supply-res-wood\"", html);
            StringAssert.Contains("data-key=\"supply-src-set-a-wh-1\"", html);
            StringAssert.Contains("data-key=\"supply-src-set-b-wh-2\"", html);
            StringAssert.Contains("data-key=\"supply-wagon-wagon-7\"", html);
        }

        [Test]
        public void KingdomDashboard_RowKeysDeriveFromStableIdentity()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Kingdom);
            var snapshot = new GameplayHtmlSnapshot
            {
                KingdomName = "Testland",
                Resources = new[]
                {
                    new GameplayResourceSnapshot("wood", 42f),
                },
                Settlements = new[]
                {
                    new GameplaySettlementViewSnapshot("set-9", "Alpha", 10, 2, null),
                },
                Warehouses = new[]
                {
                    new GameplayWarehouseViewSnapshot("wh-5", "warehouse", "Alpha", new Vector2Int(1, 2), 3f, 10, null),
                },
            };

            string html = GameplayHtmlMarkup.Build(snapshot, state, "vp-wide");

            StringAssert.Contains("data-key=\"kingdom-res-wood\"", html);
            StringAssert.Contains("data-key=\"settlement-set-9\"", html);

            state.SetDashboardTab(KingdomDashboardTab.Storage);
            string storage = GameplayHtmlMarkup.Build(snapshot, state, "vp-wide");
            StringAssert.Contains("data-key=\"warehouse-wh-5\"", storage);
        }

        [Test]
        public void Notifications_RowAndActionKeysDeriveFromNotificationId()
        {
            var state = new GameplayHtmlState();
            state.AddNotification("Supply delivered", "info");
            state.OpenPanel(GameplayHtmlPanel.Notifications);
            string html = GameplayHtmlMarkup.Build(new GameplayHtmlSnapshot(), state, "vp-wide");

            StringAssert.Contains("data-key=\"notif-1\"", html);
            StringAssert.Contains("data-key=\"notif-1-dismiss\"", html);
        }

        [Test]
        public void Recruitment_RecipeAndQueueRowsCarryStableKeys()
        {
            var state = new GameplayHtmlState();
            var snapshot = new GameplayHtmlSnapshot
            {
                SelectionKind = "building",
                SupportsRecruitment = true,
                RecruitmentQueueCapacity = 4,
                RecruitmentRecipes = new[]
                {
                    new GameplayRecruitmentRecipeSnapshot(
                        "militia", "Militia", "Melee", "Ground", "Food 10", 2, 10, 4f, true, string.Empty),
                },
                RecruitmentQueue = new[]
                {
                    new GameplayRecruitmentQueueSnapshot(77, "militia", "Militia", 1, 2, false),
                },
            };
            state.SetSelectionTab(GameplaySelectionTab.Recruit);
            string recruitHtml = GameplayHtmlMarkup.Build(snapshot, state, "vp-wide");
            StringAssert.Contains("data-key=\"recruit-militia\"", recruitHtml);

            state.SetSelectionTab(GameplaySelectionTab.Queue);
            string queueHtml = GameplayHtmlMarkup.Build(snapshot, state, "vp-wide");
            StringAssert.Contains("data-key=\"queue-77\"", queueHtml);
            StringAssert.Contains("data-key=\"queue-77-action\"", queueHtml);
        }
    }
}
