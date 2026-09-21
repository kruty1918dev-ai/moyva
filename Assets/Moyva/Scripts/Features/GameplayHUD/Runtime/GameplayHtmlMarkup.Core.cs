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
            html.Append("<view className=\"top-section kingdom-summary\"><view className=\"brand-mark\"></view><view className=\"stack\"><text className=\"eyebrow\">")
                .Append(state.T("KINGDOM")).Append("</text><text className=\"title\">")
                .Append(E(snapshot.KingdomName)).Append("</text></view><view className=\"resources\" data-tooltip=\"").Append(E(state.T("Kingdom-wide totals — construction spends local settlement stock"))).Append("\">");
            int resourceCount = Math.Min(4, snapshot.Resources.Length);
            for (int index = 0; index < resourceCount; index++)
            {
                GameplayResourceSnapshot resource = snapshot.Resources[index];
                html.Append("<view className=\"resource\"><text className=\"resource-name\">")
                    .Append(E(state.T(DisplayResource(resource.Id)))).Append("</text><text className=\"resource-value\">")
                    .Append(Amount(resource.Amount)).Append("</text></view>");
            }
            html.Append("</view></view><view className=\"top-section turn-summary\">");
            if (snapshot.TurnUiEnabled)
            {
                bool botMatch = Kruty1918.Moyva.SaveSystem.GameLaunchContext.HasBotOpponent;
                if (botMatch)
                    TurnPill(html, state.T("VS BOT"), state.T("MODE"), "local-turn");
                TurnPill(html, snapshot.Round.ToString(CultureInfo.InvariantCulture), state.T("ROUND"), string.Empty);
                if (!botMatch)
                    TurnPill(html, snapshot.GlobalTurn.ToString(CultureInfo.InvariantCulture), state.T("GLOBAL TURN"), string.Empty);
                TurnPill(html, botMatch ? state.T("BOT") : Display(snapshot.ActiveOwnerId), botMatch ? state.T("OPPONENT") : state.T("ACTIVE PLAYER"), string.Empty);
                TurnPill(html, botMatch ? (snapshot.IsLocalTurn ? state.T("YOUR TURN") : state.T("BOT TURN")) : snapshot.IsLocalTurn ? state.T("YOUR TURN") :
                    Kruty1918.Moyva.SaveSystem.GameLaunchContext.GetPlayerController(snapshot.ActiveOwnerId)
                        == Kruty1918.Moyva.SaveSystem.PlayerControllerType.Bot ? state.T("BOT TURN") : state.T("WAITING"),
                    state.T("STATUS"), snapshot.IsLocalTurn ? "local-turn" : "waiting-turn");
            }
            else
            {
                TurnPill(html, state.T("SANDBOX"), state.T("MODE"), "local-turn");
                if (snapshot.SandboxRealtime)
                {
                    TurnPill(html, GameplayProgressTimeText.Elapsed(snapshot.SandboxElapsedSeconds), state.T("GAME TIME"), string.Empty);
                    TurnPill(html, GameplayProgressTimeText.Duration(snapshot.SandboxSecondsUntilNextProgress), state.T("NEXT TICK"), string.Empty);
                }
                else
                    TurnPill(html, state.T("FREE PLAY"), state.T("SPEED"), string.Empty);
            }
            html.Append("</view><view className=\"top-section top-actions\">")
                .Append(snapshot.SandboxRealtime ? SandboxSpeedButton(1f, snapshot.SandboxSpeed, state, snapshot.RequiresFirstCastle) : string.Empty)
                .Append(snapshot.SandboxRealtime ? SandboxSpeedButton(2f, snapshot.SandboxSpeed, state, snapshot.RequiresFirstCastle) : string.Empty)
                // While the first-castle prompt is modal, the top bar is off
                // limits so Tab cannot navigate out of the forced placement.
                .Append(Button(state.T("KINGDOM"), "Globals.gameplay.Kingdom()", "button", state.T("Open kingdom dashboard"), snapshot.RequiresFirstCastle))
                .Append("<button className=\"button\" data-tooltip=\"").Append(E(state.T("Notifications"))).Append(snapshot.RequiresFirstCastle ? "\" disabled=\"true" : "").Append("\" onClick=\"Globals.gameplay.Notifications()\"><image className=\"action-icon\" src=\"global:gameplay_notifications_icon\" preserveAspect=\"true\"></image><text className=\"button-label\">")
                .Append(state.UnreadNotifications > 0 ? state.UnreadNotifications.ToString(CultureInfo.InvariantCulture) : state.T("LOG"))
                .Append("</text></button><button className=\"button\" data-tooltip=\"").Append(E(state.T("Game menu"))).Append(snapshot.RequiresFirstCastle ? "\" disabled=\"true" : "").Append("\" onClick=\"Globals.gameplay.Pause()\"><image className=\"action-icon\" src=\"global:gameplay_menu_icon\" preserveAspect=\"true\"></image><text className=\"button-label\">").Append(state.T("MENU")).Append("</text></button>")
                .Append("</view>");
        }
        private static string SandboxSpeedButton(float speed, float current, GameplayHtmlState state, bool disabled = false)
        {
            string text = $"{Amount(speed)}X";
            string selected = Math.Abs(current - speed) < 0.05f ? " selected" : string.Empty;
            return Button(text, $"Globals.gameplay.SandboxSpeed({speed.ToString(CultureInfo.InvariantCulture)})", $"button compact{selected}", state.TF("{0} sandbox speed", text), disabled);
        }

        private static void AppendContextPanel(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            if (snapshot.RequiresFirstCastle)
            {
                PanelHeader(html, state, state.T("ONBOARDING"), state.T("Place your first castle"), false);
                html.Append("<view className=\"panel-body\"><view className=\"task ")
                    .Append(snapshot.PlacementValid ? "valid" : "invalid")
                    .Append("\"><text className=\"task-title\">").Append(state.T("Found your first settlement")).Append("</text><text className=\"muted\">").Append(state.T("Select the castle, choose a valid tile, then confirm the placement.")).Append("</text><text className=\"")
                    .Append(snapshot.PlacementValid ? "status-good" : "status-bad").Append("\">")
                    .Append(E(snapshot.PlacementStatus)).Append("</text></view>");
                if (!string.IsNullOrWhiteSpace(state.Feedback))
                    html.Append("<text className=\"feedback\">").Append(E(state.Feedback)).Append("</text>");
                html.Append("</view></view>");
                return;
            }

            if (state.OpenPanelId == GameplayHtmlPanel.Construction)
            {
                PanelHeader(html, state, state.T("CONSTRUCTION"), state.T("Build in your kingdom"), true);
                html.Append("<view className=\"panel-body\"><input id=\"construction-search\" data-autofocus=\"true\" className=\"browser-search\" value=\"")
                    .Append(E(state.ConstructionSearch))
                    .Append("\" placeholder=\"").Append(E(state.T("Search buildings"))).Append("\" characterLimit=\"48\" onEndEdit=\"Globals.gameplay.SetConstructionSearch(event)\"></input><scroll className=\"category-strip\"><view className=\"filter-row\">")
                    .Append(FilterButton(state.T("ALL"), string.Empty, state.ConstructionCategory));
                var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < snapshot.BuildingOptions.Length; index++)
                {
                    string category = snapshot.BuildingOptions[index].Category;
                    if (!string.IsNullOrWhiteSpace(category) && categories.Add(category))
                        html.Append(FilterButton(state.T(category.ToUpperInvariant()), category, state.ConstructionCategory));
                }
                html.Append("</view></scroll>");
                if (!string.IsNullOrWhiteSpace(state.ConstructionProducerResource))
                {
                    html.Append("<view className=\"filter-row\"><button className=\"filter-button active\" onClick=\"Globals.gameplay.ClearProducerFilter()\"><text className=\"tab-label\">")
                        .Append(E(state.TF(
                            "Produces {0}",
                            DisplayResource(state.ConstructionProducerResource))))
                        .Append("</text></button></view>");
                }
                html.Append("<scroll className=\"panel-scroll construction-scroll\"><view className=\"building-list\">");
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
                    string cost = string.IsNullOrWhiteSpace(option.Cost) ? state.T("Free") : state.T(option.Cost);
                    html.Append("<button data-key=\"").Append(E(option.Id)).Append("\" className=\"building-row ")
                        .Append(string.Equals(option.Id, snapshot.SelectedBuildingId, StringComparison.Ordinal) ? "selected" : string.Empty)
                        .Append("\" ").Append(option.CanSelect ? string.Empty : "disabled=\"true\"")
                        .Append(" onClick=\"Globals.gameplay.SelectBuilding('").Append(J(option.Id)).Append("')\">");
                    RowIcon(html, option.HasIcon, option.IconGlobalKey, IconForBuilding(option), false);
                    html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                        .Append(E(state.T(option.Name))).Append("</text><text className=\"item-meta\">")
                        .Append(E(option.CanSelect ? state.T(option.Description) : state.T(option.UnavailableReason))).Append("</text><text className=\"item-cost\">")
                        .Append(E(cost)).Append("</text><text className=\"item-meta\">").Append(state.T("Build: "))
                        .Append(E(GameplayProgressTimeText.BuildDuration(option.BuildTurns, snapshot.SandboxRealtime, snapshot.SandboxRoundSeconds, state.Localization)))
                        .Append("</text></view><text className=\"row-chevron\">")
                        .Append(option.CanSelect ? ">" : "!").Append("</text></button>");
                }
                if (visibleCount == 0)
                    html.Append("<text className=\"empty\">").Append(state.T("No buildings match this filter.")).Append("</text>");
                html.Append("</view></scroll>");
                AppendConstructionPager(html, pageIndex, pageCount, visibleCount, firstIndex, lastExclusive, state);
                html.Append("</view></view>");
                return;
            }

            if (state.OpenPanelId == GameplayHtmlPanel.Supply)
            {
                AppendSupplyPanel(html, snapshot, state);
                return;
            }

            if (state.OpenPanelId == GameplayHtmlPanel.Notifications)
            {
                PanelHeader(html, state, state.T("ACTIVITY"), state.T("Notifications"), true);
                html.Append("<view className=\"panel-body\"><scroll className=\"panel-scroll\"><view className=\"building-list\">");
                html.Append(Button(state.T("CLEAR ALL"), "Globals.gameplay.ClearNotifications()", "button", state.T("Clear notification history"), state.Notifications.Count == 0));
                for (int index = 0; index < state.Notifications.Count; index++)
                {
                    GameplayNotificationViewSnapshot item = state.Notifications[index];
                    DataRow(html, item.Message, item.Kind.ToUpperInvariant(), item.CreatedAt.ToString("HH:mm:ss"));
                    if (item.Position.HasValue)
                        html.Append(Button(state.T("VIEW"), $"Globals.gameplay.OpenNotification('{item.Id}')", "button primary", state.T("Go to this event"), false));
                    html.Append(Button(state.T("DISMISS"), $"Globals.gameplay.RemoveNotification('{item.Id}')", "button", state.T("Remove this notification"), false));
                }
                if (state.Notifications.Count == 0)
                    html.Append("<text className=\"empty\">").Append(state.T("No notifications yet.")).Append("</text>");
                html.Append("</view></scroll></view></view>");
                return;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.SelectionKind))
            {
                PanelHeader(
                    html,
                    state,
                    state.T(snapshot.SelectionKind.ToUpperInvariant()),
                    state.T(string.IsNullOrWhiteSpace(snapshot.SelectionTitle) ? Display(snapshot.SelectionId) : snapshot.SelectionTitle),
                    true);
                html.Append("<view className=\"panel-body\"><view className=\"data-row\"><text className=\"row-icon small\">XY</text><view className=\"item-copy\"><text className=\"item-title\">").Append(state.T("Map position")).Append("</text><text className=\"item-meta\">")
                    .Append(snapshot.SelectionPosition.x).Append(", ").Append(snapshot.SelectionPosition.y)
                    .Append("</text></view></view>");
                if (!string.IsNullOrWhiteSpace(snapshot.SelectionSubtitle))
                    html.Append("<text className=\"selection-summary\">").Append(E(state.T(snapshot.SelectionSubtitle))).Append("</text>");
                if (snapshot.SupportsRecruitment || snapshot.Cargo != null)
                {
                    html.Append("<view className=\"tabs\">");
                    SelectionTab(html, state, GameplaySelectionTab.Details, state.T("DETAILS"), "ShowSelectionDetails");
                    if (snapshot.SupportsRecruitment)
                    {
                        SelectionTab(html, state, GameplaySelectionTab.Recruit, state.T("RECRUIT"), "ShowRecruitment");
                        SelectionTab(html, state, GameplaySelectionTab.Queue, state.T("QUEUE"), "ShowRecruitmentQueue");
                    }
                    if (snapshot.Cargo != null)
                    {
                        SelectionTab(html, state, GameplaySelectionTab.Cargo, state.T("CARGO"), "ShowCargo");
                        SelectionTab(html, state, GameplaySelectionTab.Route, state.T("ROUTE"), "ShowCargoRoute");
                    }
                    html.Append("</view>");
                }

                if (snapshot.Cargo != null && state.SelectionTab == GameplaySelectionTab.Cargo)
                {
                    AppendCargo(html, snapshot, state);
                    html.Append("</view></view>");
                    return;
                }
                if (snapshot.Cargo != null && state.SelectionTab == GameplaySelectionTab.Route)
                {
                    AppendCargoRoute(html, snapshot, state);
                    html.Append("</view></view>");
                    return;
                }
                html.Append("<scroll className=\"panel-scroll context-scroll\">");
                if (!snapshot.SupportsRecruitment || state.SelectionTab == GameplaySelectionTab.Details)
                    AppendSelectionDetails(html, snapshot, state);
                else if (state.SelectionTab == GameplaySelectionTab.Recruit)
                    AppendRecruitment(html, snapshot, state);
                else
                    AppendRecruitmentQueue(html, snapshot, state);
                html.Append("</scroll></view></view>");
            }
        }

        private static void AppendSelectionDetails(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.AttackSourceId)
                && !snapshot.SelectionOwnedByLocalPlayer)
            {
                DataRow(
                    html,
                    state.T("Attack"),
                    snapshot.CanAttackSelection ? state.T(snapshot.AttackPreview) : state.T("Unavailable"),
                    snapshot.CanAttackSelection
                        ? state.TF("Attacker: {0}", snapshot.AttackSourceId)
                        : state.T(snapshot.AttackUnavailableReason));
                html.Append(Button(
                    state.T("ATTACK"),
                    "Globals.gameplay.AttackSelection()",
                    "button danger wide",
                    snapshot.CanAttackSelection ? state.T("Attack selected target") : state.T(snapshot.AttackUnavailableReason),
                    !snapshot.CanAttackSelection));
            }

            if (string.Equals(snapshot.SelectionKind, "Unit", StringComparison.Ordinal)
                && snapshot.SelectionOwnedByLocalPlayer
                && snapshot.CanIssueLocalCommands)
            {
                if (!string.IsNullOrWhiteSpace(snapshot.SelectedUnitGroupId))
                {
                    DataRow(
                        html,
                        "Group",
                        $"{snapshot.SelectedUnitGroupSize} unit(s)",
                        string.IsNullOrWhiteSpace(snapshot.SelectedUnitGroupMembers)
                            ? "Move orders apply to the whole group"
                            : snapshot.SelectedUnitGroupMembers);
                    html.Append(Button(
                        "DISBAND GROUP",
                        "Globals.gameplay.GroupDisband()",
                        "button danger wide",
                        "Disband this unit group",
                        false));
                }
                html.Append(Button(
                    snapshot.GroupMergeArmed ? "CANCEL MERGE" : "MERGE INTO GROUP",
                    "Globals.gameplay.GroupMergeToggle()",
                    snapshot.GroupMergeArmed ? "button wide" : "button primary wide",
                    snapshot.GroupMergeArmed
                        ? "Merge armed: click another own unit on the map, or press to cancel"
                        : "Arm merge, then click another own unit on the map",
                    false));
            }

            if (!string.IsNullOrWhiteSpace(snapshot.AttackSourceId)
                && string.Equals(snapshot.SelectionKind, "Building", StringComparison.Ordinal)
                && !snapshot.SelectionOwnedByLocalPlayer)
            {
                DataRow(
                    html,
                    state.T("Capture"),
                    snapshot.CanCaptureSelection ? state.T(snapshot.CapturePreview) : state.T("Unavailable"),
                    snapshot.CanCaptureSelection
                        ? state.TF("Unit: {0}", snapshot.AttackSourceId)
                        : state.T(snapshot.CaptureUnavailableReason));
                html.Append(Button(
                    state.T("CAPTURE"),
                    "Globals.gameplay.CaptureSelection()",
                    "button success wide",
                    snapshot.CanCaptureSelection ? state.T("Capture selected settlement") : state.T(snapshot.CaptureUnavailableReason),
                    !snapshot.CanCaptureSelection));
            }

            for (int index = 0; index < snapshot.SelectionFacts.Length; index++)
            {
                GameplayFactSnapshot fact = snapshot.SelectionFacts[index];
                DataRow(html, state.T(fact.Label), state.T(fact.Value), state.T(fact.Context));
            }
            if (snapshot.SupportsRecruitment)
                DataRow(html, state.T("Recruitment queue"), $"{snapshot.RecruitmentQueue.Length}/{snapshot.RecruitmentQueueCapacity}", state.T("Training capacity"));
        }

        private static void AppendRecruitment(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<view className=\"building-list recruitment-list\">");
            for (int index = 0; index < snapshot.RecruitmentRecipes.Length; index++)
            {
                GameplayRecruitmentRecipeSnapshot recipe = snapshot.RecruitmentRecipes[index];
                string trainingLabel = snapshot.TurnUiEnabled
                    ? state.TF("{0} {1}", recipe.TrainingTurns, state.TN("turn", "turns", recipe.TrainingTurns))
                    : GameplayProgressTimeText.Duration(recipe.TrainingSeconds);
                string meta = $"{state.T(recipe.Role)} / {state.T(recipe.CombatType)} / {trainingLabel} / {state.TF("{0} residents", recipe.PopulationCost)} / {state.T("HP")} {recipe.HitPoints} / {state.T("Move")} {Amount(recipe.Movement)}";
                html.Append("<view className=\"recruit-row\">");
                RowIcon(html, recipe.HasIcon, recipe.IconGlobalKey, IconForUnit(recipe.UnitTypeId), false);
                html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                    .Append(E(state.T(recipe.Name))).Append("</text><text className=\"item-meta\">")
                    .Append(E(meta)).Append("</text><text className=\"")
                    .Append(recipe.CanRecruit ? "muted" : "status-bad").Append("\">")
                    .Append(E(recipe.CanRecruit ? (string.IsNullOrWhiteSpace(recipe.Cost) ? state.T("No resource cost") : state.T(recipe.Cost)) : state.T(recipe.UnavailableReason)))
                    .Append("</text>");
                if (!recipe.CanRecruit
                    && recipe.MissingResourceIds != null
                    && recipe.MissingResourceIds.Length > 0)
                {
                    html.Append("<view className=\"filter-row\">");
                    for (int missing = 0; missing < recipe.MissingResourceIds.Length; missing++)
                    {
                        string resourceId = recipe.MissingResourceIds[missing];
                        string actionId =
                            recipe.ProducerActionResourceIds != null
                            && missing < recipe.ProducerActionResourceIds.Length
                                ? recipe.ProducerActionResourceIds[missing]
                                : resourceId;
                        if (recipe.ProducerActionResourceIds != null
                            && string.IsNullOrWhiteSpace(actionId))
                        {
                            html.Append("<text className=\"muted\">")
                                .Append(E(state.TF("No achievable producer for {0}",
                                    DisplayResource(resourceId))))
                                .Append("</text>");
                            continue;
                        }
                        html.Append(Button(
                            state.TF("Find {0} producer", DisplayResource(actionId)),
                            $"Globals.gameplay.ShowProducersFor('{J(actionId)}')",
                            "button small",
                            state.T("Show buildings that produce this resource"),
                            false));
                    }
                    html.Append("</view>");
                }
                html.Append("</view>")
                    .Append(Button(state.T("RECRUIT"), $"Globals.gameplay.Recruit('{J(recipe.UnitTypeId)}')", "button primary", state.TF("Recruit {0}", state.T(recipe.Name)), !recipe.CanRecruit))
                    .Append("</view>");
            }
            if (snapshot.RecruitmentRecipes.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("This building has no recruitment recipes.")).Append("</text>");
            html.Append("</view>");
        }

        private static void AppendRecruitmentQueue(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<text className=\"section-title\">").Append(state.T("TRAINING QUEUE")).Append(' ')
                .Append(snapshot.RecruitmentQueue.Length).Append('/').Append(snapshot.RecruitmentQueueCapacity)
                .Append("</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.RecruitmentQueue.Length; index++)
            {
                GameplayRecruitmentQueueSnapshot item = snapshot.RecruitmentQueue[index];
                string status = item.Ready
                    ? state.T("READY TO DEPLOY")
                    : !item.Waiting
                        ? (snapshot.TurnUiEnabled
                            ? state.TF("TRAINING {0}/{1}", item.CompletedTurns, item.TrainingTurns)
                            : state.TF("TRAINING · {0}", GameplayProgressTimeText.Duration(item.RemainingSeconds)))
                        : (snapshot.TurnUiEnabled ? state.T("WAITING") : state.T("QUEUED"));
                string context = item.Ready
                    ? state.T("Select DEPLOY, then choose a valid map tile.")
                    : snapshot.TurnUiEnabled
                        ? state.TF("{0} {1} remaining", Math.Max(0, item.TrainingTurns - item.CompletedTurns), state.TN("turn", "turns", Math.Max(0, item.TrainingTurns - item.CompletedTurns)))
                        : state.TF("{0} {1}", GameplayProgressTimeText.Duration(item.RemainingSeconds), state.T(item.Waiting ? "training time" : "remaining"));
                DataRow(html, state.T(item.Name), status, context);
                html.Append(item.Ready
                    ? Button(state.T("DEPLOY"), $"Globals.gameplay.DeployRecruitment('{item.QueueId}')", "button primary", state.T("Place this unit"), !snapshot.CanIssueLocalCommands)
                    : Button(state.T("CANCEL"), $"Globals.gameplay.CancelRecruitment('{item.QueueId}')", "button", state.T("Cancel training and refund resources"), !snapshot.CanIssueLocalCommands));
            }
            if (snapshot.RecruitmentQueue.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("The recruitment queue is empty.")).Append("</text>");
            html.Append("</view>");
        }

        private static void AppendCommandBar(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<view className=\"command-title\"><text className=\"eyebrow\">").Append(state.T("AVAILABLE ACTIONS")).Append("</text><text className=\"subtitle\">")
                .Append(snapshot.CanIssueLocalCommands ? state.T("Issue a command") : state.T("Waiting for active player"))
                .Append("</text></view>");
            if (snapshot.RequiresFirstCastle || snapshot.PendingPlacementCount > 0)
            {
                html.Append(Button(state.T("ROTATE"), "Globals.gameplay.RotatePlacement()", "button", state.T("Rotate placement")))
                    .Append(Button(state.T("UNDO"), "Globals.gameplay.UndoPlacement()", "button", state.T("Undo placement")))
                    .Append(Button(state.T("CONFIRM"), "Globals.gameplay.ConfirmPlacement()", "button positive", state.T("Confirm placement")));
                if (snapshot.HasPendingSupplyDeficit)
                    html.Append(Button(state.T("SUPPLY"),
                        $"Globals.gameplay.OpenSupply({snapshot.PendingSupplyPosition.x},{snapshot.PendingSupplyPosition.y},'{J(snapshot.PendingSupplyBuildingId)}')",
                        "button primary",
                        state.T("Send missing resources to this settlement by wagon")));
                if (!snapshot.RequiresFirstCastle)
                    html.Append(Button(state.T("CANCEL"), "Globals.gameplay.CancelPlacement()", "button danger", state.T("Cancel placement")));
            }
            else
            {
                html.Append(Button(state.T("BUILD"), "Globals.gameplay.Construction()", "button primary", state.T("Open construction")));
                if (!string.IsNullOrWhiteSpace(snapshot.SelectionId))
                    html.Append(Button(state.T("CLEAR"), "Globals.gameplay.ClearSelection()", "button", state.T("Clear selection")));
                if (snapshot.TurnUiEnabled)
                    html.Append(Button(Kruty1918.Moyva.SaveSystem.GameLaunchContext.HasBotOpponent
                            ? (snapshot.IsLocalTurn ? state.T("END TURN") : state.T("BOT TURN"))
                            : snapshot.EndTurnPending ? state.T("WAITING...") : state.T("END TURN"),
                        "Globals.gameplay.EndTurn()", "button positive",
                        snapshot.EndTurnPending ? state.T("Waiting for host confirmation") : state.T("End current turn"),
                        snapshot.EndTurnPending || !snapshot.IsLocalTurn));
            }
        }

        private static void AppendDashboard(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            string scrimMotion = state.PanelClosing ? "fade-out" : "fade";
            string scrimDuration = state.PanelClosing
                ? GameplayHtmlState.PanelCloseSeconds.ToString("0.00", CultureInfo.InvariantCulture)
                : "0.12";
            html.Append("<view id=\"kingdom-scrim\" className=\"scrim\" data-motion=\"").Append(scrimMotion)
                .Append("\" data-motion-duration=\"").Append(scrimDuration)
                .Append("\"><view id=\"kingdom-dashboard\" className=\"dashboard\" data-motion=\"scale\" data-motion-duration=\"0.18\" data-motion-ease=\"out-back\">");
            PanelHeaderContent(html, state.T("KINGDOM"), snapshot.KingdomName, true, state);
            html.Append("<view className=\"tabs\">");
            Tab(html, state, KingdomDashboardTab.Overview, state.T("OVERVIEW"), "ShowOverview");
            Tab(html, state, KingdomDashboardTab.Resources, state.T("RESOURCES"), "ShowResources");
            Tab(html, state, KingdomDashboardTab.Storage, state.T("STORAGE"), "ShowStorage");
            Tab(html, state, KingdomDashboardTab.Buildings, state.T("BUILDINGS"), "ShowBuildings");
            Tab(html, state, KingdomDashboardTab.Units, state.T("UNITS"), "ShowUnits");
            if (snapshot.TurnUiEnabled)
                Tab(html, state, KingdomDashboardTab.Turns, state.T("TURNS"), "ShowTurns");
            Tab(html, state, KingdomDashboardTab.Logistics, state.T("LOGISTICS"), "ShowLogistics");
            html.Append("</view><view className=\"dashboard-body\"><scroll className=\"dashboard-scroll\">");
            switch (state.DashboardTab)
            {
                case KingdomDashboardTab.Resources:
                    AppendResources(html, snapshot, state);
                    break;
                case KingdomDashboardTab.Storage:
                    AppendStorage(html, snapshot, state);
                    break;
                case KingdomDashboardTab.Buildings:
                    AppendGroups(html, state.T("BUILDING PORTFOLIO"), snapshot.BuildingGroups, snapshot, false, state);
                    break;
                case KingdomDashboardTab.Units:
                    AppendGroups(html, state.T("UNIT ROSTER"), snapshot.UnitGroups, snapshot, true, state);
                    break;
                case KingdomDashboardTab.Turns:
                    if (snapshot.TurnUiEnabled)
                        AppendTurns(html, snapshot, state);
                    else
                        AppendOverview(html, snapshot, state);
                    break;
                case KingdomDashboardTab.Logistics:
                    AppendLogistics(html, snapshot);
                    break;
                default:
                    AppendOverview(html, snapshot, state);
                    break;
            }
            html.Append("</scroll></view></view></view>");
        }

        private static void AppendOverview(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<view className=\"stats-grid\">");
            Stat(html, snapshot.SettlementCount, state.T("SETTLEMENTS"));
            Stat(html, snapshot.Population, state.T("POPULATION"));
            Stat(html, snapshot.BuildingCount, state.T("BUILDINGS"));
            Stat(html, snapshot.UnitCount, state.T("UNITS"));
            if (snapshot.TurnUiEnabled)
            {
                Stat(html, snapshot.Round, state.T("CURRENT ROUND"));
                long localTurns = 0;
                for (int index = 0; index < snapshot.TurnHistory.Length; index++)
                {
                    if (snapshot.TurnHistory[index].Local)
                        localTurns = snapshot.TurnHistory[index].Completed;
                }
                Stat(html, localTurns, state.T("COMPLETED TURNS"));
            }
            html.Append("</view>");
            AppendResources(html, snapshot, state);
        }

        private static void AppendResources(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<text className=\"section-title\">").Append(state.T("KINGDOM TOTALS")).Append("</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.Resources.Length; index++)
            {
                GameplayResourceSnapshot resource = snapshot.Resources[index];
                string key = GameplayHtmlIconKeys.Resource(resource.Id);
                DataRow(html, state.T(DisplayResource(resource.Id)), Amount(resource.Amount), state.T("Total across your settlements"), snapshot.Icons.ContainsKey(key) ? key : null);
            }
            if (snapshot.Resources.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("No resource records are available.")).Append("</text>");
            html.Append("</view>");
            if (snapshot.Settlements.Length > 0)
            {
                html.Append("<text className=\"section-title\">").Append(state.T("BY SETTLEMENT")).Append("</text><view className=\"building-list\">");
                for (int settlementIndex = 0; settlementIndex < snapshot.Settlements.Length; settlementIndex++)
                {
                    GameplaySettlementViewSnapshot settlement = snapshot.Settlements[settlementIndex];
                    string totals = ResourceSummary(settlement.Resources, state);
                    DataRow(
                        html,
                        settlement.Name,
                        string.IsNullOrWhiteSpace(totals) ? state.T("No stored resources") : totals,
                        state.TF("Population {0} / Buildings {1}", settlement.Population, settlement.BuildingCount));
                }
                html.Append("</view>");
            }
        }

        private static void AppendStorage(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<text className=\"section-title\">").Append(state.T("WAREHOUSES")).Append("</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.Warehouses.Length; index++)
            {
                GameplayWarehouseViewSnapshot warehouse = snapshot.Warehouses[index];
                string capacity = warehouse.Capacity < 0
                    ? $"{Amount(warehouse.Used)} / {state.T("unlimited")}"
                    : $"{Amount(warehouse.Used)} / {warehouse.Capacity}";
                html.Append("<button className=\"storage-row\" onClick=\"Globals.gameplay.FocusWarehouse(")
                    .Append(warehouse.Position.x).Append(',').Append(warehouse.Position.y).Append(",'")
                    .Append(J(warehouse.BuildingId)).Append("')\">");
                string iconKey = GameplayHtmlIconKeys.Building(warehouse.BuildingId);
                if (snapshot.Icons.ContainsKey(iconKey)) RowIcon(html, true, iconKey, string.Empty, false);
                html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                    .Append(E(state.T(string.IsNullOrWhiteSpace(warehouse.BuildingId) ? "Warehouse" : Display(warehouse.BuildingId))))
                    .Append("</text><text className=\"item-meta\">").Append(E(warehouse.Settlement))
                    .Append(" - ").Append(state.T("grid")).Append(' ').Append(warehouse.Position.x).Append(", ").Append(warehouse.Position.y)
                    .Append(" - ").Append(E(ResourceSummary(warehouse.Resources, state)))
                    .Append("</text></view><text className=\"item-tag\">").Append(E(capacity)).Append("</text></button>");
            }
            if (snapshot.Warehouses.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("No warehouses belong to this kingdom yet.")).Append("</text>");
            html.Append("</view>");
        }

        private static void AppendGroups(StringBuilder html, string title, GameplayGroupSnapshot[] groups, GameplayHtmlSnapshot snapshot, bool units, GameplayHtmlState state)
        {
            html.Append("<text className=\"section-title\">").Append(E(title)).Append("</text><view className=\"building-list\">");
            for (int index = 0; index < groups.Length; index++)
            {
                string key = units ? GameplayHtmlIconKeys.Unit(groups[index].Id) : GameplayHtmlIconKeys.Building(groups[index].Id);
                DataRow(html, state.T(groups[index].Label), groups[index].Count.ToString(CultureInfo.InvariantCulture), state.T(groups[index].Context),
                    snapshot.Icons.ContainsKey(key) ? key : null);
            }
            if (groups.Length == 0)
                html.Append("<text className=\"empty\">").Append(state.T("Nothing to display yet.")).Append("</text>");
            html.Append("</view>");
        }

        private static void AppendTurns(StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            html.Append("<text className=\"section-title\">").Append(state.T("PARTICIPANTS")).Append("</text><view className=\"building-list\">");
            for (int index = 0; index < snapshot.TurnHistory.Length; index++)
            {
                GameplayTurnHistoryViewSnapshot turn = snapshot.TurnHistory[index];
                string status = turn.Eliminated ? state.T("ELIMINATED") : turn.Active ? state.T("ACTIVE") : state.T("WAITING");
                string context = turn.Local ? state.T("Local kingdom") : state.T("Opponent");
                DataRow(html, Display(turn.OwnerId), $"{turn.Completed} {state.TN("turn", "turns", (int)turn.Completed)} - {status}", context);
            }
            html.Append("</view>");
        }

        private static void AppendPause(StringBuilder html, GameplayHtmlState state)
        {
            html.Append("<view id=\"pause-scrim\" className=\"scrim\" data-motion=\"fade\" data-motion-duration=\"0.12\"><view id=\"pause-modal\" className=\"modal\" data-motion=\"scale\" data-motion-duration=\"0.16\" data-motion-ease=\"out-back\"><text className=\"eyebrow\">").Append(state.T("GAME PAUSED")).Append("</text><text className=\"modal-title\">Moyva</text><text className=\"modal-copy\">").Append(state.T("Return to the realm or leave this session.")).Append("</text><view className=\"row gap\">")
                .Append(Button(state.T("RESUME"), "Globals.gameplay.Resume()", "button positive", state.T("Resume game")))
                .Append(Button(state.T("EXIT TO MENU"), "Globals.gameplay.ExitToMenu()", "button danger", state.T("Exit to main menu")))
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
                ? state.T("The realm is silent")
                : victory
                    ? state.T("Victory")
                    : state.T("Defeat");
            string copy = string.IsNullOrWhiteSpace(state.WinnerId)
                ? state.T("No kingdom remains able to rule.")
                : victory
                    ? state.T("Your kingdom is the last realm with a standing center.")
                    : state.TF("{0} controls the last standing center.", Display(state.WinnerId));
            html.Append("<view id=\"gameover-scrim\" className=\"scrim\" data-motion=\"fade\" data-motion-duration=\"0.14\"><view id=\"gameover-modal\" className=\"modal result-modal\" data-motion=\"scale\" data-motion-duration=\"0.18\" data-motion-ease=\"out-back\"><text className=\"eyebrow\">").Append(state.T("MATCH ENDED")).Append("</text><text className=\"modal-title ")
                .Append(victory ? "status-good" : "status-bad")
                .Append("\">").Append(E(title)).Append("</text><text className=\"modal-copy\">")
                .Append(E(copy)).Append("</text><view className=\"result-summary\">");
            DataRow(html, state.T("Winner"), string.IsNullOrWhiteSpace(state.WinnerId) ? state.T("None") : Display(state.WinnerId), state.T("Final kingdom"));
            DataRow(html, state.T("Rounds"), snapshot.Round.ToString(CultureInfo.InvariantCulture), state.T("Campaign length"));
            DataRow(html, state.T("Buildings"), snapshot.BuildingCount.ToString(CultureInfo.InvariantCulture), state.T("Your kingdom total"));
            DataRow(html, state.T("Units"), snapshot.UnitCount.ToString(CultureInfo.InvariantCulture), state.T("Your active roster"));
            html.Append("</view><view className=\"row gap\">")
                .Append(Button(state.T("EXIT TO MENU"), "Globals.gameplay.ExitToMenu()", "button positive", state.T("Exit to main menu")))
                .Append("</view></view></view>");
        }

        private static void PanelHeader(StringBuilder html, GameplayHtmlState state, string eyebrow, string title, bool close)
        {
            // While the panel is closing the node stays mounted but swaps its
            // entry motion for the exit tween; reconciliation then unmounts it
            // after the close window expires.
            string motion = state.PanelClosing ? "fade-out" : "slide-left";
            string duration = state.PanelClosing
                ? GameplayHtmlState.PanelCloseSeconds.ToString("0.00", CultureInfo.InvariantCulture)
                : "0.16";
            html.Append("<view id=\"gameplay-side-panel\" className=\"side-panel\" data-motion=\"")
                .Append(motion).Append("\" data-motion-duration=\"").Append(duration).Append("\">");
            PanelHeaderContent(html, eyebrow, title, close, null);
        }

        private static void PanelHeaderContent(StringBuilder html, string eyebrow, string title, bool close, GameplayHtmlState state)
        {
            html.Append("<view className=\"panel-header\"><view className=\"panel-copy\"><text className=\"eyebrow\">")
                .Append(E(eyebrow)).Append("</text><text className=\"panel-title\">").Append(E(title)).Append("</text></view>");
            if (close)
                html.Append(Button("X", "Globals.gameplay.ClosePanel()", "button", state?.T("Close panel") ?? "Close panel"));
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

            if (!string.IsNullOrWhiteSpace(state.ConstructionProducerResource))
            {
                bool produces = false;
                if (option.ProducedResourceIds != null)
                {
                    for (int i = 0; i < option.ProducedResourceIds.Length; i++)
                    {
                        if (string.Equals(
                                option.ProducedResourceIds[i],
                                state.ConstructionProducerResource,
                                StringComparison.Ordinal))
                        {
                            produces = true;
                            break;
                        }
                    }
                }
                if (!produces)
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
            int lastExclusive,
            GameplayHtmlState state)
        {
            string range = total == 0
                ? state.T("No results")
                : state.TF("Showing {0}-{1} of {2}", firstIndex + 1, lastExclusive, total);
            html.Append("<view className=\"list-footer\"><text className=\"list-count\">")
                .Append(E(range)).Append("</text><view className=\"row gap\">")
                .Append(Button(state.T("PREV"), "Globals.gameplay.PreviousConstructionPage()", "button compact", state.T("Previous page"), pageIndex <= 0))
                .Append("<text className=\"page-indicator\">")
                .Append(pageIndex + 1).Append('/').Append(pageCount)
                .Append("</text>")
                .Append(Button(state.T("NEXT"), "Globals.gameplay.NextConstructionPage()", "button compact", state.T("Next page"), pageIndex >= pageCount - 1))
                .Append("</view></view>");
        }

        private static string Button(string label, string action, string classes, string tooltip, bool disabled = false)
        {
            return $"<button className=\"{classes}\" data-tooltip=\"{E(tooltip)}\" {(disabled ? "disabled=\"true\"" : string.Empty)} onClick=\"{action}\"><text className=\"button-label\">{E(label)}</text></button>";
        }

        private static string Amount(float value) => value.ToString("0.#", CultureInfo.InvariantCulture);

        private static string ResourceSummary(GameplayResourceSnapshot[] resources, GameplayHtmlState state)
        {
            if (resources == null || resources.Length == 0)
                return string.Empty;
            var summary = new StringBuilder();
            for (int index = 0; index < resources.Length; index++)
            {
                if (index > 0)
                    summary.Append(" / ");
                summary.Append(state.T(DisplayResource(resources[index].Id))).Append(' ').Append(Amount(resources[index].Amount));
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
