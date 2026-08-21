using Kruty1918.Moyva.BotAI.Diagnostics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsSettingsTests
    {
        [Test]
        public void Normalize_EnforcesSafeLowerBounds()
        {
            var settings = new BotDiagnosticsSettings
            {
                BufferCapacity = 1,
                HeartbeatSeconds = 0,
                HealthProbeSeconds = 0,
                SnapshotProbeSeconds = 0,
            };

            settings.Normalize();

            Assert.That(settings.BufferCapacity, Is.GreaterThanOrEqualTo(128));
            Assert.That(settings.HeartbeatSeconds, Is.GreaterThanOrEqualTo(0.25));
            Assert.That(settings.HealthProbeSeconds, Is.GreaterThanOrEqualTo(0.25));
            Assert.That(settings.SnapshotProbeSeconds, Is.GreaterThanOrEqualTo(0.25));
        }
    }
}
