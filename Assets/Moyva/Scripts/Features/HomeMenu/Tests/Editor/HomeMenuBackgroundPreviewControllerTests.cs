using Kruty1918.Moyva.HomeMenu.UI;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class HomeMenuBackgroundPreviewControllerTests
    {
        [Test]
        public void CalculateCoverUv_WhenViewportIsWider_CropsTextureHeight()
        {
            Rect uv = HomeMenuBackgroundPreviewController.CalculateCoverUv(
                new Vector2(16f, 9f),
                new Vector2(4f, 3f));

            Assert.That(uv.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(uv.width, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(uv.y, Is.EqualTo(0.125f).Within(0.0001f));
            Assert.That(uv.height, Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void CalculateCoverUv_WhenViewportIsNarrower_CropsTextureWidth()
        {
            Rect uv = HomeMenuBackgroundPreviewController.CalculateCoverUv(
                new Vector2(4f, 3f),
                new Vector2(16f, 9f));

            Assert.That(uv.x, Is.EqualTo(0.125f).Within(0.0001f));
            Assert.That(uv.width, Is.EqualTo(0.75f).Within(0.0001f));
            Assert.That(uv.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(uv.height, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void CalculateLivePreviewTextureSize_CapsAtFullHdWithoutChangingAspect()
        {
            Vector2Int size = HomeMenuBackgroundPreviewController.CalculateLivePreviewTextureSize(2560, 1440);

            Assert.That(size, Is.EqualTo(new Vector2Int(1920, 1080)));
        }

        [Test]
        public void CalculateLivePreviewTextureSize_KeepsSmallerTargets()
        {
            Vector2Int size = HomeMenuBackgroundPreviewController.CalculateLivePreviewTextureSize(800, 600);

            Assert.That(size, Is.EqualTo(new Vector2Int(800, 600)));
        }

        [Test]
        public void NormalizeMsaaSampleCount_UsesSupportedPowerOfTwoSamples()
        {
            Assert.That(HomeMenuBackgroundPreviewController.NormalizeMsaaSampleCount(0), Is.EqualTo(1));
            Assert.That(HomeMenuBackgroundPreviewController.NormalizeMsaaSampleCount(3), Is.EqualTo(2));
            Assert.That(HomeMenuBackgroundPreviewController.NormalizeMsaaSampleCount(7), Is.EqualTo(4));
            Assert.That(HomeMenuBackgroundPreviewController.NormalizeMsaaSampleCount(8), Is.EqualTo(8));
        }
    }
}
