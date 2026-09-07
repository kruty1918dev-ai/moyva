using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static partial class GameplayHtmlMarkup
    {
        private static void AppendCargo(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            var form = snapshot.Cargo;
            float used = form.Cargo.Resources.Sum(r => r.Value);
            html.Append("<view className=\"cargo-capacity\"><text>Wagon cargo</text><text>")
                .Append(Amount(used)).Append(" / ").Append(Amount(form.Cargo.Unit.Capacity))
                .Append("</text></view><view className=\"tabs cargo-modes\">");
            CargoMode(html, form, CaravanCargoOperation.Load, "LOAD");
            CargoMode(html, form, CaravanCargoOperation.Unload, "UNLOAD");
            CargoMode(html, form, CaravanCargoOperation.CollectLoot, "RECOVER");
            html.Append("</view><scroll className=\"panel-scroll context-scroll\"><view className=\"cargo-form\">");
            if (form.Operation != CaravanCargoOperation.CollectLoot)
            {
                var labels = form.Warehouses.Select(w =>
                    $"{w.SettlementName} / {Display(w.BuildingId)} ({w.GridPosition.x}, {w.GridPosition.y})");
                CargoSelect(html, "cargo-warehouse", "Warehouse", labels, form.WarehouseIndex,
                    "Globals.gameplay.SetCargoWarehouse(event)");
                if (form.WarehouseIndex >= 0)
                {
                    var warehouse = form.Warehouses[form.WarehouseIndex];
                    html.Append(Button("SHOW WAREHOUSE",
                        $"Globals.gameplay.FocusWarehouse({warehouse.GridPosition.x},{warehouse.GridPosition.y},'{J(warehouse.BuildingId)}')",
                        "button", "Focus the warehouse"));
                }
            }
            CargoSelect(html, "cargo-resource", "Resource", form.Resources.Select(r =>
                $"{DisplayResource(r.Key)} ({Amount(r.Value)})"), form.ResourceIndex,
                "Globals.gameplay.SetCargoResource(event)");
            html.Append("<view className=\"cargo-field\"><text className=\"cargo-label\">Amount</text>")
                .Append("<input id=\"cargo-amount\" className=\"cargo-input\" contentType=\"DecimalNumber\" characterLimit=\"12\" value=\"")
                .Append(E(form.Amount)).Append("\" onChange=\"Globals.gameplay.SetCargoAmount(event)\"></input></view>");
            if (form.Cargo.Resources.Count > 0)
            {
                html.Append("<text className=\"cargo-label\">ON BOARD</text>");
                foreach (var resource in form.Cargo.Resources.OrderBy(r => r.Key))
                {
                    string key = GameplayHtmlIconKeys.Resource(resource.Key);
                    DataRow(html, DisplayResource(resource.Key), Amount(resource.Value), "In transit",
                        snapshot.Icons.ContainsKey(key) ? key : null);
                }
            }
            html.Append("<view className=\"route-card\"><text className=\"route-title\">Found settlement</text><text className=\"route-meta\">")
                .Append(E(form.FoundSettlementAvailability.Succeeded
                    ? $"Town Hall site: {form.FoundSettlementPosition}"
                    : form.FoundSettlementAvailability.Reason))
                .Append("</text></view>");
            html.Append("</view></scroll><view className=\"cargo-actions\"><text className=\"cargo-status\">")
                .Append(E(form.Availability.Succeeded ? "Ready" : form.Availability.Reason)).Append("</text>");
            string action = form.Operation switch
            {
                CaravanCargoOperation.Load => "LOAD CARGO",
                CaravanCargoOperation.Unload => "UNLOAD CARGO",
                _ => "RECOVER CARGO",
            };
            html.Append(Button(action, "Globals.gameplay.TransferCargo()", "button primary",
                form.Availability.Succeeded ? action : form.Availability.Reason, !form.Availability.Succeeded));
            html.Append(Button("FOUND SETTLEMENT", "Globals.gameplay.FoundSettlement()", "button positive",
                form.FoundSettlementAvailability.Succeeded
                    ? "Found a Town Hall with the wagon cargo"
                    : form.FoundSettlementAvailability.Reason,
                !form.FoundSettlementAvailability.Succeeded));
            html.Append("</view>");
        }

        private static void AppendCargoRoute(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            var form = snapshot.Cargo;
            html.Append("<view className=\"cargo-capacity\"><text>Automatic route</text><text>")
                .Append(form.Route.HasValue ? E(form.Route.Value.Phase.ToString()) : "Idle")
                .Append("</text></view>");

            html.Append("<scroll className=\"panel-scroll context-scroll\"><view className=\"cargo-form\">");
            if (form.Route.HasValue)
            {
                var route = form.Route.Value;
                html.Append("<view className=\"route-card\"><text className=\"route-title\">")
                    .Append(E(RouteLabel(form, route.Request.SourceSettlementId, route.Request.SourceWarehouseKey)))
                    .Append(" -> ")
                    .Append(E(RouteLabel(form, route.Request.TargetSettlementId, route.Request.TargetWarehouseKey)))
                    .Append("</text><text className=\"route-meta\">")
                    .Append(E(route.Status)).Append("</text><text className=\"route-meta\">")
                    .Append(route.Request.Repeat ? "Repeating route" : "One delivery")
                    .Append(route.Moving ? " / moving" : " / waiting").Append("</text></view>");
                html.Append("<text className=\"cargo-label\">SHIPMENT</text>");
                foreach (var resource in route.Request.Resources.OrderBy(r => r.Key))
                {
                    string key = GameplayHtmlIconKeys.Resource(resource.Key);
                    DataRow(html, DisplayResource(resource.Key), Amount(resource.Value), "Per trip",
                        snapshot.Icons.ContainsKey(key) ? key : null);
                }
            }
            else
            {
                CargoSelect(html, "route-source", "Pickup warehouse", form.Warehouses.Select(w =>
                    $"{w.SettlementName} / {Display(w.BuildingId)} ({w.GridPosition.x}, {w.GridPosition.y})"),
                    form.WarehouseIndex, "Globals.gameplay.SetCargoWarehouse(event)");
                CargoSelect(html, "route-target", "Destination warehouse", form.Warehouses.Select(w =>
                    $"{w.SettlementName} / {Display(w.BuildingId)} ({w.GridPosition.x}, {w.GridPosition.y})"),
                    form.TargetIndex, "Globals.gameplay.SetCargoTarget(event)");
                CargoSelect(html, "route-resource", "Resource", form.Resources.Select(r =>
                    $"{DisplayResource(r.Key)} ({Amount(r.Value)})"), form.ResourceIndex,
                    "Globals.gameplay.SetCargoResource(event)");
                html.Append("<view className=\"cargo-field\"><text className=\"cargo-label\">Amount</text>")
                    .Append("<input id=\"route-amount\" className=\"cargo-input\" contentType=\"DecimalNumber\" characterLimit=\"12\" value=\"")
                    .Append(E(form.Amount)).Append("\" onChange=\"Globals.gameplay.SetCargoAmount(event)\"></input></view>")
                    .Append("<view className=\"route-toggle\"><text className=\"cargo-label\">Repeat delivery</text><toggle checked=\"")
                    .Append(form.Repeat ? "true" : "false")
                    .Append("\" onChange=\"Globals.gameplay.SetCargoRepeat(event)\"><view className=\"toggle-knob\"></view></toggle></view>");
            }
            if (form.Cargo.Resources.Count > 0)
            {
                html.Append("<text className=\"cargo-label\">ON BOARD</text>");
                foreach (var resource in form.Cargo.Resources.OrderBy(r => r.Key))
                {
                    string key = GameplayHtmlIconKeys.Resource(resource.Key);
                    DataRow(html, DisplayResource(resource.Key), Amount(resource.Value), "In transit",
                        snapshot.Icons.ContainsKey(key) ? key : null);
                }
            }
            html.Append("</view></scroll><view className=\"cargo-actions\"><text className=\"cargo-status\">");
            if (form.Route.HasValue)
                html.Append(E(form.Route.Value.Status));
            else
                html.Append(E(form.RouteAvailability.Succeeded ? "Ready to schedule" : form.RouteAvailability.Reason));
            html.Append("</text>");
            if (form.Route.HasValue)
                html.Append(Button("STOP ROUTE", "Globals.gameplay.StopCargoRoute()", "button danger",
                    "Stop automatic delivery"));
            else
                html.Append(Button("START ROUTE", "Globals.gameplay.StartCargoRoute()", "button primary",
                    form.RouteAvailability.Succeeded ? "Start automatic delivery" : form.RouteAvailability.Reason,
                    !form.RouteAvailability.Succeeded));
            html.Append("</view>");
        }

        private static string RouteLabel(GameplayCargoSnapshot form, string settlementId, string warehouseKey)
        {
            var warehouse = form.Warehouses.FirstOrDefault(w =>
                w.SettlementId == settlementId && w.WarehouseKey == warehouseKey);
            if (!string.IsNullOrWhiteSpace(warehouse.WarehouseKey))
                return $"{warehouse.SettlementName} / {Display(warehouse.BuildingId)}";
            return string.IsNullOrWhiteSpace(warehouseKey) ? "Unknown warehouse" : warehouseKey;
        }

        private static void CargoSelect(StringBuilder html, string id, string label,
            IEnumerable<string> labels, int index, string changed)
        {
            // The select component uses pipe-delimited labels; sanitize authored names before joining.
            string options = string.Join("|", labels.Select(x => x.Replace("|", "/")));
            bool empty = options.Length == 0;
            options = empty ? "None available" : "Choose...|" + options;
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
