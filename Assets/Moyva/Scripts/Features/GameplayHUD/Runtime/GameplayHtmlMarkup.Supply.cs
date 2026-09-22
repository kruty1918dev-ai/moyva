using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static partial class GameplayHtmlMarkup
    {
        private static void AppendSupplyPanel(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            var supply = snapshot.Supply;
            PanelHeader(html, state, "CONSTRUCTION SUPPLY", "Deliver missing resources by wagon", true);
            html.Append("<view className=\"panel-body\"><scroll className=\"panel-scroll context-scroll\"><view className=\"building-list\">");

            if (supply == null)
            {
                html.Append("<text className=\"empty\">Select a pending placement with a resource deficit first.</text>");
                html.Append("</view></scroll></view></view>");
                return;
            }

            if (!supply.Resolved)
            {
                html.Append("<text className=\"empty\">").Append(E(supply.Reason)).Append("</text>");
                html.Append("</view></scroll></view></view>");
                return;
            }

            DataRow(html, supply.BuildingName, supply.SettlementName,
                $"Placement {supply.Position.x}, {supply.Position.y}");
            html.Append("<text className=\"section-title\">LOCAL COVERAGE</text>");
            for (int index = 0; index < supply.Resources.Length; index++)
            {
                var line = supply.Resources[index];
                string status = line.Deficit <= 0.0001f
                    ? "covered locally"
                    : $"missing {Amount(line.Deficit)}";
                string detail = $"need {Amount(line.Required)} · local {Amount(line.LocalAvailable)}";
                if (line.Delivered > 0.0001f)
                    detail += $" · delivered {Amount(line.Delivered)}";
                DataRow(html, DisplayResource(line.ResourceId), status, detail, key: $"supply-res-{line.ResourceId}");
            }

            if (supply.Order.HasValue)
            {
                var order = supply.Order.Value;
                html.Append("<text className=\"section-title\">SUPPLY ORDER</text>");
                DataRow(html, order.SettlementName, order.Status.ToString().ToUpperInvariant(),
                    $"{order.WagonIds?.Count ?? 0} wagon(s) assigned");
            }

            html.Append("<text className=\"section-title\">SOURCE WAREHOUSE</text>");
            if (supply.Sources.Length == 0)
                html.Append("<text className=\"empty\">No other settlement stocks the missing resources.</text>");
            for (int index = 0; index < supply.Sources.Length; index++)
            {
                var source = supply.Sources[index];
                html.Append("<button className=\"filter-button")
                    .Append(index == supply.SourceIndex ? " selected" : string.Empty)
                    .Append("\" data-key=\"supply-src-").Append(E(source.SettlementId)).Append('-').Append(E(source.WarehouseKey))
                    .Append("\" onClick=\"Globals.gameplay.SetSupplySource(")
                    .Append(index.ToString(CultureInfo.InvariantCulture)).Append(")\"><text className=\"tab-label\">")
                    .Append(E(source.SettlementName)).Append(" · ").Append(E(source.WarehouseKey))
                    .Append(" · ").Append(state.T("route")).Append(" ")
                    .Append(Amount(source.RouteDistance))
                    .Append("</text></button><text className=\"item-meta\">")
                    .Append(E(source.StockSummary)).Append("</text>");
            }

            html.Append("<text className=\"section-title\">WAGON</text>");
            if (supply.Wagons.Length == 0)
                html.Append("<text className=\"empty\">No wagons. Recruit one at a caravan depot.</text>");
            for (int index = 0; index < supply.Wagons.Length; index++)
            {
                var wagon = supply.Wagons[index];
                string status = wagon.Busy ? wagon.Status : $"free {Amount(wagon.FreeCapacity)}";
                html.Append("<button className=\"filter-button")
                    .Append(index == supply.WagonIndex ? " selected" : string.Empty)
                    .Append("\" data-key=\"supply-wagon-").Append(E(wagon.UnitId))
                    .Append("\" onClick=\"Globals.gameplay.SetSupplyWagon(")
                    .Append(index.ToString(CultureInfo.InvariantCulture)).Append(")\"><text className=\"tab-label\">")
                    .Append(E(wagon.UnitId)).Append(" · ").Append(E(status))
                    .Append("</text></button>");
            }

            if (supply.PlannedShipment != null)
            {
                html.Append("<text className=\"item-meta\">").Append(state.T("Delivery plan"))
                    .Append(": ").Append(E(supply.PlannedShipment))
                    .Append(" · ").Append(state.T("route")).Append(" ")
                    .Append(Amount(supply.PlannedRouteDistance));
                if (supply.PlannedNeedsRepeat)
                    html.Append(" · ").Append(state.T("wagon repeats until the deficit is covered"));
                html.Append("</text>");
            }

            if (supply.Hints.Length > 0)
            {
                html.Append("<text className=\"section-title\">NEXT STEP</text>");
                foreach (var hint in supply.Hints)
                {
                    html.Append("<text className=\"item-meta\">").Append(E(hint.Text)).Append("</text>");
                    if (!string.IsNullOrWhiteSpace(hint.ProducerResourceId))
                        html.Append(Button(state.T("PRODUCE LOCALLY"),
                            $"Globals.gameplay.ShowProducersFor('{J(hint.ProducerResourceId)}')",
                            "button", state.T("Show buildings producing this resource"), false));
                }
            }

            html.Append("<text className=\"item-meta\">Resources move only by wagon — deliveries are reserved for this construction.</text>");
            html.Append("</view></scroll>");
            html.Append(Button("DISPATCH DELIVERY", "Globals.gameplay.DispatchSupply()",
                "button primary wide",
                supply.CanDispatch
                    ? "Send the selected wagon with missing resources"
                    : supply.DispatchUnavailableReason,
                !supply.CanDispatch));
            html.Append("</view></view>");
        }

        private static void AppendLogistics(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<text className=\"section-title\">ACTIVE ROUTES &amp; SUPPLY ORDERS</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.Logistics.Length; index++)
            {
                var entry = snapshot.Logistics[index];
                DataRow(html, entry.Title, entry.Kind.ToUpperInvariant(), entry.Detail, key: $"log-{entry.Kind}-{entry.Id}");
                if (entry.FocusPosition.HasValue)
                {
                    Vector2Int position = entry.FocusPosition.Value;
                    html.Append(Button("VIEW",
                        $"Globals.gameplay.FocusWarehouse({position.x},{position.y},'')",
                        "button primary", "Go to this order", false, key: $"log-{entry.Kind}-{entry.Id}-view"));
                }
            }
            if (snapshot.Logistics.Length == 0)
                html.Append("<text className=\"empty\">No active routes or supply orders.</text>");
            html.Append("</view>");
        }
    }
}
