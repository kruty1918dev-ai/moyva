using NUnit.Framework;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerJsonExporterTests
    {
        [Test]
        public void BuildJson_ContainsSchemaBotAndTimeline()
        {
            var settings = BotAnalyzerSettings.CreateDefault();
            var session = new BotAnalyzerSession(settings);
            var frame = new BotAnalyzerFrame
            {
                OwnerId = "bot_0",
                GlobalTurn = 11,
                Round = 4,
                Phase = "AwaitingInput",
                UtcTimestamp = "2026-08-21T08:00:00.0000000Z",
            };
            session.Append(frame, new[]
            {
                new BotAnalyzerEvent
                {
                    Type = BotAnalyzerEventType.GoalChanged,
                    Title = "Goal changed",
                    OwnerId = "bot_0",
                    GlobalTurn = 11,
                },
            });

            string json = BotAnalyzerJsonExporter.BuildJson(session, settings);

            Assert.That(json, Does.Contain(BotAnalyzerJsonExporter.Schema));
            Assert.That(json, Does.Contain("bot_0"));
            Assert.That(json, Does.Contain("GoalChanged"));
        }
    }
}
