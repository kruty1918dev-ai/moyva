using NUnit.Framework;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerMarkdownExporterTests
    {
        [Test]
        public void Build_IncludesCurrentStrategyResourcesAndTimeline()
        {
            var settings = BotAnalyzerSettings.CreateDefault();
            var session = new BotAnalyzerSession(settings);
            var frame = new BotAnalyzerFrame
            {
                OwnerId = "bot_0",
                GlobalTurn = 8,
                Round = 2,
                Phase = "AwaitingInput",
                Strategy = new BotAnalyzerStrategyState
                {
                    Available = true,
                    Posture = "EmergencyDefense",
                    Score = 1200,
                    Reason = "Visible hostile threatens Castle.",
                },
            };
            frame.Resources.Add(new BotAnalyzerResourceState
            {
                ResourceId = "wood",
                DisplayName = "Wood",
                Amount = 250,
            });
            session.Append(frame, new[]
            {
                new BotAnalyzerEvent
                {
                    Type = BotAnalyzerEventType.StrategyChanged,
                    Title = "Strategy changed",
                    Detail = "Search → EmergencyDefense",
                    GlobalTurn = 8,
                },
            });

            string markdown = BotAnalyzerMarkdownExporter.Build(session, settings);

            Assert.That(markdown, Does.Contain("# Moyva Bot Analyzer Report"));
            Assert.That(markdown, Does.Contain("EmergencyDefense"));
            Assert.That(markdown, Does.Contain("Wood"));
            Assert.That(markdown, Does.Contain("Decision & Action Timeline"));
        }
    }
}
