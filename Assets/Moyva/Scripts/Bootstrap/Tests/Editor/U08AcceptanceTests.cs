using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Economy.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// U08: the construction supply panel must present the ledger in one unit
    /// scale (need / delivered / remaining, never summed), render every order
    /// state without reopening the panel, avoid false completion after a
    /// cancelled or late order, and keep error explanations localizable.
    /// </summary>
    [TestFixture]
    internal sealed class U08AcceptanceTests
    {
        private static GameplayHtmlState SupplyOpen()
        {
            var state = new GameplayHtmlState();
            state.OpenSupplyPanel(new Vector2Int(3, 4), "sawmill");
            return state;
        }

        private static ConstructionSupplyOrderSnapshot Order(
            ConstructionSupplyOrderStatus status,
            IReadOnlyDictionary<string, float> required = null,
            IReadOnlyDictionary<string, float> delivered = null,
            IReadOnlyDictionary<string, float> remaining = null)
        {
            return new ConstructionSupplyOrderSnapshot(
                "ord-1", "player", "sawmill", new Vector2Int(3, 4), "set-a", "Alpha",
                status,
                required ?? new Dictionary<string, float>(),
                delivered ?? new Dictionary<string, float>(),
                remaining ?? new Dictionary<string, float>(),
                new List<string> { "wagon-1" });
        }

        private static GameplaySupplySnapshot Supply(ConstructionSupplyOrderSnapshot? order = null)
        {
            return new GameplaySupplySnapshot
            {
                Resolved = true,
                BuildingName = "sawmill",
                SettlementName = "Alpha",
                Position = new Vector2Int(3, 4),
                Resources = new[]
                {
                    new GameplaySupplyResourceSnapshot
                    {
                        ResourceId = "wood", Required = 10f,
                        LocalAvailable = 2f, Delivered = 4f, Deficit = 4f,
                    },
                },
                Sources = new[]
                {
                    new GameplaySupplySourceSnapshot
                    {
                        SettlementId = "set-b", SettlementName = "Beta",
                        WarehouseKey = "wh-1", StockSummary = "wood 20", RouteDistance = 12f,
                    },
                },
                Wagons = new[]
                {
                    new GameplaySupplyWagonSnapshot { UnitId = "wagon-1", FreeCapacity = 10f },
                },
                SourceIndex = 0,
                WagonIndex = 0,
                CanDispatch = true,
                Order = order,
            };
        }

        private static string Render(GameplaySupplySnapshot supply, GameplayHtmlState state)
            => GameplayHtmlMarkup.Build(new GameplayHtmlSnapshot { Supply = supply }, state, "vp-wide");

        [Test]
        public void Supply_OrderLedger_ShowsNeedDeliveredRemainingWithoutSumming()
        {
            var supply = Supply(Order(
                ConstructionSupplyOrderStatus.Active,
                required: new Dictionary<string, float> { ["wood"] = 10f },
                delivered: new Dictionary<string, float> { ["wood"] = 4f },
                remaining: new Dictionary<string, float> { ["wood"] = 6f }));

            string html = Render(supply, SupplyOpen());

            StringAssert.Contains("data-key=\"supply-order-ord-1\"", html);
            StringAssert.Contains("data-key=\"supply-order-ord-1-wood\"", html);
            int row = html.IndexOf("supply-order-ord-1-wood", System.StringComparison.Ordinal);
            int end = html.IndexOf("</view>", row, System.StringComparison.Ordinal);
            Assert.Greater(end, row);
            string segment = html.Substring(row, end - row);
            StringAssert.Contains("need 10", segment);
            StringAssert.Contains("delivered 4", segment);
            StringAssert.Contains("remaining 6", segment);
            StringAssert.DoesNotContain("14", segment);
        }

        [Test]
        public void Supply_OrderStatus_RendersAllStates()
        {
            string active = Render(Supply(Order(ConstructionSupplyOrderStatus.Active)), SupplyOpen());
            StringAssert.Contains("Active", active);
            StringAssert.DoesNotContain(">Delivered<", active);

            string ready = Render(Supply(Order(ConstructionSupplyOrderStatus.Ready)), SupplyOpen());
            StringAssert.Contains("Delivered", ready);

            string cancelled = Render(Supply(Order(ConstructionSupplyOrderStatus.Cancelled)), SupplyOpen());
            StringAssert.Contains("Cancelled", cancelled);
        }

        [Test]
        public void Supply_NoOrder_ShowsEmptyOrderState()
        {
            string html = Render(Supply(order: null), SupplyOpen());
            StringAssert.Contains("SUPPLY ORDER", html);
            StringAssert.Contains("No supply order requested", html);
            StringAssert.DoesNotContain("supply-order-ord", html);
        }

        [Test]
        public void Supply_StatusChange_UpdatesWithoutReopen()
        {
            var state = SupplyOpen();
            string before = Render(Supply(Order(ConstructionSupplyOrderStatus.Active)), state);
            string after = Render(Supply(Order(ConstructionSupplyOrderStatus.Ready)), state);
            StringAssert.Contains("Active", before);
            StringAssert.Contains("Delivered", after);
        }

        [Test]
        public void Supply_CancelledOrder_DoesNotShowFalseCompletion()
        {
            var supply = Supply(Order(
                ConstructionSupplyOrderStatus.Cancelled,
                required: new Dictionary<string, float> { ["wood"] = 10f },
                delivered: new Dictionary<string, float> { ["wood"] = 2f },
                remaining: new Dictionary<string, float> { ["wood"] = 8f }));

            string html = Render(supply, SupplyOpen());

            StringAssert.Contains("Cancelled", html);
            StringAssert.Contains("remaining 8", html);
            StringAssert.DoesNotContain(">Delivered<", html);
        }

        [Test]
        public void Supply_DisabledDispatch_ShowsVisibleReason()
        {
            var supply = Supply();
            supply.CanDispatch = false;
            supply.DispatchUnavailableReason = "Select a free wagon.";

            string html = Render(supply, SupplyOpen());

            StringAssert.Contains("disabled=\"true\"", html);
            StringAssert.Contains("Select a free wagon.", html);
        }

        [Test]
        public void Supply_Unresolved_ShowsReasonNotLedger()
        {
            var supply = Supply();
            supply.Resolved = false;
            supply.Reason = "There is no pending placement at this position.";

            string html = Render(supply, SupplyOpen());

            StringAssert.Contains("There is no pending placement at this position.", html);
            StringAssert.DoesNotContain("supply-order-", html);
            StringAssert.DoesNotContain("DISPATCH DELIVERY", html);
        }

        [Test]
        public void Supply_NullSupply_ShowsSelectionHint()
        {
            string html = GameplayHtmlMarkup.Build(new GameplayHtmlSnapshot(), SupplyOpen(), "vp-wide");
            StringAssert.Contains("Select a pending placement", html);
        }
    }
}
