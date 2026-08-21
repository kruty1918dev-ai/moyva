using Kruty1918.Moyva.BotAI.Diagnostics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Diagnostics
{
    public sealed class BotDiagnosticEventTests
    {
        [Test]
        public void Constructor_NormalizesOptionalStrings()
        {
            var e = new BotDiagnosticEvent(
                1,
                2.5,
                "2026-08-21T10:00:00Z",
                BotDiagnosticLevel.Info,
                BotDiagnosticCategory.Turn,
                " TURN.ACTIVE ",
                " bot-01 ",
                7,
                " AwaitingInput ",
                " message ",
                " reason ",
                " details ");

            Assert.That(e.Code, Is.EqualTo("TURN.ACTIVE"));
            Assert.That(e.OwnerId, Is.EqualTo("bot-01"));
            Assert.That(e.Phase, Is.EqualTo("AwaitingInput"));
            Assert.That(e.Message, Is.EqualTo("message"));
            Assert.That(e.Reason, Is.EqualTo("reason"));
            Assert.That(e.Details, Is.EqualTo("details"));
        }
    }
}
