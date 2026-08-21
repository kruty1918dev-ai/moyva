using Kruty1918.Moyva.BotAI.Diagnostics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsFormatterTests
    {
        [Test]
        public void Format_ContainsPrefixCodeContextReasonAndDetails()
        {
            var formatter = new BotDiagnosticsFormatter();
            var e = new BotDiagnosticEvent(
                12,
                2.5,
                "2026-08-21T10:00:00Z",
                BotDiagnosticLevel.Error,
                BotDiagnosticCategory.Executor,
                "EXECUTOR.START_FAILED",
                "bot-01",
                4,
                "AwaitingInput",
                "Bot executor did not start.",
                "missing planner",
                "bindings=0");

            string text = formatter.Format(e);

            Assert.That(text, Does.StartWith("[MOYVA-BOT]"));
            Assert.That(text, Does.Contain("[Error][Executor][EXECUTOR.START_FAILED]"));
            Assert.That(text, Does.Contain("[owner=bot-01]"));
            Assert.That(text, Does.Contain("[turn=4]"));
            Assert.That(text, Does.Contain("reason=missing planner"));
            Assert.That(text, Does.Contain("details=bindings=0"));
        }
    }
}
