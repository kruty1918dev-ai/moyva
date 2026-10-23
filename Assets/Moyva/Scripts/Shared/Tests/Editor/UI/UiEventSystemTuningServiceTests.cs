using NUnit.Framework;

namespace Kruty1918.Moyva.Shared.UI.Tests
{
    public sealed class UiEventSystemTuningServiceTests
    {
        [Test]
        public void ResolveDragThresholdPixels_NeverBelowDesktopDefault()
        {
            Assert.That(
                UiEventSystemTuningService.ResolveDragThresholdPixels(96f),
                Is.EqualTo(UiEventSystemTuningService.MinDragThresholdPixels));
        }

        [Test]
        public void ResolveDragThresholdPixels_ScalesWithPhoneDpi()
        {
            Assert.That(UiEventSystemTuningService.ResolveDragThresholdPixels(460f), Is.EqualTo(46));
            Assert.That(UiEventSystemTuningService.ResolveDragThresholdPixels(300f), Is.EqualTo(30));
        }

        [Test]
        public void ResolveDragThresholdPixels_InvalidDpiFallsBack()
        {
            int expected = UiEventSystemTuningService.ResolveDragThresholdPixels(
                UiEventSystemTuningService.FallbackDpi);
            Assert.That(UiEventSystemTuningService.ResolveDragThresholdPixels(0f), Is.EqualTo(expected));
            Assert.That(UiEventSystemTuningService.ResolveDragThresholdPixels(-5f), Is.EqualTo(expected));
            Assert.That(UiEventSystemTuningService.ResolveDragThresholdPixels(float.NaN), Is.EqualTo(expected));
        }
    }
}
