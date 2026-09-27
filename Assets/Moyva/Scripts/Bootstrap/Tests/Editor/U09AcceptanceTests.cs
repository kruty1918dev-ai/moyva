using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// U09 acceptance invariants for the caravan cargo UI: the panel shows
    /// onboard load vs capacity, owner, warehouse/resource pickers with real
    /// settlement labels, the transfer action is disabled with a visible
    /// reason when unavailable, and the route tab lists source → destination,
    /// shipment contents, phase and moving/waiting state.
    /// </summary>
    public sealed class U09AcceptanceTests
    {
        private GameplayHtmlState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new GameplayHtmlState();
        }

        private string Build(GameplayCargoSnapshot cargo, GameplaySelectionTab tab)
        {
            var snapshot = new GameplayHtmlSnapshot
            {
                SelectionKind = "unit",
                SelectionId = "caravan-1",
                SelectionTitle = "Trade Wagon",
                SelectionPosition = new Vector2Int(41, 27),
                Cargo = cargo,
            };
            _state.SetSelectionTab(tab);
            return GameplayHtmlMarkup.Build(snapshot, _state, "vp-wide");
        }

        [Test]
        public void Cargo_ShowsLoadCapacityOwner_AndWarehouseLabels()
        {
            string html = Build(GameplayCargoSnapshot.CreatePreview(), GameplaySelectionTab.Cargo);

            StringAssert.Contains("20 / 120", html,
                "onboard 20 of capacity 120 must render as raw units — no kg conversion");
            StringAssert.Contains("player_0", html);
            StringAssert.Contains("Northhold", html);
            StringAssert.Contains("Rivergate", html);
            StringAssert.Contains("(42, 27)", html,
                "warehouse picker must disambiguate settlements by position");
            StringAssert.Contains("TransferCargo()", html);
            StringAssert.Contains("FocusWarehouse(", html);
        }

        [Test]
        public void Cargo_RejectedTransfer_DisabledWithReason()
        {
            var cargo = GameplayCargoSnapshot.CreatePreview();
            cargo.Availability = CaravanTransferResult.Rejected("Not enough stock in the warehouse");

            string html = Build(cargo, GameplaySelectionTab.Cargo);
            StringAssert.Contains("Not enough stock in the warehouse", html,
                "the rejection reason must be visible");
            StringAssert.Contains("disabled=\"true\"", html,
                "the transfer button must be disabled when unavailable");
            Assert.IsFalse(html.Contains(">Ready<"),
                "status must not claim Ready when the action is rejected");
        }

        [Test]
        public void Cargo_EmptyHold_ShowsEmptyState_NotZeroLoad()
        {
            var cargo = GameplayCargoSnapshot.CreatePreview();
            cargo.Cargo = new CaravanCargoSnapshot(
                new CaravanUnitSnapshot("player_0", new Vector2Int(41, 27), 120),
                new Dictionary<string, float>());

            string html = Build(cargo, GameplaySelectionTab.Cargo);
            StringAssert.Contains("0 / 120", html);
            StringAssert.Contains("The hold is empty.", html);
        }

        [Test]
        public void RouteTab_ShowsSourceDestinationShipment_AndPhase()
        {
            var cargo = GameplayCargoSnapshot.CreatePreview();
            var request = new CaravanRouteRequest(
                "player_0", "caravan-1",
                "northhold", "42:27",
                "rivergate", "67:31",
                new Dictionary<string, float> { ["stone-materials-resources"] = 15f },
                repeat: true);
            cargo.Route = new CaravanRouteSnapshot(
                request, CaravanRoutePhase.ToDestination,
                status: "Delivering", moving: true);

            string html = Build(cargo, GameplaySelectionTab.Route);
            StringAssert.Contains("Northhold", html);
            StringAssert.Contains("Rivergate", html);
            StringAssert.Contains("-&gt;", html.Replace("->", "-&gt;"),
                "source → destination direction must be visible");
            StringAssert.Contains("Delivering", html);
            StringAssert.Contains("Repeating route", html);
            StringAssert.Contains("moving", html);
            StringAssert.Contains("15", html, "shipment amount must render");
            StringAssert.Contains("STOP ROUTE", html,
                "an active route must offer stop, not start");
        }

        [Test]
        public void RouteTab_NoRoute_ShowsPickers_AndStartRespectsAvailability()
        {
            var cargo = GameplayCargoSnapshot.CreatePreview();
            cargo.Route = null;
            cargo.RouteAvailability = CaravanTransferResult.Success();
            string html = Build(cargo, GameplaySelectionTab.Route);
            StringAssert.Contains("Pickup warehouse", html);
            StringAssert.Contains("Destination warehouse", html);
            StringAssert.Contains("Repeat delivery", html);
            StringAssert.Contains("StartCargoRoute()", html);
            Assert.IsFalse(html.Contains("disabled=\"true\""),
                "ready route start must not be disabled");

            cargo.RouteAvailability = CaravanTransferResult.Rejected("No route target selected");
            html = Build(cargo, GameplaySelectionTab.Route);
            StringAssert.Contains("No route target selected", html);
            StringAssert.Contains("disabled=\"true\"", html);
        }

        [Test]
        public void Cargo_LongLocalizedNames_AreEscaped_NotTruncated()
        {
            var cargo = GameplayCargoSnapshot.CreatePreview();
            cargo.Warehouses = new[]
            {
                new EconomyWarehouseSnapshot("1:1", "storage", "settle-1",
                    "ДужеДовгаНазваПоселенняЗПереносамиТаСимволами <script>",
                    new Vector2Int(1, 1), 5, 100,
                    new Dictionary<string, float>()),
            };

            string html = Build(cargo, GameplaySelectionTab.Cargo);
            StringAssert.Contains("ДужеДовгаНазваПоселення", html,
                "long localized names must render un-truncated");
            Assert.IsFalse(html.Contains("<script>"),
                "authored markup characters in names must be escaped");
        }
    }
}
