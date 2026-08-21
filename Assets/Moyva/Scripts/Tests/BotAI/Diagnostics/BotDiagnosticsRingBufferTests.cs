using Kruty1918.Moyva.BotAI.Diagnostics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsRingBufferTests
    {
        [Test]
        public void Add_TrimsOldestAndCaptureSinceUsesSequence()
        {
            var settings = new BotDiagnosticsSettings { BufferCapacity = 128 };
            settings.Normalize();
            var buffer = new BotDiagnosticsRingBuffer(settings);

            for (int i = 1; i <= 140; i++)
            {
                buffer.Add(new BotDiagnosticEvent(
                    i,
                    i,
                    string.Empty,
                    BotDiagnosticLevel.Trace,
                    BotDiagnosticCategory.Health,
                    "X",
                    string.Empty,
                    0,
                    string.Empty,
                    "event",
                    string.Empty,
                    string.Empty));
            }

            Assert.That(buffer.Count, Is.EqualTo(128));
            Assert.That(buffer.Capture()[0].Sequence, Is.EqualTo(13));
            Assert.That(buffer.CaptureSince(135).Count, Is.EqualTo(5));
        }
    }
}
