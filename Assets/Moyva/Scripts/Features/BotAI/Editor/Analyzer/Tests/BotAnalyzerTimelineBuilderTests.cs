using NUnit.Framework;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerTimelineBuilderTests
    {
        [Test]
        public void Filters_MapSemanticEventsWithoutInventingContent()
        {
            var decision = new BotAnalyzerEvent
            {
                Type = BotAnalyzerEventType.StrategyChanged,
                Title = "Strategy changed",
                Detail = "Search → Pressure",
                GlobalTurn = 3,
            };
            var move = new BotAnalyzerEvent
            {
                Type = BotAnalyzerEventType.UnitMoved,
                Severity = BotAnalyzerEventSeverity.Action,
                Title = "Unit moved",
                GlobalTurn = 3,
            };

            Assert.That(BotAnalyzerTimelineBuilder.MatchesFilter(decision, BotAnalyzerTimelineFilter.Decision), Is.True);
            Assert.That(BotAnalyzerTimelineBuilder.MatchesFilter(decision, BotAnalyzerTimelineFilter.Movement), Is.False);
            Assert.That(BotAnalyzerTimelineBuilder.MatchesFilter(move, BotAnalyzerTimelineFilter.Movement), Is.True);
            Assert.That(BotAnalyzerTimelineBuilder.FormatDetailed(decision), Does.Contain("Search → Pressure"));
        }
    }
}
