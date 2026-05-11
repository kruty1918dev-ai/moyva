using Kruty1918.Moyva.Shared.Performance;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    [Category("PerformanceAcceptance")]
    public sealed class PerformanceAcceptanceTests
    {
        [Test]
        public void Acceptance_FrameBudgetDefaults_Are60FpsOrBetter()
        {
            var budget = FrameBudgetSettings.CreateDefault().Normalize();

            Assert.GreaterOrEqual(budget.TargetFps, 60f, "Target FPS must not be below 60.");
            Assert.LessOrEqual(budget.CpuFrameBudgetMs, 1000f / 55f, "CPU budget is too relaxed for 60 FPS objective.");
            Assert.LessOrEqual(budget.GpuFrameBudgetMs, 1000f / 55f, "GPU budget is too relaxed for 60 FPS objective.");
        }

        [Test]
        public void Acceptance_RenderScalePolicy_HasSafeBoundaries()
        {
            var policy = RenderScalePolicySettings.CreateDefault().Normalize();

            Assert.LessOrEqual(policy.MinimumScale, policy.MaximumScale);
            Assert.GreaterOrEqual(policy.MinimumScale, 0.42f);
            Assert.LessOrEqual(policy.MaximumScale, 1f);
        }

        [Test]
        public void Acceptance_MobileThresholds_AreStrictEnough_For60Fps()
        {
            var thresholds = MobilePerformanceThresholds.CreateDefault().Normalize();

            Assert.GreaterOrEqual(thresholds.LowFpsThreshold, 55f);
            Assert.GreaterOrEqual(thresholds.HealthyFpsThreshold, thresholds.LowFpsThreshold);
        }
    }
}
