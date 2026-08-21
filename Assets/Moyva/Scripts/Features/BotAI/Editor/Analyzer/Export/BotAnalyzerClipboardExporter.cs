using UnityEditor;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal static class BotAnalyzerClipboardExporter
    {
        public static string CopySummary(BotAnalyzerSession session)
        {
            string text = BotAnalyzerTimelineBuilder.BuildCurrentSummary(session?.LatestFrame);
            EditorGUIUtility.systemCopyBuffer = text ?? string.Empty;
            return text;
        }

        public static string CopyTimeline(BotAnalyzerSession session)
        {
            string text = BotAnalyzerTimelineBuilder.BuildTimelineText(session?.Events);
            EditorGUIUtility.systemCopyBuffer = text ?? string.Empty;
            return text;
        }

        public static string CopyFullReport(
            BotAnalyzerSession session,
            BotAnalyzerSettings settings)
        {
            string text = BotAnalyzerMarkdownExporter.Build(session, settings);
            EditorGUIUtility.systemCopyBuffer = text ?? string.Empty;
            return text;
        }

        public static string CopyJson(
            BotAnalyzerSession session,
            BotAnalyzerSettings settings)
        {
            string text = BotAnalyzerJsonExporter.BuildJson(session, settings);
            EditorGUIUtility.systemCopyBuffer = text ?? string.Empty;
            return text;
        }
    }
}
