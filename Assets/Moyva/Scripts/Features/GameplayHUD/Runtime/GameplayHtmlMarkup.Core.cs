using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static partial class GameplayHtmlMarkup
    {
        private const int ConstructionPageSize = 4;

        public static string Build(
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state,
            string viewportClass)
            => BuildDocument(BuildRegions(snapshot, state), viewportClass);

        private static void AppendTopBar(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<view className=\"top-section kingdom-summary\"><view className=\"brand-mark\"></view><view className=\"stack\"><text className=\"eyebrow\">KINGDOM</text><text className=\"title\">")
                .Append(E(snapshot.KingdomName)).Append("</text></view><view className=\"resources\">");
            int resourceCount = Math.Min(4, snapshot.Resources.Length);
            for (int index = 0; index < resourceCount; index++)
            {
                GameplayResourceSnapshot resource = snapshot.Resources[index];
                html.Append("<view className=\"resource\"><text className=\"resource-name\">")
                    .Append(E(DisplayResource(resource.Id))).Append("</text><text className=\"resource-value\">")
                    .Append(Amount(resource.Amount)).Append("</text></view>");
            }
            html.Append("</view></view><view className=\"top-section turn-summary\">");
            if (snapshot.TurnUiEnabled)
            {
                TurnPill(html, snapshot.Round.ToString(CultureInfo.InvariantCulture), "ROUND", string.Empty);
                TurnPill(html, snapshot.GlobalTurn.ToString(CultureInfo.InvariantCulture), "GLOBAL TURN", string.Empty);
                TurnPill(html, Display(snapshot.ActiveOwnerId), "ACTIVE PLAYER", string.Empty);
                TurnPill(html, snapshot.IsLocalTurn ? "YOUR TURN" : "WAITING", "STATUS", snapshot.IsLocalTurn ? "local-turn" : "waiting-turn");
            }
            else
            {
                TurnPill(html, "SANDBOX", "MODE", "local-turn");
                if (snapshot.SandboxRealtime)
                {
                    TurnPill(html, GameplayProgressTimeText.Elapsed(snapshot.SandboxElapsedSeconds), "GAME TIME", string.Empty);
                    TurnPill(html, GameplayProgressTimeText.Duration(snapshot.SandboxSecondsUntilNextProgress), "NEXT TICK", string.Empty);
                }
                else
                    TurnPill(html, "FREE PLAY", "SPEED", string.Empty);
            }
            html.Append("</view><view className=\"top-section top-actions\">")
                .Append(snapshot.SandboxRealtime ? SandboxSpeedButton(1f, snapshot.SandboxSpeed) : string.Empty)
                .Append(snapshot.SandboxRealtime ? SandboxSpeedButton(2f, snapshot.SandboxSpeed) : string.Empty)
                .Append(Button("KINGDOM", "Globals.gameplay.Kingdom()", "button", "Open kingdom dashboard"))
                .Append("<button className=\"button\" data-tooltip=\"Notifications\" onClick=\"Globals.gameplay.Notifications()\"><image className=\"action-icon\" src=\"global:gameplay_notifications_icon\" preserveAspect=\"true\"></image><text className=\"button-label\">")
                .Append(state.UnreadNotifications > 0 ? state.UnreadNotifications.ToString(CultureInfo.InvariantCulture) : "LOG")
                .Append("</text></button><button className=\"button\" data-tooltip=\"Game menu\" onClick=\"Globals.gameplay.Pause()\"><image className=\"action-icon\" src=\"global:gameplay_menu_icon\" preserveAspect=\"true\"></image><text className=\"button-label\">MENU</text></button>")
                .Append("</view>");
        }
        private static string SandboxSpeedButton(float speed, float current)
        {
            string text = $"{Amount(speed)}X";
            string selected = Math.Abs(current - speed) < 0.05f ? " selected" : string.Empty;
            return Button(text, $"Globals.gameplay.SandboxSpeed({speed.ToString(CultureInfo.InvariantCulture)})", $"button compact{selected}", $"{text} sandbox speed");
        }

        private static void AppendContextPanel(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            if (snapshot.RequiresFirstCastle)
            {
                PanelHeader(html, "ONBOARDING", "Place your first castle", false);
                html.Append("<view className=\"panel-body\"><view className=\"task ")
                    .Append(snapshot.PlacementValid ? "valid" : "invalid")
                    .Append("\"><text className=\"task-title\">Found your first settlement</text><text className=\"muted\">Select the castle, choose a valid tile, then confirm the placement.</text><text className=\"")
                    .Append(snapshot.PlacementValid ? "status-good" : "status-bad").Append("\">")
                    .Append(E(snapshot.PlacementStatus)).Append("</text></view>");
                if (!string.IsNullOrWhiteSpace(state.Feedback))
                    html.Append("<text className=\"feedback\">").Append(E(state.Feedback)).Append("</text>");
                html.Append("</view></view>");
                return;
            }

            if (state.OpenPanelId == GameplayHtmlPanel.Construction)
            {
                PanelHeader(html, "CONSTRUCTION", "Build in your kingdom", true);
                html.Append("<view className=\"panel-body\"><input id=\"construction-search\" className=\"browser-search\" value=\"")
                    .Append(E(state.ConstructionSearch))
                    .Append("\" placeholder=\"Search buildings\" characterLimit=\"48\" onEndEdit=\"Globals.gameplay.SetConstructionSearch(event)\"></input><scroll className=\"category-strip\"><view className=\"filter-row\">")
                    .Append(FilterButton("ALL", string.Empty, state.ConstructionCategory));
                var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < snapshot.BuildingOptions.Length; index++)
                {
                    string category = snapshot.BuildingOptions[index].Category;
                    if (!string.IsNullOrWhiteSpace(category) && categories.Add(category))
                        html.Append(FilterButton(category.ToUpperInvariant(), category, state.ConstructionCategory));
                }
                html.Append("</view></scroll><scroll className=\"panel-scroll construction-scroll\"><view className=\"building-list\">");
                var filteredOptions = new List<GameplayBuildingOptionSnapshot>(snapshot.BuildingOptions.Length);
                for (int index = 0; index < snapshot.BuildingOptions.Length; index++)
                {
                    GameplayBuildingOptionSnapshot option = snapshot.BuildingOptions[index];
                    if (!MatchesConstructionFilter(option, state))
                        continue;
                    filteredOptions.Add(option);
                }

                int visibleCount = filteredOptions.Count;
                int pageCount = Math.Max(1, (visibleCount + ConstructionPageSize - 1) / ConstructionPageSize);
                int pageIndex = Math.Min(Math.Max(0, state.ConstructionPageIndex), pageCount - 1);
                int firstIndex = pageIndex * ConstructionPageSize;
                int lastExclusive = Math.Min(firstIndex + ConstructionPageSize, visibleCount);
                for (int index = firstIndex; index < lastExclusive; index++)
                {
                    GameplayBuildingOptionSnapshot option = filteredOptions[index];
                    string cost = string.IsNullOrWhiteSpace(option.Cost) ? "Free" : option.Cost;
                    html.Append("<button data-key=\"").Append(E(option.Id)).Append("\" className=\"building-row ")
                        .Append(string.Equals(option.Id, snapshot.SelectedBuildingId, StringComparison.Ordinal) ? "selected" : string.Empty)
                        .Append("\" ").Append(option.CanSelect ? string.Empty : "disabled=\"true\"")
                        .Append(" onClick=\"Globals.gameplay.SelectBuilding('").Append(J(option.Id)).Append("')\">");
                    RowIcon(html, option.HasIcon, option.IconGlobalKey, IconForBuilding(option), false);
                    html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                        .Append(E(option.Name)).Append("</text><text className=\"item-meta\">")
                        .Append(E(option.CanSelect ? option.Description : option.UnavailableReason)).Append("</text><text className=\"item-cost\">")
                        .Append(E(cost)).Append("</text><text className=\"item-meta\">Build: ")
                        .Append(E(GameplayProgressTimeText.BuildDuration(option.BuildTurns, snapshot.SandboxRealtime, snapshot.SandboxRoundSeconds)))
                        .Append("</text></view><text className=\"row-chevron\">")
                        .Append(option.CanSelect ? ">" : "!").Append("</text></button>");
                }
                if (visibleCount == 0)
                    html.Append("<text className=\"empty\">No buildings match this filter.</text>");
                html.Append("</view></scroll>");
                AppendConstructionPager(html, pageIndex, pageCount, visibleCount, firstIndex, lastExclusive);
                html.Append("</view></view>");
                return;
            }

            if (state.OpenPanelId == GameplayHtmlPanel.Notifications)
            {
                PanelHeader(html, "ACTIVITY", "Notifications", true);
                html.Append("<view className=\"panel-body\"><scroll className=\"panel-scroll\"><view className=\"building-list\">");
                html.Append(Button("CLEAR ALL", "Globals.gameplay.ClearNotifications()", "button", "Clear notification history", state.Notifications.Count == 0));
                for (int index = 0; index < state.Notifications.Count; index++)
                {
                    GameplayNotificationViewSnapshot item = state.Notifications[index];
                    DataRow(html, item.Message, item.Kind.ToUpperInvariant(), item.CreatedAt.ToString("HH:mm:ss"));
                    if (item.Position.HasValue)
                        html.Append(Button("VIEW", $"Globals.gameplay.OpenNotification('{item.Id}')", "button primary", "Go to this event", false));
                    html.Append(Button("DISMISS", $"Globals.gameplay.RemoveNotification('{item.Id}')", "button", "Remove this notification", false));
                }
                if (state.Notifications.Count == 0)
                    html.Append("<text className=\"empty\">No notifications yet.</text>");
                html.Append("</view></scroll></view></view>");
                return;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.SelectionKind))
            {
                PanelHeader(
                    html,
                    snapshot.SelectionKind.ToUpperInvariant(),
                    string.IsNullOrWhiteSpace(snapshot.SelectionTitle) ? Display(snapshot.SelectionId) : snapshot.SelectionTitle,
                    true);
                html.Append("<view className=\"panel-body\"><view className=\"data-row\"><text className=\"row-icon small\">XY</text><view className=\"item-copy\"><text className=\"item-title\">Map position</text><text className=\"item-meta\">")
                    .Append(snapshot.SelectionPosition.x).Append(", ").Append(snapshot.SelectionPosition.y)
                    .Append("</text></view></view>");
                if (!string.IsNullOrWhiteSpace(snapshot.SelectionSubtitle))
                    html.Append("<text className=\"selection-summary\">").Append(E(snapshot.SelectionSubtitle)).Append("</text>");
                if (snapshot.SupportsRecruitment || snapshot.Cargo != null)
                {
                    html.Append("<view className=\"tabs\">");
                    SelectionTab(html, state, GameplaySelectionTab.Details, "DETAILS", "ShowSelectionDetails");
                    if (snapshot.SupportsRecruitment)
                    {
                        SelectionTab(html, state, GameplaySelectionTab.Recruit, "RECRUIT", "ShowRecruitment");
                        SelectionTab(html, state, GameplaySelectionTab.Queue, "QUEUE", "ShowRecruitmentQueue");
                    }
                    if (snapshot.Cargo != null)
                    {
                        SelectionTab(html, state, GameplaySelectionTab.Cargo, "CARGO", "ShowCargo");
                        SelectionTab(html, state, GameplaySelectionTab.Route, "ROUTE", "ShowCargoRoute");
                    }
                    html.Append("</view>");
                }

                if (snapshot.Cargo != null && state.SelectionTab == GameplaySelectionTab.Cargo)
                {
                    AppendCargo(html, snapshot);
                    html.Append("</view></view>");
                    return;
                }
                if (snapshot.Cargo != null && state.SelectionTab == GameplaySelectionTab.Route)
                {
                    AppendCargoRoute(html, snapshot);
                    html.Append("</view></view>");
                    return;
                }
                html.Append("<scroll className=\"panel-scroll context-scroll\">");
                if (!snapshot.SupportsRecruitment || state.SelectionTab == GameplaySelectionTab.Details)
                    AppendSelectionDetails(html, snapshot);
                else if (state.SelectionTab == GameplaySelectionTab.Recruit)
                    AppendRecruitment(html, snapshot);
                else
                    AppendRecruitmentQueue(html, snapshot);
                html.Append("</scroll></view></view>");
            }
        }

        private static void AppendSelectionDetails(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.AttackSourceId)
                && !snapshot.SelectionOwnedByLocalPlayer)
            {
                DataRow(
                    html,
                    "Attack",
                    snapshot.CanAttackSelection ? snapshot.AttackPreview : "Unavailable",
                    snapshot.CanAttackSelection
                        ? $"Attacker: {snapshot.AttackSourceId}"
                        : snapshot.AttackUnavailableReason);
                html.Append(Button(
                    "ATTACK",
                    "Globals.gameplay.AttackSelection()",
                    "button danger wide",
                    snapshot.CanAttackSelection ? "Attack selected target" : snapshot.AttackUnavailableReason,
                    !snapshot.CanAttackSelection));
            }

            if (!string.IsNullOrWhiteSpace(snapshot.AttackSourceId)
                && string.Equals(snapshot.SelectionKind, "Building", StringComparison.Ordinal)
                && !snapshot.SelectionOwnedByLocalPlayer)
            {
                DataRow(
                    html,
                    "Capture",
                    snapshot.CanCaptureSelection ? snapshot.CapturePreview : "Unavailable",
                    snapshot.CanCaptureSelection
                        ? $"Unit: {snapshot.AttackSourceId}"
                        : snapshot.CaptureUnavailableReason);
                html.Append(Button(
                    "CAPTURE",
                    "Globals.gameplay.CaptureSelection()",
                    "button success wide",
                    snapshot.CanCaptureSelection ? "Capture selected settlement" : snapshot.CaptureUnavailableReason,
                    !snapshot.CanCaptureSelection));
            }

            for (int index = 0; index < snapshot.SelectionFacts.Length; index++)
            {
                GameplayFactSnapshot fact = snapshot.SelectionFacts[index];
                DataRow(html, fact.Label, fact.Value, fact.Context);
            }
            if (snapshot.SupportsRecruitment)
                DataRow(html, "Recruitment queue", $"{snapshot.RecruitmentQueue.Length}/{snapshot.RecruitmentQueueCapacity}", "Training capacity");
        }

        private static void AppendRecruitment(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<view className=\"building-list recruitment-list\">");
            for (int index = 0; index < snapshot.RecruitmentRecipes.Length; index++)
            {
                GameplayRecruitmentRecipeSnapshot recipe = snapshot.RecruitmentRecipes[index];
                string trainingLabel = snapshot.TurnUiEnabled
                    ? $"{recipe.TrainingTurns} turns"
                    : GameplayProgressTimeText.Duration(recipe.TrainingSeconds);
                string meta = $"{recipe.Role} / {recipe.CombatType} / {trainingLabel} / {recipe.PopulationCost} residents / HP {recipe.HitPoints} / Move {Amount(recipe.Movement)}";
                html.Append("<view className=\"recruit-row\">");
                RowIcon(html, recipe.HasIcon, recipe.IconGlobalKey, IconForUnit(recipe.UnitTypeId), false);
                html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                    .Append(E(recipe.Name)).Append("</text><text className=\"item-meta\">")
                    .Append(E(meta)).Append("</text><text className=\"")
                    .Append(recipe.CanRecruit ? "muted" : "status-bad").Append("\">")
                    .Append(E(recipe.CanRecruit ? (string.IsNullOrWhiteSpace(recipe.Cost) ? "No resource cost" : recipe.Cost) : recipe.UnavailableReason))
                    .Append("</text></view>")
                    .Append(Button("RECRUIT", $"Globals.gameplay.Recruit('{J(recipe.UnitTypeId)}')", "button primary", $"Recruit {recipe.Name}", !recipe.CanRecruit))
                    .Append("</view>");
            }
            if (snapshot.RecruitmentRecipes.Length == 0)
                html.Append("<text className=\"empty\">This building has no recruitment recipes.</text>");
            html.Append("</view>");
        }

        private static void AppendRecruitmentQueue(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<text className=\"section-title\">TRAINING QUEUE ")
                .Append(snapshot.RecruitmentQueue.Length).Append('/').Append(snapshot.RecruitmentQueueCapacity)
                .Append("</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.RecruitmentQueue.Length; index++)
            {
                GameplayRecruitmentQueueSnapshot item = snapshot.RecruitmentQueue[index];
                string status = item.Ready
                    ? "READY TO DEPLOY"
                    : !item.Waiting
                        ? (snapshot.TurnUiEnabled
                            ? $"TRAINING {item.CompletedTurns}/{item.TrainingTurns}"
                            : $"TRAINING · {GameplayProgressTimeText.Duration(item.RemainingSeconds)}")
                        : (snapshot.TurnUiEnabled ? "WAITING" : "QUEUED");
                string context = item.Ready
                    ? "Select DEPLOY, then choose a valid map tile."
                    : snapshot.TurnUiEnabled
                        ? $"{Math.Max(0, item.TrainingTurns - item.CompletedTurns)} turns remaining"
                        : $"{GameplayProgressTimeText.Duration(item.RemainingSeconds)} {(item.Waiting ? "training time" : "remaining")}";
                DataRow(html, item.Name, status, context);
                html.Append(item.Ready
                    ? Button("DEPLOY", $"Globals.gameplay.DeployRecruitment('{item.QueueId}')", "button primary", "Place this unit", !snapshot.CanIssueLocalCommands)
                    : Button("CANCEL", $"Globals.gameplay.CancelRecruitment('{item.QueueId}')", "button", "Cancel training and refund resources", !snapshot.CanIssueLocalCommands));
            }
            if (snapshot.RecruitmentQueue.Length == 0)
                html.Append("<text className=\"empty\">The recruitment queue is empty.</text>");
            html.Append("</view>");
        }

        private static void AppendCommandBar(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<view className=\"command-title\"><text className=\"eyebrow\">AVAILABLE ACTIONS</text><text className=\"subtitle\">")
                .Append(snapshot.CanIssueLocalCommands ? "Issue a command" : "Waiting for active player")
                .Append("</text></view>");
            if (snapshot.RequiresFirstCastle || snapshot.PendingPlacementCount > 0)
            {
                html.Append(Button("ROTATE", "Globals.gameplay.RotatePlacement()", "button", "Rotate placement"))
                    .Append(Button("UNDO", "Globals.gameplay.UndoPlacement()", "button", "Undo placement"))
                    .Append(Button("CONFIRM", "Globals.gameplay.ConfirmPlacement()", "button positive", "Confirm placement"));
                if (!snapshot.RequiresFirstCastle)
                    html.Append(Button("CANCEL", "Globals.gameplay.CancelPlacement()", "button danger", "Cancel placement"));
            }
            else
            {
                html.Append(Button("BUILD", "Globals.gameplay.Construction()", "button primary", "Open construction"));
                if (!string.IsNullOrWhiteSpace(snapshot.SelectionId))
                    html.Append(Button("CLEAR", "Globals.gameplay.ClearSelection()", "button", "Clear selection"));
                if (snapshot.TurnUiEnabled)
                    html.Append(Button(snapshot.EndTurnPending ? "WAITING..." : "END TURN",
                        "Globals.gameplay.EndTurn()", "button positive",
                        snapshot.EndTurnPending ? "Waiting for host confirmation" : "End current turn",
                        snapshot.EndTurnPending || !snapshot.IsLocalTurn));
            }
        }

        private static void AppendDashboard(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            html.Append("<view id=\"kingdom-scrim\" className=\"scrim\" data-motion=\"fade\" data-motion-duration=\"0.12\"><view id=\"kingdom-dashboard\" className=\"dashboard\" data-motion=\"scale\" data-motion-duration=\"0.18\" data-motion-ease=\"out-back\">");
            PanelHeaderContent(html, "KINGDOM", snapshot.KingdomName, true);
            html.Append("<view className=\"tabs\">");
            Tab(html, state, KingdomDashboardTab.Overview, "OVERVIEW", "ShowOverview");
            Tab(html, state, KingdomDashboardTab.Resources, "RESOURCES", "ShowResources");
            Tab(html, state, KingdomDashboardTab.Storage, "STORAGE", "ShowStorage");
            Tab(html, state, KingdomDashboardTab.Buildings, "BUILDINGS", "ShowBuildings");
            Tab(html, state, KingdomDashboardTab.Units, "UNITS", "ShowUnits");
            if (snapshot.TurnUiEnabled)
                Tab(html, state, KingdomDashboardTab.Turns, "TURNS", "ShowTurns");
            html.Append("</view><view className=\"dashboard-body\"><scroll className=\"dashboard-scroll\">");
            switch (state.DashboardTab)
            {
                case KingdomDashboardTab.Resources:
                    AppendResources(html, snapshot);
                    break;
                case KingdomDashboardTab.Storage:
                    AppendStorage(html, snapshot);
                    break;
                case KingdomDashboardTab.Buildings:
                    AppendGroups(html, "BUILDING PORTFOLIO", snapshot.BuildingGroups, snapshot, false);
                    break;
                case KingdomDashboardTab.Units:
                    AppendGroups(html, "UNIT ROSTER", snapshot.UnitGroups, snapshot, true);
                    break;
                case KingdomDashboardTab.Turns:
                    if (snapshot.TurnUiEnabled)
                        AppendTurns(html, snapshot);
                    else
                        AppendOverview(html, snapshot);
                    break;
                default:
                    AppendOverview(html, snapshot);
                    break;
            }
            html.Append("</scroll></view></view></view>");
        }

        private static void AppendOverview(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<view className=\"stats-grid\">");
            Stat(html, snapshot.SettlementCount, "SETTLEMENTS");
            Stat(html, snapshot.Population, "POPULATION");
            Stat(html, snapshot.BuildingCount, "BUILDINGS");
            Stat(html, snapshot.UnitCount, "UNITS");
            if (snapshot.TurnUiEnabled)
            {
                Stat(html, snapshot.Round, "CURRENT ROUND");
                long localTurns = 0;
                for (int index = 0; index < snapshot.TurnHistory.Length; index++)
                {
                    if (snapshot.TurnHistory[index].Local)
                        localTurns = snapshot.TurnHistory[index].Completed;
                }
                Stat(html, localTurns, "COMPLETED TURNS");
            }
            html.Append("</view>");
            AppendResources(html, snapshot);
        }

        private static void AppendResources(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<text className=\"section-title\">KINGDOM TOTALS</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.Resources.Length; index++)
            {
                GameplayResourceSnapshot resource = snapshot.Resources[index];
                string key = GameplayHtmlIconKeys.Resource(resource.Id);
                DataRow(html, DisplayResource(resource.Id), Amount(resource.Amount), "Total across your settlements", snapshot.Icons.ContainsKey(key) ? key : null);
            }
            if (snapshot.Resources.Length == 0)
                html.Append("<text className=\"empty\">No resource records are available.</text>");
            html.Append("</view>");
            if (snapshot.Settlements.Length > 0)
            {
                html.Append("<text className=\"section-title\">BY SETTLEMENT</text><view className=\"building-list\">");
                for (int settlementIndex = 0; settlementIndex < snapshot.Settlements.Length; settlementIndex++)
                {
                    GameplaySettlementViewSnapshot settlement = snapshot.Settlements[settlementIndex];
                    string totals = ResourceSummary(settlement.Resources);
                    DataRow(
                        html,
                        settlement.Name,
                        string.IsNullOrWhiteSpace(totals) ? "No stored resources" : totals,
                        $"Population {settlement.Population} / Buildings {settlement.BuildingCount}");
                }
                html.Append("</view>");
            }
        }

        private static void AppendStorage(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<text className=\"section-title\">WAREHOUSES</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.Warehouses.Length; index++)
            {
                GameplayWarehouseViewSnapshot warehouse = snapshot.Warehouses[index];
                string capacity = warehouse.Capacity < 0
                    ? $"{Amount(warehouse.Used)} / unlimited"
                    : $"{Amount(warehouse.Used)} / {warehouse.Capacity}";
                html.Append("<button className=\"storage-row\" onClick=\"Globals.gameplay.FocusWarehouse(")
                    .Append(warehouse.Position.x).Append(',').Append(warehouse.Position.y).Append(",'")
                    .Append(J(warehouse.BuildingId)).Append("')\">");
                string iconKey = GameplayHtmlIconKeys.Building(warehouse.BuildingId);
                if (snapshot.Icons.ContainsKey(iconKey)) RowIcon(html, true, iconKey, string.Empty, false);
                html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                    .Append(E(string.IsNullOrWhiteSpace(warehouse.BuildingId) ? "Warehouse" : Display(warehouse.BuildingId)))
                    .Append("</text><text className=\"item-meta\">").Append(E(warehouse.Settlement))
                    .Append(" - grid ").Append(warehouse.Position.x).Append(", ").Append(warehouse.Position.y)
                    .Append(" - ").Append(E(ResourceSummary(warehouse.Resources)))
                    .Append("</text></view><text className=\"item-tag\">").Append(E(capacity)).Append("</text></button>");
            }
            if (snapshot.Warehouses.Length == 0)
                html.Append("<text className=\"empty\">No warehouses belong to this kingdom yet.</text>");
            html.Append("</view>");
        }

        private static void AppendGroups(StringBuilder html, string title, GameplayGroupSnapshot[] groups, GameplayHtmlSnapshot snapshot, bool units)
        {
            html.Append("<text className=\"section-title\">").Append(E(title)).Append("</text><view className=\"building-list\">");
            for (int index = 0; index < groups.Length; index++)
            {
                string key = units ? GameplayHtmlIconKeys.Unit(groups[index].Id) : GameplayHtmlIconKeys.Building(groups[index].Id);
                DataRow(html, groups[index].Label, groups[index].Count.ToString(CultureInfo.InvariantCulture), groups[index].Context,
                    snapshot.Icons.ContainsKey(key) ? key : null);
            }
            if (groups.Length == 0)
                html.Append("<text className=\"empty\">Nothing to display yet.</text>");
            html.Append("</view>");
        }

        private static void AppendTurns(StringBuilder html, GameplayHtmlSnapshot snapshot)
        {
            html.Append("<text className=\"section-title\">PARTICIPANTS</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.TurnHistory.Length; index++)
            {
                GameplayTurnHistoryViewSnapshot turn = snapshot.TurnHistory[index];
                string status = turn.Eliminated ? "ELIMINATED" : turn.Active ? "ACTIVE" : "WAITING";
                string context = turn.Local ? "Local kingdom" : "Opponent";
                DataRow(html, Display(turn.OwnerId), $"{turn.Completed} turns - {status}", context);
            }
            html.Append("</view>");
        }

        private static void AppendPause(StringBuilder html)
        {
            html.Append("<view id=\"pause-scrim\" className=\"scrim\" data-motion=\"fade\" data-motion-duration=\"0.12\"><view id=\"pause-modal\" className=\"modal\" data-motion=\"scale\" data-motion-duration=\"0.16\" data-motion-ease=\"out-back\"><text className=\"eyebrow\">GAME PAUSED</text><text className=\"modal-title\">Moyva</text><text className=\"modal-copy\">Return to the realm or leave this session.</text><view className=\"row gap\">")
                .Append(Button("RESUME", "Globals.gameplay.Resume()", "button positive", "Resume game"))
                .Append(Button("EXIT TO MENU", "Globals.gameplay.ExitToMenu()", "button danger", "Exit to main menu"))
                .Append("</view></view></view>");
        }

        private static void AppendGameOver(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            bool victory = !string.IsNullOrWhiteSpace(state.WinnerId)
                && string.Equals(
                    state.WinnerId,
                    snapshot.OwnerId,
                    StringComparison.Ordinal);
            string title = string.IsNullOrWhiteSpace(state.WinnerId)
                ? "The realm is silent"
                : victory
                    ? "Victory"
                    : "Defeat";
            string copy = string.IsNullOrWhiteSpace(state.WinnerId)
                ? "No kingdom remains able to rule."
                : victory
                    ? "Your kingdom is the last realm with a standing center."
                    : $"{Display(state.WinnerId)} controls the last standing center.";
            html.Append("<view id=\"gameover-scrim\" className=\"scrim\" data-motion=\"fade\" data-motion-duration=\"0.14\"><view id=\"gameover-modal\" className=\"modal result-modal\" data-motion=\"scale\" data-motion-duration=\"0.18\" data-motion-ease=\"out-back\"><text className=\"eyebrow\">MATCH ENDED</text><text className=\"modal-title ")
                .Append(victory ? "status-good" : "status-bad")
                .Append("\">").Append(E(title)).Append("</text><text className=\"modal-copy\">")
                .Append(E(copy)).Append("</text><view className=\"result-summary\">");
            DataRow(html, "Winner", string.IsNullOrWhiteSpace(state.WinnerId) ? "None" : Display(state.WinnerId), "Final kingdom");
            DataRow(html, "Rounds", snapshot.Round.ToString(CultureInfo.InvariantCulture), "Campaign length");
            DataRow(html, "Buildings", snapshot.BuildingCount.ToString(CultureInfo.InvariantCulture), "Your kingdom total");
            DataRow(html, "Units", snapshot.UnitCount.ToString(CultureInfo.InvariantCulture), "Your active roster");
            html.Append("</view><view className=\"row gap\">")
                .Append(Button("EXIT TO MENU", "Globals.gameplay.ExitToMenu()", "button positive", "Exit to main menu"))
                .Append("</view></view></view>");
        }

        private static void PanelHeader(StringBuilder html, string eyebrow, string title, bool close)
        {
            html.Append("<view id=\"gameplay-side-panel\" className=\"side-panel\" data-motion=\"slide-left\" data-motion-duration=\"0.16\">");
            PanelHeaderContent(html, eyebrow, title, close);
        }

        private static void PanelHeaderContent(StringBuilder html, string eyebrow, string title, bool close)
        {
            html.Append("<view className=\"panel-header\"><view className=\"panel-copy\"><text className=\"eyebrow\">")
                .Append(E(eyebrow)).Append("</text><text className=\"panel-title\">").Append(E(title)).Append("</text></view>");
            if (close)
                html.Append(Button("X", "Globals.gameplay.ClosePanel()", "button", "Close panel"));
            html.Append("</view>");
        }

        private static void Tab(StringBuilder html, GameplayHtmlState state, KingdomDashboardTab tab, string label, string method)
        {
            html.Append("<button className=\"tab ").Append(state.DashboardTab == tab ? "active" : string.Empty)
                .Append("\" onClick=\"Globals.gameplay.").Append(method).Append("()\"><text className=\"tab-label\">")
                .Append(label).Append("</text></button>");
        }

        private static void SelectionTab(StringBuilder html, GameplayHtmlState state, GameplaySelectionTab tab, string label, string method)
        {
            html.Append("<button className=\"tab ").Append(state.SelectionTab == tab ? "active" : string.Empty)
                .Append("\" onClick=\"Globals.gameplay.").Append(method).Append("()\"><text className=\"tab-label\">")
                .Append(label).Append("</text></button>");
        }

        private static bool MatchesConstructionFilter(
            GameplayBuildingOptionSnapshot option,
            GameplayHtmlState state)
        {
            if (!string.IsNullOrWhiteSpace(state.ConstructionCategory)
                && !string.Equals(option.Category, state.ConstructionCategory, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(state.ConstructionSearch))
                return true;
            string search = state.ConstructionSearch;
            return option.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || option.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || option.Category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FilterButton(string label, string value, string selected)
        {
            string active = string.Equals(value, selected, StringComparison.OrdinalIgnoreCase) ? " active" : string.Empty;
            return $"<button className=\"filter-button{active}\" onClick=\"Globals.gameplay.SetConstructionCategory('{J(value)}')\"><text className=\"tab-label\">{E(label)}</text></button>";
        }

        private static void TurnPill(StringBuilder html, string value, string label, string cssClass)
        {
            html.Append("<view className=\"turn-pill\"><text className=\"turn-value ").Append(cssClass).Append("\">")
                .Append(E(value)).Append("</text><text className=\"turn-label\">").Append(E(label)).Append("</text></view>");
        }

        private static void Stat(StringBuilder html, long value, string label)
        {
            html.Append("<view className=\"stat-card\"><text className=\"stat-value\">").Append(value)
                .Append("</text><text className=\"stat-label\">").Append(E(label)).Append("</text></view>");
        }

        private static void DataRow(StringBuilder html, string title, string value, string context, string iconKey = null)
        {
            html.Append("<view className=\"data-row\">");
            if (!string.IsNullOrEmpty(iconKey)) RowIcon(html, true, iconKey, string.Empty, true);
            html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                .Append(E(title)).Append("</text><text className=\"item-meta\">").Append(E(context))
                .Append("</text></view><text className=\"item-tag\">").Append(E(value)).Append("</text></view>");
        }

        private static void RowIcon(
            StringBuilder html,
            bool hasIcon,
            string globalKey,
            string fallback,
            bool small)
        {
            if (hasIcon && !string.IsNullOrWhiteSpace(globalKey))
            {
                html.Append("<image className=\"row-icon sprite-icon")
                    .Append(small ? " small" : string.Empty)
                    .Append("\" src=\"global:")
                    .Append(E(globalKey))
                    .Append("\" preserveAspect=\"true\"></image>");
                return;
            }

            html.Append("<text className=\"row-icon")
                .Append(small ? " small" : string.Empty)
                .Append("\">")
                .Append(E(fallback))
                .Append("</text>");
        }

        private static void AppendConstructionPager(
            StringBuilder html,
            int pageIndex,
            int pageCount,
            int total,
            int firstIndex,
            int lastExclusive)
        {
            string range = total == 0
                ? "No results"
                : $"Showing {firstIndex + 1}-{lastExclusive} of {total}";
            html.Append("<view className=\"list-footer\"><text className=\"list-count\">")
                .Append(E(range)).Append("</text><view className=\"row gap\">")
                .Append(Button("PREV", "Globals.gameplay.PreviousConstructionPage()", "button compact", "Previous page", pageIndex <= 0))
                .Append("<text className=\"page-indicator\">")
                .Append(pageIndex + 1).Append('/').Append(pageCount)
                .Append("</text>")
                .Append(Button("NEXT", "Globals.gameplay.NextConstructionPage()", "button compact", "Next page", pageIndex >= pageCount - 1))
                .Append("</view></view>");
        }

        private static string Button(string label, string action, string classes, string tooltip, bool disabled = false)
        {
            return $"<button className=\"{classes}\" data-tooltip=\"{E(tooltip)}\" {(disabled ? "disabled=\"true\"" : string.Empty)} onClick=\"{action}\"><text className=\"button-label\">{E(label)}</text></button>";
        }

        private static string Amount(float value) => value.ToString("0.#", CultureInfo.InvariantCulture);

        private static string ResourceSummary(GameplayResourceSnapshot[] resources)
        {
            if (resources == null || resources.Length == 0)
                return string.Empty;
            var summary = new StringBuilder();
            for (int index = 0; index < resources.Length; index++)
            {
                if (index > 0)
                    summary.Append(" / ");
                summary.Append(DisplayResource(resources[index].Id)).Append(' ').Append(Amount(resources[index].Amount));
            }
            return summary.ToString();
        }

        private static string IconForButton(string label)
        {
            string normalized = label?.Trim().ToUpperInvariant() ?? string.Empty;
            return normalized switch
            {
                "BUILD" => "+",
                "END TURN" => ">",
                "CONFIRM" => "OK",
                "CANCEL" => "X",
                "ROTATE" => "R",
                "UNDO" => "U",
                "CLEAR" => "CL",
                "KINGDOM" => "K",
                "RECRUIT" => "+",
                "EXIT TO MENU" => "X",
                "RESUME" => ">",
                "PREV" => "<",
                "NEXT" => ">",
                _ => string.Empty,
            };
        }

        private static string IconForBuilding(GameplayBuildingOptionSnapshot option)
        {
            string id = option.Id?.Trim().ToLowerInvariant() ?? string.Empty;
            if (id.Contains("castle") || id.Contains("town"))
                return "KT";
            if (id.Contains("warehouse") || id.Contains("storage"))
                return "ST";
            if (id.Contains("wall") || id.Contains("gate"))
                return "WL";
            if (id.Contains("farm") || id.Contains("food"))
                return "FD";
            if (id.Contains("wood") || id.Contains("lumber"))
                return "WD";
            if (id.Contains("barrack") || id.Contains("guard"))
                return "ML";

            string category = option.Category?.Trim().ToLowerInvariant() ?? string.Empty;
            if (category.StartsWith("mil", StringComparison.Ordinal))
                return "ML";
            if (category.StartsWith("ind", StringComparison.Ordinal))
                return "IN";
            if (category.StartsWith("wall", StringComparison.Ordinal))
                return "WL";
            if (category.StartsWith("civ", StringComparison.Ordinal))
                return "CV";
            if (category.StartsWith("set", StringComparison.Ordinal))
                return "KT";
            return "BL";
        }

        private static string IconForCategory(string value, string label)
        {
            string category = string.IsNullOrWhiteSpace(value)
                ? label?.Trim().ToLowerInvariant() ?? string.Empty
                : value.Trim().ToLowerInvariant();
            if (category.StartsWith("all", StringComparison.Ordinal))
                return "*";
            if (category.StartsWith("mil", StringComparison.Ordinal))
                return "ML";
            if (category.StartsWith("ind", StringComparison.Ordinal))
                return "IN";
            if (category.StartsWith("wall", StringComparison.Ordinal))
                return "WL";
            if (category.StartsWith("civ", StringComparison.Ordinal))
                return "CV";
            if (category.StartsWith("set", StringComparison.Ordinal))
                return "KT";
            return "BL";
        }

        private static string IconForUnit(string unitTypeId)
        {
            string id = unitTypeId?.Trim().ToLowerInvariant() ?? string.Empty;
            if (id.Contains("arch"))
                return "AR";
            if (id.Contains("guard") || id.Contains("soldier"))
                return "GD";
            if (id.Contains("worker"))
                return "WK";
            return "UN";
        }

        private static string IconForData(string title, string value)
        {
            string source = $"{title} {value}".ToLowerInvariant();
            if (source.Contains("resource") || source.Contains("food") || source.Contains("wood") || source.Contains("gold"))
                return "$";
            if (source.Contains("turn") || source.Contains("round"))
                return "T";
            if (source.Contains("unit"))
                return "UN";
            if (source.Contains("building") || source.Contains("castle") || source.Contains("warehouse"))
                return "BL";
            if (source.Contains("population") || source.Contains("settlement"))
                return "P";
            if (source.Contains("status"))
                return "!";
            return "#";
        }

        private static string Display(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";
            string normalized = value.Replace('-', ' ').Replace('_', ' ').Trim();
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
        }

        private static string DisplayResource(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";
            string normalized = value.Trim();
            normalized = TrimSuffix(normalized, "-materials-resources");
            normalized = TrimSuffix(normalized, "_materials_resources");
            normalized = TrimSuffix(normalized, "-resources");
            normalized = TrimSuffix(normalized, "_resources");
            normalized = TrimSuffix(normalized, "-resource");
            normalized = TrimSuffix(normalized, "_resource");
            return Display(normalized);
        }

        private static string TrimSuffix(string value, string suffix)
        {
            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(0, value.Length - suffix.Length)
                : value;
        }

        private static string E(string value)
            => (value ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        private static string J(string value)
            => (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
    }
}
