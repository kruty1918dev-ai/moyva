using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static partial class GameplayHtmlMarkup
    {
        private const string TopBarRegion = "gameplay-topbar";
        private const string ContextRegion = "gameplay-workspace";
        private const string CommandsRegion = "gameplay-command-bar";
        private const string FeedbackRegion = "gameplay-feedback-region";
        private const string OverlayRegion = "gameplay-overlay-region";

        public static IReadOnlyDictionary<string, string> BuildRegions(
            GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            var regions = new Dictionary<string, string>(5);
            var html = new StringBuilder(4096);
            AppendTopBar(html, snapshot, state);
            regions[TopBarRegion] = html.ToString();
            html.Clear();
            AppendContextPanel(html, snapshot, state);
            regions[ContextRegion] = html.ToString();
            html.Clear();
            AppendCommandBar(html, snapshot, state);
            regions[CommandsRegion] = html.ToString();
            html.Clear();
            if (!string.IsNullOrWhiteSpace(state.Feedback) && !snapshot.RequiresFirstCastle)
                html.Append("<text id=\"gameplay-toast\" className=\"toast\" data-motion-role=\"toast\">")
                    .Append(E(state.Feedback)).Append("</text>");
            html.Append("<view className=\"control-prompts\" style=\"position:absolute;bottom:82px;left:25%;width:50%;height:26px;align-items:center;\"><text style=\"font-size:12px;color:#e9eee7;\">")
                .Append(E(state.ControlHints)).Append("</text></view>");
            if (state.GamepadAim)
                html.Append("<view style=\"position:absolute;left:50%;top:50%;width:12px;height:12px;\"><text style=\"color:#ffffff;font-size:18px;\">+</text></view>");
            regions[FeedbackRegion] = html.ToString();
            html.Clear();
            if (state.OpenPanelId == GameplayHtmlPanel.Kingdom)
            {
                AppendDashboard(html, snapshot, state);
                AppendRegionsPanel(html, snapshot, state);
            }
            if (state.Guidance != null && state.Guidance.Open)
                AppendGuidance(html, snapshot, state);
            if (state.IsGameOver)
                AppendGameOver(html, snapshot, state);
            else if (state.IsPaused)
                AppendPause(html, state);
            regions[OverlayRegion] = html.ToString();
            return regions;
        }

        public static string BuildDocument(IReadOnlyDictionary<string, string> regions, string viewportClass)
        {
            var html = new StringBuilder(16384);
            html.Append("<view className=\"gameplay-ui ").Append(E(viewportClass)).Append("\">");
            Region(html, regions, TopBarRegion, "topbar", "edge-top");
            Region(html, regions, ContextRegion, "workspace");
            Region(html, regions, CommandsRegion, "command-bar", "edge-bottom");
            Region(html, regions, FeedbackRegion, "presentation-region");
            Region(html, regions, OverlayRegion, "presentation-region");
            return html.Append("</view>").ToString();
        }

        // Region rail pinned to the left edge while the Kingdom panel is open.
        // Read-only navigator: rows are focusable but carry no onClick, so a
        // selection change can never transfer ownership or mutate gameplay.
        private static void AppendRegionsPanel(
            StringBuilder html,
            GameplayHtmlSnapshot snapshot,
            GameplayHtmlState state)
        {
            GameplaySettlementViewSnapshot[] settlements = snapshot?.Settlements;
            int count = settlements?.Length ?? 0;
            html.Append("<view id=\"kingdom-regions-layer\" style=\"position:absolute;left:0;top:0;right:0;bottom:0;pointer-events:none;display:flex;flex-direction:row;align-items:flex-start;justify-content:flex-start;padding:96px 0 0 14px;\">")
                .Append("<view id=\"kingdom-regions\" style=\"pointer-events:auto;display:flex;flex-direction:column;width:272px;max-height:72%;min-height:0;padding:13px;background-color:rgba(10,12,17,0.97);border-width:1px;border-color:rgba(218,177,76,0.6);border-radius:3px;\">")
                .Append("<view className=\"panel-header\"><view className=\"panel-copy\"><text className=\"eyebrow\">")
                .Append(E(state.T("REGIONS"))).Append("</text><text className=\"item-meta\">")
                .Append(E(state.TF("{0} settlement(s)", count))).Append("</text></view></view>")
                .Append("<scroll className=\"panel-scroll\"><view className=\"building-list\">");
            if (count == 0)
            {
                html.Append("<text className=\"empty\">")
                    .Append(E(state.T("No regions yet. Found a settlement to claim one.")))
                    .Append("</text>");
            }
            else
            {
                string active = state.T("Active");
                string kingdom = snapshot.KingdomName ?? string.Empty;
                for (int index = 0; index < count; index++)
                {
                    GameplaySettlementViewSnapshot settlement = settlements[index];
                    string owner = string.Concat(state.T("Owner"), ": ", kingdom, " · ", active);
                    string stats = string.Concat(
                        state.T("Population"), " ", settlement.Population.ToString(CultureInfo.InvariantCulture),
                        " · ", state.T("Buildings"), " ", settlement.BuildingCount.ToString(CultureInfo.InvariantCulture));
                    html.Append("<button data-key=\"region-").Append(E(settlement.Id))
                        .Append("\" className=\"storage-row region-row\" data-tooltip=\"")
                        .Append(E(state.TF("{0} — {1}", settlement.Name, owner))).Append('"');
                    if (index == 0)
                        html.Append(" data-autofocus=\"true\"");
                    html.Append("><view className=\"item-copy\"><text className=\"item-title\">")
                        .Append(E(settlement.Name)).Append("</text><text className=\"item-meta\">")
                        .Append(E(owner)).Append("</text><text className=\"item-meta\">")
                        .Append(E(stats)).Append("</text></view><text className=\"item-tag\">")
                        .Append(E(active.ToUpperInvariant())).Append("</text></button>");
                }
            }
            html.Append("</view></scroll></view></view>");
        }

        private static void Region(StringBuilder html, IReadOnlyDictionary<string, string> regions,
            string id, string css, string motionRole = null)
        {
            html.Append("<view id=\"").Append(id).Append("\" className=\"").Append(css).Append('"');
            if (motionRole != null)
                html.Append(" data-motion-role=\"").Append(motionRole).Append('"');
            html.Append('>').Append(regions[id]).Append("</view>");
        }
    }
}
