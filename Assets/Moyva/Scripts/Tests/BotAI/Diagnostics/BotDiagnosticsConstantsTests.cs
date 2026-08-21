using Kruty1918.Moyva.BotAI.Diagnostics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsConstantsTests
    {
        [Test]
        public void Prefix_IsStableAndConsoleSearchFriendly()
        {
            Assert.That(BotDiagnosticsConstants.Prefix, Is.EqualTo("[MOYVA-BOT]"));
            Assert.That(BotDiagnosticsConstants.DefaultBufferCapacity, Is.GreaterThanOrEqualTo(1024));
        }
    }
}
