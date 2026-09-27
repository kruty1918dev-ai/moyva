using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.Economy.API;
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
                html.Append("<text className=\"empty\">").Append(state.T("Select a pending placement with a resource deficit first.")).Append("</text>");
                html.Append("</view></scroll></view></view>");
                return;
            }

            if (!supply.Resolved)
            {
                html.Append("<text className=\"empty\">").Append(E(state.T(supply.Reason))).Append("</text>");
                html.Append("</view></scroll></view></view>");
                return;
            }

            DataRow(html, state.T(supply.BuildingName), supply.SettlementName,
                $"Placement {supply.Position.x}, {supply.Position.y}");
            html.Append("<text className=\"section-title\">").Append(state.T("LOCAL COVERAGE")).Append("</text>");
            for (int index = 0; index < supply.Resources.Length; index++)
            {
                var line = supply.Resources[index];
                string status = line.Deficit <= 0.0001f
                    ? state.T("covered locally")
                    : $"{state.T("missing")} {Amount(line.Deficit)}";
                string detail = $"{state.T("need")} {Amount(line.Required)} · {state.T("local")} {Amount(line.LocalAvailable)}";
                if (line.Delivered > 0.0001f)
                    detail += $" · {state.T("delivered")} {Amount(line.Delivered)}";
                DataRow(html, DisplayResource(line.ResourceId), status, detail, key: $"supply-res-{line.ResourceId}");
            }

            html.Append("<text className=\"section-title\">").Append(state.T("SUPPLY ORDER")).Append("</text>");
            if (!supply.Order.HasValue)
            {
                html.Append("<text className=\"empty\">")
                    .Append(state.T("No supply order requested — dispatch a wagon to start one."))
                    .Append("</text>");
            }
            else
            {
                var order = supply.Order.Value;
                DataRow(html, order.SettlementName, SupplyOrderStatusLabel(order.Status, state),
                    $"{order.WagonIds?.Count ?? 0} {state.T("wagon(s) assigned")}",
                    key: $"supply-order-{order.OrderId}");
                AppendSupplyOrderResources(html, order, state);
            }

            html.Append("<text className=\"section-title\">").Append(state.T("SOURCE WAREHOUSE")).Append("</text>");
            if (supply.Sources.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("No other settlement stocks the missing resources.")).Append("</text>");
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

            html.Append("<text className=\"section-title\">").Append(state.T("WAGON")).Append("</text>");
            if (supply.Wagons.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("No wagons. Recruit one at a caravan depot.")).Append("</text>");
            for (int index = 0; index < supply.Wagons.Length; index++)
            {
                var wagon = supply.Wagons[index];
                string status = wagon.Busy ? state.T(wagon.Status) : $"{state.T("free")} {Amount(wagon.FreeCapacity)}";
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
                html.Append("<text className=\"section-title\">").Append(state.T("NEXT STEP")).Append("</text>");
                foreach (var hint in supply.Hints)
                {
                    html.Append("<text className=\"item-meta\">").Append(E(hint.Text)).Append("</text>");
                    if (!string.IsNullOrWhiteSpace(hint.ProducerResourceId))
                        html.Append(Button(state.T("PRODUCE LOCALLY"),
                            $"Globals.gameplay.ShowProducersFor('{J(hint.ProducerResourceId)}')",
                            "button", state.T("Show buildings producing this resource"), false));
                }
            }

            html.Append("<text className=\"item-meta\">").Append(state.T("Resources move only by wagon — deliveries are reserved for this construction.")).Append("</text>");
            html.Append("</view></scroll>");
            if (!supply.CanDispatch && !string.IsNullOrWhiteSpace(supply.DispatchUnavailableReason))
                html.Append("<text className=\"item-meta\">").Append(E(state.T(supply.DispatchUnavailableReason))).Append("</text>");
            html.Append(Button(state.T("DISPATCH DELIVERY"), "Globals.gameplay.DispatchSupply()",
                "button primary wide",
                supply.CanDispatch
                    ? state.T("Send the selected wagon with missing resources")
                    : state.T(supply.DispatchUnavailableReason),
                !supply.CanDispatch));
            html.Append("</view></view>");
        }

        // Per-resource order ledger: need / delivered / remaining in the same
        // units — delivered and remaining are shown separately, never summed
        // into required, so reserved cargo is not counted as new freight.
        private static void AppendSupplyOrderResources(
            StringBuilder html, ConstructionSupplyOrderSnapshot order, GameplayHtmlState state)
        {
            var resourceIds = new List<string>();
            CollectResourceIds(resourceIds, order.Required);
            CollectResourceIds(resourceIds, order.Delivered);
            CollectResourceIds(resourceIds, order.Remaining);
            resourceIds.Sort(StringComparer.Ordinal);
            for (int index = 0; index < resourceIds.Count; index++)
            {
                string resourceId = resourceIds[index];
                float required = Lookup(order.Required, resourceId);
                float delivered = Lookup(order.Delivered, resourceId);
                float remaining = Lookup(order.Remaining, resourceId);
                string status = remaining <= 0.0001f
                    ? state.T("delivered")
                    : $"{state.T("remaining")} {Amount(remaining)}";
                string detail = $"{state.T("need")} {Amount(required)} · {state.T("delivered")} {Amount(delivered)}";
                DataRow(html, state.T(DisplayResource(resourceId)), status, detail,
                    key: $"supply-order-{order.OrderId}-{resourceId}");
            }
        }

        private static void CollectResourceIds(List<string> ids, IReadOnlyDictionary<string, float> values)
        {
            if (values == null) return;
            foreach (var pair in values)
                if (!ids.Contains(pair.Key)) ids.Add(pair.Key);
        }

        private static float Lookup(IReadOnlyDictionary<string, float> values, string resourceId)
        {
            if (values != null && values.TryGetValue(resourceId, out float amount)) return amount;
            return 0f;
        }

        private static string SupplyOrderStatusLabel(
            ConstructionSupplyOrderStatus status, GameplayHtmlState state)
        {
            switch (status)
            {
                case ConstructionSupplyOrderStatus.Ready:
                    return state.T("Delivered");
                case ConstructionSupplyOrderStatus.Cancelled:
                    return state.T("Cancelled");
                default:
                    return state.T("Active");
            }
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
