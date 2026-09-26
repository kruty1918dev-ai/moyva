using System.Globalization;
using System.Text;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>Blocked-action guidance popup: renders the guidance snapshot
    /// the read model rebuilds from canonical queries — the goal, every
    /// current blocker with Need/Have/Missing, per-blocker resolution
    /// actions, blocker navigation and the return-to-goal CTA.</summary>
    internal static partial class GameplayHtmlMarkup
    {
        private static void AppendGuidance(
            StringBuilder html, GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            GameplayGuidanceViewSnapshot guidance = snapshot.Guidance;
            if (guidance == null)
                return;

            string goalLabel = state.T(guidance.GoalLabel);
            string title = guidance.GoalKind == GuidanceGoalKind.Recruitment
                ? state.TF("Cannot recruit {0}", goalLabel)
                : guidance.PlacementCount > 1
                    ? state.TF("Cannot build — {0} placements", guidance.PlacementCount)
                    : state.TF("Cannot build {0}", goalLabel);

            html.Append("<view id=\"guidance-scrim\" className=\"scrim\">");
            html.Append("<view id=\"guidance-dialog\" className=\"dashboard guidance-dialog\" data-motion-role=\"dialog\">");
            html.Append("<view className=\"panel-header\"><view className=\"panel-copy\"><text className=\"eyebrow\">")
                .Append(E(state.T("BLOCKED ACTION"))).Append("</text><text className=\"panel-title\">")
                .Append(E(title)).Append("</text><text className=\"item-meta\">")
                .Append(E(guidance.PendingBlockers > 0
                    ? state.TF("{0} blocker(s) remain", guidance.PendingBlockers)
                    : state.T("All prerequisites are met.")))
                .Append("</text></view>")
                .Append(Button("X", "Globals.gameplay.GuidanceClose()",
                    "button", state.T("Hide this helper (your goal stays saved)")))
                .Append("</view>");

            if (guidance.AllResolved)
            {
                html.Append("<view className=\"guidance-banner\"><text className=\"item-title\">")
                    .Append(E(state.T("Everything needed is now available.")))
                    .Append("</text></view>");
            }

            html.Append("<scroll className=\"panel-scroll guidance-scroll\"><view className=\"building-list\">");
            if (guidance.Blockers.Length == 0)
            {
                html.Append("<text className=\"empty\">")
                    .Append(E(state.T("No blockers remain — you can resume the action.")))
                    .Append("</text>");
            }
            for (int index = 0; index < guidance.Blockers.Length; index++)
            {
                var blocker = guidance.Blockers[index];
                AppendGuidanceBlocker(html, blocker, index, guidance.FocusIndex, state);
            }
            html.Append("</view></scroll>");

            html.Append("<view className=\"list-footer\"><view className=\"row gap\">")
                .Append(Button(state.T("PREV"),
                    $"Globals.gameplay.GuidanceMove(-1,{guidance.TotalBlockers})",
                    "button compact", state.T("Previous blocker"),
                    guidance.FocusIndex <= 0 || guidance.TotalBlockers <= 0))
                .Append("<text className=\"page-indicator\">")
                .Append(guidance.TotalBlockers == 0 || guidance.FocusIndex < 0
                    ? "—" : $"{guidance.FocusIndex + 1}/{guidance.TotalBlockers}")
                .Append("</text>")
                .Append(Button(state.T("NEXT"),
                    $"Globals.gameplay.GuidanceMove(1,{guidance.TotalBlockers})",
                    "button compact", state.T("Next blocker"),
                    guidance.FocusIndex >= guidance.TotalBlockers - 1))
                .Append("</view>")
                .Append(Button(state.T("BACK TO GOAL"), "Globals.gameplay.GuidanceResumeGoal()",
                    guidance.AllResolved ? "button positive" : "button",
                    state.T("Return to what you were doing"),
                    guidance.AllResolved ? false : false))
                .Append(Button(state.T("FORGET"), "Globals.gameplay.GuidanceDismissGoal()",
                    "button", state.T("Drop this goal entirely")))
                .Append("</view></view></view>");
        }

        private static void AppendGuidanceBlocker(
            StringBuilder html, GameplayGuidanceBlockerSnapshot blocker,
            int index, int focusIndex, GameplayHtmlState state)
        {
            html.Append("<view className=\"guidance-blocker")
                .Append(blocker.Resolved ? " resolved" : string.Empty)
                .Append(index == focusIndex ? " focused" : string.Empty)
                .Append("\"><button className=\"guidance-blocker-head\" onClick=\"Globals.gameplay.GuidanceSelect(")
                .Append(index.ToString(CultureInfo.InvariantCulture)).Append(")\">");

            bool resourceLike = blocker.Kind == GuidanceBlockerKind.Resource
                || blocker.Kind == GuidanceBlockerKind.Population;
            if (resourceLike && !string.IsNullOrWhiteSpace(blocker.ResourceId))
                RowIcon(html, true, blocker.IconGlobalKey,
                    Display(blocker.ResourceId), true);
            else
                html.Append("<text className=\"row-icon small\">!")
                    .Append("</text>");

            html.Append("<view className=\"item-copy\"><text className=\"item-title\">")
                .Append(E(state.T(blocker.Title))).Append("</text>");
            if (resourceLike)
            {
                html.Append("<text className=\"item-meta\">")
                    .Append(E(state.TF(
                        "Need {0} · Have {1} · Missing {2}",
                        Amount(blocker.Required),
                        Amount(blocker.Available),
                        Amount(blocker.Missing))))
                    .Append(blocker.Reserved > 0.0001f && blocker.Kind == GuidanceBlockerKind.Resource
                        ? E(state.TF(" ({0} reserved)", Amount(blocker.Reserved)))
                        : string.Empty)
                    .Append("</text>");
            }
            if (!string.IsNullOrWhiteSpace(blocker.Detail))
                html.Append("<text className=\"item-meta\">")
                    .Append(E(state.T(blocker.Detail))).Append("</text>");
            html.Append("</view>")
                .Append(blocker.Resolved
                    ? "<text className=\"row-chevron\">✓</text>"
                    : "<text className=\"row-chevron\">!</text>")
                .Append("</button>");

            // Options: actionable buttons for real fixes; an Unobtainable row
            // renders as honest muted text.
            if (blocker.Options.Length > 0 && index == focusIndex)
            {
                html.Append("<view className=\"guidance-options\">");
                for (int o = 0; o < blocker.Options.Length; o++)
                    AppendGuidanceOption(html, blocker.Options[o], state);
                html.Append("</view>");
            }
            html.Append("</view>");
        }

        private static void AppendGuidanceOption(
            StringBuilder html, GameplayGuidanceOptionSnapshot option, GameplayHtmlState state)
        {
            switch (option.Kind)
            {
                case GuidanceOptionKind.BuildProducer:
                case GuidanceOptionKind.BuildHousing:
                case GuidanceOptionKind.ProduceFood:
                    html.Append(Button(
                        state.TF("Build {0}", state.T(Display(option.BuildingId))),
                        $"Globals.gameplay.GuidanceBuildProducer('{J(option.BuildingId)}')",
                        "button small primary",
                        string.IsNullOrWhiteSpace(option.Detail)
                            ? state.T("Open construction focused on this building")
                            : state.T(option.Detail)));
                    break;
                case GuidanceOptionKind.FocusProducer:
                    html.Append(Button(
                        state.TF("Open {0}", state.T(Display(option.BuildingId))),
                        $"Globals.gameplay.GuidanceFocusBuilding({option.Position.x},{option.Position.y},'{J(option.BuildingId)}')",
                        "button small",
                        string.IsNullOrWhiteSpace(option.Detail)
                            ? state.T("Focus this building on the map")
                            : state.T(option.Detail),
                        !option.HasPosition));
                    break;
                case GuidanceOptionKind.ProducerConstructing:
                    html.Append(Button(
                        state.TF("See {0}", state.T(Display(option.BuildingId))),
                        $"Globals.gameplay.GuidanceFocusBuilding({option.Position.x},{option.Position.y},'{J(option.BuildingId)}')",
                        "button small",
                        string.IsNullOrWhiteSpace(option.Detail)
                            ? state.T("This producer is still under construction")
                            : state.T(option.Detail),
                        !option.HasPosition));
                    break;
                case GuidanceOptionKind.OpenSupply:
                    html.Append(Button(
                        state.T("Supply by wagon"),
                        $"Globals.gameplay.GuidanceSupply({option.Position.x},{option.Position.y},'')",
                        "button small",
                        string.IsNullOrWhiteSpace(option.Detail)
                            ? state.T("Send this resource to the funding settlement")
                            : state.T(option.Detail),
                        !option.HasPosition));
                    break;
                case GuidanceOptionKind.OpenQueue:
                    html.Append(Button(
                        state.T("Review queue"),
                        "Globals.gameplay.GuidanceOpenQueue()",
                        "button small",
                        string.IsNullOrWhiteSpace(option.Detail)
                            ? state.T("Queued training reserves resources — cancel entries to free them")
                            : state.T(option.Detail)));
                    break;
                default:
                    html.Append("<text className=\"muted\">")
                        .Append(E(string.IsNullOrWhiteSpace(option.Detail)
                            ? state.T("No way to obtain this right now.")
                            : state.T(option.Detail)))
                        .Append("</text>");
                    break;
            }
        }
    }
}
