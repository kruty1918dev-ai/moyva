using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static partial class GameplayHtmlMarkup
    {
        private static void AppendCargo(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            var form = snapshot.Cargo;
            float used = form.Cargo.Resources.Sum(r => r.Value);
            html.Append("<view className=\"cargo-capacity\"><text>").Append(state.T("Wagon cargo")).Append("</text><text>")
                .Append(Amount(used)).Append(" / ").Append(Amount(form.Cargo.Unit.Capacity))
                .Append("</text></view><view className=\"tabs cargo-modes\">");
            CargoMode(html, form, CaravanCargoOperation.Load, state.T("LOAD"));
            CargoMode(html, form, CaravanCargoOperation.Unload, state.T("UNLOAD"));
            CargoMode(html, form, CaravanCargoOperation.CollectLoot, state.T("RECOVER"));
            html.Append("</view><scroll className=\"panel-scroll context-scroll\"><view className=\"cargo-form\">");
            if (form.Operation != CaravanCargoOperation.CollectLoot)
            {
                var labels = form.Warehouses.Select(w =>
                    $"{w.SettlementName} / {state.T(Display(w.BuildingId))} ({w.GridPosition.x}, {w.GridPosition.y})");
                CargoSelect(html, "cargo-warehouse", state.T("Warehouse"), labels, form.WarehouseIndex,
                    "Globals.gameplay.SetCargoWarehouse(event)", state);
                if (form.WarehouseIndex >= 0)
                {
                    var warehouse = form.Warehouses[form.WarehouseIndex];
                    html.Append(Button(state.T("SHOW WAREHOUSE"),
                        $"Globals.gameplay.FocusWarehouse({warehouse.GridPosition.x},{warehouse.GridPosition.y},'{J(warehouse.BuildingId)}')",
                        "button", state.T("Focus the warehouse")));
                }
            }
            CargoSelect(html, "cargo-resource", state.T("Resource"), form.Resources.Select(r =>
                $"{state.T(DisplayResource(r.Key))} ({Amount(r.Value)})"), form.ResourceIndex,
                "Globals.gameplay.SetCargoResource(event)", state);
            html.Append("<view className=\"cargo-field\"><text className=\"cargo-label\">").Append(state.T("Amount")).Append("</text>")
                .Append("<input id=\"cargo-amount\" className=\"cargo-input\" contentType=\"DecimalNumber\" characterLimit=\"12\" value=\"")
                .Append(E(form.Amount)).Append("\" onChange=\"Globals.gameplay.SetCargoAmount(event)\"></input></view>");
            if (form.Cargo.Resources.Count > 0)
            {
                html.Append("<text className=\"cargo-label\">").Append(state.T("ON BOARD")).Append("</text>");
                foreach (var resource in form.Cargo.Resources.OrderBy(r => r.Key))
                {
                    string iconKey = GameplayHtmlIconKeys.Resource(resource.Key);
                    DataRow(html, state.T(DisplayResource(resource.Key)), Amount(resource.Value), state.T("In transit"),
                        snapshot.Icons.ContainsKey(iconKey) ? iconKey : null, key: $"cargo-{resource.Key}");
                }
            }
            html.Append("<view className=\"route-card\"><text className=\"route-title\">").Append(state.T("Found settlement")).Append("</text><text className=\"route-meta\">")
                .Append(E(form.FoundSettlementAvailability.Succeeded
                    ? state.TF("Town Hall site: {0}", form.FoundSettlementPosition)
                    : state.T(form.FoundSettlementAvailability.Reason)))
                .Append("</text></view>");
            html.Append("</view></scroll><view className=\"cargo-actions\"><text className=\"cargo-status\">")
                .Append(E(form.Availability.Succeeded ? state.T("Ready") : state.T(form.Availability.Reason))).Append("</text>");
            string action = form.Operation switch
            {
                CaravanCargoOperation.Load => state.T("LOAD CARGO"),
                CaravanCargoOperation.Unload => state.T("UNLOAD CARGO"),
                _ => state.T("RECOVER CARGO"),
            };
            html.Append(Button(action, "Globals.gameplay.TransferCargo()", "button primary",
                form.Availability.Succeeded ? action : state.T(form.Availability.Reason), !form.Availability.Succeeded));
            html.Append(Button(state.T("FOUND SETTLEMENT"), "Globals.gameplay.FoundSettlement()", "button positive",
                form.FoundSettlementAvailability.Succeeded
                    ? state.T("Found a Town Hall with the wagon cargo")
                    : state.T(form.FoundSettlementAvailability.Reason),
                !form.FoundSettlementAvailability.Succeeded));
            html.Append("</view>");
        }

        private static void AppendCargoRoute(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            var form = snapshot.Cargo;
            html.Append("<view className=\"cargo-capacity\"><text>").Append(state.T("Automatic route")).Append("</text><text>")
                .Append(form.Route.HasValue ? E(form.Route.Value.Phase.ToString()) : state.T("Idle"))
                .Append("</text></view>");

            html.Append("<scroll className=\"panel-scroll context-scroll\"><view className=\"cargo-form\">");
            if (form.Route.HasValue)
            {
                var route = form.Route.Value;
                html.Append("<view className=\"route-card\"><text className=\"route-title\">")
                    .Append(E(RouteLabel(form, route.Request.SourceSettlementId, route.Request.SourceWarehouseKey, state)))
                    .Append(" -> ")
                    .Append(E(RouteLabel(form, route.Request.TargetSettlementId, route.Request.TargetWarehouseKey, state)))
                    .Append("</text><text className=\"route-meta\">")
                    .Append(E(state.T(route.Status))).Append("</text><text className=\"route-meta\">")
                    .Append(route.Request.Repeat ? state.T("Repeating route") : state.T("One delivery"))
                    .Append(route.Moving ? state.T(" / moving") : state.T(" / waiting")).Append("</text></view>");
                html.Append("<text className=\"cargo-label\">").Append(state.T("SHIPMENT")).Append("</text>");
                foreach (var resource in route.Request.Resources.OrderBy(r => r.Key))
                {
                    string iconKey = GameplayHtmlIconKeys.Resource(resource.Key);
                    DataRow(html, state.T(DisplayResource(resource.Key)), Amount(resource.Value), state.T("Per trip"),
                        snapshot.Icons.ContainsKey(iconKey) ? iconKey : null, key: $"route-res-{resource.Key}");
                }
            }
            else
            {
                CargoSelect(html, "route-source", state.T("Pickup warehouse"), form.Warehouses.Select(w =>
                    $"{w.SettlementName} / {state.T(Display(w.BuildingId))} ({w.GridPosition.x}, {w.GridPosition.y})"),
                    form.WarehouseIndex, "Globals.gameplay.SetCargoWarehouse(event)", state);
                CargoSelect(html, "route-target", state.T("Destination warehouse"), form.Warehouses.Select(w =>
                    $"{w.SettlementName} / {state.T(Display(w.BuildingId))} ({w.GridPosition.x}, {w.GridPosition.y})"),
                    form.TargetIndex, "Globals.gameplay.SetCargoTarget(event)", state);
                CargoSelect(html, "route-resource", state.T("Resource"), form.Resources.Select(r =>
                    $"{state.T(DisplayResource(r.Key))} ({Amount(r.Value)})"), form.ResourceIndex,
                    "Globals.gameplay.SetCargoResource(event)", state);
                html.Append("<view className=\"cargo-field\"><text className=\"cargo-label\">").Append(state.T("Amount")).Append("</text>")
                    .Append("<input id=\"route-amount\" className=\"cargo-input\" contentType=\"DecimalNumber\" characterLimit=\"12\" value=\"")
                    .Append(E(form.Amount)).Append("\" onChange=\"Globals.gameplay.SetCargoAmount(event)\"></input></view>")
                    .Append("<view className=\"route-toggle\"><label className=\"cargo-label\" for=\"#cargo-repeat-toggle\">").Append(state.T("Repeat delivery")).Append("</label><toggle id=\"cargo-repeat-toggle\" checked=\"")
                    .Append(form.Repeat ? "true" : "false")
                    .Append("\" onChange=\"Globals.gameplay.SetCargoRepeat(event)\"><view className=\"toggle-knob\"></view></toggle></view>");
            }
            if (form.Cargo.Resources.Count > 0)
            {
                html.Append("<text className=\"cargo-label\">").Append(state.T("ON BOARD")).Append("</text>");
                foreach (var resource in form.Cargo.Resources.OrderBy(r => r.Key))
                {
                    string iconKey = GameplayHtmlIconKeys.Resource(resource.Key);
                    DataRow(html, state.T(DisplayResource(resource.Key)), Amount(resource.Value), state.T("In transit"),
                        snapshot.Icons.ContainsKey(iconKey) ? iconKey : null, key: $"cargo-{resource.Key}");
                }
            }
            html.Append("</view></scroll><view className=\"cargo-actions\"><text className=\"cargo-status\">");
            if (form.Route.HasValue)
                html.Append(E(state.T(form.Route.Value.Status)));
            else
                html.Append(E(state.T(form.RouteAvailability.Succeeded ? "Ready to schedule" : form.RouteAvailability.Reason)));
            html.Append("</text>");
            if (form.Route.HasValue)
                html.Append(Button(state.T("STOP ROUTE"), "Globals.gameplay.StopCargoRoute()", "button danger",
                    state.T("Stop automatic delivery")));
            else
                html.Append(Button(state.T("START ROUTE"), "Globals.gameplay.StartCargoRoute()", "button primary",
                    state.T(form.RouteAvailability.Succeeded ? "Start automatic delivery" : form.RouteAvailability.Reason),
                    !form.RouteAvailability.Succeeded));
            html.Append("</view>");
        }

        private static string RouteLabel(GameplayCargoSnapshot form, string settlementId, string warehouseKey, GameplayHtmlState state)
        {
            var warehouse = form.Warehouses.FirstOrDefault(w =>
                w.SettlementId == settlementId && w.WarehouseKey == warehouseKey);
            if (!string.IsNullOrWhiteSpace(warehouse.WarehouseKey))
                return $"{warehouse.SettlementName} / {state.T(Display(warehouse.BuildingId))}";
            return string.IsNullOrWhiteSpace(warehouseKey) ? state.T("Unknown warehouse") : warehouseKey;
        }

        private static void CargoSelect(StringBuilder html, string id, string label,
            IEnumerable<string> labels, int index, string changed, GameplayHtmlState state)
        {
            // The select component uses pipe-delimited labels; sanitize authored names before joining.
            string options = string.Join("|", labels.Select(x => x.Replace("|", "/")));
            bool empty = options.Length == 0;
            options = empty ? state.T("None available") : state.T("Choose...") + "|" + options;
            html.Append("<view className=\"cargo-field\"><text className=\"cargo-label\">").Append(E(label))
                .Append("</text><select id=\"").Append(id).Append("\" className=\"cargo-select\" options=\"")
                .Append(E(options)).Append("\" value=\"")
                .Append((index + 1).ToString(CultureInfo.InvariantCulture))
                .Append("\" onChange=\"").Append(changed).Append('"');
            if (empty) html.Append(" disabled=\"true\"");
            html.Append("></select></view>");
        }

        private static void CargoMode(StringBuilder html, GameplayCargoSnapshot form,
            CaravanCargoOperation mode, string label)
        {
            html.Append("<button className=\"tab").Append(form.Operation == mode ? " active" : string.Empty)
                .Append("\" onClick=\"Globals.gameplay.SetCargoOperation(").Append((int)mode)
                .Append(")\"><text>").Append(label).Append("</text></button>");
        }
    }
}
