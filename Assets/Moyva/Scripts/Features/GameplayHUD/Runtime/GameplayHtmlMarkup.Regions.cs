using System.Collections.Generic;
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
                AppendDashboard(html, snapshot, state);
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
