using NUnit.Framework;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerSessionTests
    {
        [Test]
        public void Session_BoundsHistoryAndSupportsFilteredSearch()
        {
            var settings = BotAnalyzerSettings.CreateDefault();
            settings.MaxFrames = 50;
            settings.MaxTimelineEvents = 100;
            var session = new BotAnalyzerSession(settings);

            for (int i = 0; i < 60; i++)
            {
                session.Append(
                    new BotAnalyzerFrame { OwnerId = "bot", GlobalTurn = i + 1 },
                    new[]
                    {
                        new BotAnalyzerEvent
                        {
                            Type = i % 2 == 0 ? BotAnalyzerEventType.UnitMoved : BotAnalyzerEventType.ResourceChanged,
                            Severity = BotAnalyzerEventSeverity.Action,
                            Title = i % 2 == 0 ? $"move unit_{i}" : $"wood change {i}",
                            GlobalTurn = i + 1,
                        },
                        new BotAnalyzerEvent
                        {
                            Type = BotAnalyzerEventType.System,
                            Title = "tick",
                            GlobalTurn = i + 1,
                        },
                    });
            }

            Assert.That(session.Frames.Count, Is.EqualTo(50));
            Assert.That(session.Events.Count, Is.EqualTo(100));

            var movement = session.GetFilteredEvents(
                BotAnalyzerTimelineFilter.Movement,
                "unit_",
                100,
                newestFirst: false);

            Assert.That(movement.Count, Is.GreaterThan(0));
            Assert.That(movement.TrueForAll(x => x.Type == BotAnalyzerEventType.UnitMoved), Is.True);
        }
    }
}
