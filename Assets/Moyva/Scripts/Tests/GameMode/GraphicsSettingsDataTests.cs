using Kruty1918.Moyva.Shared.Graphics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.GameMode
{
    public sealed class GraphicsSettingsDataTests
    {
        [TestCase(30)]
        [TestCase(45)]
        [TestCase(60)]
        [TestCase(90)]
        [TestCase(120)]
        [TestCase(144)]
        public void WithTargetFrameRate_PreservesSupportedPcValue(int frameRate)
        {
            GraphicsSettingsData settings = GraphicsSettingsData.CreateDefault()
                .WithTargetFrameRate(frameRate);

            Assert.That(settings.TargetFrameRate, Is.EqualTo(frameRate));
            Assert.That(settings.Profile, Is.EqualTo(GraphicsQualityProfile.Custom));
        }

        [Test]
        public void WithVSync_StoresActualPolicy()
        {
            GraphicsSettingsData settings = GraphicsSettingsData.CreateDefault()
                .WithVSync(true);

            Assert.That(settings.VSync, Is.True);
        }

        [TestCase(1, 30)]
        [TestCase(999, 360)]
        public void WithTargetFrameRate_ClampsUnsafeValues(int requested, int expected)
        {
            GraphicsSettingsData settings = GraphicsSettingsData.CreateDefault()
                .WithTargetFrameRate(requested);

            Assert.That(settings.TargetFrameRate, Is.EqualTo(expected));
        }
    }
}
